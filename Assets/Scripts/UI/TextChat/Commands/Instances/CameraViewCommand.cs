using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Dimensions;
using PlayerModule;
using Items;
namespace UI.Chat {
    public class CameraViewCommand : ChatCommand, IAutoFillChatCommand
    {
        public CameraViewCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {
        }
        public override void execute()
        {
            string sizeName = parameters[0];
#if UNITY_EDITOR
            if (sizeName == "debug") {
                double debugSize = System.Convert.ToDouble(parameters[1]);
                CameraView.Instance.setDebugRange((float)debugSize);
                return;
            }
#endif
            Dictionary<string, CameraViewSize> dict = getNameDict();
            if (!dict.ContainsKey(sizeName)) {
                chatUI.SendChatMessage($"{sizeName} is not a valid camera size");
                return;
            }
            CameraViewSize size = dict[sizeName];
            CameraView.Instance.SetViewRange(size);
        }

        public List<string> getAutoFill(int paramIndex)
        {
            if (paramIndex > 0) {
                return null;
            }
            string[] names = Enum.GetNames(typeof(CameraViewSize));
            List<string> lowerCase = new List<string>();
            foreach (string name in names) {
                lowerCase.Add(name.ToLower());
            }
            #if UNITY_EDITOR
            lowerCase.Add("debug");
            #endif
            return lowerCase;
        }

        public Dictionary<string, CameraViewSize> getNameDict() {
            CameraViewSize[] viewSizes = (CameraViewSize[])Enum.GetValues(typeof(CameraViewSize));
            Dictionary<string, CameraViewSize> dict = new Dictionary<string, CameraViewSize>();
            foreach (CameraViewSize cameraViewSize in viewSizes) {
                dict[cameraViewSize.ToString().ToLower()] = cameraViewSize; 
            }
            return dict;

        }
        
        public override string getDescription()
        {
            return "/give id amount\nGives player amount of item with id";
        }
    }
}
