using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

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
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Items/Main/TransmutableItems/Saved")) {
            AssetDatabase.CreateFolder("Assets/Resources/Items/Main/TransmutableItems","Saved");
        }
        foreach (TransmutableItemMaterial transmutableItemMaterial in transmutableItemMaterials) {
            TransmutableMaterialDict dict = new TransmutableMaterialDict(transmutableItemMaterial);
            string path = "Assets/Resources/Items/Main/TransmutableItems/Saved";
            if (AssetDatabase.IsValidFolder(path + "/" +transmutableItemMaterial.name)){
                ItemObject[] materialItemObjects = Resources.LoadAll<ItemObject>(path);
                foreach (ItemObject itemObject in materialItemObjects) {
                    addToDict(itemObject);
                }
            } else {
                AssetDatabase.CreateFolder(path,transmutableItemMaterial.name);
                path += "/" +transmutableItemMaterial.name + "/";
                foreach (TransmutableStateOptions itemConstructionData in transmutableItemMaterial.states) {
                    Debug.Log("Creating itemobjects for material" + transmutableItemMaterial.name);
                    string prefix = TransmutableItemStateFactory.getPrefix(itemConstructionData.state);
                    string suffix = TransmutableItemStateFactory.getSuffix(itemConstructionData.state);
                    string name = "";
                    if (itemConstructionData.prefix.Length == 0) {
                        name += prefix;
                    } else {
                        name = itemConstructionData.prefix;
                    }
                    name += " " + transmutableItemMaterial.name + " ";
                    if (itemConstructionData.suffix.Length == 0) {
                        name += suffix;
                    } else {
                        name += itemConstructionData.suffix;
                    }        
                    transmutableItemMaterial.color.a = 1;
                    TransmutableItemObject itemObject = ScriptableObject.CreateInstance<TransmutableItemObject>();
                    itemObject.name = name;
                    itemObject.materialDict = dict;
                    itemObject.state = itemConstructionData.state;
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
                            
                            string spritePath = path + name.Replace(" ", "") +"Sprite";
                            byte[] pngBytes = newTexture.EncodeToPNG();
                            File.WriteAllBytes(spritePath+".png", pngBytes);
                            AssetDatabase.Refresh();

                            TextureImporter textureImporter = AssetImporter.GetAtPath(spritePath + ".png") as TextureImporter;
                            textureImporter.textureType = TextureImporterType.Sprite;
                            textureImporter.spritePixelsPerUnit = sprite.pixelsPerUnit;
                            AssetDatabase.ImportAsset(spritePath + ".png", ImportAssetOptions.ForceUpdate);
                            AssetDatabase.Refresh();

                            Sprite sprite1 = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + ".png");
                            itemObject.sprite = sprite1;
                        }
                        
                    } else {
                        itemObject.sprite = itemConstructionData.sprite;
                    }
                    itemObject.id = (prefix + "_"+ transmutableItemMaterial.id + "_" + suffix).ToLower();
                    
                    AssetDatabase.CreateAsset(itemObject,path + name.Replace(" ","") + ".asset");
                    addToDict(itemObject);
                }
            }
            
        }
        Debug.Log("Item registry loaded  " + items.Count + " items");
        AssetDatabase.Refresh();

        RecipeRegistry.getInstance();
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
