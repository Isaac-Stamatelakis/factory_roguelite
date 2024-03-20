using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.QuestBook {
    public abstract class QuestBookTaskUI<Task> : MonoBehaviour where Task : QuestBookTask
    {
        public abstract void init(Task task); 
    }
}

