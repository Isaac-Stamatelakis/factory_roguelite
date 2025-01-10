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
            try {
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
            } catch (IndexOutOfRangeException) {
                chatUI.sendMessage("Invalid parameter format");
            }
            catch (FormatException e) {
                TextChatUI.Instance.sendMessage(e.ToString());
            } catch (OverflowException e) {
                TextChatUI.Instance.sendMessage(e.ToString());
            }
        }

        public override string getDescription()
        {
            return "/flight (T or F) \nToggles flight mode";
        }
    }
}

