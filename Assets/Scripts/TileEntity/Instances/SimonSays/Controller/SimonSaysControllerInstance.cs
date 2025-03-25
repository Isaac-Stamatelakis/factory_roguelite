using System;
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
using Conduits;
using Conduits.Ports;
using Item.Slot;
using TileMaps;
using TileMaps.Type;
using UI.Chat;
using Random = UnityEngine.Random;

namespace TileEntity.Instances.SimonSays {
    public class SimonSaysControllerInstance : TileEntityInstance<SimonSaysController>, IConditionalRightClickableTileEntity, ILoadableTileEntity, ISerializableTileEntity
    {
        
        public SimonSaysControllerInstance(SimonSaysController tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        private enum ConclusionState
        {
            Standard,
            MatchedAll,
            Cheater
        }
        private const int INITIAL_ATTEMPTS = 3;
        private const int SPECIAL_MESSAGE_ATTEMPT = -1;
        private List<SimonSaysColoredTileEntityInstance> coloredTiles;
        public List<SimonSaysColoredTileEntityInstance> ColoredTiles {get => coloredTiles;}
        private SimonSaysCoroutineController coroutineController;
        
        private List<int> currentSequence;
        private List<int> playerSequence;
        public SimonSaysCoroutineController CoroutineController { get => coroutineController; set => coroutineController = value; }
        public bool PlayingSequence;
        public bool Restarting;
        public List<int> PlayerSequence { get => playerSequence; set => playerSequence = value; }
        private SimonSaysData simonSaysData;
        private const string CHAT_NAME = "<color=purple>Unknown Entity</color>";

        public void OnRightClick()
        {
            bool started = !ReferenceEquals(coroutineController, null);
            if (started || chunk is not ILoadedChunk loadedChunk) return;
            

            TextChatUI.Instance.SendChatMessage(simonSaysData == null
                ? "Another challenger... Very well... Repeat the pattern and I should rework you..."
                : "A familiar face... Next time don't run away...", CHAT_NAME);
            simonSaysData ??= new SimonSaysData(INITIAL_ATTEMPTS,0);
            InitTiles();
            GameObject controllerObject = new GameObject();
            controllerObject.name = "SimonSaysController";
            coroutineController = controllerObject.AddComponent<SimonSaysCoroutineController>();
            coroutineController.init(this);
            controllerObject.transform.SetParent(loadedChunk.GetTileEntityContainer(),false);
            InitGame();
        }

        public bool CanRightClick()
        {
            return ReferenceEquals(coroutineController, null);
        }

        public bool EvaluateSequence() {
            for (int i = 0; i < playerSequence.Count; i++) {
                if (playerSequence[i] != currentSequence[i]) {
                    Lose();
                    return false;
                }
            }
            if (playerSequence.Count >= currentSequence.Count) {
                SequenceMatch();
            }

            return true;
        }

        public void InitGame()
        {
            if (simonSaysData.Attempts < 1)
            {
                simonSaysData.Attempts = SPECIAL_MESSAGE_ATTEMPT;
                Conclude(ConclusionState.Cheater);
                return;
            } 
            Restarting = false;
            playerSequence = new List<int>();
            currentSequence = new List<int>();
            IncreaseCurrentSequenceLength(2);
            coroutineController.Display(currentSequence);
        }

        public void SequenceMatch() {
            if (playerSequence.Count > simonSaysData.HighestMatchingSequence)
            {
                simonSaysData.HighestMatchingSequence = playerSequence.Count;
            }
            
            if (playerSequence.Count >= TileEntityObject.MaxLength)
            {
                Conclude(ConclusionState.MatchedAll);
                return;
            }
            playerSequence = new List<int>();
            IncreaseCurrentSequenceLength(1);
            coroutineController.Display(currentSequence);
        }
        public void Lose()
        {
            simonSaysData.Attempts--;
            if (simonSaysData.Attempts > 0) {
                Restarting = true;
                TextChatUI.Instance.SendChatMessage("Oof wrong tile...",CHAT_NAME);
                coroutineController.RestartGame();
            } else {
                Conclude(ConclusionState.Standard);
            }
            
        }

        private string GetConclusionText(ConclusionState state)
        {
            switch (state)
            {
                case ConclusionState.Standard:
                    return $"The game has concluded. Your highest matching sequence was {simonSaysData.HighestMatchingSequence}";
                case ConclusionState.MatchedAll:
                    return $"Congratulations! You bested me and for that I shall grant you a special reward";
                case ConclusionState.Cheater:
                    return $"Nice Try... My memory isn't that bad... Next time don't run away from the challenge...";
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
        /// <summary>
        /// Removes this simon says controller, replaces it with other blocks, spawns loot
        /// </summary>
        private void Conclude(ConclusionState state) {
            TextChatUI.Instance.SendChatMessage(GetConclusionText(state),CHAT_NAME);
            int lootAmount = (simonSaysData.HighestMatchingSequence*4)/TileEntityObject.MaxLength;
            
            if (chunk is not ILoadedChunk loadedChunk) return;
                
           
            ClosedChunkSystem closedChunkSystem = loadedChunk.GetSystem();
            IWorldTileMap blockTileMap = closedChunkSystem.GetTileMap(TileMapType.Block);
            
            BreakStructure(blockTileMap,closedChunkSystem);
            PlaceBricks(blockTileMap,closedChunkSystem);
            PlaceChests(lootAmount, closedChunkSystem);
            
            DeActiveGame();
        }

        private void BreakStructure(IWorldTileMap blockTileMap, ClosedChunkSystem closedChunkSystem)
        {
            const int RADIUS = 1;
            for (int x = -RADIUS; x <= RADIUS; x++)
            {
                for (int y = -RADIUS; y <= RADIUS; y++)
                {
                    blockTileMap.BreakTile(GetCellPosition() + new Vector2Int(x,y));
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
                Vector2 worldPlacePosition = GetWorldPosition() + new Vector2(position.x / 2f, position.y / 2f); 
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

                Vector2Int chestPlacePosition = GetCellPosition() + position;
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
                IChunk chestChunk = closedChunkSystem.GetChunk(chestChunkPosition);
                ITileEntityInstance tileEntityInstance = TileEntityUtils.placeTileEntity(chestTile, chestPositionInChunk, chestChunk, true);
                if (ConduitFactory.GetInteractableFromTileEntity(tileEntityInstance, ConduitType.Item) is not IItemConduitInteractable itemConduitInteractable)
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
                    if (ItemSlotUtils.IsItemSlotNull(itemSlot)) continue;
                    itemConduitInteractable.InsertItem(ItemState.Solid, itemSlot, Vector2Int.zero);
                }

                Vector2 worldPlacePosition = GetWorldPosition() + new Vector2(position.x / 2f, position.y / 2f);
                PlaceTile.placeTile(chestTile, worldPlacePosition, chestLayerTileMap, closedChunkSystem, tileEntityInstance);
            }
        }
        
        

        private void InitTiles() {
            coloredTiles = new List<SimonSaysColoredTileEntityInstance>();
            ILoadedChunkSystem system = chunk.GetChunkSystem();
            const int RADIUS = 1;
            for (int x = -RADIUS; x <= RADIUS; x++) {
                for (int y = -RADIUS; y <= RADIUS; y++)
                {
                    bool isController = x == 0 && y == 0;
                    if (isController) continue;
                    var (partition, positionInPartition) =
                        system.GetPartitionAndPositionAtCellPosition(new Vector2Int(x, y) + GetCellPosition());
                    ITileEntityInstance tileEntityInstance = partition.GetTileEntity(positionInPartition);
                    if (tileEntityInstance is not SimonSaysColoredTileEntityInstance simonSaysColoredTile) {
                        continue;
                    }
                    coloredTiles.Add(simonSaysColoredTile);
                    simonSaysColoredTile.Controller=this;
                }
            }
        }

        private void IncreaseCurrentSequenceLength(int amount) {
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
            bool activated = !ReferenceEquals(coroutineController,null);
            if (simonSaysData != null)
            {
                if (simonSaysData.Attempts > 0 && activated) TextChatUI.Instance.SendChatMessage("Leaving so soon?");
                simonSaysData.Attempts--;
            }

            DeActiveGame();
        }

        private void DeActiveGame()
        {
            if (ReferenceEquals(coroutineController, null)) return;
            GameObject.Destroy(coroutineController);
        }


        public string Serialize()
        {
            return JsonConvert.SerializeObject(simonSaysData);
        }

        public void Unserialize(string data)
        {
            simonSaysData = JsonConvert.DeserializeObject<SimonSaysData>(data);
        }

        private class SimonSaysData
        {
            public int HighestMatchingSequence;
            public int Attempts;

            public SimonSaysData(int attempts, int highestMatchingSequence)
            {
                Attempts = attempts;
                HighestMatchingSequence = highestMatchingSequence;
            }
        }
    }
}

