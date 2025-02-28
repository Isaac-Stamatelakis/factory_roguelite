using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Newtonsoft.Json;
using UI;


namespace TileEntity.Instances.Signs {
    public class SignInstance : TileEntityInstance<Sign>, IRightClickableTileEntity, ISerializableTileEntity
    {
        public SignInstance(Sign tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }
        private SignData signData;
        public void OnRightClick()
        {
            GameObject signUIPrefab = TileEntityObject.UIManager.getUIElement();
            if (signData == null) {
                signData = new SignData("","","");
            }
            if (signUIPrefab == null) {
                Debug.LogError("Sign '"+ TileEntityObject.name + "' has null ui prefab");
                return;
            }
            
            GameObject instantiated = GameObject.Instantiate(signUIPrefab);
            SignUIController signUIController = instantiated.GetComponent<SignUIController>();
            if (signUIController == null) {
                Debug.LogError("Sign '" + TileEntityObject.name + "' ui prefab doesn't have SignUIController MonoBehavior Attached");
                GameObject.Destroy(instantiated);
                return;
            }
            signUIController.init(signData);
            MainCanvasController.Instance.DisplayObject(instantiated);
        }

        public string Serialize(SerializationMode mode)
        {
            return JsonConvert.SerializeObject(signData);
        }

        public void Unserialize(string data)
        {
            if (data != null) {
                signData = JsonConvert.DeserializeObject<SignData>(data);
            }
        }
    }
}
