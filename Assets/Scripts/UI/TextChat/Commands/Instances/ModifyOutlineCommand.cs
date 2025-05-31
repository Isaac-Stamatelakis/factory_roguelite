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
            switch (parameters.Length)
            {
                case 1:
                    SingleParameter();
                    break;
                case 2:
                    bool wireFrame = ChatCommandParameterParser.parseBool(parameters, 0, "wireframe");
                    SetWireFrameState(wireFrame,null);
                    Color color = ChatCommandParameterParser.parseColor(parameters,1);
                    SetWireFrameState(wireFrame,color);
                    break;
                default:
                    throw new ChatParseException("Invalid number of parameters");
            }
        }

        private void SingleParameter()
        {
            try
            {
                bool wireFrame = ChatCommandParameterParser.parseBool(parameters, 0, "wireframe");
                SetWireFrameState(wireFrame,null);
                return;
            }
            catch (ChatParseException)
            {
                
            }
            
            try
            {
                Color color = ChatCommandParameterParser.parseColor(parameters,0);
                SetWireFrameState(null,color);
                return;
            }
            catch (ChatParseException)
            {
                
            }
            throw new ChatParseException($"Invalid parameter '{parameters[0]}'");
        }

        private void SetWireFrameState(bool? wireFrame, Color? color)
        {
            DimensionManager dimensionManager = DimensionManager.Instance;
            BlockWorldTileMap[] outlineTileGridMaps = GameObject.FindObjectsOfType<BlockWorldTileMap>();
            foreach (BlockWorldTileMap outlineTileGridMap in outlineTileGridMaps) {
                outlineTileGridMap.SetView(wireFrame,color);
            }
        }

        public List<string> getAutoFill(int paramIndex)
        {
            switch (paramIndex)
            {
                case 0:
                {
                    List<string> fills = new List<string>
                    {
                        "on",
                        "off"
                    };
                    fills.AddRange(ChatCommandParameterParser.PresetColors.Keys.ToList());
                    return fills;
                }
                case 1:
                    return ChatCommandParameterParser.PresetColors.Keys.ToList();
                default:
                    return null;
            }
        }

        public override string getDescription()
        {
            List<string> first = getAutoFill(0);
            List<string> second = getAutoFill(1);
            
            return $"/outline {ChatCommandParameterParser.FormatParameters(first)}? {ChatCommandParameterParser.FormatParameters(second)}?\nSets state and/or color of outline";
        }
    }
}
