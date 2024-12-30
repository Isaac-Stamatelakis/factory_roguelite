using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntity;
using Newtonsoft.Json;

namespace Conduits.Ports {
    [System.Serializable]
    public enum EntityPortType {
        All,
        Input,
        Output,
        None
    }
    [System.Serializable]
    public class TileEntityPortData {
        
        public EntityPortType portType;
        public Vector2Int position;
        public TileEntityPortData(EntityPortType type, Vector2Int position) {
            this.portType = type;
            this.position = position;
        }
    }
}
