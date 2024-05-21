using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Items.Transmutable;

public static class TransmutableItemFactory
{
    public static ITransmutableItem generateItem(TransmutableItemState state, TransmutableItemMaterial material, string name, Sprite[] sprites, string id, string path) {
        if (state == TransmutableItemState.Block) {
            return generateTileItem(state,material,name,sprites,id,path);
        } else {
            return generateItemObject(state,material,name,sprites,id,path);
        }
    }

    private static TransmutableItemObject generateItemObject(TransmutableItemState state, TransmutableItemMaterial material, string name, Sprite[] sprites, string id, string path) {
        TransmutableItemObject transmutableItemObject = ScriptableObject.CreateInstance<TransmutableItemObject>();
        transmutableItemObject.name = name;
        transmutableItemObject.setSprites(sprites);
        transmutableItemObject.setState(state);
        transmutableItemObject.setMaterial(material);
        transmutableItemObject.id = id;
        AssetDatabase.CreateAsset(transmutableItemObject,path);
        return transmutableItemObject;
    }

    private static TransmutableTileItem generateTileItem(TransmutableItemState state, TransmutableItemMaterial material, string name, Sprite[] sprites, string id, string path) {
        TransmutableTileItem transmutableItemObject = ScriptableObject.CreateInstance<TransmutableTileItem>();
        transmutableItemObject.name = name;
        StandardTile tile = ItemEditorFactory.standardTileCreator(sprites[0],TileColliderType.Tile);
        tile.name = "T~" + name;
        tile.id = id;
        transmutableItemObject.setState(state);
        transmutableItemObject.setMaterial(material);
        AssetDatabase.CreateAsset(tile,path);
        transmutableItemObject.tile = tile;
        transmutableItemObject.id = id;
        AssetDatabase.CreateAsset(transmutableItemObject,path);
        return transmutableItemObject;
    }
}