using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TileEntityModule.Instances
{
    [CreateAssetMenu(fileName = "New Machine", menuName = "Tile Entity/Machine")]
    public class Machine : TileEntity, ITickableTileEntity, IClickableTileEntity, ISerializableTileEntity
    {
        public RecipeProcessor recipeProcessor;
        public int tier;
        public GameObject gui;
        public void onClick()   
        {
            throw new System.NotImplementedException();
        }

        public string serialize()
        {
            return null;
        }

        public void tickUpdate()
        {
            throw new System.NotImplementedException();
        }

        public void unserialize(string data)
        {
            throw new System.NotImplementedException();
        }
    }
}