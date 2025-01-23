using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PlayerModule;

namespace UI.Chat {
    public class MiscDevCommand : ChatCommand, IAutoFillChatCommand
    {
        private const string INSTA_BREAK = "instabreak";
        private const string NO_BREAK_CD = "nobreakcd";
        private const string NO_HIT = "nohit";
        private const string NO_PLACE_COST = "nobreak";
        private const string NO_PLACE_LIMIT = "noplacelimit";
        private const string NO_ENERGY_COST = "noenergycost";
        public MiscDevCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {
        }

        public override void execute()
        {
            string parameter = parameters[0];
            bool? state = (parameters.Length > 1) ? ChatCommandParameterParser.parseBool(parameters,1,"state") : null;
            
            DevMode devMode = DevMode.Instance;
            switch (parameter)
            {
                case INSTA_BREAK:
                    SetState(ref devMode.instantBreak, state);
                    break;
                case NO_BREAK_CD:
                    SetState(ref devMode.noBreakCooldown, state);
                    break;
                case NO_HIT:
                    SetState(ref devMode.noHit, state);
                    break;
                case NO_PLACE_LIMIT:
                    SetState(ref devMode.noPlaceLimit, state);
                    break;
                case NO_PLACE_COST:
                    SetState(ref devMode.noPlaceCost, state);
                    break;
                case NO_ENERGY_COST:
                    SetState(ref devMode.NoEnergyCost, state);
                    break;
            }
        }

        private void SetState(ref bool value, bool? state)
        {
            if (state == null)
            {
                value = !value;
                return;
            }
            value = (bool)state;
        }

        public override string getDescription()
        {
            return "/devmode (option) (optional bool) \nToggles various devmodes";
        }

        public List<string> getAutoFill(int paramIndex)
        {
            return new List<string>
            {
                INSTA_BREAK,
                NO_BREAK_CD,
                NO_HIT,
                NO_PLACE_LIMIT,
                NO_PLACE_LIMIT,
                NO_ENERGY_COST
            };
        }
    }
}

