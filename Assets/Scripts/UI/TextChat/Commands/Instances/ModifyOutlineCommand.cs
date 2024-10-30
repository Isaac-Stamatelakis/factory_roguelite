using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Dimensions;
using PlayerModule;
using Items;
using UnityEngine.Rendering.Universal;
using TileMaps;

namespace UI.Chat {
    public class ModifyOutlineCommand : ChatCommand
    {
        public ModifyOutlineCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {

        }
        public override void execute()
        {
            try {
                bool wireFrame = ChatCommandParameterParser.parseBool(parameters,0,"wireframe");
                Color color = Color.black;
                if (parameters.Length > 1) {
                    color = ChatCommandParameterParser.parseColor(parameters,1);
                }
                DimensionManager dimensionManager = DimensionManager.Instance;
                OutlineTileGridMap[] outlineTileGridMaps = GameObject.FindObjectsOfType<OutlineTileGridMap>();
                Debug.Log(outlineTileGridMaps.Length);
                foreach (OutlineTileGridMap outlineTileGridMap in outlineTileGridMaps) {
                    outlineTileGridMap.setView(wireFrame,color);
                }
            } catch (ChatParseException e) {
                chatUI.sendMessage(e.Message);
            }
        }

        public override string getDescription()
        {
            return "/outline wireframe color \nSets view\nwireframe (bool)\nOptional color (r,g,b)";
        }
    }
}
