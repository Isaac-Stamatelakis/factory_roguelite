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
using Conduits.Ports;
using Item.Slot;
using TileMaps;
using TileMaps.Type;
using UI.Chat;

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

        public void OnRightClick()
        {
            
            bool started = !ReferenceEquals(coroutineController, null);
            if (started || chunk is not ILoadedChunk loadedChunk) return;
            TextChatUI.Instance.sendMessage("Another challenger... Very well... Repeat the pattern and I should rework you...");
            InitTiles();
            currentChances = TileEntityObject.Chances;
            GameObject controllerObject = new GameObject();
            controllerObject.name = "SimonSaysController";
            coroutineController = controllerObject.AddComponent<SimonSaysCoroutineController>();
            coroutineController.init(this);
            controllerObject.transform.SetParent(loadedChunk.getTileEntityContainer(),false);
            InitGame();
        }

        public void EvaluateSequence() {
            for (int i = 0; i < playerSequence.Count; i++) {
                if (playerSequence[i] != currentSequence[i]) {
                    Lose();
                    return;
                }
            }
            if (playerSequence.Count >= currentSequence.Count) {
                SequenceMatch();
            }
            
        }

        private void InitGame() {
            playerSequence = new List<int>();
            currentSequence = new List<int>();
            increaseCurrentSequenceLength(2);
            coroutineController.display(currentSequence);
        }

        private void SequenceMatch() {
            highestMatchingSequence = Mathf.Max(highestMatchingSequence,playerSequence.Count);
            if (highestMatchingSequence >= TileEntityObject.MaxLength) {
                Conclude();
                return;
            }
            playerSequence = new List<int>();
            increaseCurrentSequenceLength(1);
            coroutineController.display(currentSequence);
        }
        private void Lose() {
            
            currentChances--;
            if (currentChances > 0) {
                TextChatUI.Instance.sendMessage("Oof wrong tile...");
                InitGame();
            } else {
                Conclude();
            }
            
        }

        /// <summary>
        /// Removes this simon says controller, replaces it with other blocks, spawns loot
        /// </summary>
        private void Conclude() {
            TextChatUI.Instance.sendMessage($"The game has concluded. Your highest matching sequence was {highestMatchingSequence}");
            int lootAmount = (highestMatchingSequence*4)/TileEntityObject.MaxLength;
            
            if (chunk is not ILoadedChunk loadedChunk) return;
                
           
            ClosedChunkSystem closedChunkSystem = loadedChunk.getSystem();
            IWorldTileMap blockTileMap = closedChunkSystem.GetTileMap(TileMapType.Block);
            
            BreakStructure(blockTileMap,closedChunkSystem);
            PlaceBricks(blockTileMap,closedChunkSystem);
            PlaceChests(lootAmount, closedChunkSystem);
            
            Unload();
        }

        private void BreakStructure(IWorldTileMap blockTileMap, ClosedChunkSystem closedChunkSystem)
        {
            const int RADIUS = 1;
            for (int x = -RADIUS; x <= RADIUS; x++)
            {
                for (int y = -RADIUS; y <= RADIUS; y++)
                {
                    blockTileMap.BreakTile(getCellPosition() + new Vector2Int(x,y));
                }
            }
        }

        private void PlaceBricks(IWorldTileMap blockTileMap, ClosedChunkSystem closedChunkSystem)
        {
            List<Vector2Int> brickPlacePositions = new List<Vector2Int>{
                new Vector2Int(-1,-1),
                new Vector2Int(-1,1),
                new Vector2Int(1,-1),
                new Vector2Int(1,1),
                Vector2Int.zero
            };
            foreach (Vector2Int position in brickPlacePositions) {
                Vector2 worldPlacePosition = getWorldPosition() + new Vector2(position.x / 2f, position.y / 2f); 
                PlaceTile.placeTile(tileEntityObject.BrickTile, worldPlacePosition, blockTileMap, closedChunkSystem);
            }
        }

        private void PlaceChests(int lootAmount, ClosedChunkSystem closedChunkSystem)
        {
            var chestPlacePositions = new List<Vector2Int>
            {
                Vector2Int.left,
                Vector2Int.right,
                Vector2Int.up,
                Vector2Int.down,
            };
            TileItem chestTile = tileEntityObject.ChestTile;
            IWorldTileMap chestLayerTileMap = closedChunkSystem.GetTileMap(chestTile.tileType.toTileMapType());
            for (var index = 0; index < chestPlacePositions.Count; index++)
            {
                var position = chestPlacePositions[index];
                if (index >= lootAmount)
                {
                    return;
                }

                Vector2Int chestPlacePosition = getCellPosition() + position;
                Vector2Int chestPositionInChunk = Global.getPositionInChunk(chestPlacePosition);
                if (ReferenceEquals(tileEntityObject.ChestTile?.tileEntity, null))
                {
                    if (ReferenceEquals(tileEntityObject.ChestTile, null))
                    {
                        Debug.LogError($"{tileEntityObject.name} chest tile is null");
                    } else if (ReferenceEquals(tileEntityObject.ChestTile.tileEntity, null))
                    {
                        Debug.LogError($"{tileEntityObject.name} chest tile is has no tile entity");
                    }
                    return;
                }

                Vector2Int chestChunkPosition = Global.getChunkFromCell(chestPlacePosition);
                IChunk chestChunk = closedChunkSystem.getChunk(chestChunkPosition);
                ITileEntityInstance tileEntityInstance = TileEntityUtils.placeTileEntity(chestTile, chestPositionInChunk, chestChunk, true);
                if (tileEntityInstance is not IItemConduitInteractable itemConduitInteractable)
                {
                    Debug.LogError($"{tileEntityObject.name} chest tile is not item interactable");
                    return;
                }

                List<ItemSlot> loot = LootTableUtils.OpenWithAmount(TileEntityObject.LootTable, 3);
                if (index == chestPlacePositions.Count - 1)
                {
                    loot.AddRange(LootTableUtils.OpenWithAmount(TileEntityObject.CompletionLootTable, 1));
                }

                foreach (ItemSlot itemSlot in loot)
                {
                    itemConduitInteractable.InsertItem(ItemState.Solid, itemSlot, Vector2Int.zero);
                }

                Vector2 worldPlacePosition = getWorldPosition() + new Vector2(position.x / 2f, position.y / 2f);
                PlaceTile.placeTile(chestTile, worldPlacePosition, chestLayerTileMap, closedChunkSystem, tileEntityInstance);
            }
        }

        private void InitTiles() {
            coloredTiles = new List<SimonSaysColoredTileEntityInstance>();
            IChunkSystem system = chunk.GetChunkSystem();
            const int RADIUS = 1;
            for (int x = -RADIUS; x <= RADIUS; x++) {
                for (int y = -RADIUS; y <= RADIUS; y++)
                {
                    bool isController = x == 0 && y == 0;
                    if (isController) continue;
                    var (partition, positionInPartition) =
                        system.GetPartitionAndPositionAtCellPosition(new Vector2Int(x, y) + getCellPosition());
                    ITileEntityInstance tileEntityInstance = partition.GetTileEntity(positionInPartition);
                    if (tileEntityInstance is not SimonSaysColoredTileEntityInstance simonSaysColoredTile) {
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

        

        public void Load()
        {
            // No inital load
        }

        public void Unload()
        {
            if (!ReferenceEquals(coroutineController,null)) {
                GameObject.Destroy(coroutineController);
            }
        }

    
    }
}

