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

namespace TileEntityModule.Instances.SimonSays {
    [CreateAssetMenu(fileName = "E~New Simon Says Controller", menuName = "Tile Entity/SimonSays/Controller")]
    public class SimonSaysController : TileEntity
    {
        [Header("The number of simon says sequences which are played")]
        public int MaxLength;
        public int Chances;
        public LootTable LootTable;
        [Header("Extra rewards for successfully finishing the puzzle")]
        public LootTable CompletionLootTable;

        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new SimonSaysControllerInstance(this,tilePosition,tileItem,chunk);
        }
    }

}
