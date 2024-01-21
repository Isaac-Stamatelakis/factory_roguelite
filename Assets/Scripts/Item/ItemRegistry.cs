using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Singleton
public class ItemRegistry {
    private static Dictionary<string,ItemObject> items;
    private static ItemRegistry instance;
    private ItemRegistry() {
        items = new Dictionary<string, ItemObject>();
        ItemObject[] itemObjects = Resources.LoadAll<ItemObject>("");
        foreach (ItemObject itemObject in itemObjects) {
            addToDict(itemObject);
        }
        TransmutableItemSprites sprites = TransmutableItemSprites.getInstance();
        TransmutableItemMaterial[] transmutableItemMaterials = Resources.LoadAll<TransmutableItemMaterial>("");
        foreach (TransmutableItemMaterial transmutableItemMaterial in transmutableItemMaterials) {
            TransmutableMaterialDict dict = new TransmutableMaterialDict(transmutableItemMaterial);
            foreach (TransmutableStateOptions itemConstructionData in transmutableItemMaterial.states) {
                transmutableItemMaterial.color.a = 1;
                TransmutableItemObject itemObject = ScriptableObject.CreateInstance<TransmutableItemObject>();
                itemObject.materialDict = dict;
                if (itemConstructionData.sprite == null) {
                    Sprite sprite = sprites.getSprite(itemConstructionData.state);
                    if (sprite == null) {
                        itemObject.sprite = Resources.Load<Sprite>("Sprites/tileobject16by16");
                        Debug.LogError("Attempted to load transmutable item sprite for " + itemConstructionData.state.ToString() + " which does not exist");
                    } else {
                        Texture2D texture2D = sprite.texture;
                        Rect rect = new Rect(0, 0, texture2D.width, texture2D.height);
                        Vector2 pivot = new Vector2(0.5f, 0.5f); 
                        Color[] pixels = texture2D.GetPixels();
                        for (int i = 0; i < pixels.Length; i++)
                        {
                            pixels[i] *= transmutableItemMaterial.color;
                        }
                        Texture2D newTexture = new Texture2D(texture2D.width, texture2D.height, TextureFormat.RGBA32, texture2D.mipmapCount > 1);
                        newTexture.SetPixels(pixels);
                        newTexture.Apply();
                        Sprite newSprite = Sprite.Create(newTexture, rect, pivot);
                        itemObject.state = itemConstructionData.state;
                        itemObject.sprite = newSprite;
                    }
                    
                } else {
                    itemObject.sprite = itemConstructionData.sprite;
                }
                
                
                if (itemConstructionData.prefix.Length == 0) {
                    itemObject.name += TransmutableItemStateFactory.getPrefix(itemObject.state);
                } else {
                    itemObject.name = itemConstructionData.prefix;
                }

                itemObject.name += " " + transmutableItemMaterial.name + " ";
                if (itemConstructionData.suffix.Length == 0) {
                    itemObject.name += TransmutableItemStateFactory.getSuffix(itemObject.state);
                } else {
                    itemObject.name += itemConstructionData.suffix;
                }
                itemObject.id = itemObject.name.ToLower().Replace(" ","_");
                
                addToDict(itemObject);
            }
        }
        Debug.Log("Item registry loaded  " + items.Count + " items");
    }

    private bool addToDict(ItemObject itemObject) {
        if (!items.ContainsKey(itemObject.id)) {
            items[itemObject.id] = itemObject;
            return true;
        } else {
            ItemObject contained = items[itemObject.id];
            Debug.LogError("Duplicate id for objects " + contained.name + " and " + itemObject.name + " with id: " + itemObject.id);
            return false;
        }
    }
    public static ItemRegistry getInstance() {
        if (instance == null) {
            instance = new ItemRegistry();
        }
        return instance;
    }
    ///
    /// Returns tileItem if id maps to tile item, null otherwise
    ///
    public TileItem getTileItem(string id) {
        if (!items.ContainsKey(id)) {
            return null;
        }
        ItemObject itemObject = items[id];
        if (itemObject is TileItem) {
            return (TileItem) itemObject;
        } else {
            return null;
        }
    }

    public ItemObject getItemObject(string id) {
        if (!items.ContainsKey(id)) {
            return null;
        }
        return items[id];   
    }
    ///
    /// Returns ConduitItem if id maps to ConduitItem, null otherwise
    ///
    public ConduitItem GetConduitItem(string id) {
        if (!items.ContainsKey(id)) {
            return null;
        }
        ItemObject itemObject = items[id];
        if (itemObject is ConduitItem) {
            return (ConduitItem) itemObject;
        } else {
            return null;
        }
    }

    public List<ItemObject> query(string serach) {
        List<ItemObject> queried = new List<ItemObject>();
        foreach (ItemObject itemObject in items.Values) {
            if (itemObject.name.ToLower().Contains(serach.ToLower())) {
                queried.Add(itemObject);
            }
        }
        return queried;
    }
}
