using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Newtonsoft.Json;
using Chunks;
using Items;
using Tiles;
using TileMaps.Place;
using Chunks.Systems;
using Chunks.Partitions;

namespace TileEntity.Instances.SimonSays {
    public class SimonSaysControllerInstance : TileEntityInstance<SimonSaysController>, IRightClickableTileEntity, ILoadableTileEntity
    {
        public SimonSaysControllerInstance(SimonSaysController tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }
        private List<SimonSaysColoredTileEntityInstance> coloredTiles;
        public List<SimonSaysColoredTileEntityInstance> ColoredTiles {get => coloredTiles;}
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

        public void onRightClick()
        {
            bool started = coroutineController != null;
            if (!started) {
                if (getChunk() is not ILoadedChunk loadedChunk) {
                    return;
                }
                initTiles();
                currentChances = TileEntityObject.Chances;
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
            highestMatchingSequence = Mathf.Max(highestMatchingSequence,playerSequence.Count);
            if (highestMatchingSequence >= TileEntityObject.MaxLength) {
                conclude();
                return;
            }
            playerSequence = new List<int>();
            increaseCurrentSequenceLength(1);
            coroutineController.display(currentSequence);
        }
        private void lose() {
            currentChances--;
            if (currentChances > 0) {
                initGame();
            } else {
                conclude();
            }
            
        }

        /// <summary>
        /// Removes this simon says controller, replaces it with other blocks, spawns loot
        /// </summary>
        private void conclude() {
            int lootamount = (highestMatchingSequence*4)/TileEntityObject.MaxLength;
            List<Vector2Int> brickPlacePositions = new List<Vector2Int>{
                new Vector2Int(-1,-1),
                new Vector2Int(-1,1),
                new Vector2Int(1,-1),
                new Vector2Int(1,1),
                Vector2Int.zero
            };
            TileItem bricks = ItemRegistry.GetInstance().GetTileItem("simons_brick");
            if (chunk is not ILoadedChunk loadedChunk) {
                Debug.LogError("Somehow managed to get to deletion phase of simon says game in a non loaded chunk");
                return;
            }
            ClosedChunkSystem closedChunkSystem = loadedChunk.getSystem();
            foreach (Vector2Int position in brickPlacePositions) {
                Vector2 worldPlacePosition = getWorldPosition() + new Vector2(position.x/2f,position.y/2f);
                PlaceTile.PlaceFromWorldPosition(bricks,worldPlacePosition,closedChunkSystem,checkConditions: false,useOffset:false);
            }
            List<Vector2Int> chestPlacePositions = new List<Vector2Int>{
                new Vector2Int(-1,0),
                new Vector2Int(1,0),
                new Vector2Int(0,-1),
                new Vector2Int(0,1)
            };
            TileItem chestTile = ItemRegistry.GetInstance().GetTileItem("small_chest");
            int count = 0;
            foreach (Vector2Int position in chestPlacePositions) {
                if (count >= lootamount) {
                    break;
                }
                TileEntityObject chestTileEntityObject = chestTile.tileEntity;
                if (chestTileEntityObject is not Chest chest) {
                    Debug.LogError("SimonSaysController attempted to give items to a non chest tile entity");
                    break;
                }
                List<ItemSlot> loot = LootTableHelper.openWithAmount(TileEntityObject.LootTable,3);
                if (count == 3) {
                    loot.AddRange(LootTableHelper.openWithAmount(TileEntityObject.CompletionLootTable,1));
                }
                Vector2Int cellPosition = getCellPosition() + position;
                Vector2Int chestPositionInChunk = Global.getPositionInChunk(cellPosition);
                //chestTileEntity.initalize(chestPositionInChunk, chestTile.tile, chunk);
                //chest.giveItems(loot);
                Vector2 worldPlacePosition = getWorldPosition() + new Vector2(position.x/2f,position.y/2f);
                PlaceTile.PlaceFromWorldPosition(chestTile,worldPlacePosition,closedChunkSystem,checkConditions:false,chestTileEntityObject,useOffset:false);
                //IChunkPartition partition = chunk.getPartition()
                count ++;
            }
            unload();
        }

        private void initTiles() {
            coloredTiles = new List<SimonSaysColoredTileEntityInstance>();
            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    if (x == 0 && y == 0) {
                        continue;
                    }
                    ITileEntityInstance tileEntity = TileEntityHelper.getAdjacentTileEntity(this,new Vector2Int(x,y));
                    if (tileEntity == null) {
                        continue;
                    }
                    if (tileEntity is not SimonSaysColoredTileEntityInstance simonSaysColoredTile) {
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

