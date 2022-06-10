using System;
using UnityEditor;
using UnityEngine;
using static Ryuu.HierarchyIcon.Editor.HierarchyIconController;

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
            if (!Model)
            {
                ButtonCreateModel();
                EditorGUILayout.Space();
                NoModelHelpBox();
            }
            else
            {
                ObjectFieldModel();
                ButtonEnableDisable();
                EditorGUILayout.Space();
                ObjectFieldComponent();
                ButtonComponent();
            }

            static void ButtonCreateModel()
            {
                if (!GUILayout.Button(nameof(CreateModel))) return;

                CreateModel();
                SetModel();
            }

            static void NoModelHelpBox()
            {
                EditorGUILayout.HelpBox(
                    $"There is no {nameof(HierarchyIconModel)}.\n" +
                    $" You can create an info file by clicking {nameof(ButtonCreateModel)}.",
                    MessageType.Warning
                );
            }

            static void ObjectFieldModel()
            {
                Model = (HierarchyIconModel) EditorGUILayout.ObjectField(
                    $"Target {nameof(HierarchyIconModel)}",
                    Model,
                    typeof(HierarchyIconModel),
                    false
                );
            }

            static void ButtonEnableDisable()
            {
                if (GUILayout.Button("Enable / Disable")) EnableOrDisable();
            }

            static void ObjectFieldComponent()
            {
                component = (Component) EditorGUILayout.ObjectField(
                    $"Target {nameof(Component)}",
                    component,
                    typeof(Component),
                    true
                );
            }

            static void ButtonComponent()
            {
                if (component == null)
                {
                    return;
                }

                Type type = component.GetType();
                while (type is {AssemblyQualifiedName: { }} && !type.AssemblyQualifiedName.Equals(typeof(Component).AssemblyQualifiedName))
                {
                    if (GUILayout.Button($"Show / Hide : {type.FullName}"))
                    {
                        ShowHideIconByComponentType(type);
                    }

                    type = type.BaseType;
                }
            }
        }
    }
}