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
            previewTile(devMode.placeID,mousePosition.x,mousePosition.y);
        } else {
            previewTile(playerInventory.getSelectedTileId(), mousePosition.x, mousePosition.y);
        }
        
    }   
    public void previewTile(string id, float x, float y) {
        if (id == null) {
            return;
        }
        ItemObject itemObject = ItemRegistry.getInstance().getItemObject(id);
        if (itemObject == null) {
            return;
        }
        Vector3Int placePosition = Vector3Int.zero;
        if (itemObject is TileItem) {
            TileItem tileItem = (TileItem) itemObject;
            placePosition = (Vector3Int)PlaceTile.getPlacePosition(tileItem,x,y);
        } else if (itemObject is ConduitItem) {

        }
        

        
        if (previouslyPreviewed == placePosition && previousId == id) {
            return;
        }
        
        if (itemObject is TileItem) {
            TileItem tileItem = (TileItem) itemObject;
            TileData tileData = new TileData(itemObject: tileItem,options:tileItem.getOptions());
            if (tileData.options.ContainsKey(TileItemOption.Rotation)) {
                tileData.options[TileItemOption.Rotation] = devMode.rotation;
            }
            tilemap.SetTile(placePosition, TileFactory.generateTile(tileData));
            if (PlaceTile.tileBlockPlacable(tileItem,x,y)) {
                tilemap.color = new Color(111f/255f,180f/255f,248f/255f);
            } else {
                tilemap.color = new Color(255f/255f,153f/255f,153/255f);
            }
        } else if (itemObject is ConduitItem) {
            ConduitItem conduitItem = (ConduitItem) itemObject;
            tilemap.SetTile(placePosition, conduitItem.ruleTile);
            if (PlaceTile.conduitPlacable(conduitItem, new Vector2(x,y))) {
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
