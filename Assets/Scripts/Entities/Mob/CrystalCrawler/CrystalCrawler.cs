using Entities.Mobs;
using Item.Transmutation.Items;
using UnityEngine;

namespace Entities.Mob.CrystalCrawler
{
    public class CrystalCrawler : MonoBehaviour, ICaveInitiazableMobComponent
    {
        [SerializeField] private SpriteRenderer mBodyRenderer;
        [SerializeField] private SpriteRenderer mCrystalRenderer;
        [SerializeField] private SpriteRenderer mEyeRenderer;

        private string oreId;
        public SerializableMobComponentType ComponentType => SerializableMobComponentType.CrystalCrawler;

        public string Serialize()
        {
            return oreId;
        }

        public void Deserialize(string data)
        {
            oreId = data;
            TransmutableTileItemObject transmutableTileItem = Item
        }

        public string Initialize()
        {
            throw new System.NotImplementedException();
        }
    }
}
