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
        protected ClosedChunkSystem activeSystem;
        [SerializeField] public PlayerIO playerIO;
        private int playerXChunk;
        private int playerYChunk;   
        private int previousPlayerXChunk;
        private int previousPlayerYChunk;
        private int playerXPartition;
        private int playerYPartition;
        private int previousPlayerXPartition;
        private int previousPlayerYPartition;
        protected DimController currentDimension;
        protected int dim;
        public int Dim {get => dim;}
        public DimController CurrentDimension { get => currentDimension; set => currentDimension = value; }
        public ClosedChunkSystem ActiveSystem { get => activeSystem; set => activeSystem = value; }
        public abstract void Start();
        public void Update()
        {
            if (ActiveSystem == null) {
                return;
            }
            Transform playerTransform = playerIO.transform;
            previousPlayerXChunk = playerXChunk;
            previousPlayerYChunk = playerYChunk;
            playerXChunk = (int) Mathf.Floor(playerTransform.position.x / (Global.PartitionsPerChunk/2));
            playerYChunk = (int) Mathf.Floor(playerTransform.position.y / (Global.PartitionsPerChunk/2));
            
            if (previousPlayerXChunk != playerXChunk || previousPlayerYChunk != playerYChunk) {
                ActiveSystem.playerChunkUpdate();
            }

            previousPlayerXPartition = playerXPartition;
            previousPlayerYPartition = playerYPartition;
            playerXPartition = (int) Mathf.Floor(playerTransform.position.x) / (Global.ChunkPartitionSize/2) % Global.PartitionsPerChunk;
            playerYPartition = (int) Mathf.Floor(playerTransform.position.y) / (Global.ChunkPartitionSize/2) % Global.PartitionsPerChunk;
            if (previousPlayerXPartition != playerXPartition || previousPlayerYPartition != playerYPartition) {
                ActiveSystem.playerPartitionUpdate();
            }
        }
        public ClosedChunkSystem GetClosedChunkSystem() {
            return ActiveSystem;
        }

        public async Task setActiveSystemFromCellPosition(int dim, Vector2Int cellPosition) {
            this.dim = dim;
            currentDimension = getCurrentController();
            ClosedChunkSystem newSystem = null;
            if (currentDimension is ISingleSystemController singleSystemController) {
                newSystem = singleSystemController.getSystem();
            } else if (currentDimension is IMultipleSystemController multipleSystemController) {
                newSystem = multipleSystemController.getSystemFromCellPositon(cellPosition);
            }
            if (newSystem == null) {
                return;
            }
            await activateSystem(newSystem);
        }
        private async Task activateSystem(ClosedChunkSystem newSystem) {
            
            await resetMobRegistry(newSystem);
            if (activeSystem != null) {
                activeSystem.deactivateAllPartitions();
                GameObject.Destroy(activeSystem.gameObject);
            }
            activeSystem = newSystem;
            
        }

        private async Task resetMobRegistry(ClosedChunkSystem system) {
            EntityRegistry entityRegistry = EntityRegistry.getInstance();
            entityRegistry.reset();
            await entityRegistry.cacheFromSystem(system);
        }

        public abstract DimController getCurrentController();

        public void setPlayerPosition(Vector2 position) {
            playerIO.transform.position = position;
            activeSystem.playerPartitionUpdate();
        }
        public void setPlayerPositionFromCell(Vector2Int cellPosition) {
            playerIO.transform.position = new Vector2(cellPosition.x/2f,cellPosition.y/2f);
            activeSystem.playerPartitionUpdate();
        }
    }
}

