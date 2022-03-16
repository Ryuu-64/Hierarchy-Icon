using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryuu.HierarchyIcon.Editor
{
    internal class HierarchyIconInfo : ScriptableObject
    {
        public bool enableWhenOpenEditor;
        public int iconCount;
        public int iconSize;
        public int iconGap;
        public Vector2 iconOffset;
        public List<string> targetNames;
        public List<Type> targetTypes;

        private HierarchyIconInfo()
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
            Refresh();
        }

        private void OnValidate() => Refresh();

        public void Refresh()
        {
            targetTypes.Clear();
            foreach (string targetName in targetNames)
            {
                targetTypes.Add(Type.GetType(targetName));
            }
        }
    }
}