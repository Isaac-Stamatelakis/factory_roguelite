using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.QuestBook {
    public static class QuestBookHelper 
    {
        private static string nodeObjectPrefabPath = "UI/Quest/QuestBookNode";
        private static string editModePrefabPath = "UI/Quest/EditModeElements";
        private static float gridSize = 64f;

        public static string NodeObjectPrefabPath { get => nodeObjectPrefabPath;}
        public static string EditModePrefabPath { get => editModePrefabPath;}
        public static float GridSize { get => gridSize;}

        public static Vector2 snapGrid(Vector2 position) {
            float snappedX = Mathf.Round(position.x / gridSize) * gridSize;
            float snappedY = Mathf.Round(position.y / gridSize) * gridSize;
            return new Vector2(snappedX, snappedY);
        }
    }
}

