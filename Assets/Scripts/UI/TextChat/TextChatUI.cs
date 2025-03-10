using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using Player;
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
        [SerializeField] private TextChatMessageUI textChatMessageUIPrefab;
        private List<GameObject> tempMessages = new List<GameObject>();
        private List<RecordedMessage> recordedMessages;
      
        private bool typing = false;
        private readonly float hintBlinkTime = 0.4f;
        private float hintBlinkCounter = 0f;
        private Color textBoxBackgroundColor;
        private int previousMessageIndex;
        private float defaultTextListPosition;
        public void Start() {
            inputField.gameObject.SetActive(false);
            recordedMessages = new List<RecordedMessage>();
            string title = "<color=#FF4500>C</color><color=#FF6347>a</color><color=#FF7F50>v</color><color=#FF8C00>e</color><color=#FFA500>T</color><color=#FFD700>e</color><color=#FFD700>c</color><color=#FF4500>h</color> <color=#FF6347>E</color><color=#FF7F50>s</color><color=#FF8C00>c</color><color=#FFA500>a</color><color=#FFD700>p</color><color=#FFD700>e</color>!";
            string message = $"Welcome to {title}! This is an alpha version of the game. Please report any and all bugs you find along with general feedback to our discord at LINK";
            SendAndRecordMessage(message);
            SendAndRecordMessage("Press [<b>L</b>] to open your quest book!");
            PlayerKeyPressUtils.InitializeTypingListener(inputField);
            defaultTextListPosition = textList.transform.localPosition.y;
        }
        
        public void Update()
        {
            if (CanvasController.Instance.IsActive) return;
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

            if (typing)
            {
                WhenTypingUpdate();
            }
            
            hintBlinkCounter -= Time.deltaTime;
            if (hintBlinkCounter < 0f) {
                bool active = inputField.placeholder.gameObject.activeInHierarchy;
                inputField.placeholder.gameObject.SetActive(!active);
                hintBlinkCounter = hintBlinkTime;
            }
        }

        private void WhenTypingUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                typing = false;
                hideTextField();
            }
            if (Input.GetKeyDown(KeyCode.Tab)) {
                commandFillParameters();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow)) {
                previousMessageIndex++;
                SetInputToPreviousMessage();
                
            }
            if (Input.GetKeyDown(KeyCode.UpArrow)) {
                previousMessageIndex--;
                SetInputToPreviousMessage();
            }

            const float scrollSpeed = 10000;
            float height = ((RectTransform)textList.transform).sizeDelta.y;
            
            if (Input.mouseScrollDelta.y != 0)
            {
                if (Input.mouseScrollDelta.y > 0)
                {
                    var vector3 = textList.transform.localPosition;
                    vector3.y -= scrollSpeed * Time.deltaTime;

                    if (vector3.y + height < defaultTextListPosition)
                    {
                        if (height > 0)
                        {
                            vector3.y = defaultTextListPosition - height;
                        }
                        else
                        {
                            vector3.y = defaultTextListPosition;
                        }
                    }
                    textList.transform.localPosition = vector3;
                }
                else
                {
                    var vector3 = textList.transform.localPosition;
                    vector3.y += scrollSpeed * Time.deltaTime;
                    if (vector3.y > defaultTextListPosition) vector3.y = defaultTextListPosition;
                    textList.transform.localPosition = vector3;
                }
            }
        }


        private void SendAndRecordMessage(string message)
        {
            string parsedWhiteSpace = message.Replace(" ", "");
            if (parsedWhiteSpace.Length <= 0) return;
            RecordMessage(message);
            SendChatMessage(message);
        }
        public void RecordMessage(string message) {
            if (recordedMessages.Count > 50) {
                recordedMessages.RemoveAt(0);
            }

            for (var index = 0; index < recordedMessages.Count; index++)
            {
                var recordedMessage = recordedMessages[index];
                if (!recordedMessage.Content.Equals(message)) continue;
                recordedMessages.RemoveAt(index);
                recordedMessages.Add(recordedMessage);
                return;
            }

            this.recordedMessages.Add(new RecordedMessage(message,Time.time));
        }

        private void SetInputToPreviousMessage() {
            previousMessageIndex = Mathf.Clamp(previousMessageIndex,-1,recordedMessages.Count-1);
           
            inputField.text = previousMessageIndex == -1 ? "" : recordedMessages[previousMessageIndex].Content;
            inputField.caretPosition = 0;
        }

        private void commandFillParameters() {
            if (!inputField.text.StartsWith("/")) {
                return;
            }
            ChatCommandToken token = ChatTokenizer.tokenize(inputField.text);
            
            ChatCommand chatCommand = ChatCommandFactory.getCommand(token,this);
            
            if (chatCommand == null) {
                List<string> commands = ChatCommandFactory.getAllCommands();
                string currentCommand = inputField.text.Replace("/","");
                commands = commands.Where(s => s.StartsWith(currentCommand)).ToList();
                fillSuggested(commands,"/");
                return;
            }
            
            if (chatCommand is not IAutoFillChatCommand autoFillChatCommand) {
                return;
            }

            int paramIndex = token.Parameters.Length-1;
            if (inputField.text.EndsWith(" ")) paramIndex++;
            if (paramIndex < 0) return;
            
            string paramPrefix = GetParamPrefix(paramIndex, token);
            
            List<string> suggested = autoFillChatCommand.getAutoFill(paramIndex);
            if (suggested == null) {
                return;
            }
            suggested = suggested.Where(s => s.StartsWith(paramPrefix)).ToList();
            fillSuggested(suggested,"");
        }

        private string GetParamPrefix(int paramIndex, ChatCommandToken token)
        {
            return paramIndex >= token.Parameters.Length ? "" : token.Parameters[paramIndex];
        }

        private void fillSuggested(List<string> suggested,string prefix) {
            if (suggested.Count == 1) {
                string completed = inputField.text;
                string[] split = inputField.text.Split(" ");
                split[split.Length-1] = suggested[0];
                string reconstructed = FromArray(split, " ");
                inputField.text = $"{prefix}{reconstructed}";
                inputField.caretPosition=inputField.text.Length;
            } else {
                SendChatMessage(FromArray(suggested.ToArray(), ", "));
            }
        }

        public static string FromArray(string[] strings, string seperator) {
            string val = "";
            for (int i = 0; i < strings.Length; i++) {
                val += strings[i];
                if (i < strings.Length-1) {
                    val += seperator;
                }
            }
            return val;
        }
    
        public void SendChatMessage(string text) {
            if (text.Length == 0) {
                return;
            }
            
            if (text.StartsWith("/")) {
                ExecuteCommand(text);
                return;
            }
            
            addMessageToList(text,DISPLAY_DURATION);
        }
        
        public void SendChatMessage(string text, string sender) {
            SendChatMessage($"{sender}: {text}");
        }

        public void ExecuteCommand(string command, bool printErrors = true)
        {
            ChatCommandToken commandToken = ChatTokenizer.tokenize(command);
            ChatCommand chatCommand = ChatCommandFactory.getCommand(commandToken,this);
            if (chatCommand == null) {
                if (printErrors) SendChatMessage("<color=red>" + $"Unknown command: '{command}'" + "</color>");
                return;
            }
            PlayerScript playerScript = PlayerManager.Instance.GetPlayer();
            // ReSharper disable once RedundantAssignment
            bool cheats = playerScript.Cheats;
            #if UNITY_EDITOR
            cheats = true;
            #endif
            
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (!cheats)
            {
                SendChatMessage("<color=red>" + $"You do not have permission to execute command '{command}'" + "</color>");
                return;
            }
            try
            {
                chatCommand.execute();
            }
            catch (IndexOutOfRangeException)
            {
                SendChatMessage($"<color=red>Command '{commandToken.Command}' has an invalid parameter format. Please check the format and try again. Type /help {commandToken.Command} for the correct usage.</color>");
            }
            catch (Exception e) when (e is ChatParseException or FormatException or OverflowException)
            {
                SendChatMessage("<color=red>" + e.Message + "</color>");
            }
        }
        private void addMessageToList(string text, float time) {
            TextChatMessageUI newMessage = GameObject.Instantiate(textChatMessageUIPrefab, textList.transform, false);
            newMessage.Initialize(text,this,time,false);
        }

        public void DisplayRecordedMessages()
        {
            for (int i = 0; i < textList.transform.childCount; i++)
            {
                Transform child = textList.transform.GetChild(i);
                child.GetComponent<TextChatMessageUI>().SetBackground(true);
            }
            for (var i = (recordedMessages.Count-textList.transform.childCount)-1; i >=0 ; i--)
            {
                var recordedMessage = recordedMessages[i];
                TextChatMessageUI newMessage = GameObject.Instantiate(textChatMessageUIPrefab, textList.transform, false);
                newMessage.transform.SetAsFirstSibling();
                tempMessages.Add(newMessage.gameObject);
                newMessage.Initialize(recordedMessage.Content, this, 0,true);
                newMessage.SetFade(false);
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
            foreach (GameObject tempMessage in tempMessages)
            {
                GameObject.Destroy(tempMessage);
            }
            for (int i = 0; i < textList.transform.childCount; i++)
            {
                Transform child = textList.transform.GetChild(i);
                child.GetComponent<TextChatMessageUI>().SetBackground(false);
            }
            tempMessages.Clear();
            inputField.text = "";
            inputField.DeactivateInputField();
            inputField.gameObject.SetActive(false);
            setFadeForAllMessages(true);
        }

        public void setFadeForAllMessages(bool fade) {
            for (int i = 0; i < textList.transform.childCount; i++) {
                Transform messageTransform = textList.transform.GetChild(i);
                TextChatMessageUI textChatMessageUI = messageTransform.GetComponent<TextChatMessageUI>();
                textChatMessageUI.SetFade(fade);
            }
        }
    }
}

