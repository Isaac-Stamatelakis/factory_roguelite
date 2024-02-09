using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WorldDataModule {
    public static class OpenWorld 
    {
        public static void loadWorld(string worldName) {
            if (!WorldCreation.worldExists(worldName)) {
                Debug.LogError("Attempted to open world which doesn't exist");
            }
            Global.WorldName = worldName;
            SceneManager.sceneLoaded += test;
            SceneManager.LoadScene("MainScene");
        }

        public static void createDimController(int dim) {
            GameObject dimController = new GameObject();
            dimController.name = "DimController";
            if (dim == 0) {
                Dim0Controller dim0Controller = dimController.AddComponent<Dim0Controller>();
            }
            
        }
        private static void test(Scene scene, LoadSceneMode mode) {
            SceneManager.sceneLoaded -= test;
            int dim = 0;
            createDimController(dim);
        }
    }

}
