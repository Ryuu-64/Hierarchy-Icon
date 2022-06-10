using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ryuu.HierarchyIcon.Editor
{
    internal class HierarchyIconModel : ScriptableObject
    {
        public bool enableWhenOpenEditor;
        public int iconCount;
        public int iconSize;
        public int iconGap;
        public Vector2 iconOffset;
        [SerializeField] private List<string> targetNames;
        [SerializeField] private List<Type> targetTypes;

        private HierarchyIconModel()
        {
            enableWhenOpenEditor = true;
            iconCount = 8;
            iconSize = 16;
            iconGap = 1;
            iconOffset = Vector2.zero;
            targetNames = new List<string>
            {
                typeof(Transform).AssemblyQualifiedName,
                typeof(RectTransform).AssemblyQualifiedName,
            };
            targetTypes = new List<Type>();
            RefreshTargetTypes();
        }

        private void OnValidate() => RefreshTargetTypes();

        private void RefreshTargetTypes()
        {
            targetNames = targetNames.Distinct().ToList();

            targetTypes.Clear();
            foreach (string targetName in targetNames)
            {
                targetTypes.Add(Type.GetType(targetName));
            }
        }

        public bool ContainsTargetType(Type type)
        {
            return targetNames.Contains(type.AssemblyQualifiedName);
        }

        public void RemoveTargetType(Type type)
        {
            targetNames.Remove(type.AssemblyQualifiedName);
            RefreshTargetTypes();
        }

        public void AddTargetType(Type type)
        {
            targetNames.Add(type.AssemblyQualifiedName);
            RefreshTargetTypes();
        }

        public bool IsTargetTypes(Type typeIn)
        {
            return targetTypes.Any(type => type == typeIn || typeIn.IsSubclassOf(type));
        }
    }
}