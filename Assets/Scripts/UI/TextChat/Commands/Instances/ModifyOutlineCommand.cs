using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Dimensions;
using PlayerModule;
using Items;
using UnityEngine.Rendering.Universal;
using TileMaps;
using System.Linq;

namespace UI.Chat {
    public class ModifyOutlineCommand : ChatCommand, IAutoFillChatCommand
    {
        public ModifyOutlineCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {

        }
        public override void execute()
        {
            try {
                bool wireFrame = ChatCommandParameterParser.parseBool(parameters,0,"wireframe");
                Color? color = null;
                if (parameters.Length > 1) {
                    color = ChatCommandParameterParser.parseColor(parameters,1);
                }
                DimensionManager dimensionManager = DimensionManager.Instance;
                OutlineWorldTileGridMap[] outlineTileGridMaps = GameObject.FindObjectsOfType<OutlineWorldTileGridMap>();
                foreach (OutlineWorldTileGridMap outlineTileGridMap in outlineTileGridMaps) {
                    outlineTileGridMap.setView(wireFrame,color);
                }
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
            return "/outline wireframe color \nSets view\nwireframe (bool)\nOptional color (r,g,b)";
        }
    }
}
