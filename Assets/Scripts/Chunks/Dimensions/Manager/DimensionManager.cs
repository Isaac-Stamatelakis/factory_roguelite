using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DimensionModule;
using PlayerModule.IO;

namespace DimensionModule {
    public class DimensionManager : MonoBehaviour
    {
        public PlayerIO playerIO;
        public Dim0Controller dim0Controller;
        public CaveController caveController;
        private DimController currentDimension;
        private int dim;
        public DimController CurrentDimension { get => currentDimension; set => currentDimension = value; }

        public void Start() {
            DimensionManagerContainer.getInstance();
            setDim(playerIO.playerData.dim);
        }

        public void setDim(int dim) {
            this.dim = dim;
            if (CurrentDimension != null) {
                CurrentDimension.gameObject.SetActive(false);
            }
            CurrentDimension = getCurrentController();
            CurrentDimension.gameObject.SetActive(true);
            
        }

        public DimController getCurrentController() {
            switch (dim) {
                case 0:
                    return dim0Controller;
                case -1:
                    return caveController;
            }
            return null;
        }
    }
}

