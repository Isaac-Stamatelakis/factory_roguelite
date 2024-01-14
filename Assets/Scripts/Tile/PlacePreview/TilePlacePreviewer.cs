using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilePlacePreviewer : MonoBehaviour
{
    public Vector3Int previouslyPreviewed;
    public Vector3 previousMousePosition;
    private Tilemap tilemap;
    private DevMode devMode;
    private PlayerInventory playerInventory;
    private int previousId = -1;
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
            previewTile((int) devMode.placeID,mousePosition.x,mousePosition.y);
        } else {
            previewTile(playerInventory.getSelectedTileId(), mousePosition.x, mousePosition.y);
        }
        
    }   
    public void previewTile(int id, float x, float y) {
        if (id < 0) {
            return;
        }
        //Vector3Int adjustedChunkPosition = new Vector3Int(16 * Mathf.FloorToInt(x/8f),16 * Mathf.FloorToInt(y/8f),0);
        Vector3Int placePosition = (Vector3Int)PlaceTile.getPlacePosition(id,x,y);

        
        if (previouslyPreviewed == placePosition && previousId == id) {
            return;
        }
        
        IdData idData = IdDataMap.getInstance().GetIdData(id);
        if (idData is TileData) {
            TileData tileData = IdDataMap.getInstance().copyTileData(id);
            if (tileData.tileOptions.containsKey("rotation")) {
                tileData.tileOptions.set("rotation", devMode.rotation);
            }
            tilemap.SetTile(placePosition, TileFactory.generateTile(tileData));
            if (PlaceTile.tileBlockPlacable(id,x,y)) {
                tilemap.color = new Color(111f/255f,180f/255f,248f/255f);
            } else {
                tilemap.color = new Color(255f/255f,153f/255f,153/255f);
            }
        } else if (idData is ConduitData) {
            ConduitData conduitData = (ConduitData) idData;
            tilemap.SetTile(placePosition, Resources.Load<RuleTile>(conduitData.ruleTilePath));
            if (PlaceTile.conduitPlacable(id,conduitData.conduitType, new Vector2(x,y))) {
                tilemap.color = new Color(111f/255f,180f/255f,248f/255f);
            } else {
                tilemap.color = new Color(255f/255f,153f/255f,153/255f);
            }
        }

        if (previouslyPreviewed != placePosition) {
                tilemap.SetTile(previouslyPreviewed, null);
            }
        
        previouslyPreviewed = placePosition;
        previousId = id;
        
         
    }
}
