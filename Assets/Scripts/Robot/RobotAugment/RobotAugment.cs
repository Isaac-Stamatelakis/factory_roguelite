using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items;

namespace RobotModule {
    public class RobotAugment : ScriptableObject, IRobotAugment
    {
        [SerializeField] public RobotAugmentType augmentType;
        [SerializeField] public ItemObject item;
    }
}

