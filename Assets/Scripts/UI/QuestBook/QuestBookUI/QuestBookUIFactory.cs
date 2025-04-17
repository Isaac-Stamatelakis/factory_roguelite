using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.QuestBook {
    public static class QuestBookUIFactory
    {
        public static void GenerateLine(Vector2 nodeAPosition, Vector2 nodeBPosition, Transform container, bool discovered, GameObject linePrefab, ref Color highlightColor) {
            GameObject line = GameObject.Instantiate(linePrefab);
            RectTransform rectTransform = line.GetComponent<RectTransform>();
            line.transform.position = (nodeAPosition+nodeBPosition)/2f;
            float width = rectTransform.sizeDelta.x; // width is set by prefab
            rectTransform.sizeDelta = new Vector2(width,(nodeAPosition-nodeBPosition).magnitude);
            Image image = line.GetComponent<Image>();
            image.color = discovered ? (highlightColor.a == 0 ? Color.yellow : highlightColor) : new Color(105/255f,105/255f,105/255f,1f);
            Vector2 direction = (nodeBPosition - nodeAPosition).normalized;
            Quaternion rotation = Quaternion.FromToRotation(Vector2.up, direction);
            line.transform.rotation = rotation;
            line.transform.SetParent(container,false); // Even though rider suggests changing this it is wrong to
        }
    }
}

