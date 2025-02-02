using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Items.Inventory;
using Items.Tags;
using TileEntity.Instances;
using TileEntity.Instances.Machines;
using TMPro;
using UI.Chat;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using WorldModule.Caves;

namespace TileEntity.Instances
{
    public class CaveProcessorUI : MonoBehaviour, ITileEntityUI<CaveProcessorInstance>
    {
        [SerializeField] private InventoryUI mDriveInputUI;
        [SerializeField] private InventoryUI mDriveOutputUI;
        [SerializeField] private ArrowProgressController mProgressBar;
        [SerializeField] private TMP_InputField mTextInput;
        [SerializeField] private VerticalLayoutGroup mTextList;
        
        [SerializeField] private TextMeshProUGUI mTextPrefab;
        
        private CaveProcessorInstance caveProcessorInstance;
        public CaveProcessorInstance CaveProcessorInstance => caveProcessorInstance;
        private List<Cave> caves;
        public List<Cave> Caves => caves;
        
        private int previousMessageIndex;
        private List<string> recordedMessages = new List<string>();
        
        public void DisplayTileEntityInstance(CaveProcessorInstance tileEntityInstance)
        {
            caveProcessorInstance = tileEntityInstance;
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

        public void Update()
        {
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
            var command = CaveProcessorFactory.GetCommand( this, token);
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
                SendTerminalMessage($"<color=red>Could not parse command {token.Command}: {e.Message}</color>");
            }
            
            
        }

        private void FillParameters(string text)
        {
            ChatCommandToken token = ChatTokenizer.tokenize(text,enforceSlash:false);
            var command = CaveProcessorFactory.GetCommand( this, token);
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
            List<string> commands = CaveProcessorFactory.GetCommands();
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

    internal static class CaveProcessorFactory
    {
        private static readonly Dictionary<string, Func<CaveProcessorUI, ChatCommandToken, CaveProcessorTerminalCommand>> commandMap = new()
            {
                { "help", (processorUI, token) => new HelpCommand(processorUI, token) },
                { "research-cave", (processorUI, token) => new ResearchCommand(processorUI, token) },
                { "download-cave", (processorUI, token) => new DownloadCommand(processorUI, token) },
                { "cave-list", (processorUI, token) => new CaveListCommand(processorUI, token) },
            };

        public static CaveProcessorTerminalCommand GetCommand(CaveProcessorUI processorUI, ChatCommandToken token)
        {
            if (commandMap.TryGetValue(token.Command, out var commandConstructor))
            {
                return commandConstructor(processorUI, token);
            }
            return null;
        }
        
        public static string GetCommandHelpText(string command)
        {
            if (commandMap.TryGetValue(command, out var commandConstructor))
            {
                return commandConstructor(null, new ChatCommandToken(command,null)).GetHelpText();
            }
            return null;
        }

        public static List<string> GetCommands()
        {
            return commandMap.Keys.ToList();
        }
    }
    
    internal abstract class CaveProcessorTerminalCommand
    {
        protected CaveProcessorUI caveProcessorUI;
        protected ChatCommandToken token;
        public abstract void Execute();

        protected CaveProcessorTerminalCommand(CaveProcessorUI caveProcessorUI, ChatCommandToken token)
        {
            this.caveProcessorUI = caveProcessorUI;
            this.token = token;
        }

        public abstract string GetHelpText();
        public abstract List<string> GetAutoFill();
    }

    internal class HelpCommand : CaveProcessorTerminalCommand
    {
        public HelpCommand(CaveProcessorUI caveProcessorUI, ChatCommandToken token) : base(caveProcessorUI, token)
        {
        }

        public override void Execute()
        {
            List<string> commands = CaveProcessorFactory.GetCommands();
            foreach (string command in commands)
            {
                string message = $"{command}\t{CaveProcessorFactory.GetCommandHelpText(command).Replace("\n","")}";
                caveProcessorUI.SendTerminalMessage(message);
            }
        }

        public override string GetHelpText()
        {
            return "Help";
        }

        public override List<string> GetAutoFill()
        {
            return CaveProcessorFactory.GetCommands();
        }
    }

    internal class ResearchCommand : CaveProcessorTerminalCommand
    {
        public ResearchCommand(CaveProcessorUI caveProcessorUI, ChatCommandToken token) : base(caveProcessorUI, token)
        {
        }

        public override void Execute()
        {
            string id = token.Parameters[0];
            List<string> ids = caveProcessorUI.GetCaveIds();
            if (caveProcessorUI.CaveProcessorInstance.ResearchDriveProcess != null)
                throw new ChatParseException("Already researching cave");
            if (!ids.Contains(id)) 
                throw new ChatParseException($"Invalid research cave ID: {id}");
            if (caveProcessorUI.CaveProcessorInstance.ResearchedCaves.Contains(id))
                throw new ChatParseException($"Cave already researched: {id}");

            Tier tier = GetCaveTier(id);
            ResearchDriveProcess researchDriveProcess = new ResearchDriveProcess(tier,id);
            caveProcessorUI.CaveProcessorInstance.ResearchDriveProcess = researchDriveProcess;
        }

        private Tier GetCaveTier(string id)
        {
            List<Cave> caves = caveProcessorUI.Caves;
            foreach (Cave cave in caves)
            {
                if (cave.Id.Equals(id)) return cave.tier;
            }

            return Tier.Basic;
        }

        public override string GetHelpText()
        {
            return $"{token.Command} [CAVE_ID]";
        }

        public override List<string> GetAutoFill()
        {
            return caveProcessorUI.GetCaveIds();
        }
    }

    internal class CaveListCommand : CaveProcessorTerminalCommand
    {
        private const string UNKNOWN_FLAG = "-u";
        private const string RESEARCHED_FLAG = "-r";
        public CaveListCommand(CaveProcessorUI caveProcessorUI, ChatCommandToken token) : base(caveProcessorUI, token)
        {
        }

        public override void Execute()
        {
            if (token.Parameters.Length > 1)
                throw new ChatParseException("Too many arguments. At most one option may be provided.");
            List<string> caves = GetCaves();
            string message = TextChatUI.FromArray(caves.ToArray(), " ");
            caveProcessorUI.SendTerminalMessage(message);
        }

        private List<string> GetCaves()
        {
            if (token.Parameters.Length == 0) return caveProcessorUI.GetCaveIds();
            if (token.Parameters[0] == UNKNOWN_FLAG)
            {
                List<string> unknown = new List<string>();
                foreach (string id in caveProcessorUI.GetCaveIds())
                {
                    if (caveProcessorUI.CaveProcessorInstance.ResearchedCaves.Contains(id)) continue;
                    unknown.Add(id);
                }

                return unknown;
            }

            if (token.Parameters[0] == RESEARCHED_FLAG)
            {
                List<string> known = new List<string>();
                foreach (string id in caveProcessorUI.GetCaveIds())
                {
                    if (!caveProcessorUI.CaveProcessorInstance.ResearchedCaves.Contains(id)) continue;
                    known.Add(id);
                }
                return known;
            }

            throw new ChatParseException($"Unknown flag {token.Parameters[0]}");
        }

        public override string GetHelpText()
        {
            return $"{token.Command} [OPTION]\n" +
                   $" {RESEARCHED_FLAG}    List Researched Caves\n" +
                   $" {UNKNOWN_FLAG}    List Un-Researched Caves";
        }

        public override List<string> GetAutoFill()
        {
            return new List<string>
            {
                RESEARCHED_FLAG,
                UNKNOWN_FLAG
            };
        }
    }

    internal class DownloadCommand : CaveProcessorTerminalCommand
    {
        public DownloadCommand(CaveProcessorUI caveProcessorUI, ChatCommandToken token) : base(caveProcessorUI, token)
        {
        }

        public override void Execute()
        {
            string cave = token.Parameters[0];
            if (!caveProcessorUI.CaveProcessorInstance.ResearchedCaves.Contains(cave))
                throw new ChatParseException($"Cannot download unknown cave '{cave}'");
            if (cave.Equals(caveProcessorUI.CaveProcessorInstance.CurrentlyCopyingCave))
                throw new ChatParseException($"Already downloading '{cave}'");
            caveProcessorUI.CaveProcessorInstance.CurrentlyCopyingCave = cave;
        }

        public override string GetHelpText()
        {
            return $"{token.Command} [CAVE_ID]";
        }

        public override List<string> GetAutoFill()
        {
            return caveProcessorUI.CaveProcessorInstance.ResearchedCaves;
        }
    }

    
}
