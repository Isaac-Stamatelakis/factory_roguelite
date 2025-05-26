using Player;
using UnityEngine;

namespace Robot.Tool
{
    public class RobotToolLaserManager
    {
        private static readonly int ColorKey = Shader.PropertyToID("_Color");
        private LineRenderer lineRenderer;
        private Material defaultMaterial;
        private readonly PlayerRobot playerRobot;
        public void UpdateLineRenderer(Vector2 mousePosition, Color color)
        {
            if (!lineRenderer) return;
            Vector2 dif =  mousePosition - (Vector2)lineRenderer.transform.position;
            Vector2 edgePosition = playerRobot.gunController.GetEdgePosition(.5f);
            Vector2 linePosition = lineRenderer.transform.position;
            lineRenderer.SetPositions(new Vector3[] {edgePosition-linePosition, dif });
            
            lineRenderer.material.SetColor(ColorKey,color);
        }

        public RobotToolLaserManager(LineRenderer lineRenderer, PlayerScript playerScript)
        {
            this.lineRenderer = lineRenderer;
            defaultMaterial = lineRenderer.sharedMaterials[0];
            playerRobot = playerScript.PlayerRobot;
        }

        public void Terminate()
        {
            if (!lineRenderer) return;
            GameObject.Destroy(lineRenderer.gameObject);
            lineRenderer = null;
        }

        public void SetMaterial(Material material)
        {
            if (!lineRenderer) return;
            lineRenderer.material = material ? material : defaultMaterial;
        }
    }
}
