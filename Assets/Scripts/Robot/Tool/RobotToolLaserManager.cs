using UnityEngine;

namespace Robot.Tool
{
    public class RobotToolLaserManager
    {
        private LineRenderer lineRenderer;
    
        public void UpdateLineRenderer(Vector2 mousePosition, Color color)
        {
            if (!lineRenderer) return;
            Vector2 dif =  mousePosition - (Vector2)lineRenderer.transform.position;
            lineRenderer.SetPositions(new Vector3[] { Vector3.up/2f, dif });
            
            Gradient gradient = lineRenderer.colorGradient;
            GradientColorKey[] colorKeys = gradient.colorKeys;
            colorKeys[1].color = color;
            gradient.colorKeys = colorKeys;
            lineRenderer.colorGradient = gradient;
            
        }

        public RobotToolLaserManager(LineRenderer lineRenderer)
        {
            this.lineRenderer = lineRenderer;
        }

        public void Terminate()
        {
            if (!lineRenderer) return;
            GameObject.Destroy(lineRenderer.gameObject);
            lineRenderer = null;
        }
    }
}
