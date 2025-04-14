using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PlayerModule;

namespace UI.Chat {
    public class FlightCommand : ChatCommand
    {
        public FlightCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {
        }

        public override void execute()
        {
            if (parameters.Length == 0)
            {
                DevMode.Instance.flight = !DevMode.Instance.flight;
            }
            else
            {
                bool state = ChatCommandParameterParser.parseBool(parameters,0,"state");
                DevMode.Instance.flight = state;
            }
            PlayerManager.Instance.GetPlayer().PlayerRobot.SetFlightProperties();
        }

        public override string getDescription()
        {
            return "/flight (on|off)? \nModifies flight mode";
        }
    }
}

