using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using PlayerModule;
using Robot;

namespace RobotModule.Instances {
    /// <summary>
    /// The starter robot
    /// <summary>   
    [CreateAssetMenu(fileName = "RB~New Happy", menuName = "Robots/HAPPY")]
    public class HAPPY : RobotObject, IEnergyRechargeRobot
    {
        public ulong RechargeRate = 8;
        public ulong EnergyRechargeRate => RechargeRate;
    }
}

