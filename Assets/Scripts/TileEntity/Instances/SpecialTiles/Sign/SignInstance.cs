using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Newtonsoft.Json;
using UI;


namespace TileEntity.Instances.Signs {
    public class SignInstance : TileEntityInstance<Sign>, ILockUnInteractableRightClickTileEntity, ISerializableTileEntity, IPlaceInitializable, IWorldToolTipTileEntity
    {
        public SignInstance(Sign tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }
        public SignData SignData;
       
        public string Serialize()
        {
            return JsonConvert.SerializeObject(SignData);
        }

        public void Unserialize(string data)
        {
            SignData = JsonConvert.DeserializeObject<SignData>(data);
        }

        public void PlaceInitialize()
        {
            SignData = new SignData("","","");
        }

        public string GetTextPreview()
        {
            string text = string.Empty;

            void AddText(string value)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    text += value + "\n";
                }
            }
            AddText(SignData.line1);
            AddText(SignData.line2);
            AddText(SignData.line3);
            text = text.Trim('\n');
            return text;
        }
    }
}
