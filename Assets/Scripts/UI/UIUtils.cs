using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI {
    public static class UIUtils
    {
        private static readonly string globalUIContainerName = "GlobalUIController";
        private static readonly string textChatName = "TextChat";

        public static string GlobalUIContainerName => globalUIContainerName;

        public static string TextChatName => textChatName;
    }
}

