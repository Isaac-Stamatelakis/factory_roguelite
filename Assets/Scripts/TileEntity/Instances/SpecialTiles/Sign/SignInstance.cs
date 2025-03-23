using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Newtonsoft.Json;
using UI;


namespace TileEntity.Instances.Signs {
    public class SignInstance : TileEntityInstance<Sign>, ILockUnInteractableRightClickTileEntity, ISerializableTileEntity, IPlaceInitializable
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
    }
}
