using System;
using System.Collections.Generic;
using DevTools;
using Item.Slot;
using Player;
using PlayerModule;
using TMPro;
using UI.Chat;
using UI.QuestBook.Tasks.Rewards.Command;
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
        [SerializeField] private TextMeshProUGUI mTitle;
        [SerializeField] private Button mLeftButton;
        [SerializeField] private Button mRightButton;
        [SerializeField] private VerticalLayoutGroup mElementContainer;
        [SerializeField] private Button mClaimButton;
        [SerializeField] private Button mAddButton;
        [SerializeField] private Toggle mToggle;

        [SerializeField] private QuestCommandEditorUI questCommandEditorUIPrefab;
        [SerializeField] private RewardListElement rewardListElementPrefab;
        [SerializeField] private CommandRewardListElement commandRewardListElementPrefab;
        [SerializeField] private TextMeshProUGUI textPrefab;
        
        private RewardPage currentPage = RewardPage.Items;
        private QuestBookTaskPageUI parentUI;
        public QuestBookTaskPageUI ParentUI => parentUI;
        private QuestBookNode questBookNode;
        [HideInInspector] public int SelectedRewardIndex = -1;

        public void Initialize(QuestBookNode questBookNode, QuestBookTaskPageUI parentUI)
        {
            mLeftButton.onClick.RemoveAllListeners();
            mRightButton.onClick.RemoveAllListeners();
            mAddButton.onClick.RemoveAllListeners();
            mClaimButton.onClick.RemoveAllListeners();

            this.questBookNode = questBookNode;
            this.parentUI = parentUI;
            
            mAddButton.onClick.AddListener(AddButtonPress);
            mClaimButton.onClick.AddListener(ClaimPress);

            if (!DevToolUtils.OnDevToolScene)
            {
                mAddButton.gameObject.SetActive(false);
                mToggle.gameObject.SetActive(false);
                if (questBookNode.Content.ItemRewards.Rewards.Count > 0)
                {
                    currentPage = RewardPage.Items;
                } else if (questBookNode.Content.CommandRewards.CommandRewards.Count > 0)
                {
                    currentPage = RewardPage.Commands;
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

        private int GetPages()
        {
            bool itemActive = questBookNode.Content.ItemRewards.Rewards.Count > 0;
            bool commandActive = questBookNode.Content.CommandRewards.CommandRewards.Count > 0;

            int activeCount = 0; // This is overkill formatted this way in case I ever add more reward types
            if (itemActive) activeCount++;
            if (commandActive) activeCount++;
            return activeCount;
        }

        private void DisplayArrows()
        {
            if (DevToolUtils.OnDevToolScene)
            {
                mLeftButton.gameObject.SetActive(true);
                mRightButton.gameObject.SetActive(true);
                return;
            }

            int pages = GetPages();
            
            if (pages <= 1)
            {
                mLeftButton.gameObject.SetActive(false);
                mRightButton.gameObject.SetActive(false);
                return;
            }

            int currentPageIndex = (int)currentPage;
            mLeftButton.gameObject.SetActive(currentPage != 0);
            mRightButton.gameObject.SetActive(currentPageIndex != pages - 1);
        }

        public void Display()
        {
            GlobalHelper.deleteAllChildren(mElementContainer.transform);
            UpdateClaimButtonImage();
            DisplayArrows();
            switch (currentPage)
            {
                case RewardPage.Items:
                    mToggle.gameObject.SetActive(DevToolUtils.OnDevToolScene);
                    QuestBookItemRewards questBookItemRewards = questBookNode.Content.ItemRewards;
                    mToggle.onValueChanged.RemoveAllListeners();
                    mToggle.isOn = questBookItemRewards.LimitOne;
                    mToggle.onValueChanged.AddListener((state) =>
                    {
                        switch (currentPage)
                        {
                            case RewardPage.Items:
                                this.questBookNode.Content.ItemRewards.LimitOne = state;
                                Display();
                                break;
                            case RewardPage.Commands:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    });
                    if (questBookItemRewards.LimitOne)
                    {
                        mTitle.text = "Item Reward";
                    }
                    else
                    {
                        mTitle.text = "Item Rewards";
                    }
                    questBookItemRewards.Rewards ??= new List<SerializedItemSlot>();
                    for (int i = 0; i < questBookItemRewards.Rewards.Count; i++) {
                        RewardListElement rewardListElement = GameObject.Instantiate(rewardListElementPrefab, mElementContainer.transform, false);
                        rewardListElement.Initialize(questBookItemRewards.Rewards,i,this);
                    }
                    break;
                case RewardPage.Commands:
                    mToggle.gameObject.SetActive(false);
                    mTitle.text = "Special Rewards";
                    QuestBookCommandRewards commandRewards = questBookNode.Content.CommandRewards;
                    for (int i = 0; i < commandRewards.CommandRewards.Count; i++) {
                        CommandRewardListElement commandRewardListElement = GameObject.Instantiate(commandRewardListElementPrefab, mElementContainer.transform, false);
                        commandRewardListElement.Initialize(this,commandRewards,i);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool RewardClaimed()
        {
            switch (currentPage)
            {
                case RewardPage.Items:
                    return questBookNode.TaskData.RewardStatus.ItemsClaimed;
                case RewardPage.Commands:
                    return questBookNode.TaskData.RewardStatus.CommandsClaimed;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public void UpdateClaimButtonImage()
        {
            bool rewardClaimed = RewardClaimed();
            bool canClaim = !rewardClaimed && questBookNode.TaskData.Complete;
            mClaimButton.GetComponent<Image>().color = canClaim ? Color.green : Color.gray;
            int pages = GetPages();
            TextMeshProUGUI claimTextUI = mClaimButton.GetComponentInChildren<TextMeshProUGUI>();
            if (pages == 0)
            {
                mClaimButton.interactable = false;
                claimTextUI.text = "Nothing to Claim";
            }
            else
            {
                claimTextUI.text = rewardClaimed ? "Claimed" : "Claim";
            }
            
            mClaimButton.interactable = canClaim || DevToolUtils.OnDevToolScene;
        }

        public void ClaimPress()
        {
            if (!questBookNode.TaskData.Complete) return;

            bool cheatMode = DevToolUtils.OnDevToolScene;
            
            switch (currentPage)
            {
                case RewardPage.Items:
                    if (questBookNode.TaskData.RewardStatus.ItemsClaimed) break;
                    
                    questBookNode.TaskData.RewardStatus.ItemsClaimed = true;
                    break;
                case RewardPage.Commands:
                    if (!cheatMode && questBookNode.TaskData.RewardStatus.CommandsClaimed) break;

                    if (!questBookNode.TaskData.RewardStatus.CommandsClaimed)
                    {
                        foreach (var commandReward in questBookNode.Content.CommandRewards.CommandRewards)
                        {
                            TextChatUI.Instance.ExecuteCommand(commandReward.Command, printErrors: false);
                        }
                    }
                    
                    questBookNode.TaskData.RewardStatus.CommandsClaimed = !questBookNode.TaskData.RewardStatus.CommandsClaimed; // Allows edit mode to reverse
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            UpdateClaimButtonImage();


        }

        private bool GiveItemRewards(QuestBookItemRewards itemRewards)
        {
            PlayerInventory playerInventory = PlayerManager.Instance.GetPlayer().PlayerInventory;
            if (itemRewards.LimitOne)
            {
                if (SelectedRewardIndex < 0 || SelectedRewardIndex >= itemRewards.Rewards.Count) return false;
                ItemSlot itemSlot = ItemSlotFactory.deseralizeItemSlot(itemRewards.Rewards[SelectedRewardIndex]);
                playerInventory.Give(itemSlot);
                HighlightItem(SelectedRewardIndex);
                return true;
            }
                    
            foreach (var sItemSlot in itemRewards.Rewards)
            {
                ItemSlot itemSlot = ItemSlotFactory.deseralizeItemSlot(sItemSlot);
                if (ItemSlotUtils.IsItemSlotNull(itemSlot)) continue;
                        
                playerInventory.Give(itemSlot);
            }

            return true;
        }

        public void AddButtonPress()
        {
            switch (currentPage)
            {
                case RewardPage.Items:
                    questBookNode.Content.ItemRewards.Rewards.Add(new SerializedItemSlot("stone",1,null));
                    break;
                case RewardPage.Commands:
                    questBookNode.Content.CommandRewards.CommandRewards.Add(new QuestBookCommandReward("Empty Description", "/help"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Display();
        }
        
        public void SelectReward(int index)
        {
            if (questBookNode.TaskData.RewardStatus.ItemsClaimed) return;
            if (SelectedRewardIndex >= 0)
            {
                HighlightItem(SelectedRewardIndex); // Deselect highlighted
            }
            
            
            SelectedRewardIndex = index;
            HighlightItem(index); // Highlight new
            
        }

        private void HighlightItem(int index)
        {
            mElementContainer.transform.GetChild(SelectedRewardIndex).GetComponent<RewardListElement>().ToggleHighlight();
        }

        public void DisplayCommandRewardEditor(int index)
        {
            QuestCommandEditorUI editorUI = Instantiate(questCommandEditorUIPrefab, parentUI.transform, false);
            editorUI.Initialize(questBookNode.Content.CommandRewards,this,index);
            
        }
    }
}
