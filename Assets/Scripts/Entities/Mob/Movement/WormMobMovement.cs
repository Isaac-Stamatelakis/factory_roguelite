using System;
using Entities.Mob.Display;
using UnityEngine;

namespace Entities.Mob.Movement
{
    public class WormMobMovement : MonoBehaviour
    {
        private float radius;
        private WormBodyController wormBodyController;

        public void Start()
        {
            wormBodyController = GetComponent<WormBodyController>();
            if (!wormBodyController) Debug.LogError($"Worm Body Controller not found in entity '{name}'");
        }
        
    }
}
