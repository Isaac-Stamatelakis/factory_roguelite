using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DimensionModule;
using PlayerModule.IO;
using WorldModule;
using ChunkModule.ClosedChunkSystemModule;

namespace DimensionModule {
    public enum Dimension {
        OverWorld,
        Cave,
        CompactMachine
    }
    public class DimensionManager : MonoBehaviour
    {
        protected ClosedChunkSystem activeSystem;
        protected Transform playerTransform;
        private int playerXChunk;
        private int playerYChunk;   

        private int previousPlayerXChunk;
        private int previousPlayerYChunk;
        private int playerXPartition;
        private int playerYPartition;
        private int previousPlayerXPartition;
        private int previousPlayerYPartition;
        [SerializeField] public PlayerIO playerIO;
        [SerializeField] public Dim0Controller overworldDimController;
        [SerializeField] public CaveController caveDimController;
        [SerializeField] public CompactMachineDimController compactMachineDimController;
        private DimController currentDimension;
        private int dim;
        public int Dim {get => dim;}
        public DimController CurrentDimension { get => currentDimension; set => currentDimension = value; }
        public ClosedChunkSystem ActiveSystem { get => activeSystem; set => activeSystem = value; }

        public void Start() {
            Debug.Log("Loading world: " + WorldCreation.getWorldPath(Global.WorldName));
            playerTransform = playerIO.transform;
            DimensionManagerContainer.getInstance();
            Vector2Int playerCellPosition = Global.getCellPositionFromWorld(playerIO.getPlayerPosition());
            //setActiveSystemFromCellPosition(playerIO.playerData.dim,playerCellPosition);
            //setPlayerPosition(playerIO.getPlayerPosition());
            setActiveSystemFromCellPosition(0,Vector2Int.zero);
            setPlayerPosition(Vector2.zero);
        }

        
        public virtual void Update()
        {
            if (ActiveSystem == null) {
                return;
            }
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

        public void setActiveSystemFromCellPosition(int dim, Vector2Int cellPosition) {
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
            activateSystem(newSystem);
        }
        private void activateSystem(ClosedChunkSystem newSystem) {
            if (activeSystem != null) {
                activeSystem.deactivateAllPartitions();
                GameObject.Destroy(activeSystem.gameObject);
            }
            activeSystem = newSystem;
            Debug.Log("DimensionManager loaded system " + activeSystem.name);
        }


        public DimController getCurrentController() {
            switch (dim) {
                case 0:
                    return overworldDimController;
                case -1:
                    return caveDimController;
                case 1:
                    return compactMachineDimController;
            }
            return null;
        }

        public void setPlayerPosition(Vector2 position) {
            playerIO.transform.position = position;
            activeSystem.playerPartitionUpdate();
        }
        public void setPlayerPositionFromCell(Vector2Int cellPosition) {
            playerIO.transform.position = new Vector2(cellPosition.x/2f,cellPosition.y/2f);
            activeSystem.playerPartitionUpdate();
        }

        public Dim0Controller getDim0Controller() {
            return overworldDimController;
        }

        public CaveController getCaveController() {
            return caveDimController;
        }

        public CompactMachineDimController GetCompactMachineDimController() {
            return compactMachineDimController;
        }
    }
}

