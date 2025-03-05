using System;
using TMPro;
using UI.QuestBook.Data.Rewards;
using UnityEngine;
using UnityEngine.UI;

namespace UI.QuestBook.Tasks.Rewards.Command
{
    public class QuestCommandEditorUI : MonoBehaviour
    {
        [SerializeField] private Button mBackButton;
        [SerializeField] private Button mDeleteButton;
        [SerializeField] private TMP_InputField mDescriptionField;
        [SerializeField] private TMP_InputField mCommandField;
        private QuestBookRewardUI questBookRewardUI;

        public void Initialize(QuestBookCommandRewards rewards, QuestBookRewardUI rewardUI, int index)
        {
            questBookRewardUI = rewardUI;
            
            mBackButton.onClick.AddListener(() =>
            {
                GameObject.Destroy(gameObject);
            });
            
            mDeleteButton.onClick.AddListener(() =>
            {
                rewards.CommandRewards.RemoveAt(index);
                GameObject.Destroy(gameObject);
            });
            
            mDescriptionField.text = rewards.CommandRewards[index].Description;
            mCommandField.text = rewards.CommandRewards[index].Command;
            
            mDescriptionField.onValueChanged.AddListener((text) =>
            {
                rewards.CommandRewards[index].Description = text;
            });
            mCommandField.onValueChanged.AddListener((text) =>
            {
                rewards.CommandRewards[index].Command = text;
            });
        }

        public void OnDestroy()
        {
            questBookRewardUI.Display();
        }
    }
}
