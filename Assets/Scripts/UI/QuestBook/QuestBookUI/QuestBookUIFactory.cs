using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.QuestBook {
    public static class QuestBookUIFactory
    {
        public static void generateLine(Vector2 nodeAPosition, Vector2 nodeBPosition, Transform container, bool discovered) {
            GameObject line = GameObject.Instantiate(Resources.Load<GameObject>(QuestBookHelper.LineModePrefabPath));
            RectTransform rectTransform = line.GetComponent<RectTransform>();
            line.transform.position = (nodeAPosition+nodeBPosition)/2f;
            float width = rectTransform.sizeDelta.x; // width is set by prefab
            rectTransform.sizeDelta = new Vector2(width,(nodeAPosition-nodeBPosition).magnitude);
            Image image = line.GetComponent<Image>();
            image.color = discovered ? new Color(1f, 0.8431f, 0f) : new Color(0.10196f, 0.10196f, 0.10196f);
            Vector2 direction = (nodeBPosition - nodeAPosition).normalized;
            Quaternion rotation = Quaternion.FromToRotation(Vector2.up, direction);
            line.transform.rotation = rotation;
            line.transform.SetParent(container,false);
        }
    }
}

