using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemModule;

namespace Entitys.Scriptable {
    public interface IMovableEntity {
        public ScriptableEntityMovement getMovement();
    }

    public interface IDamageCollidableEntity {
        public int getDamage();
    }

    public interface ISpecialAttackEntity {
        
    }

    public interface IBreedableEntity {
        public void breedUpdate();
        public void breed();
    }

    public interface IItemDropEntity {
        public LootTable getLootTable();
    }
   
}
