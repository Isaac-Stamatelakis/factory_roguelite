using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.Scriptable {
    public class ScriptableEntityCollidable : ScriptableEntityObject
    {
        [SerializeField] private int damage;
        public int Damage {get => damage;}
    }
}

