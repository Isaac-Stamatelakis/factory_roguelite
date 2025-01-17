using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;

namespace UI.QuestBook {
    public static class QuestTaskFactory
    {
        public static string seralizeList(List<QuestBookTask> tasks) {
            List<SerializedTaskQuest> seralized = new List<SerializedTaskQuest>();
            foreach (QuestBookTask task in tasks) {
                QuestTaskType taskType = task.GetTaskType();
                string taskJson = JsonConvert.SerializeObject(task);
                seralized.Add(new SerializedTaskQuest(taskJson,taskType.ToString()));
            }
            return JsonConvert.SerializeObject(seralized);
        }

        public static string serialize(QuestBookTask task) {
            if (task == null) {
                return null;
            }
            QuestTaskType taskType = task.GetTaskType();
            string taskJson = JsonConvert.SerializeObject(task);
            return JsonConvert.SerializeObject(new SerializedTaskQuest(taskJson,taskType.ToString()));
        }

        public static QuestBookTask deseralize(string json) {
            SerializedTaskQuest serializedTaskQuests = JsonConvert.DeserializeObject<SerializedTaskQuest>(json);
            return deseralizeTask(serializedTaskQuests);
        }

        public static List<QuestBookTask> deseralizeList(string json) {
            List<SerializedTaskQuest> serializedTaskQuests = JsonConvert.DeserializeObject<List<SerializedTaskQuest>>(json);
            List<QuestBookTask> tasks = new List<QuestBookTask>();
            foreach (SerializedTaskQuest serializedTaskQuest in serializedTaskQuests) {
                tasks.Add(deseralizeTask(serializedTaskQuest));
            }
            return tasks;
        }

        private static QuestBookTask deseralizeTask(SerializedTaskQuest serializedTaskQuest) {
            if (Enum.TryParse<QuestTaskType>(serializedTaskQuest.type, out QuestTaskType taskType)) {
                switch (taskType) {
                    case QuestTaskType.Item:
                        return JsonConvert.DeserializeObject<ItemQuestTask>(serializedTaskQuest.questBookTaskJson);
                    case QuestTaskType.Checkmark:
                        return JsonConvert.DeserializeObject<CheckMarkQuestTask>(serializedTaskQuest.questBookTaskJson);
                    case QuestTaskType.Dimension:
                        return JsonConvert.DeserializeObject<VisitDimensionQuestTask>(serializedTaskQuest.questBookTaskJson);
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

