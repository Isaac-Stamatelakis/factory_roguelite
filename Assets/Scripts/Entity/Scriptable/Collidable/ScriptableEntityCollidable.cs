using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entitys.Scriptable {
    public class ScriptableEntityCollidable : ScriptableEntityObject
    {
        [SerializeField] private int damage;
        public int Damage {get => damage;}
    }
}

