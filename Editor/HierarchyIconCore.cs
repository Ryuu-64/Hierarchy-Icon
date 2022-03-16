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
        private static int offset;

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

            // set info
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
                $"{nameof(HierarchyIconInfo)} created. Check {HierarchyIconEditorWindow.MENU_ITEM_PATH}"
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
            offset = 0;
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


            int offsetIndex = gameObject == offsetGameObject ? HierarchyIconCore.offset : 0;
            rect.width += rect.x;
            rect.x += Info.iconOffset.x;
            rect.y += Info.iconOffset.y;

            // icon
            int i = 0;
            for (; i + offsetIndex < components.Count && i < Info.iconCount; i++)
            {
                var texture = AssetPreview.GetMiniThumbnail(components[i + offsetIndex]);

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
                    HierarchyIconCore.offset = 0;
                }

                HierarchyIconCore.offset += Info.iconCount;

                // reset offset
                if (HierarchyIconCore.offset >= components.Count)
                {
                    HierarchyIconCore.offset = 0;
                }
            }

            static bool IsTargetTypes(Type typeIn) =>
                Info.targetTypes.Any(type => type == typeIn || typeIn.IsSubclassOf(type));
        }
    }
}