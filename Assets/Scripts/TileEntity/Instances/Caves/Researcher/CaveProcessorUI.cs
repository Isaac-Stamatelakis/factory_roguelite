using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Items.Inventory;
using Items.Tags;
using TileEntity.Instances.Machines;
using TMPro;
using UI.Chat;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using WorldModule.Caves;

namespace TileEntity.Instances.Caves.Researcher
{
    public class CaveProcessorUI : MonoBehaviour, ITileEntityUI<CaveProcessorInstance>
    {
        [SerializeField] private InventoryUI mDriveInputUI;
        [SerializeField] private InventoryUI mDriveOutputUI;
        [SerializeField] private ArrowProgressController mProgressBar;
        [SerializeField] private TMP_InputField mTextInput;
        [SerializeField] private VerticalLayoutGroup mTextList;
        [SerializeField] private TextMeshProUGUI mResearchText;
        [SerializeField] private TextMeshProUGUI mStatusText;
        [SerializeField] private Button mTerminalBlocker;
        
        [SerializeField] private TextMeshProUGUI mTextPrefab;
        
        private CaveProcessorInstance caveProcessorInstance;
        public CaveProcessorInstance CaveProcessorInstance => caveProcessorInstance;
        private List<Cave> caves;
        public List<Cave> Caves => caves;
        
        private int previousMessageIndex;
        private List<string> recordedMessages = new List<string>();

        private const string START_MESSAGE =
            "===============================================\n" +
            "               TERMINAL ONLINE\n" +
            "===============================================\n";
        public void DisplayTileEntityInstance(CaveProcessorInstance tileEntityInstance)
        {
            caveProcessorInstance = tileEntityInstance;
            DisplayText();
            SendTerminalMessage(START_MESSAGE);
            SendTerminalMessage("Welcome Back!\n");
            //SendTerminalMessage("Current Status: ");
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
            
            mDriveInputUI.DisplayInventory(tileEntityInstance.InputDrives);
            mDriveInputUI.AddTagRestriction(ItemTag.CaveData);
            mDriveInputUI.AddListener(tileEntityInstance);
            
            mDriveOutputUI.DisplayInventory(tileEntityInstance.OutputDrives);
            mDriveOutputUI.SetInteractMode(InventoryInteractMode.BlockInput);
            mDriveOutputUI.AddListener(tileEntityInstance);
            
            mTextInput.ActivateInputField();
            mTextInput.Select();
            
            StartCoroutine(LoadCaves());

        }
        

        private IEnumerator LoadCaves()
        {
            var handle = Addressables.LoadAssetsAsync<Cave>("cave",null);
            yield return handle;
            var result = handle.Result;
            caves = new List<Cave>();
            foreach (Cave cave in result)
            {
                caves.Add(cave);
            }
        }
        
        public List<string> GetCaveIds() {
            List<string> caveIds = new List<string>();
            if (caves == null) return caveIds;
            foreach (Cave cave in caves)
            {
                caveIds.Add(cave.Id);
            }

            return caveIds;
        }

        private void DisplayText()
        {
            mStatusText.text = caveProcessorInstance.ResearchDriveProcess == null 
                ? "Awaiting Instruction"
                : $"{caveProcessorInstance.ResearchDriveProcess.Progress:P2}";
            mResearchText.text = caveProcessorInstance.ResearchDriveProcess == null 
                ? "Researching: None" 
                : $"Researching: {caveProcessorInstance.ResearchDriveProcess.ResearchId}";
        }
        public void Update()
        {
            DisplayText();
            if (Input.GetKeyDown(KeyCode.Return) ) {
                
                EnterCommand(mTextInput.text);
                mTextInput.text = string.Empty;
                mTextInput.ActivateInputField();
                mTextInput.Select();
            }
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                FillParameters(mTextInput.text);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow)) {
                if (previousMessageIndex >= 0) previousMessageIndex--;
                
                SetInputToPreviousMessage();
                
            }
            if (Input.GetKeyDown(KeyCode.UpArrow)) {
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
    }

}
