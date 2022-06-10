using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ryuu.HierarchyIcon.Editor
{
    internal static class HierarchyIconController
    {
        public static HierarchyIconModel Model;
        private static GameObject offsetGameObjectCache;
        private static int offsetIndexCache;

        [InitializeOnLoadMethod]
        public static void HierarchyIconInitialize()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(HierarchyIconModel)}");

            if (guids == null || guids.Length == 0)
            {
                CreateModel();
            }

            SetModel();
        }

        public static void CreateModel()
        {
            const string modelPath = "Assets/Plugins/Ryuu/Hierarchy Icon/";

            var info = ScriptableObject.CreateInstance<HierarchyIconModel>();
            if (!Directory.Exists(modelPath))
            {
                Directory.CreateDirectory(modelPath);
            }

            AssetDatabase.CreateAsset(info,
                $"{modelPath}{nameof(HierarchyIconModel)}.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(
                $"{nameof(HierarchyIconModel)} created.\n Check {HierarchyIconEditorWindow.MENU_ITEM_PATH}"
            );
        }

        public static void SetModel()
        {
            if (Model == null)
            {
                string[] guids = AssetDatabase.FindAssets($"t:{nameof(HierarchyIconModel)}");
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                Model = AssetDatabase.LoadAssetAtPath<HierarchyIconModel>(path);
            }

            if (Model.enableWhenOpenEditor)
            {
                Enable();
            }
        }

        public static void ShowHideIconByComponentType(Type type)
        {
            if (Model.ContainsTargetType(type))
            {
                Model.RemoveTargetType(type);
            }
            else
            {
                Model.AddTargetType(type);
            }

            offsetIndexCache = 0;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void EnableOrDisable()
        {
            if (EditorApplication.hierarchyWindowItemOnGUI
                .GetInvocationList()
                .Any(
                    @delegate => @delegate.Method.Name.Equals(nameof(HierarchyIconOnGUI))
                )
               )
            {
                Disable();
            }
            else
            {
                Enable();
            }
        }

        private static void Enable()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyIconOnGUI;
        }

        private static void Disable()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= HierarchyIconOnGUI;
        }

        private static void HierarchyIconOnGUI(int instanceID, Rect rect)
        {
            if (Model == null)
            {
                Debug.LogWarning(
                    $"There is no {nameof(HierarchyIconModel)}.\n" +
                    $" Go to Tools -> Hierarchy Icon. Set the {nameof(HierarchyIconModel)}.\n" +
                    $"You can create info file by click {nameof(CreateModel)} button in {nameof(HierarchyIconEditorWindow)}."
                );
                return;
            }

            // type check
            if (EditorUtility.InstanceIDToObject(instanceID) is not GameObject gameObject)
            {
                return;
            }

            // target components
            var components = new List<Component>(gameObject.GetComponents<Component>());
            components.RemoveAll(component => component == null || Model.IsTargetTypes(component.GetType()));

            if (components.Count == 0)
            {
                return;
            }

            int offsetIndex = gameObject == offsetGameObjectCache ? offsetIndexCache : 0;
            rect.width += rect.x;
            rect.x += Model.iconOffset.x;
            rect.y += Model.iconOffset.y;

            // icon
            int i = 0;
            for (; i + offsetIndex < components.Count && i < Model.iconCount; i++)
            {
                Texture2D texture = AssetPreview.GetMiniThumbnail(components[i + offsetIndex]);

                if (texture)
                {
                    GUI.DrawTexture(
                        new Rect(rect.width - (Model.iconSize + Model.iconGap) * (i + 1), rect.y, Model.iconSize,
                            Model.iconSize), texture);
                }
            }

            // more button
            if (components.Count > Model.iconCount)
            {
                if (!GUI.Button(
                        new Rect(rect.width - (Model.iconSize + Model.iconGap) * (i + 1), rect.y, Model.iconSize,
                            Model.iconSize),
                        "···",
                        new GUIStyle(GUI.skin.label)
                        {
                            fontSize = 18,
                            alignment = TextAnchor.MiddleCenter
                        }
                    ))
                {
                    return;
                }

                // reset target gameObject
                if (offsetGameObjectCache != gameObject)
                {
                    offsetGameObjectCache = gameObject;
                    offsetIndexCache = 0;
                }

                offsetIndexCache += Model.iconCount;

                // reset offset
                if (offsetIndexCache >= components.Count)
                {
                    offsetIndexCache = 0;
                }
            }
        }
    }
}