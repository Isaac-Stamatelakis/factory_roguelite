using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Dimensions;
using System.IO;

namespace WorldModule {
    public static class OpenWorld 
    {
        public static void loadWorld(string worldName) {
            if (!WorldLoadUtils.defaultWorldExists(worldName)) {
                Debug.LogError("Attempted to open world which doesn't exist");
            }
            WorldManager.getInstance().setWorldPath(Path.Combine(WorldLoadUtils.DefaultWorldFolder,worldName));
            SceneManager.LoadScene("MainScene");
        }

        public static void createDimController(int dim) {
            GameObject dimController = new GameObject();
            dimController.name = "DimController";
            if (dim == 0) {
                Dim0Controller dim0Controller = dimController.AddComponent<Dim0Controller>();
            }
            
        }
        

        public static void loadWorldFromMain(string worldName) {
            
            createDimController(0);
        }
    }

}
