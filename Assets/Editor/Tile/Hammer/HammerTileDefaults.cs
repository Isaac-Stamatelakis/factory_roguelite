using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using NUnit.Framework.Constraints;
using UnityEditor;

public enum HammerTileType
{
    Slab,
    Slant,
    Stair
}

public enum NatureHammerTileType
{
    Slab,
    Slant
}
public class HammerTileValues
{
    private static readonly string FOLDER_PATH = "Assets/Editor/Sprites/Objects";
    private static readonly string SLAB_PATH = "Slab/Slab.asset";
    private static readonly string SLANT_PATH = "Slant/Slant.asset";
    private static readonly string STAIR_PATH = "Stair/Stair.asset";
    private static readonly string NATURE_SLAB_PATH = "NatureSlabs";
    private static readonly string NATURE_SLANT_PATH = "NatureSlants";
    private static readonly int NATURE_SLAB_COUNT = 6;
    private static readonly int NATURE_SLANT_COUNT = 6;

    private static string GetInteralAssetPath(HammerTileType type)
    {
        return type switch
        {
            HammerTileType.Slab => SLAB_PATH,
            HammerTileType.Slant => SLANT_PATH,
            HammerTileType.Stair => STAIR_PATH,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
    
    private static string GetInteralAssetPath(NatureHammerTileType type)
    {
        return type switch
        {
            NatureHammerTileType.Slab => NATURE_SLAB_PATH,
            NatureHammerTileType.Slant => NATURE_SLANT_PATH,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private static int GetNatureCount(NatureHammerTileType type)
    {
        return type switch
        {
            NatureHammerTileType.Slab => NATURE_SLAB_COUNT,
            NatureHammerTileType.Slant => NATURE_SLANT_COUNT,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
    private static string GetNaturePath(NatureHammerTileType type, int index)
    {
        return Path.Combine(GetInteralAssetPath(type), index.ToString() + ".asset");
    }

    private static string GetSpriteCollectionPath(HammerTileType type)
    {
        return Path.Combine(FOLDER_PATH, GetInteralAssetPath(type));
    }
    
    private SpriteRotationCollection[] GetNatureCollections(NatureHammerTileType type)
    {
        int count = GetNatureCount(type);
        SpriteRotationCollection[] spriteCollections = new SpriteRotationCollection[count];
        for (int i = 0; i < count; i++)
        {
            string path = GetNaturePath(type, i);
            spriteCollections[i] = AssetDatabase.LoadAssetAtPath<SpriteRotationCollection>(path);
        }

        return spriteCollections;
    }
    public SpriteRotationCollection Slab;
    public SpriteRotationCollection Slant;
    public SpriteRotationCollection Stairs;
    public SpriteRotationCollection[] NatureSlabs;
    public SpriteRotationCollection[] NatureSlants;
    public HammerTileValues()
    {
        Slab = AssetDatabase.LoadAssetAtPath<SpriteRotationCollection>(GetSpriteCollectionPath(HammerTileType.Slab));
        Slant = AssetDatabase.LoadAssetAtPath<SpriteRotationCollection>(GetSpriteCollectionPath(HammerTileType.Slant));
        Stairs = AssetDatabase.LoadAssetAtPath<SpriteRotationCollection>(GetSpriteCollectionPath(HammerTileType.Stair));
        NatureSlabs = GetNatureCollections(NatureHammerTileType.Slab);
    }
    
    
}
