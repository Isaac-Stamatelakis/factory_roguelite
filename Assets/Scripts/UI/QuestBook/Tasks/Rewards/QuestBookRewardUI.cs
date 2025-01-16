using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace UI.QuestBook.Tasks.Rewards
{
    internal enum RewardPage
    {
        Items,
        Commands
    }
    public class QuestBookRewardUI : MonoBehaviour
    {
        [SerializeField] private Button mLeftButton;
        [SerializeField] private Button mRightButton;
        [SerializeField] private VerticalLayoutGroup mElementContainer;
        [SerializeField] private Button mClaimButton;
        [SerializeField] private Button mAddButton;
        
        [SerializeField] private RewardListElement rewardListElementPrefab;
        [SerializeField] private TextMeshProUGUI textPrefab;
        
        private RewardPage currentPage = RewardPage.Items;
        private QuestBookTaskPageUI parentUI;
        public QuestBookTaskPageUI ParentUI => parentUI;
        private QuestBookNodeContent content;
        [HideInInspector] public List<int> SelectedRewards = new List<int>();

        public void Initialize(QuestBookNodeContent content, QuestBookTaskPageUI parentUI)
        {
            mLeftButton.onClick.RemoveAllListeners();
            mRightButton.onClick.RemoveAllListeners();
            mAddButton.onClick.RemoveAllListeners();
            mClaimButton.onClick.RemoveAllListeners();
            
            this.content = content;
            this.parentUI = parentUI;
            
            mAddButton.onClick.AddListener(AddButtonPress);
            mClaimButton.onClick.AddListener(ClaimPress);

            if (!QuestBookHelper.EditMode)
            {
                bool itemActive = content.ItemRewards.Rewards.Count > 0;
                bool commandActive = content.CommandRewards.CommandRewards.Count > 0;

                int activeCount = 0; // This is overkill formatted this way in case I ever add more reward types
                if (itemActive) activeCount++;
                if (commandActive) activeCount++;
                if (activeCount == 0)
                {
                    mClaimButton.interactable = false;
                    TextMeshProUGUI claimText = mClaimButton.GetComponentInChildren<TextMeshProUGUI>();
                    claimText.text = "Nothing to Claim";
                }
                if (itemActive)
                {
                    currentPage = RewardPage.Items;
                } else if (commandActive)
                {
                    currentPage = RewardPage.Commands;
                }
                
                if (activeCount <= 1)
                {
                    mLeftButton.gameObject.SetActive(false);
                    mRightButton.gameObject.SetActive(false);
                    return;
                }
            }
            
            mLeftButton.onClick.AddListener(() =>
            {
                currentPage = GlobalHelper.ShiftEnum(-1, currentPage);
                Display();
            });
                
            mRightButton.onClick.AddListener(() =>
            {
                currentPage = GlobalHelper.ShiftEnum(1, currentPage);
                Display();
            });
            Display();
            
        }


        public void Display()
        {
            GlobalHelper.deleteAllChildren(mElementContainer.transform);
            switch (currentPage)
            {
                case RewardPage.Items:
                    QuestBookItemRewards questBookItemRewards = content.ItemRewards;
                    if (questBookItemRewards.Selectable < questBookItemRewards.Rewards.Count)
                    {
                        TextMeshProUGUI text = Instantiate(textPrefab, mElementContainer.transform, false);
                        text.text = "Select " + questBookItemRewards.Rewards.Count + " Rewards";
                    }
                    questBookItemRewards.Rewards ??= new List<SerializedItemSlot>();
                    for (int i = 0; i < questBookItemRewards.Rewards.Count; i++) {
                        RewardListElement rewardListElement = GameObject.Instantiate(rewardListElementPrefab, mElementContainer.transform, false);
                        rewardListElement.Initialize(questBookItemRewards.Rewards,i,this);
                    }
                    break;
                case RewardPage.Commands:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void ClaimPress()
        {
            switch (currentPage)
            {
                case RewardPage.Items:
                    break;
                case RewardPage.Commands:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void AddButtonPress()
        {
            switch (currentPage)
            {
                case RewardPage.Items:
                    content.ItemRewards.Rewards.Add(new SerializedItemSlot("stone",1,null));
                    Display();
                    break;
                case RewardPage.Commands:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public void AddReward(int index) {
            if (SelectedRewards.Count < content.ItemRewards.Selectable) {
                SelectedRewards.Add(index);
            } else {
                SelectedRewards.RemoveAt(0);
                SelectedRewards.Add(index);
            }
        }

        public void RemoveReward(int index) {
            SelectedRewards.RemoveAt(index);
        }
    }
}
