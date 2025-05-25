using TileMaps.Layer;
using TileMaps.Type;
using UnityEngine;

namespace Player.Movement
{
    public class PlayerTeleportEvent
    {
        private Vector2 teleportPosition;
        private Bounds playerBounds;
        private Transform playerTransform;
        private const float COOLDOWN = 0.2f;
        private float timeStamp;

        public bool Expired()
        {
            return Time.time - timeStamp >= COOLDOWN;
        }

        public PlayerTeleportEvent(Transform playerTransform, Vector2 teleportPosition, Bounds playerBounds, float time)
        {
            this.teleportPosition = teleportPosition;
            this.playerBounds = playerBounds;
            this.playerTransform = playerTransform;
            this.timeStamp = time;
        }

        public bool TryTeleport()
        {
            int blockLayer = 1 << LayerMask.NameToLayer(TileMapType.Block.ToString());
            Vector2 castSize = playerBounds.extents;
            var cast = Physics2D.BoxCast(teleportPosition, new Vector2(castSize.x, castSize.y), 0f, Vector2.zero, Mathf.Infinity, blockLayer);
            if (cast.collider) return false;
            Vector3 adjustedTeleportPosition = teleportPosition;
            adjustedTeleportPosition.z = playerTransform.position.z;
            playerTransform.position = adjustedTeleportPosition;
            return true;
        }
    }
}