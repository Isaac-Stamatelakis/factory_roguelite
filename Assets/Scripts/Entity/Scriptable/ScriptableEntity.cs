using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entitys.Scriptable {
    public class ScriptableEntity : ScriptableObject
    {
        [SerializeField] private ScriptableEntityMovement movement;
        public ScriptableEntityMovement Movement { get => movement;}
        [SerializeField] private ScriptableEntityBreeding breeding;
        public ScriptableEntityBreeding Breeding { get => breeding;}
        [SerializeField] private ScriptableEntityCollidable collision;
        public ScriptableEntityCollidable Collision {get => collision;}
    }
}

