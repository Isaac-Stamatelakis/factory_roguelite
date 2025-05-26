using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Item.Slot;
using Items;
using Items.Inventory;
using Items.Tags;
using TileEntity.Instances.Machine.UI;
using TileEntity.Instances.Machines;
using TMPro;
using UI.Chat;
using UI.PlayerInvUI;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using WorldModule.Caves;

namespace TileEntity.Instances.Caves.Researcher
{
    public class CaveProcessorUI : MonoBehaviour, ITileEntityUI, IInventoryUITileEntityUI
    {
        [SerializeField] private InventoryUI mDriveInputUI;
        [SerializeField] private InventoryUI mDriveOutputUI;
        [SerializeField] private Transform mCostContainer;
        [SerializeField] private InventoryUI mResearchCostOverlayUI;
        [SerializeField] private InventoryUI mResearchItemsUI;
        [SerializeField] private ArrowProgressController mProgressBar;
        [SerializeField] private TMP_InputField mTextInput;
        [SerializeField] private VerticalLayoutGroup mTextList;
        [FormerlySerializedAs("mResearchText")] [SerializeField] private TextMeshProUGUI mDownloadText;
        [SerializeField] private TextMeshProUGUI mStatusText;
        [SerializeField] private Button mTerminalBlocker;
        
        [SerializeField] private TextMeshProUGUI mTextPrefab;
        
        private CaveProcessorInstance caveProcessorInstance;
        public CaveProcessorInstance CaveProcessorInstance => caveProcessorInstance;
        private List<CaveObject> caves;
        public List<CaveObject> Caves => caves;
        
        private int previousMessageIndex;
        private List<string> recordedMessages = new List<string>();
        private CaveObject currentResearchCave;
        private const string START_MESSAGE =
            "===============================================\n" +
            "               AVARICE TERMINAL ONLINE\n" +
            "WARNING: RESOURCE CONSUMPTION PROTOCOLS ACTIVE\n" +
            "© 20XX CAVETECH. UNAUTHORIZED IS PROHIBITED\n" +
            "===============================================\n";
        public void DisplayTileEntityInstance(ITileEntityInstance tileEntityInstance)
        {
            if (tileEntityInstance is not CaveProcessorInstance caveProcessor) return;
            caveProcessorInstance = caveProcessor;
            DisplayText();
            SendTerminalMessage(START_MESSAGE);
            SendTerminalMessage("Welcome Back!\n");
            SendTerminalMessage("Available Commands:");
            List<string> commands = CaveProcessorCommandFactory.GetCommands();
            foreach (string command in commands)
            {
                var commandInstance = CaveProcessorCommandFactory.GetCommand(this,new ChatCommandToken(command,null));
                SendTerminalMessage($" - '{command}': {commandInstance.GetDescription()}");
            }
            mTerminalBlocker.onClick.AddListener(() =>
            {
                mTextInput.ActivateInputField();
                mTextInput.Select();
            });

            mCostContainer.gameObject.SetActive(false);
            
            mDriveInputUI.DisplayInventory(caveProcessorInstance.InputDrives);
            mDriveInputUI.SetRestrictionMode(InventoryRestrictionMode.WhiteList);
            mDriveInputUI.AddTagRestriction(ItemTag.CaveData);
            
            mDriveInputUI.AddCallback(caveProcessorInstance.InventoryUpdate);
            
            mDriveOutputUI.DisplayInventory(caveProcessorInstance.OutputDrives);
            mDriveOutputUI.SetInteractMode(InventoryInteractMode.BlockInput);
            mDriveOutputUI.AddCallback(caveProcessorInstance.InventoryUpdate);
            
            mTextInput.ActivateInputField();
            mTextInput.Select();
            
            StartCoroutine(LoadCaves());
        }
        

        

        private IEnumerator LoadCaves()
        {
            var handle = Addressables.LoadAssetsAsync<CaveObject>("cave",null);
            yield return handle;
            var result = handle.Result;
            caves = new List<CaveObject>();
            string researchId = caveProcessorInstance.ResearchDriveProcess?.ResearchId;
            foreach (CaveObject cave in result)
            {
                if (researchId == cave.GetId())
                {
                    currentResearchCave = cave;
                }
                caves.Add(cave);
            }

            if (!currentResearchCave) yield break;
            if (caveProcessorInstance.ResearchDriveProcess == null || caveProcessorInstance.ResearchDriveProcess.Progress > 0) yield break;
            DisplayCaveResearchCost();
        }

        public void SetDisplayableCave(string id)
        {
            foreach (CaveObject cave in caves)
            {
                if (id == cave.GetId())
                {
                    currentResearchCave = cave;
                }
            }
        }
        
        public CaveObject LookUpCave(string id)
        {
            foreach (CaveObject cave in caves)
            {
                if (id == cave.GetId())
                {
                    return cave;
                }
            }

            return null;
        }

        public void DisplayCaveResearchCost()
        {
            if (!currentResearchCave) return;

            void OnResearchSuccess()
            {
                ToolTipController.Instance.HideToolTip();
                SendTerminalMessage($"Researching cave '{currentResearchCave.name}'");
                caveProcessorInstance.ResearchDriveProcess.Satisfied = true;
                mCostContainer.gameObject.SetActive(false);
            }
            List<ItemSlot> requiredItems = ItemSlotFactory.FromEditorObjects(currentResearchCave?.ResearchCost);
            if (requiredItems == null || requiredItems.Count == 0)
            {
                OnResearchSuccess();
                return;
            }
            /*
             * Summary of how cost is displayed:
             * There are two inventory UIs ResearchOverlay and ResearchItems. As the name suggests ResearchOverlay is displayed overtop of
             * ResearchItems with an transparency layer to suggest to players to input items in the inventory.
             * ResearchItems amount text is locked and is set through a lambda function callback when the inventory is updated.
             */
            mCostContainer.gameObject.SetActive(true);
            caveProcessorInstance.ResearchItems = ItemSlotFactory.createEmptyInventory(requiredItems.Count);
            mResearchItemsUI.DisplayInventory(caveProcessorInstance.ResearchItems);

            bool Restriction(ItemObject itemObject, int index)
            {
                return string.Equals(itemObject?.id, requiredItems[index]?.itemObject.id);
            }
            mResearchItemsUI.SetInputRestrictionCallBack(Restriction);
            mResearchItemsUI.ApplyFunctionToAllSlots((ItemSlotUI itemSlotUI) =>
            {
                itemSlotUI.LockBottomText = true;
            });
            
            void OnItemSlotChange(int index)
            {
                ItemSlot required = requiredItems[index];
                ItemSlot current = caveProcessorInstance.ResearchItems[index];
                
                uint requiredAmount = required.amount;
                uint amount = current?.amount ?? 0;
                Color color = amount >= requiredAmount ? Color.green : Color.red;
                if (current != null && amount > requiredAmount)
                {
                    ItemSlot spliced = ItemSlotFactory.Splice(current, amount - requiredAmount);
                    current.amount = requiredAmount;
                    PlayerManager.Instance.GetPlayer().PlayerInventory.Give(spliced);
                    GetComponentInParent<StackedPlayerInvUIElement>().PlayerInventoryUI.RefreshSlots();
                    amount = requiredAmount;
                }
                TextMeshProUGUI textMeshProUGUI = mResearchItemsUI.GetItemSlotUI(index).mBottomText;
                textMeshProUGUI.text = $"{amount}/{requiredAmount}";
                textMeshProUGUI.color = color;
                
            }

            string GetToolTip(int index)
            {
                ItemSlot required = requiredItems[index];
                return required?.itemObject?.name;
            }
            mResearchItemsUI.AddCallback(OnItemSlotChange);
            mResearchItemsUI.AddCallback(CheckResearchSatisfied);
            
            mResearchItemsUI.SetToolTipOverride(GetToolTip);
            for (int i = 0; i < requiredItems.Count; i++)
            {
                OnItemSlotChange(i);
            }
    
            mResearchCostOverlayUI.DisplayInventory(requiredItems);
            mResearchCostOverlayUI.InventoryInteractMode = InventoryInteractMode.UnInteractable;
            
            void CheckResearchSatisfied(int index)
            {
                for (int i = 0; i < requiredItems.Count; i++)
                {
                    ItemSlot required = requiredItems[i];
                    ItemSlot current = caveProcessorInstance.ResearchItems[i];
                    if (ItemSlotUtils.IsItemSlotNull(current)) return;
                    if (current.amount < required.amount) return; 
                }

                OnResearchSuccess();
            }
        }
        
        public List<string> GetCaveIds() {
            List<string> caveIds = new List<string>();
            if (caves == null) return caveIds;
            foreach (CaveObject cave in caves)
            {
                caveIds.Add(cave.GetId());
            }

            return caveIds;
        }

        private void DisplayText()
        {
            if (caveProcessorInstance.ResearchDriveProcess == null)
            {
                mStatusText.text = "Researching: 'None'";
            }
            else
            {
                mStatusText.text = !caveProcessorInstance.ResearchDriveProcess.Satisfied
                    ? $"Researching '{caveProcessorInstance.ResearchDriveProcess.ResearchId}'\nStatus:Awaiting Items" 
                    : $"<color=green>Researching '{caveProcessorInstance.ResearchDriveProcess.ResearchId}'\nProgress:{caveProcessorInstance.ResearchDriveProcess.Progress:P2}</color>";
            }
            mDownloadText.text = $"Downloading: '{caveProcessorInstance.CurrentlyCopyingCave ?? "None"}'";
        }
        public void Update()
        {
            DisplayText();
            var researchDriveProcessor = caveProcessorInstance.ResearchDriveProcess;
            if (researchDriveProcessor != null)
            {
                if (!researchDriveProcessor.CompletionActionTriggered  && researchDriveProcessor.Progress> 0.99f)
                {
                    SendTerminalMessage($"<color=green>Research of '{currentResearchCave?.name}' complete!</color>");
                    mCostContainer.gameObject.SetActive(false);
                    researchDriveProcessor.CompletionActionTriggered = true;
                }
            }
            if (Keyboard.current.enterKey.wasPressedThisFrame) {
                
                EnterCommand(mTextInput.text);
                mTextInput.text = string.Empty;
                mTextInput.ActivateInputField();
                mTextInput.Select();
            }
            if (Keyboard.current.tabKey.wasPressedThisFrame)
            {
                FillParameters(mTextInput.text);
            }
            if (Keyboard.current.downArrowKey.wasPressedThisFrame) {
                if (previousMessageIndex >= 0) previousMessageIndex--;
                
                SetInputToPreviousMessage();
                
            }
            if (Keyboard.current.upArrowKey.wasPressedThisFrame) {
                if (previousMessageIndex < recordedMessages.Count) previousMessageIndex++;
                
                SetInputToPreviousMessage();
            }
        }
        
        private void SetInputToPreviousMessage() {
            previousMessageIndex = Mathf.Clamp(previousMessageIndex,-1,recordedMessages.Count-1);

            mTextInput.text = previousMessageIndex < 0 ? "" : recordedMessages[previousMessageIndex];
            mTextInput.caretPosition = 0;
        }

        private void EnterCommand(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            ChatCommandToken token = ChatTokenizer.tokenize(text,enforceSlash:false);
            var command = CaveProcessorCommandFactory.GetCommand( this, token);
            if (recordedMessages.Contains(text))
            {
                recordedMessages.Remove(text);
            }
            recordedMessages.Insert(0,text);
            
            if (command == null)
            {
                SendTerminalMessage($"<color=red>Unknown command: {token.Command}</color>");
                return;
            }

            if (token.Parameters.Contains("-h"))
            {
                SendTerminalMessage($"Usage: {command.GetHelpText()}");
                return;
            }

            try
            {
                command.Execute();
            }
            catch (Exception e) when (e is IndexOutOfRangeException or ChatParseException) 
            {
                SendTerminalMessage($"<color=red>Could not execute '{token.Command}': {e.Message}</color>");
            }
            
            
        }

        private void FillParameters(string text)
        {
            ChatCommandToken token = ChatTokenizer.tokenize(text,enforceSlash:false);
            var command = CaveProcessorCommandFactory.GetCommand( this, token);
            if (command == null)
            {
                FillCommand(text);
                return;
            }

            var autoFill = command.GetAutoFill();
            autoFill.Add("-h");
            if (token.Parameters.Length > 0)
            {
                autoFill = autoFill.Where(s => s.StartsWith(token.Parameters.Last())).ToList();
            }
            
            FillSuggested(autoFill);

        }
        
        private void FillSuggested(List<string> suggested)
        {
            if (suggested == null) return;
            if (suggested.Count == 1) {
                string[] split = mTextInput.text.Split(" ");
                split[split.Length-1] = suggested[0];
                string reconstructed = TextChatUI.FromArray(split, " ");
                mTextInput.text = $"{reconstructed}";
                mTextInput.caretPosition = mTextInput.text.Length;
            } else {
                SendTerminalMessage(TextChatUI.FromArray(suggested.ToArray(), ", "));
            }
        }

        private void FillCommand(string text)
        {
            List<string> commands = CaveProcessorCommandFactory.GetCommands();
            string currentCommand = text;
            commands = commands.Where(s => s.StartsWith(currentCommand)).ToList();
            FillSuggested(commands);
            
            
        }

        public void SendTerminalMessage(string text)
        {
            TextMeshProUGUI textElement = GameObject.Instantiate(mTextPrefab, mTextList.transform, false);
            textElement.text = text;
        }

        public void FixedUpdate()
        {
            float progress = caveProcessorInstance.CopyDriveProcess?.CopyId == null ? 0 : caveProcessorInstance.CopyDriveProcess.Progress;
            mProgressBar.SetArrowProgress(progress);
            mDriveOutputUI.RefreshSlots();
            mDriveInputUI.RefreshSlots();
            
        }

        public InventoryUI GetInput()
        {
            return mDriveInputUI;
        }

        public List<InventoryUI> GetAllInventoryUIs()
        {
            return new List<InventoryUI>
            {
                mDriveInputUI,
                mDriveOutputUI
            };
        }
    }

}
