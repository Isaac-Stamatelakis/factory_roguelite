using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.Chat {
    public static class ChatTokenizer
    {
        public static ChatCommandToken tokenize(string text) {
            if (!text.StartsWith("/")) {
                return null;
            }
            string[] split = text.Split(" ");
            string textCommand = split[0];
            textCommand = textCommand.Replace("/","");
            
            string[] parameters = new string[split.Length-1];
            for (int i = 0; i < split.Length-1; i++) {
                parameters[i] = split[i+1];
            }
            return new ChatCommandToken(textCommand,parameters);
        }
    }
}

