using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using WorldModule;

public static class GlobalHelper 
{
    public static GameObject instantiateFromResourcePath(string path) {
        return GameObject.Instantiate(Resources.Load<GameObject>(path));
    }
    public static void DeleteAllChildren(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++) {
            GameObject.Destroy(parent.GetChild(i).gameObject);
        }
    }
    
    
    public static ulong BinaryExponentiation(ulong baseValue, int exponent)
    {
        if (exponent < 0)
        {
            throw new ArgumentException("Exponent must be a non-negative integer.");
        }

        ulong result = 1; 
        
        while (exponent > 0)
        {
            if ((exponent & 1) == 1)
            {
                result *= baseValue;
            }
            
            baseValue *= baseValue;
            exponent >>= 1;
        }

        return result;
    }

    public static uint MaxUInt(uint a, uint b)
    {
        return a > b ? a : b;
    }
    
    public static uint MinUInt(uint a, uint b)
    {
        return a < b ? a : b;
    }

    public static uint Clamp(uint val, uint min, uint max)
    {
        return val < min ? min : val > max ? max : val;
    }

    public static T ShiftEnum<T>(int amount, T enumValue) where T : Enum
    {
        int enumCount = Enum.GetValues(typeof(T)).Length;
        int value = Convert.ToInt32(enumValue);
        int shiftedValue = (value + amount + enumCount) % enumCount;
        return (T)Enum.ToObject(typeof(T), shiftedValue);
    }

    public static List<TMP_Dropdown.OptionData> EnumToDropDown<T>() where T : Enum
    {
        Array values = Enum.GetValues(typeof(T));
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        foreach (T value in values)
        {
            options.Add(new TMP_Dropdown.OptionData(value.ToString()));
        }
        return options;
    }
    
    public static List<TMP_Dropdown.OptionData> StringListToDropDown(List<string> strings)
    {
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        foreach (string stringVal in strings)
        {
            options.Add(new TMP_Dropdown.OptionData(stringVal));
        }
        return options;
    }
    
    public static void CopyDirectory(string sourceDir, string targetDir)
    {
        DirectoryInfo sourceDirInfo = new DirectoryInfo(sourceDir);
            
        if (!sourceDirInfo.Exists)
        {
            throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");
        }
            
        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }
            
        foreach (FileInfo file in sourceDirInfo.GetFiles())
        {
            string targetFilePath = Path.Combine(targetDir, file.Name);
            file.CopyTo(targetFilePath, overwrite: true);
        }
            
        foreach (DirectoryInfo subDir in sourceDirInfo.GetDirectories())
        {
            string newTargetDir = Path.Combine(targetDir, subDir.Name);
            CopyDirectory(subDir.FullName, newTargetDir);
        }
    }
    
    /// <summary>
    /// Generates a random hash
    /// </summary>
    /// <returns></returns>
    public static string GenerateHash()
    {
        byte[] hash = new byte[8]; 
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(hash);
        }

        return BitConverter.ToString(hash).Replace("-","");
    }
    
    public static string GenerateHash(List<string> ids)
    {
        const int ATTEMPTS = 64;
        for (int i = 0; i < ATTEMPTS; i++)
        {
            string hash = GenerateHash();
            bool found = false;
            foreach (string id in ids)
            {
                if (id == hash)
                {
                    found = true;
                    break;
                }
            }

            if (!found) return hash;
        }

        return null;
    }

    public static uint TileEntitySecondsToTicks(float seconds)
    {
        uint fixedUpdates = (uint)(seconds / Time.fixedDeltaTime);
        return fixedUpdates * Global.TILE_ENTITY_TICK_RATE;
    }

    public static void SerializeCompressedJson<T>(T classInstance, string path)
    {
        string json = JsonConvert.SerializeObject(classInstance);
        byte[] bytes = WorldLoadUtils.CompressString(json);
        File.WriteAllBytes(path, bytes);
    }

    public static T DeserializeCompressedJson<T>(string path)
    {
        if (!File.Exists(path)) return default(T);
        byte[] bytes = File.ReadAllBytes(path);
        string json = WorldLoadUtils.DecompressString(bytes);
        return JsonConvert.DeserializeObject<T>(json);
    }
    public static string AddSpaces(string text)
    {
        string result = string.Empty;
        for (var i = 0; i < text.Length-1; i++)
        {
            var c = text[i];
            result += c;
            if (char.IsLower(text[i]) && char.IsUpper(text[i+1]))
            {
                result += " ";
            }
        }

        result += text[^1];
        return result;
    }
}
