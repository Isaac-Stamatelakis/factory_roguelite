using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.QuestBook {
    public static class QuestBookHelper 
    {
        private static string nodeObjectPrefabPath = "UI/Quest/QuestBookNode";
        private static string editModePrefabPath = "UI/Quest/EditModeElements";
        private static string lineModePrefabPath = "UI/Quest/Line";
        private static float gridSize = 64f;

        public static string NodeObjectPrefabPath { get => nodeObjectPrefabPath;}
        public static string EditModePrefabPath { get => editModePrefabPath;}
        public static string LineModePrefabPath { get => lineModePrefabPath;}
        public static float GridSize { get => gridSize;}

        public static Vector2 snapGrid(Vector2 position, Vector2 containerPosition, float containerScale) {
            float scaledGrid = gridSize*containerScale;
            float snappedX = Mathf.Round((position.x - containerPosition.x) / scaledGrid) * scaledGrid + containerPosition.x;
            float snappedY = Mathf.Round((position.y - containerPosition.y) / scaledGrid) * scaledGrid + containerPosition.y;
            return new Vector2(snappedX, snappedY);
        }
    }
}
