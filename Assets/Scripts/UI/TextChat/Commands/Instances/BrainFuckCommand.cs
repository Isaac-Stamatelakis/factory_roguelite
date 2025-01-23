using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Player;
using PlayerModule;
using UnityEditor;
using WorldModule;

namespace UI.Chat {
    public class BrainFuckCommand : ChatCommand
    {
        public BrainFuckCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {
        }

        public override void execute()
        {
            PlayerScript playerScript = PlayerManager.Instance.GetPlayer();
            var rb = playerScript.GetComponent<Rigidbody2D>();
            rb.freezeRotation = !rb.freezeRotation;
            
            chatUI.sendMessage(rb.freezeRotation ? "Back to normal" : "Why would you run this?");
            if (!rb.freezeRotation) return;
            
            var rotation = playerScript.transform.rotation;
            rotation.z = 0;
            playerScript.transform.rotation = rotation;

        }

        public override string getDescription()
        {
            return "/unknowncommand\nWhat does this do?";
        }
    }
}

