using System;
using System.Collections;
using System.Collections.Generic;
using Player.Tool.Object;
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
        private static Dictionary<RobotToolType, RobotToolObject> ToolDict;

        public T GetToolObject<T>(RobotToolType robotToolType) where T : RobotToolObject
        {
            if (ToolDict.TryGetValue(robotToolType, out RobotToolObject toolObject))
            {
                return toolObject as T;
            }
            Debug.LogWarning($"Tried to access tool not in dict {robotToolType}");
            return null;
        }
        public static IEnumerator LoadTools()
        {
            if (instance != null) {
                yield break;
            }
            
            instance = new PlayerToolRegistry();
            
            var handle = Addressables.LoadAssetsAsync<RobotToolObject>("tool", null);
            yield return handle;
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("Failed to load recipe processors from addressables: " + handle.OperationException);
                yield break;
            }

            ToolDict = new Dictionary<RobotToolType, RobotToolObject>();
            foreach (var asset in handle.Result)
            {
                if (asset is RobotDrillObject playerDrillObject)
                {
                    ToolDict[RobotToolType.LaserDrill] = playerDrillObject;
                }
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
