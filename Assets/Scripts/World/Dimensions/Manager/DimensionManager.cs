using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerModule.IO;
using WorldModule;
using Chunks.ClosedChunkSystemModule;
using Entities.Mobs;
using System.Threading.Tasks;

namespace Dimensions {
    public interface ICompactMachineDimManager {
        public CompactMachineDimController GetCompactMachineDimController();
    }
    public abstract class DimensionManager : MonoBehaviour
    {
        private static DimensionManager instance;
        public static DimensionManager Instance {get => instance;}
        public void Awake() {
            instance = this;
        }
        protected DimController currentDimension;
        public DimController CurrentDimension { get => currentDimension; set => currentDimension = value; }
        private Dictionary<PlayerIO, PlayerWorldData> playerWorldData = new Dictionary<PlayerIO, PlayerWorldData>(); 
        private Dictionary<ClosedChunkSystem, Vector2Int> activeSystems = new Dictionary<ClosedChunkSystem, Vector2Int>();
        public void Start() {
            PlayerIO[] players = GameObject.FindObjectsOfType<PlayerIO>();
            foreach (PlayerIO player in players) {
                DimController dimController = getDimController(0);
                playerWorldData[player] = new PlayerWorldData(null,null,null);
                setPlayerSystem(player,0,Vector2Int.right);
            }
        }

        public void Update()
        {
            foreach (KeyValuePair<PlayerIO,PlayerWorldData> kvp in playerWorldData) {
                PlayerWorldData worldData = playerWorldData[kvp.Key];
                Transform playerTransform = kvp.Key.transform;

                int playerXPartition = (int) Mathf.Floor(playerTransform.position.x) / (Global.ChunkPartitionSize/2) % Global.PartitionsPerChunk;
                int playerYPartition = (int) Mathf.Floor(playerTransform.position.y) / (Global.ChunkPartitionSize/2) % Global.PartitionsPerChunk;
                if (worldData.partitionPos != null) {
                    Vector2Int partition = (Vector2Int)worldData.partitionPos;
                    if (playerXPartition == partition.x && playerYPartition == partition.y) {
                        return;
                    }
                }
                
                worldData.closedChunkSystem.playerPartitionUpdate();
                worldData.partitionPos = new Vector2Int(playerXPartition,playerYPartition);
                

                int playerXChunk = (int) Mathf.Floor(playerTransform.position.x / (Global.PartitionsPerChunk/2));
                int playerYChunk = (int) Mathf.Floor(playerTransform.position.y / (Global.PartitionsPerChunk/2));
                
                if (worldData.chunkPos != null) {
                    Vector2Int chunk = (Vector2Int)worldData.chunkPos;
                    if (playerXChunk == chunk.x && playerYChunk == chunk.y) {
                        return;
                    }
                }
                
                worldData.closedChunkSystem.playerChunkUpdate();
                worldData.chunkPos = new Vector2Int(playerXChunk,playerYChunk);
                
            }
        }
        public ClosedChunkSystem getPlayerSystem(PlayerIO playerIO) {
            if (!playerWorldData.ContainsKey(playerIO)) {
                return null;
            }
            return playerWorldData[playerIO].closedChunkSystem;
        }
        public int getPlayerDimension(PlayerIO playerIO) {
            if (playerWorldData.ContainsKey(playerIO)) {
                return playerWorldData[playerIO].closedChunkSystem.Dim;
            }
            Debug.LogError($"Player {playerIO.name} was not in playerDimension Dict");
            return 0;
        }
        public void setPlayerSystem(PlayerIO player, int dim, Vector2Int cellPosition) {
            DimController controller = getDimController(dim);
            ClosedChunkSystem activeSystem = null;
            if (controller is ISingleSystemController singleSystemController) {
                activeSystem = singleSystemController.getActiveSystem();
                if (activeSystem == null) {
                    //Vector2Int systemPosition = getNextSystemPosition();
                    Vector2Int systemPosition = Vector2Int.left;
                    Vector2Int offset = systemPosition*DimensionUtils.ACTIVE_SYSTEM_SIZE;
                    activeSystem = singleSystemController.activateSystem(offset);
                    activeSystems[activeSystem] = systemPosition;
                }
            } else if (controller is IMultipleSystemController multipleSystemController) {

            }

            if (activeSystem == null) {
                Debug.LogError("Could not switch player system");
                return;
            }
            if (playerWorldData.ContainsKey(player)) {
                ClosedChunkSystem previousSystem = playerWorldData[player].closedChunkSystem;
                playerWorldData[player].closedChunkSystem = activeSystem;
                bool systemEmpty = true;
                if (previousSystem != null) {
                    foreach (KeyValuePair<PlayerIO,PlayerWorldData> kvp in playerWorldData) {
                        if (previousSystem.Equals(kvp.Value.closedChunkSystem)) {
                            systemEmpty = false;
                            break;
                        }
                    }
                    if (systemEmpty) {
                        activeSystem.deactivateAllPartitions();
                        GameObject.Destroy(activeSystem.gameObject);
                    }
                }
            }
            Vector2Int systemOffset = activeSystems[activeSystem]*DimensionUtils.ACTIVE_SYSTEM_SIZE;
            playerWorldData[player].chunkPos = null;
            playerWorldData[player].partitionPos = null;
            Vector2Int tpPosition = (cellPosition-systemOffset);
            Vector3 playerPosition = player.transform.position;
            playerPosition.x = tpPosition.x/2f;
            playerPosition.y = tpPosition.y/2f;
            player.transform.position = playerPosition;
        }
        
        protected Vector2Int getNextSystemPosition() {
            int count = activeSystems.Count;
            int x = (count+1) / 2;
            if (count % 2 == 1) {
                x *= -1;
            }
            return new Vector2Int(x,0);
        }

        private async Task resetMobRegistry(ClosedChunkSystem system) {
            EntityRegistry entityRegistry = EntityRegistry.getInstance();
            entityRegistry.reset();
            await entityRegistry.cacheFromSystem(system);
        }

        public abstract DimController getDimController(int dim);

        private class PlayerWorldData {
            public Vector2Int? chunkPos;
            public Vector2Int? partitionPos;
            public ClosedChunkSystem closedChunkSystem;

            public PlayerWorldData(Vector2Int? chunkPos, Vector2Int? partitionPos, ClosedChunkSystem closedChunkSystem)
            {
                this.chunkPos = chunkPos;
                this.partitionPos = partitionPos;
                this.closedChunkSystem = closedChunkSystem;
            }
        }
    }
}

