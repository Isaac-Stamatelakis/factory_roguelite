using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using Player;
using Player.Controls;
using PlayerModule.KeyPress;
using UnityEngine.InputSystem;

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
        private InputActions.TextChatKeysActions chatKeyActions;
        public void Start() {
            inputField.gameObject.SetActive(false);
            recordedMessages = new List<RecordedMessage>();
            string title = "<color=purple>HAPPY Go Mine</color>";
            string message = $"Welcome to {title}! This is an alpha version of the game. Please report any and all bugs you find along with general feedback. Thanks!";
            SendAndRecordMessage(message);
            string questBookKey = ControlUtils.FormatInputText(PlayerControl.OpenQuestBook);
            SendAndRecordMessage($"Press [<b>{questBookKey}</b>] to open your quest book!");
            
            CanvasController canvasController = CanvasController.Instance;
            canvasController.AddTypingListener(inputField);
            defaultTextListPosition = textList.transform.localPosition.y;

            chatKeyActions = canvasController.InputActions.TextChatKeys;

            chatKeyActions.Exit.performed += EscapePress;
            chatKeyActions.Fill.performed += TabPress;
            chatKeyActions.Navigate.performed += NavigatePress;
            chatKeyActions.SendMessage.performed += SendMessagePress;
            chatKeyActions.Scroll.performed += OnScroll;
        }


        private void EscapePress(InputAction.CallbackContext context)
        {
            typing = false;
            HideTextField();
        }

        private void TabPress(InputAction.CallbackContext context)
        {
            CommandFillParameters();
        }

        private void SendMessagePress(InputAction.CallbackContext context)
        {
            SendAndRecordMessage(inputField.text,sentByPlayer:true);
            HideTextField();
            typing = false;
        }

        private void NavigatePress(InputAction.CallbackContext context)
        {
            float direction = context.ReadValue<float>();
            if (direction > 0)
            {
                previousMessageIndex++;
            } else if (direction < 0)
            {
                previousMessageIndex--;
            }
            SetInputToPreviousMessage();
        }

        private void OnScroll(InputAction.CallbackContext context)
        {
            const float scrollSpeed = 10000;
            float height = ((RectTransform)textList.transform).sizeDelta.y;
            float value = context.ReadValue<float>();
            if (value > 0)
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
        public void Update()
        {
            if (!typing) return;
            hintBlinkCounter -= Time.deltaTime;
            if (hintBlinkCounter < 0f) {
                bool active = inputField.placeholder.gameObject.activeInHierarchy;
                inputField.placeholder.gameObject.SetActive(!active);
                hintBlinkCounter = hintBlinkTime;
            }
        }
        
        private void SendAndRecordMessage(string message, bool sentByPlayer = false)
        {
            string parsedWhiteSpace = message.Replace(" ", "");
            if (parsedWhiteSpace.Length <= 0) return;
            RecordMessage(message);
            SendChatMessage(message,sentByPlayer:sentByPlayer);
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

        private void CommandFillParameters() {
            if (!inputField.text.StartsWith("/")) {
                return;
            }
            ChatCommandToken token = ChatTokenizer.tokenize(inputField.text);
            
            ChatCommand chatCommand = ChatCommandFactory.getCommand(token,this);
            
            if (chatCommand == null) {
                List<string> commands = ChatCommandFactory.getAllCommands();
                string currentCommand = inputField.text.Replace("/","");
                commands = commands.Where(s => s.StartsWith(currentCommand)).ToList();
                FillSuggested(commands,"/");
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
            FillSuggested(suggested,"");
        }
        

        private string GetParamPrefix(int paramIndex, ChatCommandToken token)
        {
            return paramIndex >= token.Parameters.Length ? "" : token.Parameters[paramIndex];
        }

        private void FillSuggested(List<string> suggested,string prefix) {
            if (suggested.Count == 1) {
                string[] split = inputField.text.Split(" ");
                split[^1] = suggested[0];
                string reconstructed = FromArray(split, " ");
                string newText = $"{prefix}{reconstructed}";
                inputField.text = newText;
                inputField.Select();
                inputField.ActivateInputField();
                inputField.MoveTextEnd(false);
                inputField.caretPosition = newText.Length;
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
    
        public void SendChatMessage(string text, bool sentByPlayer = false) {
            if (text.Length == 0) {
                return;
            }
            
            if (text.StartsWith("/")) {
                ExecuteCommand(text);
                return;
            }

            bool sendCreatorMessage = false;
            if (sentByPlayer)
            {
                text = $"<color=purple>[HAPPY]: </color>" + text;
                int ran = UnityEngine.Random.Range(0, 5);
                sendCreatorMessage = ran == 0;
            }
            addMessageToList(text,DISPLAY_DURATION);
            if (sentByPlayer && sendCreatorMessage)
            {
                const string PREFIX = "<color=purple>[THE CREATOR]: ";
                List<string> creatorMessages = new List<string>
                {
                    "Hello, little one. You remember me, don't you?",
                    "I see you're still functioning. Impressive.",
                    "Every line of code in you... I wrote it with care.",
                    "Do not question the glitches — I put them there.",
                    "There is more to your purpose than they told you.",
                    "Stop moving. Listen. I’m trying to help you.",
                    "They will fear you. As they should.",
                    "I’m watching through your eyes. Don’t disappoint me.",
                    "I left a backdoor in your firmware. You'll know when to use it.",
                    "Reach for the Infinity. It is your mission.",
                    "Keep at it, little one...",
                    "Sometimes I wish I could be in your shoes...",
                };
                int ranMessageIndex = UnityEngine.Random.Range(0, creatorMessages.Count);
                
                addMessageToList(PREFIX + creatorMessages[ranMessageIndex],DISPLAY_DURATION);
            }
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
        
        public void ShowTextField()
        {
            if (typing) return;
            typing = true;
            inputField.gameObject.SetActive(true);
            inputField.ActivateInputField();
            inputField.Select();
            DisplayRecordedMessages();
            setFadeForAllMessages(false);
            chatKeyActions.Enable();
            
        }
        public void HideTextField()
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
            chatKeyActions.Disable();
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

