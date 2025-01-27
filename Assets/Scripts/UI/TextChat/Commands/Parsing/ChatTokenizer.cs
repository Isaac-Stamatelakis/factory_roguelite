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

            List<string> parameters = new List<string>();
            for (int i = 0; i < split.Length-1; i++) {
                string splitValue = split[i+1].Trim();
                if (string.IsNullOrEmpty(splitValue)) continue;
                parameters.Add(splitValue);
            }
            return new ChatCommandToken(textCommand,parameters.ToArray());
        }
    }
}

