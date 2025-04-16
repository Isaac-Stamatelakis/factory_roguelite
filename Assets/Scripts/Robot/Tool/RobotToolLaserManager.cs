using Player;
using UnityEngine;

namespace Robot.Tool
{
    public class RobotToolLaserManager
    {
        private static readonly int ColorKey = Shader.PropertyToID("_Color");
        private LineRenderer lineRenderer;
        private Material defaultMaterial;
    
        public void UpdateLineRenderer(Vector2 mousePosition, Color color)
        {
            if (!lineRenderer) return;
            Vector2 dif =  mousePosition - (Vector2)lineRenderer.transform.position;
            lineRenderer.SetPositions(new Vector3[] { Vector3.up/2f, dif });
            
            lineRenderer.material.SetColor(ColorKey,color);
            
            
        }

        public RobotToolLaserManager(LineRenderer lineRenderer)
        {
            this.lineRenderer = lineRenderer;
            defaultMaterial = lineRenderer.sharedMaterials[0];
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

    public class MultiButtonRobotToolLaserManager
    {
        private int mouseBitMap;
        private RobotToolLaserManager laserManager;
        private readonly PlayerScript playerScript;
        private readonly LineRenderer lineRendererPrefab;
        public MultiButtonRobotToolLaserManager(PlayerScript playerScript, LineRenderer lineRendererPrefab)
        {
            this.playerScript = playerScript;
            this.lineRendererPrefab = lineRendererPrefab;
        }
        public void Update(ref Vector2 mousePosition, Color color, MouseButtonKey mouseButtonKey)
        {
            mouseBitMap |= (int)mouseButtonKey;
            laserManager ??= new RobotToolLaserManager(GameObject.Instantiate(lineRendererPrefab, playerScript.transform));
            laserManager.UpdateLineRenderer(mousePosition, color);
        }
        

        public void DeActivate(MouseButtonKey mouseButtonKey)
        {
            mouseBitMap &= ~(int)mouseButtonKey;
            if (laserManager != null && mouseBitMap == 0)
            {
                laserManager.Terminate();
                laserManager = null;
            }
        }

        public void SetMaterial(Material material)
        {
            laserManager?.SetMaterial(material);
        }

        
    }
}
