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
        private struct RecordedMessage
        {
            public string Content;
            public float Time;

            public RecordedMessage(string content, float time)
            {
                Content = content;
                Time = time;
            }
        }
        private const float DISPLAY_DURATION = 6f;
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
        [SerializeField] private VerticalLayoutGroup previousMessageList;
        [SerializeField] private TextChatMessageUI textChatMessageUIPrefab;
        private List<RecordedMessage> recordedMessages;
      
        private bool typing = false;
        private readonly float hintBlinkTime = 0.4f;
        private float hintBlinkCounter = 0f;
        private Color textBoxBackgroundColor;
        private int previousMessageIndex;
        public void Start() {
            inputField.gameObject.SetActive(false);
            recordedMessages = new List<RecordedMessage>();
            string title = "<color=#FF4500>C</color><color=#FF6347>a</color><color=#FF7F50>v</color><color=#FF8C00>e</color><color=#FFA500>T</color><color=#FFD700>e</color><color=#FFD700>c</color><color=#FF4500>h</color> <color=#FF6347>E</color><color=#FF7F50>s</color><color=#FF8C00>c</color><color=#FFA500>a</color><color=#FFD700>p</color><color=#FFD700>e</color>!";
            string message = $"Welcome to {title}! This is an alpha version of the game. Please report any and all bugs you find along with general feedback to our discord at LINK";
            SendAndRecordMessage(message);
            SendAndRecordMessage("Press [<b>L</b>] to open your quest book!");
            PlayerKeyPressUtils.InitializeTypingListener(inputField);
        }
        
        public void Update() {
            if (Input.GetKeyDown(KeyCode.Return) ) {
                typing = !typing;
                if (typing) {
                    showTextField();
                } else
                {
                    SendAndRecordMessage(inputField.text);
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
                previousMessageIndex++;
                SetInputToPreviousMessage();
                
            }
            if (typing && Input.GetKeyDown(KeyCode.UpArrow)) {
                previousMessageIndex--;
                SetInputToPreviousMessage();
            }
            hintBlinkCounter -= Time.deltaTime;
            if (hintBlinkCounter < 0f) {
                bool active = inputField.placeholder.gameObject.activeInHierarchy;
                inputField.placeholder.gameObject.SetActive(!active);
                hintBlinkCounter = hintBlinkTime;
            }
        }


        private void SendAndRecordMessage(string message)
        {
            string parsedWhiteSpace = message.Replace(" ", "");
            if (parsedWhiteSpace.Length <= 0) return;
            RecordMessage(message);
            sendMessage(message);
        }
        public void RecordMessage(string message) {
            if (recordedMessages.Count > 50) {
                recordedMessages.RemoveAt(0);
            }
            this.recordedMessages.Add(new RecordedMessage(message,Time.time));
        }

        private void SetInputToPreviousMessage() {
            previousMessageIndex = Mathf.Clamp(previousMessageIndex,-1,recordedMessages.Count-1);
           
            inputField.text = previousMessageIndex == -1 ? "" : recordedMessages[previousMessageIndex].Content;
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
            
            if (text.StartsWith("/")) {
                ExecuteCommand(text);
                return;
            }
            
            addMessageToList(text,DISPLAY_DURATION);
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
            TextChatMessageUI newMessage = GameObject.Instantiate(textChatMessageUIPrefab, textList.transform, false);
            newMessage.init(text,this,time);
        }

        public void DisplayRecordedMessages()
        {
            for (var i = 0; i < recordedMessages.Count-textList.transform.childCount; i++)
            {
                var recordedMessage = recordedMessages[i];
                TextChatMessageUI newMessage = GameObject.Instantiate(textChatMessageUIPrefab, previousMessageList.transform, false);
                newMessage.init(recordedMessage.Content, this, 0);
                newMessage.setFade(false);
            }
        }
        public void showTextField() {
            inputField.gameObject.SetActive(true);
            inputField.ActivateInputField();
            inputField.Select();
            DisplayRecordedMessages();
            setFadeForAllMessages(false);
            
        }
        public void hideTextField()
        {
            previousMessageIndex = recordedMessages.Count;
            EventSystem.current.SetSelectedGameObject(null);
            GlobalHelper.deleteAllChildren(previousMessageList.transform);
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

