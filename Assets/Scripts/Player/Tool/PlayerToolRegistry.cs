using System;
using System.Collections;
using System.Collections.Generic;
using Recipe.Collection;
using Recipe.Processor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Player.Tool
{
    public class PlayerToolRegistry
    {
        private static PlayerToolRegistry instance;
        private static Dictionary<PlayerToolType, PlayerToolObject> ToolDict;

        public PlayerToolObject GetToolObject(PlayerToolType playerToolType)
        {
            if (ToolDict.TryGetValue(playerToolType, out PlayerToolObject toolObject))
            {
                return toolObject;
            }
            Debug.LogWarning($"Tried to access tool not in dict {playerToolType}");
            return null;
        }
        public static IEnumerator LoadTools()
        {
            if (instance != null) {
                yield break;
            }
            
            instance = new PlayerToolRegistry();
            
            var handle = Addressables.LoadAssetsAsync<PlayerToolObject>("tool", null);
            yield return handle;
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("Failed to load recipe processors from addressables: " + handle.OperationException);
                yield break;
            }

            ToolDict = new Dictionary<PlayerToolType, PlayerToolObject>();
            foreach (var asset in handle.Result)
            {
                PlayerToolType type = asset.ToolType;
                if (ToolDict.TryGetValue(type, out var value))
                {
                    Debug.LogWarning($"Duplicate tool objects of type {type}: {asset.name} and {value.name}");
                }
                ToolDict[type] = asset;
            }
        }

        public static PlayerToolRegistry GetInstance()
        {
            if (instance == null)
            {
                throw new NullReferenceException("Tried to access null PlayerToolRegistry");
            }

            return instance;
        }
    }
}
