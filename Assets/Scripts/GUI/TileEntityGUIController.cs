using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;

namespace GUIModule {
    public class TileEntityGUIController : MonoBehaviour
    {
        private GameObject GUIGameObject = null;
        public bool isActive {get{return this.gameObject != null;}}
        // Start is called before the first frame update
        public void Start()
        {
            
        }

        // Update is called once per frame
        public void Update()
        {
            if (GUIGameObject != null) {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E)) {
                    removeGUI();
                }
            }
        }

        public void removeGUI() {
            if (this.GUIGameObject == null) {
                return;
            }
            GameObject.Destroy(GUIGameObject);
            GUIGameObject = null;
            
        }

        public void setGUI(TileEntity tileEntity, GameObject gui) {
            removeGUI();
            if (gui == GUIGameObject) {
                GUIGameObject = null;
                return;
            }
            
            this.GUIGameObject = gui;
            GUIGameObject.transform.SetParent(transform,false);
        }
    
    }
}
