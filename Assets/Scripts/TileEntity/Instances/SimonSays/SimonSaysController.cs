using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Newtonsoft.Json;
using ChunkModule;
using ItemModule;

namespace TileEntityModule.Instances.SimonSays {
    [CreateAssetMenu(fileName = "E~New Simon Says Controller", menuName = "Tile Entity/SimonSays/Controller")]
    public class SimonSaysController : TileEntity, IClickableTileEntity, ILoadableTileEntity
    {
        [Header("The number of simon says sequences which are played")]
        [SerializeField] public int length;
        [SerializeField] public LootTable lootTable;
        [Header("Extra rewards for successfully finishing the puzzle")]
        [SerializeField] public LootTable completionLootTable;
        private List<SimonSaysColoredTileEntity> coloredTiles;
        private SimonSaysCoroutineController coroutineController;
        private bool displayingSequence;
        private ColorPosition[] currentSequence;
        public bool AllowPlayerPlace {get => !displayingSequence;}
        public void onClick()
        {
            bool started = coroutineController != null;
            if (!started) {
                if (getChunk() is not ILoadedChunk loadedChunk) {
                    return;
                }
                initTiles();
                GameObject controllerObject = new GameObject();
                controllerObject.name = "SimonSaysController";
                coroutineController = controllerObject.AddComponent<SimonSaysCoroutineController>();
                coroutineController.init(coloredTiles);
                TileEntityHelper.setParentOfSpawnedObject(controllerObject, loadedChunk);
                currentSequence = generateSequence(32);
                coroutineController.display(currentSequence);
                
            }
        }

        private void initTiles() {
            coloredTiles = new List<SimonSaysColoredTileEntity>();
            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    if (x == 0 && y == 0) {
                        continue;
                    }
                    TileEntity tileEntity = TileEntityHelper.getAdjacentTileEntity(this,new Vector2Int(x,y));
                    if (tileEntity == null) {
                        continue;
                    }
                    if (tileEntity is not SimonSaysColoredTileEntity simonSaysColoredTile) {
                        continue;
                    }
                    coloredTiles.Add(simonSaysColoredTile);
                }
            }
            Debug.Log(coloredTiles.Count);
        }
        private ColorPosition[] generateSequence(int depth) {
            if (depth <= 0) {
                Debug.LogError(name +  " Tried to generate sequence with <= 0 depth");
                return null;
            }
            ColorPosition[] sequence = new ColorPosition[depth];
            for (int i = 0; i < depth; i++) {
                int color = Random.Range(1,5);
                int position = Random.Range(0,coloredTiles.Count);
                sequence[i] = new ColorPosition(color,position);
            }
            return sequence;
        }

        

        public void load()
        {
            // No inital load
        }

        public void unload()
        {
            if (coroutineController != null) {
                GameObject.Destroy(coroutineController);
            }
        }
    }

    public class ColorPosition {
        public ColorPosition(int color, int position) {
            this.color = color;
            this.position = position;
        }
        public int color;
        public int position;
    }
    public enum SimonSaysColor {
        White,
        Red,
        Green,
        Blue,
        Yellow
    }
}

