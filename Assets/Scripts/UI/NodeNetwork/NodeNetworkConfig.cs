using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.NodeNetwork {
    public static class NodeNetworkConfig
    {
        private static float minScale = 0.35f;
        private static float maxScale = 3f;
        private static float zoomSpeed = 0.3f;
        private static float gridSize = 64f;
        public static float MINSCALE {get => minScale;}
        public static float MAXSCALE {get => maxScale;}
        public static float ZOOMSPEED {get => zoomSpeed;}
        public static float GRIDSIZE {get => gridSize;}
    }
}

