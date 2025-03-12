using UnityEngine;

namespace World.Cave.Ore
{
    public class CaveOreDistributor : ICaveDistributor
    {
        
        public void Distribute(SeralizedWorldData worldData, int width, int height, Vector2Int bottomLeftCorner)
        {
            
        }
    }
    
    [System.Serializable]
    public class CaveOreDistribution
    {
        [Header("Percentage of stone tiles which are replaced with ore")]
        [Range(0,1)] public float Fill;
        
    }
}
