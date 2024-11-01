using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.Chat {
    public interface IAutoFillChatCommand {
        public List<string> getAutoFill(int paramIndex);
    }
    public abstract class ChatCommand
    {
        protected string[] parameters;
        protected TextChatUI chatUI;
        protected ChatCommand(string[] parameters, TextChatUI textChatUI)
        {
            this.parameters = parameters;
            this.chatUI = textChatUI;
        }

        public abstract void execute();
        public abstract string getDescription();
    }
}

