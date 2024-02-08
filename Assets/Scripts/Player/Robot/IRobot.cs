using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobotModule {
    public interface IRobot {
        public void handleMovement(Transform playerTransform);
        public void init(GameObject playerGameObject);
    }
}
