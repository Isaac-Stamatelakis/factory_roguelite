using System;
using System.Collections.Generic;
using Item.Slot;
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
        private QuestBookNodeContent content;
        [HideInInspector] public int SelectedRewardIndex = -1;

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

            if (!QuestBookUtils.EditMode)
            {
                mAddButton.gameObject.SetActive(false);
                mToggle.gameObject.SetActive(false);
                int pages = GetPages();
                if (pages == 0)
                {
                    mClaimButton.interactable = false;
                    TextMeshProUGUI claimText = mClaimButton.GetComponentInChildren<TextMeshProUGUI>();
                    claimText.text = "Nothing to Claim";
                }
                if (content.ItemRewards.Rewards.Count > 0)
                {
                    currentPage = RewardPage.Items;
                } else if (content.CommandRewards.CommandRewards.Count > 0)
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
            bool itemActive = content.ItemRewards.Rewards.Count > 0;
            bool commandActive = content.CommandRewards.CommandRewards.Count > 0;

            int activeCount = 0; // This is overkill formatted this way in case I ever add more reward types
            if (itemActive) activeCount++;
            if (commandActive) activeCount++;
            return activeCount;
        }

        private void DisplayArrows()
        {
            if (QuestBookUtils.EditMode)
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
                    mToggle.gameObject.SetActive(QuestBookUtils.EditMode);
                    QuestBookItemRewards questBookItemRewards = content.ItemRewards;
                    mToggle.onValueChanged.RemoveAllListeners();
                    mToggle.isOn = questBookItemRewards.LimitOne;
                    mToggle.onValueChanged.AddListener((state) =>
                    {
                        switch (currentPage)
                        {
                            case RewardPage.Items:
                                this.content.ItemRewards.LimitOne = state;
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
                    QuestBookCommandRewards commandRewards = content.CommandRewards;
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
                    return content.ItemRewards.Claimed;
                case RewardPage.Commands:
                    return content.CommandRewards.Claimed;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public void UpdateClaimButtonImage()
        {
            bool rewardClaimed = RewardClaimed();
            bool canClaim = !rewardClaimed && content.Task.IsComplete();
            mClaimButton.GetComponent<Image>().color = canClaim ? Color.green : Color.gray;
            mClaimButton.interactable = canClaim;
            
        }

        public void ClaimPress()
        {
            if (!content.Task.IsComplete()) return;
            
            switch (currentPage)
            {
                case RewardPage.Items:
                    var itemRewards = content.ItemRewards;
                    if (itemRewards.Claimed) return;

                    PlayerInventory playerInventory = PlayerManager.Instance.GetPlayer().PlayerInventory;
                    if (itemRewards.LimitOne)
                    {
                        if (SelectedRewardIndex < 0 || SelectedRewardIndex >= itemRewards.Rewards.Count) return;
                        ItemSlot itemSlot = ItemSlotFactory.deseralizeItemSlot(itemRewards.Rewards[SelectedRewardIndex]);
                        playerInventory.Give(itemSlot);
                        itemRewards.Claimed = true;
                        return;
                    }
                    
                    foreach (var sItemSlot in itemRewards.Rewards)
                    {
                        ItemSlot itemSlot = ItemSlotFactory.deseralizeItemSlot(sItemSlot);
                        if (ItemSlotUtils.IsItemSlotNull(itemSlot)) continue;
                        
                        playerInventory.Give(itemSlot);
                    }
                    itemRewards.Claimed = true;
                    
                    break;
                case RewardPage.Commands:
                    if (content.CommandRewards.Claimed) return;

                    foreach (var commandReward in content.CommandRewards.CommandRewards)
                    {
                        TextChatUI.Instance.ExecuteCommand(commandReward.Command, printErrors: false);
                    }

                    content.CommandRewards.Claimed = true;
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
                    break;
                case RewardPage.Commands:
                    content.CommandRewards.CommandRewards.Add(new QuestBookCommandReward("Empty Description", "/help"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Display();
        }
        
        public void SelectReward(int index) {
            if (SelectedRewardIndex > 0)
            {
                mElementContainer.transform.GetChild(SelectedRewardIndex).GetComponent<RewardListElement>().ToggleHighlight();
            }
            if (SelectedRewardIndex == index)
            {
                SelectedRewardIndex = -1;
                return;
            }
            SelectedRewardIndex = index;
        }

        public void DisplayCommandRewardEditor(int index)
        {
            QuestCommandEditorUI editorUI = Instantiate(questCommandEditorUIPrefab, parentUI.transform, false);
            editorUI.Initialize(content.CommandRewards,this,index);
            
        }
    }
}
