using Entities.Mobs;
using Item.Transmutation.Items;
using Items;
using Items.Transmutable;
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
            ITransmutableItem transmutableItem = ItemRegistry.GetInstance().GetTransmutableItemObject(oreId);
            TransmutableItemMaterial material = transmutableItem?.getMaterial();
            Color color = material ? material.color : new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            mBodyRenderer.color = new Color(94 / 255f, 94 / 255f, 94 / 255f, 1f);
            mCrystalRenderer.color = color;
            Color invertedColor = new Color(1-color.r,1-color.g,1-color.b);
            mEyeRenderer.color = invertedColor;
        }

        public string Initialize()
        {
            return "iron_ore";
        }
    }
}
