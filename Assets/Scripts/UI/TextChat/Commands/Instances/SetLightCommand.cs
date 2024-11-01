using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Dimensions;
using PlayerModule;
using Items;
using UnityEngine.Rendering.Universal;
using System.Linq;

namespace UI.Chat {
    public class SetLightCommand : ChatCommand, IAutoFillChatCommand
    {
        public SetLightCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {
        }
        public override void execute()
        {
            Light2D globalLight = GameObject.Find("GlobalLight").GetComponent<Light2D>();

            
            double intensity;
            Color color;
            try {
                intensity = Convert.ToDouble(parameters[0]);
            } catch (IndexOutOfRangeException) {
                chatUI.sendMessage("Intensity not provided");
                return;
            } catch (FormatException) {
                chatUI.sendMessage("Intensity is not a number");
                return;
            }
            globalLight.intensity = (float) intensity;
            if (parameters.Length == 1) {
                globalLight.color = Color.white;
                return;
            }
            try {
                color = ChatCommandParameterParser.parseColor(parameters,1);
                globalLight.color = color;
            } catch (ChatParseException e) {
                chatUI.sendMessage(e.Message);
            }
            
        }

        public List<string> getAutoFill(int paramIndex)
        {
            if (paramIndex == 1) {
                return ChatCommandParameterParser.PresetColors.Keys.ToList();
            }
            return null;
        }

        public override string getDescription()
        {
            return "/lightmode intensity (r,g,b) \nSets light to intensity in range [0,inf)\nOptional: provide (r,g,b) in range [0,255]";
        }
    }
}
