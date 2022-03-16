using UnityEditor;
using UnityEngine;
using static Ryuu.HierarchyIcon.Editor.HierarchyIconCore;

namespace Ryuu.HierarchyIcon.Editor
{
    internal class HierarchyIconEditorWindow : EditorWindow
    {
        public const string MENU_ITEM_PATH = "Tools/Hierarchy Icon";
        private static Component component;

        public HierarchyIconEditorWindow() => titleContent = new GUIContent(nameof(HierarchyIconEditorWindow));

        [MenuItem(MENU_ITEM_PATH)]
        public static void ShowWindow() => GetWindow<HierarchyIconEditorWindow>();

        private void OnGUI()
        {
            BtnCreateInfoInfo();
            EditorGUILayout.Space();
            ObjFldInfo();
            BtnEnableDisable();
            EditorGUILayout.Space();
            ObjFldComponent();
            BtnComponent();

            static void BtnCreateInfoInfo()
            {
                if (Info)
                {
                    return;
                }

                if (GUILayout.Button(nameof(CreateInfo)))
                {
                    CreateInfo();
                    SetInfo();
                }

                EditorGUILayout.HelpBox(
                    $"There is no {nameof(HierarchyIconInfo)}.\n" +
                    $" You can create an info file by clicking {nameof(CreateInfo)} button.",
                    MessageType.Warning
                );
            }

            static void ObjFldInfo()
            {
                Info = (HierarchyIconInfo) EditorGUILayout.ObjectField(
                    $"Target {nameof(HierarchyIconInfo)}",
                    Info,
                    typeof(HierarchyIconInfo),
                    false
                );
            }

            static void BtnEnableDisable()
            {
                if (GUILayout.Button("Enable / Disable")) EnableOrDisable();
            }

            static void ObjFldComponent()
            {
                component = (Component) EditorGUILayout.ObjectField(
                    $"Target {nameof(Component)}",
                    component,
                    typeof(Component),
                    true
                );
            }

            static void BtnComponent()
            {
                if (component == null)
                {
                    return;
                }

                var type = component.GetType();
                while (type != null && !type.Assembly.Equals(typeof(Component).Assembly))
                {
                    if (GUILayout.Button($"Show / Hide : {type.FullName}"))
                    {
                        ShowHideIcon(type.AssemblyQualifiedName);
                    }

                    type = type.BaseType;
                }
            }
        }
    }
}