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
        [SerializeField] public int maxLength;
        [SerializeField] public int chances;
        [SerializeField] public LootTable lootTable;
        [Header("Extra rewards for successfully finishing the puzzle")]
        [SerializeField] public LootTable completionLootTable;
        private List<SimonSaysColoredTileEntity> coloredTiles;
        public List<SimonSaysColoredTileEntity> ColoredTiles {get => coloredTiles;}
        private SimonSaysCoroutineController coroutineController;
        private bool displayingSequence;
        private int highestMatchingSequence;
        private int currentChances;
        private List<int> currentSequence;
        private List<int> playerSequence;
        public bool AllowPlayerPlace {get => !DisplayingSequence;}
        public SimonSaysCoroutineController CoroutineController { get => coroutineController; set => coroutineController = value; }
        public bool DisplayingSequence { get => displayingSequence; set => displayingSequence = value; }
        public List<int> PlayerSequence { get => playerSequence; set => playerSequence = value; }

        public void onClick()
        {
            bool started = coroutineController != null;
            if (!started) {
                if (getChunk() is not ILoadedChunk loadedChunk) {
                    return;
                }
                initTiles();
                currentChances = chances;
                GameObject controllerObject = new GameObject();
                controllerObject.name = "SimonSaysController";
                coroutineController = controllerObject.AddComponent<SimonSaysCoroutineController>();
                coroutineController.init(this);
                TileEntityHelper.setParentOfSpawnedObject(controllerObject, loadedChunk);
                initGame();
            }
        }

        public void evaluateSequence() {
            for (int i = 0; i < playerSequence.Count; i++) {
                if (playerSequence[i] != currentSequence[i]) {
                    lose();
                    return;
                }
            }
            if (playerSequence.Count >= currentSequence.Count) {
                sequenceMatch();
            }
            
        }

        private void initGame() {
            playerSequence = new List<int>();
            currentSequence = new List<int>();
            increaseCurrentSequenceLength(2);
            coroutineController.display(currentSequence);
        }

        private void sequenceMatch() {
            if (highestMatchingSequence == maxLength) {
                win();
                return;
            }
            playerSequence = new List<int>();
            increaseCurrentSequenceLength(1);
            coroutineController.display(currentSequence);
        }
        private void win() {
            
        }
        private void lose() {
            highestMatchingSequence = Mathf.Max(highestMatchingSequence,playerSequence.Count);
            chances--;
            if (chances > 0) {
                initGame();
                return;
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
                    simonSaysColoredTile.Controller=this;
                }
            }
        }

        private void increaseCurrentSequenceLength(int amount) {
            for (int i = 0; i < amount; i++) {
                currentSequence.Add(Random.Range(0,coloredTiles.Count));
            }
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

}

