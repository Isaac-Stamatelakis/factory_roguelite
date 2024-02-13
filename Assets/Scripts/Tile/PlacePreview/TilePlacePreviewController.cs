using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TileMapModule.Previewer {
    public class TilePlacePreviewController : MonoBehaviour
    {
        [SerializeField]
        public bool on;
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void toggle() {
            if (on) {
                turnOff();
            } else {
                turnOn();
            }
            on = !on;
        }

        public void turnOff() {
            GameObject.Destroy(Global.findChild(transform,"PlacePreviewer"));
        }

        public void turnOn() {
            GameObject placePreviewer = new GameObject();
            placePreviewer.name = "PlacePreviewer";
            Grid grid = placePreviewer.AddComponent<Grid>();
            grid.cellSize = new Vector3(0.5f,0.5f,1);
            placePreviewer.AddComponent<Tilemap>();
            placePreviewer.AddComponent<TilemapRenderer>();
            placePreviewer.AddComponent<TilePlacePreviewer>();
            placePreviewer.transform.SetParent(transform);
            placePreviewer.transform.localPosition = Vector3.zero;
        }

        public void setActive(bool on) {
            this.on = on;
            if (on) {
                turnOn();
            } else {
                turnOff();
            }
        }
    }
}