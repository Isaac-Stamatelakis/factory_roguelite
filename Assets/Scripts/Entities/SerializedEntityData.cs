using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities {
    public enum EntityType {
        Item,
        Mob
    }
    public class SeralizedEntityData {
        public EntityType type;
        public float x;
        public float y;
        public string data;

        public SeralizedEntityData(EntityType type, Vector2 position, string data)
        {
            this.type = type;
            this.x = position.x;
            this.y = position.y;
            this.data = data;
        }
    }
}

