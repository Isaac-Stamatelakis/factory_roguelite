using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.Chat {
    public class ChatCommandToken
    {
        public string Command;
        public string[] Parameters;

        public ChatCommandToken(string command, string[] parameters)
        {
            Command = command;
            Parameters = parameters;
        }
    }

}
