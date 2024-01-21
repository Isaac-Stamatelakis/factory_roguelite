using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Recipe",menuName="Crafting/Recipe")]
public class Recipe : ScriptableObject
{
    public List<ScriptableItemSlot> inputs;
    public List<ScriptableItemSlot> outputs;
    // Enable loading of items which have been deleted and recreated (such as transmutables)
    [Header("DO NOT EDIT\nPaths for inputs/outputs\nSet automatically")]
    public List<string> inputGUIDs;
    public List<string> outputGUIDs;
    
    public List<string> InputPaths {get{return inputGUIDs;} set{inputGUIDs = value;}}
    public List<string> OutputPaths {get{return outputGUIDs;} set{outputGUIDs = value;}}
}   

[System.Serializable]
public class ScriptableItemSlot {
    public ItemObject item;
    public int amount;
}