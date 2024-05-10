using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.Scriptable {
    /// <summary>
    /// Inherients behavior from entity object script
    /// </summary>
    public abstract class ScriptableEntityObject : Entity
    {
        private ScriptableEntityMovement movement;
        private ScriptableEntityBreeding breeding;
        private ScriptableEntityCollidable collision;
        
        public void init(ScriptableEntity scriptableEntity) {
            movement = scriptableEntity.Movement;
            breeding = scriptableEntity.Breeding;
            collision = scriptableEntity.Collision;
        }
        public void FixedUpdate() {
            if (movement != null) {
                movement.move(transform);
            }
        }
        public override EntityData GetData()
        {
            throw new System.NotImplementedException();
        }

        public override void initalize()
        {
            throw new System.NotImplementedException();
        }
    }
}

