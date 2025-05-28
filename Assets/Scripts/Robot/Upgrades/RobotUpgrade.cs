using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DevTools;
using Newtonsoft.Json;
using Player.Tool;
using Robot.Upgrades.LoadOut;
using RobotModule;
using TMPro;
using UI.NodeNetwork;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using WorldModule;

namespace Robot.Upgrades {
    

    

    internal enum RobotUpgrade
    {
        Speed = 0,
        JumpHeight = 1,
        BonusJump = 2,
        RocketBoots = 3,
        Flight = 4,
        Reach = 5,
        Dash = 6,
        Hover = 7,
        Teleport = 8,
        Light = 9,
        NightVision = 10,
        Health = 11,
        Energy = 12,
        NanoBots = 13,
    }
    
    internal enum RobotDrillUpgrade
    {
        Speed = 0,
        Fortune = 1,
        MultiBreak = 2,
        VeinMine = 3,
        Tier = 4,
        Item_Magnet = 5,
    }
    
    

    internal enum ConduitSlicerUpgrade
    {
        VeinMine = 0,
        Item_Magnet = 1,
    }

    internal enum BuildinatorUpgrade
    {
        MultiHit = 0,
    }
    
    
    
    public enum RobotUpgradeType
    {
        Tool = 0,
        Robot = 1,
        
    }

    public enum LaserGunUpgrade
    {
        FireRate = 0,
        MultiShot = 1,
        AoE = 2,
        Knockback = 3
    }
    

    
    

    

    
    
    
}

