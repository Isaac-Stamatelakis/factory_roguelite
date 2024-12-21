using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Newtonsoft.Json;
using UI;


namespace TileEntityModule.Instances.Signs {
    public class SignInstance : TileEntityInstance<Sign>, IRightClickableTileEntity, ISerializableTileEntity
    {
        public SignInstance(Sign tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }
        private SignData signData;
        public void onRightClick()
        {
            GameObject signUIPrefab = tileEntity.UIManager.getUIElement();
            if (signData == null) {
                signData = new SignData("","","");
            }
            if (signUIPrefab == null) {
                Debug.LogError("Sign '"+ tileEntity.name + "' has null ui prefab");
                return;
            }
            
            GameObject instantiated = GameObject.Instantiate(signUIPrefab);
            SignUIController signUIController = instantiated.GetComponent<SignUIController>();
            if (signUIController == null) {
                Debug.LogError("Sign '" + tileEntity.name + "' ui prefab doesn't have SignUIController MonoBehavior Attached");
                GameObject.Destroy(instantiated);
                return;
            }
            signUIController.init(signData);
            MainCanvasController.Instance.DisplayObject(instantiated);
        }

        public string serialize()
        {
            return JsonConvert.SerializeObject(signData);
        }

        public void unserialize(string data)
        {
            if (data != null) {
                signData = JsonConvert.DeserializeObject<SignData>(data);
            }
        }
    }
}
