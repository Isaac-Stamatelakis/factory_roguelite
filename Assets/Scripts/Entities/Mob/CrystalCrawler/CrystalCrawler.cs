using System;
using Entities.Mobs;
using Item.Transmutation.Items;
using Items;
using Items.Transmutable;
using Newtonsoft.Json;
using UnityEngine;
using World.Cave.Registry;
using Random = UnityEngine.Random;

namespace Entities.Mob.CrystalCrawler
{
    public class CrystalCrawler : MonoBehaviour, ICaveInitiazableMobComponent
    {
        [SerializeField] private SpriteRenderer mBodyRenderer;
        [SerializeField] private SpriteRenderer mCrystalRenderer;
        [SerializeField] private SpriteRenderer mEyeRenderer;
        
        private int colorBitMap;
        public SerializableMobComponentType ComponentType => SerializableMobComponentType.CrystalCrawler;

        public string Serialize()
        {
            return colorBitMap.ToString();
        }

        public void Deserialize(string data)
        {
            if (data == null)
            {
                colorBitMap = ColorToInt(GetRandomColor());
            }

            if (!int.TryParse(data, out colorBitMap))
            {
                colorBitMap = ColorToInt(GetRandomColor());
            }

            Color color = FromBitMap(colorBitMap);
            mBodyRenderer.color = new Color(94 / 255f, 94 / 255f, 94 / 255f, 1f);
            mCrystalRenderer.color = color;
            Color invertedColor = new Color(1-color.r,1-color.g,1-color.b);
            mEyeRenderer.color = invertedColor;
        }

        public int ColorToInt(Color32 color)
        {
            return (color.a << 24) | (color.b << 16) | (color.g << 8) | color.r;
        }

        public Color FromBitMap(int bitmap)
        {
            int a = (bitmap >> 24) & 0xFF;
            int b = (bitmap >> 16) & 0xFF;
            int g = (bitmap >> 8)  & 0xFF;
            int r = bitmap & 0xFF;
            return new Color(r/255f, g/255f, b/255f, a/255f);
        }
        private Color32 GetRandomColor()
        {
            return new Color32(
                (byte)Random.Range(0, 256),
                (byte)Random.Range(0, 256),
                (byte)Random.Range(0, 256),
                255                          
            );
        }

        public string Initialize(CaveTileCollection caveTileCollection)
        {
            string id = caveTileCollection.GetRandomOreId();
            ITransmutableItem transmutableItem = ItemRegistry.GetInstance().GetTransmutableItemObject(id);
            TransmutableItemMaterial material = transmutableItem?.getMaterial();
            Color color = material ? material.color : GetRandomColor();
            colorBitMap =  ColorToInt(color);
            return JsonConvert.SerializeObject(colorBitMap);
        }
    }
}
