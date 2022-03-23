using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ryuu.HierarchyIcon.Editor
{
    internal static class HierarchyIconCore
    {
        public static HierarchyIconInfo Info;
        private static GameObject offsetGameObject;
        private static int offsetIndex;

        [InitializeOnLoadMethod]
        public static void HierarchyIconInitialize()
        {
            // get file
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(HierarchyIconInfo)}");

            // create info if no exist
            if (guids == null || guids.Length == 0)
            {
                CreateInfo();
            }

            SetInfo();
        }

        public static void CreateInfo()
        {
            var info = ScriptableObject.CreateInstance<HierarchyIconInfo>();
            if (!Directory.Exists("Assets/Plugins/Ryuu/Hierarchy Icon/"))
            {
                Directory.CreateDirectory("Assets/Plugins/Ryuu/Hierarchy Icon/");
            }

            AssetDatabase.CreateAsset(info,
                $"Assets/Plugins/Ryuu/Hierarchy Icon/{nameof(HierarchyIconInfo)}.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(
                $"{nameof(HierarchyIconInfo)} created.\n Check {HierarchyIconEditorWindow.MENU_ITEM_PATH}"
            );
        }

        public static void SetInfo()
        {
            if (Info == null)
            {
                string[] guids = AssetDatabase.FindAssets($"t:{nameof(HierarchyIconInfo)}");
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                Info = AssetDatabase.LoadAssetAtPath<HierarchyIconInfo>(path);
            }

            if (Info.enableWhenOpenEditor)
            {
                Enable();
            }
        }

        public static void ShowHideIcon(string assemblyQualifiedName)
        {
            if (Info.targetNames.Contains(assemblyQualifiedName))
            {
                Info.targetNames.Remove(assemblyQualifiedName);
            }
            else
            {
                Info.targetNames.Add(assemblyQualifiedName);
            }

            Info.Refresh();
            offsetIndex = 0;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void EnableOrDisable()
        {
            if (EditorApplication.hierarchyWindowItemOnGUI.GetInvocationList().Any(@delegate =>
                    @delegate.Method.Name.Equals(nameof(HierarchyIconOnGUI))))
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
            // remove delegate if exist
            EditorApplication.hierarchyWindowItemOnGUI -= HierarchyIconOnGUI;
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyIconOnGUI;
        }

        private static void Disable()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= HierarchyIconOnGUI;
        }

        private static void HierarchyIconOnGUI(int instanceID, Rect rect)
        {
            if (Info == null)
            {
                Debug.LogWarning(
                    $"There is no {nameof(HierarchyIconInfo)}. Go to Tools -> Hierarchy Icon. Set the {nameof(HierarchyIconInfo)}.\n" +
                    $"You can create info file by click {nameof(CreateInfo)} button in {nameof(HierarchyIconEditorWindow)}."
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
            components.RemoveAll(component => component == null || IsTargetTypes(component.GetType()));

            if (components.Count == 0)
            {
                return;
            }


            int tmpOffsetIndex = gameObject == offsetGameObject ? offsetIndex : 0;
            rect.width += rect.x;
            rect.x += Info.iconOffset.x;
            rect.y += Info.iconOffset.y;

            // icon
            int i = 0;
            for (; i + tmpOffsetIndex < components.Count && i < Info.iconCount; i++)
            {
                var texture = AssetPreview.GetMiniThumbnail(components[i + tmpOffsetIndex]);

                if (texture)
                {
                    GUI.DrawTexture(
                        new Rect(rect.width - (Info.iconSize + Info.iconGap) * (i + 1), rect.y, Info.iconSize,
                            Info.iconSize), texture);
                }
            }

            // more
            if (components.Count > Info.iconCount)
            {
                if (!GUI.Button(
                        new Rect(rect.width - (Info.iconSize + Info.iconGap) * (i + 1), rect.y, Info.iconSize,
                            Info.iconSize),
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
                if (offsetGameObject != gameObject)
                {
                    offsetGameObject = gameObject;
                    offsetIndex = 0;
                }

                offsetIndex += Info.iconCount;

                // reset offset
                if (offsetIndex >= components.Count)
                {
                    offsetIndex = 0;
                }
            }

            static bool IsTargetTypes(Type typeIn) =>
                Info.targetTypes.Any(type => type == typeIn || typeIn.IsSubclassOf(type));
        }
    }
}