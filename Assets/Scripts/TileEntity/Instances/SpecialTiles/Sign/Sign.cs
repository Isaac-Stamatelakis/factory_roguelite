using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace TileEntityModule.Instances.Signs {
    [CreateAssetMenu(fileName = "New Sign", menuName = "Tile Entity/Sign")]
    public class Sign : TileEntity, IRightClickableTileEntity, ISerializableTileEntity
    {
        public GameObject signUIPrefab;
        private SignData signData;
        public void onRightClick()
        {
            if (signData == null) {
                signData = new SignData("","","");
            }
            if (signUIPrefab == null) {
                Debug.LogError("Sign '"+ name + "' has null ui prefab");
                return;
            }
            
            GameObject instantiated = GameObject.Instantiate(signUIPrefab);
            SignUIController signUIController = instantiated.GetComponent<SignUIController>();
            if (signUIController == null) {
                Debug.LogError("Sign '" + name + "' ui prefab doesn't have SignUIController MonoBehavior Attached");
                GameObject.Destroy(instantiated);
                return;
            }
            signUIController.init(signData);
            
            GlobalUIController globalUIController = GlobalUIContainer.getInstance().getUiController();
            globalUIController.setGUI(instantiated);

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
    [System.Serializable]
    public class SignData {
        public string line1;
        public string line2;
        public string line3;
        public SignData(string line1, string line2, string line3) {
            this.line1 = line1;
            this.line2 = line2;
            this.line3 = line3;
        }
    }
}

