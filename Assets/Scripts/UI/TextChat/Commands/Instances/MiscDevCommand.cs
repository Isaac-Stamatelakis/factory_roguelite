using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PlayerModule;

namespace UI.Chat {
    public class MiscDevCommand : ChatCommand, IAutoFillChatCommand
    {
        private const string INSTA_BREAK = "instabreak";
        private const string NO_BREAK_CD = "nocooldown";
        private const string NO_HIT = "nohit";
        private const string NO_PLACE_COST = "nobreakcost";
        private const string NO_PLACE_LIMIT = "noplacelimit";
        private const string NO_ENERGY_COST = "noenergycost";
        private const string TOGGLE_LIGHT = "lighton";
        private const string NO_TELEPORT_COOLDOWN = "noteleportcooldown";
        public MiscDevCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {
        }

        public override void execute()
        {
            bool? defaultState = null;
            if (parameters.Length > 1)
            {
                try 
                {
                    defaultState = (parameters.Length > 1) ? ChatCommandParameterParser.parseBool(parameters,parameters.Length-1,"state") : null;
                } catch (ChatParseException) 
                {
                    // Just toggle
                }
            }

            int iterations = parameters.Length;
            if (defaultState.HasValue)
            {
                iterations--;
            }
            DevMode devMode = DevMode.Instance;
            for (int i = 0; i < iterations; i++)
            {
                bool newValue = SetState(ref GetBoolValue(parameters[i],devMode), defaultState);
                string stateMessage = newValue ? "on" : "off";
                chatUI.SendChatMessage($"Set {parameters[i]} to {stateMessage}");
            }
        }

        private ref bool GetBoolValue(string parameter, DevMode devMode)
        {
            switch (parameter)
            {
                case INSTA_BREAK:
                    return ref devMode.instantBreak;
                case NO_BREAK_CD:
                    return ref devMode.noBreakCooldown;
                case NO_HIT:
                    return ref devMode.noHit;
                case NO_PLACE_LIMIT:
                    return ref devMode.noPlaceLimit;
                case NO_PLACE_COST:
                    return ref devMode.noPlaceCost;
                case NO_ENERGY_COST:
                    return ref devMode.NoEnergyCost;
                case TOGGLE_LIGHT:
                    return ref devMode.LightOn;
                case NO_TELEPORT_COOLDOWN:
                    return ref devMode.NoTeleportCoolDown;
                default:
                    throw new ChatParseException("Invalid command parameter: " + parameter);
            }
        }

        private bool SetState(ref bool value, bool? state)
        {
            if (state == null)
            {
                value = !value;
                return value;
            }
            value = (bool)state;
            return value;
        }

        public override string getDescription()
        {
            List<string> options = getAutoFill(0);
            return $"/devmode {ChatCommandParameterParser.FormatParameters(options)}+ (on|off)? \nSet values of various devmodes";
        }

        public List<string> getAutoFill(int paramIndex)
        {
            List<string> options = new List<string>
            {
                INSTA_BREAK,
                NO_BREAK_CD,
                NO_HIT,
                NO_PLACE_LIMIT,
                NO_PLACE_LIMIT,
                NO_ENERGY_COST,
                TOGGLE_LIGHT,
                NO_TELEPORT_COOLDOWN,
            };
            if (paramIndex > 0)
            {
                options.Add("on");
                options.Add("off");
            }
            return options;
        }
    }
}

