using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMaps.Place;
using PlayerModule;
using Tiles;
using Items;

namespace TileMaps.Previewer {
    public class TilePlacePreviewer : MonoBehaviour
    {
        public Vector3Int previouslyPreviewed;
        public Vector3 previousMousePosition;
        private Tilemap tilemap;
        private DevMode devMode;
        private PlayerInventory playerInventory;
        private string previousId = null;
        // Start is called before the first frame update
        void Start()
        {
            devMode = GameObject.Find("Player").GetComponent<DevMode>();
            playerInventory = GameObject.Find("Player").GetComponent<PlayerInventory>();
            tilemap = GetComponent<Tilemap>();
            
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (devMode.placeSelectedID) {
                previewTile(devMode.placeID,mousePosition);
            } else {
                previewTile(playerInventory.getSelectedId(), mousePosition);
            }
            
        }   
        public void previewTile(string id, Vector2 position) {
            ItemObject itemObject = ItemRegistry.getInstance().getItemObject(id);
            if (itemObject == null || itemObject is not IPlacableItem placableTile) {
                tilemap.SetTile(previouslyPreviewed, null);
                return;
            }
            Vector3Int placePosition = PlaceTile.getItemPlacePosition(itemObject,position);
            if (previouslyPreviewed == placePosition && previousId == id) {
                return;
            }
            TileBase tileBase = null;
            TileBase itemTileBase = placableTile.getTile();
            int state = 0;
            if (itemTileBase is IRestrictedTile restrictedTile) {
                state = restrictedTile.getStateAtPosition(position,MousePositionFactory.getVerticalMousePosition(position),MousePositionFactory.getHorizontalMousePosition(position));
            }
            if (itemTileBase is IStateTile stateTile) {
                tileBase = stateTile.getTileAtState(state);
            } else {
                tileBase = itemTileBase;
            } 
            tilemap.SetTile(placePosition,tileBase);
            if (PlaceTile.itemPlacable(itemObject,position)) {
                tilemap.color = new Color(111f/255f,180f/255f,248f/255f);
            } else {
                tilemap.color = new Color(255f/255f,153f/255f,153/255f);
            }
            if (previouslyPreviewed != placePosition) {
                tilemap.SetTile(previouslyPreviewed, null);
            }
            
            previouslyPreviewed = placePosition;
            previousId = id;
            
            
        }
    }
}