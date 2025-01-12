using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TileMaps.Previewer {
    public class TilePlacePreviewController : MonoBehaviour
    {
        [SerializeField]
        public bool on;

        private GameObject placePreviewer;
        
        public void Toggle() {
            if (on) {
                TurnOff();
            } else {
                turnOn();
            }
            on = !on;
        }

        public void TurnOff() {
            GameObject.Destroy(placePreviewer);
        }

        public void turnOn() {
            placePreviewer = new GameObject();
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
                TurnOff();
            }
        }
    }
}