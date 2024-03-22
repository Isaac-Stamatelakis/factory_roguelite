using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.QuestBook {
    public static class QuestBookUIFactory
    {
        public static void generateLine(Vector2 nodeAPosition, Vector2 nodeBPosition, Transform container, bool discovered) {
            GameObject line = GameObject.Instantiate(Resources.Load<GameObject>(QuestBookHelper.LineModePrefabPath));
            RectTransform rectTransform = line.GetComponent<RectTransform>();
            line.transform.position = (nodeAPosition-nodeBPosition)/2f;
            float width = rectTransform.sizeDelta.x; // width is set by prefab
            rectTransform.sizeDelta = new Vector2(width,(nodeAPosition-nodeBPosition).magnitude);
            float angle = Vector2.Angle(nodeAPosition,nodeBPosition);
            Quaternion rotation = line.transform.rotation;
            rotation.z = angle;
            line.transform.rotation = rotation;
            line.transform.SetParent(container,false);
        }

        public static void generateNode(QuestBookNode node, Transform container, QuestBookUI questBookUI) {
            GameObject instantiated = GameObject.Instantiate(Resources.Load<GameObject>(QuestBookHelper.NodeObjectPrefabPath));
            QuestBookNodeObject nodeObject = instantiated.GetComponent<QuestBookNodeObject>();
            nodeObject.init(node,questBookUI);
            nodeObject.transform.SetParent(container,false);
        }
    }
}

