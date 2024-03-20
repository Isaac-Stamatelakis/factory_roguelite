using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;

namespace UI.QuestBook {
    public static class QuestTaskFactory
    {
        public static string seralize(QuestBookTask task) {
            QuestTaskType taskType = QuestTaskType.Checkmark;
            string taskJson = JsonConvert.SerializeObject(task);
            if (task is ItemQuestTask) {
                taskType = QuestTaskType.Item;
            }
            SerializedTaskQuest serializedTaskQuest = new SerializedTaskQuest(taskJson,taskType.ToString());
            return JsonConvert.SerializeObject(serializedTaskQuest);
        }

        public static QuestBookTask deseralize(string json) {
            SerializedTaskQuest serializedTaskQuest = JsonConvert.DeserializeObject<SerializedTaskQuest>(json);
            if (Enum.TryParse<QuestTaskType>(serializedTaskQuest.type, out QuestTaskType taskType)) {
                switch (taskType) {
                    case QuestTaskType.Item:
                        return JsonConvert.DeserializeObject<ItemQuestTask>(serializedTaskQuest.questBookTaskJson);
                    case QuestTaskType.Checkmark:
                    case QuestTaskType.Dimension:
                        break;
                }
            } else {
                Debug.Log($"{serializedTaskQuest.type} did not match any QuestTaskType enums");
            }

            return null;
        }


        private class SerializedTaskQuest {
            public string questBookTaskJson;
            public string type;
            public SerializedTaskQuest(string taskJson, string type) {
                this.questBookTaskJson = taskJson;
                this.type = type;
            }
        }
    }
}

