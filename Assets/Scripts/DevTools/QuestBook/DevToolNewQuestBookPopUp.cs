using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DevTools.QuestBook
{
    internal class DevToolNewQuestBookPopUp : MonoBehaviour
    {
        [SerializeField] private TMP_InputField mInputField;
        [SerializeField] private Button mBackButton;
        [SerializeField] private Button mCreateButton;
        
        internal void Initialize(Action<string> callback)
        {
            mBackButton.onClick.AddListener(() =>
            {
                GameObject.Destroy(gameObject);
            });
            mCreateButton.onClick.AddListener(() =>
            {
                callback(mInputField.text);
                GameObject.Destroy(gameObject);
            });
        }
        
    }
}
