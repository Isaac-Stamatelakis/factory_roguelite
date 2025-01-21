using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using PlayerModule.KeyPress;

namespace UI.Chat {
    public class TextChatUI : MonoBehaviour
    {
        private static readonly float messageDisplayDuration = 6f;
        private static TextChatUI instance;
        public static TextChatUI Instance {get => instance;}
        public void Awake() {
            if (instance != null) {
                Debug.LogWarning("Duplicate text chat ui's. There should only be one");
                return;
            }
            instance = this;
        }
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private VerticalLayoutGroup textList;
        [SerializeField] private TextChatMessageUI textChatMessageUIPrefab;
        private List<string> recordedMessages;
        private List<string> sentMessages;
        private bool typing = false;
        private readonly float hintBlinkTime = 0.4f;
        private float hintBlinkCounter = 0f;
        private Color textBoxBackgroundColor;
        private int previousMessageIndex;
        public void Start() {
            inputField.gameObject.SetActive(false);
            recordedMessages = new List<string>();
            sentMessages = new List<string>();
            string title = "<color=#FF4500>C</color><color=#FF6347>a</color><color=#FF7F50>v</color><color=#FF8C00>e</color><color=#FFA500>T</color><color=#FFD700>e</color><color=#FFD700>c</color><color=#FF4500>h</color> <color=#FF6347>E</color><color=#FF7F50>s</color><color=#FF8C00>c</color><color=#FFA500>a</color><color=#FFD700>p</color><color=#FFD700>e</color>!";
            string message = $"Welcome to {title} This is an alpha version of the game. Please report any and all bugs you find along with general feedback to our discord at LINK";
            sendMessage(message);
            PlayerKeyPressUtils.InitializeTypingListener(inputField);
        }
        public void Update() {
            if (Input.GetKeyDown(KeyCode.Return) ) {
                typing = !typing;
                if (typing) {
                    showTextField();
                } else {
                    sentMessages.Insert(0,inputField.text);
                    previousMessageIndex = 0;
                    sendMessage(inputField.text);
                    hideTextField();
                }
            }
            if (typing && Input.GetKeyDown(KeyCode.Escape)) {
                typing = false;
                hideTextField();
            }
            if (typing && Input.GetKeyDown(KeyCode.Tab)) {
                commandFillParameters();
            }
            if (typing && Input.GetKeyDown(KeyCode.DownArrow)) {
                previousMessageIndex--;
                setInputToPreviousMessage();
                
            }
            if (typing && Input.GetKeyDown(KeyCode.UpArrow)) {
                previousMessageIndex++;
                setInputToPreviousMessage();
            }
            hintBlinkCounter -= Time.deltaTime;
            if (hintBlinkCounter < 0f) {
                bool active = inputField.placeholder.gameObject.activeInHierarchy;
                inputField.placeholder.gameObject.SetActive(!active);
                hintBlinkCounter = hintBlinkTime;
            }
        }

        public void RecordMessage(string message) {
            this.recordedMessages.Add(message);
        }

        private void setInputToPreviousMessage() {
            previousMessageIndex = Mathf.Clamp(previousMessageIndex,-1,sentMessages.Count-1);
            if (previousMessageIndex == -1) {
                inputField.text = "";
            } else {
                inputField.text = sentMessages[previousMessageIndex];
            }
            inputField.caretPosition = inputField.text.Length;
        }

        private void commandFillParameters() {
            if (!inputField.text.StartsWith("/")) {
                return;
            }
            ChatCommandToken token = ChatTokenizer.tokenize(inputField.text);
            int paramIndex = token.Parameters.Length-1;
            if (paramIndex < 0) {
                List<string> commands = ChatCommandFactory.getAllCommands();
                string currentCommand = inputField.text.Replace("/","");
                commands = commands.Where(s => s.StartsWith(currentCommand)).ToList();
                fillSuggested(commands,"/");
                return;
            }
            
            ChatCommand chatCommand = ChatCommandFactory.getCommand(token,this);
            if (chatCommand == null) {
                return;
            }

            if (chatCommand is not IAutoFillChatCommand autoFillChatCommand) {
                return;
            }
            string paramPrefix = token.Parameters[paramIndex];
            List<string> suggested = autoFillChatCommand.getAutoFill(paramIndex);
            if (suggested == null) {
                return;
            }
            suggested = suggested.Where(s => s.StartsWith(paramPrefix)).ToList();
            fillSuggested(suggested,"");
        }

        private void fillSuggested(List<string> suggested,string prefix) {
            if (suggested.Count == 1) {
                string completed = inputField.text;
                string[] split = inputField.text.Split(" ");
                split[split.Length-1] = suggested[0];
                string reconstructed = fromArray(split, " ");
                inputField.text = $"{prefix}{reconstructed}";
                inputField.caretPosition=inputField.text.Length;
            } else {
                sendMessage(fromArray(suggested.ToArray(), ", "));
            }
        }

        private string fromArray(string[] strings, string seperator) {
            string val = "";
            for (int i = 0; i < strings.Length; i++) {
                val += strings[i];
                if (i < strings.Length-1) {
                    val += seperator;
                }
            }
            return val;
        }

        public void sendMessage(string text) {
            if (text.Length == 0) {
                return;
            }
            if (recordedMessages.Count > 50) {
                recordedMessages.RemoveAt(0);
            }
            RecordMessage(text);
            if (text.StartsWith("/")) {
                ExecuteCommand(text);
                return;
            }
            
            addMessageToList(text,messageDisplayDuration);
        }

        public void ExecuteCommand(string command, bool printErrors = true)
        {
            ChatCommandToken commandToken = ChatTokenizer.tokenize(command);
            ChatCommand chatCommand = ChatCommandFactory.getCommand(commandToken,this);
            if (chatCommand == null) {
                if (printErrors) sendMessage("<color=red>" + $"Unknown command: '{command}'" + "</color>");
                return;
            }
            try
            {
                chatCommand.execute();
            }
            catch (IndexOutOfRangeException)
            {
                if (printErrors) sendMessage("<color=red>Invalid parameter format</color>");
            }
            catch (Exception e) when (e is ChatParseException or FormatException or OverflowException)
            {
                sendMessage("<color=red>" + e.Message + "</color>");
            }
        }
        private void addMessageToList(string text, float time) {
            TextChatMessageUI newMessage = GameObject.Instantiate(textChatMessageUIPrefab);
            newMessage.init(text,this,time);
            newMessage.transform.SetParent(textList.transform,false);
            LayoutRebuilder.ForceRebuildLayoutImmediate(textList.GetComponent<RectTransform>());
        }

        public void displayRecordedMessages() {
            foreach (string message in recordedMessages) {
                addMessageToList(message,0f);
            }
        }
        public void showTextField() {
            inputField.gameObject.SetActive(true);
            inputField.ActivateInputField();
            inputField.Select();
            displayRecordedMessages();
            setFadeForAllMessages(false);
            
        }
        public void hideTextField() {
            EventSystem.current.SetSelectedGameObject(null);
            inputField.text = "";
            inputField.DeactivateInputField();
            inputField.gameObject.SetActive(false);
            setFadeForAllMessages(true);
        }

        public void setFadeForAllMessages(bool fade) {
            for (int i = 0; i < textList.transform.childCount; i++) {
                Transform messageTransform = textList.transform.GetChild(i);
                TextChatMessageUI textChatMessageUI = messageTransform.GetComponent<TextChatMessageUI>();
                textChatMessageUI.setFade(fade);
            }
        }
    }
}

