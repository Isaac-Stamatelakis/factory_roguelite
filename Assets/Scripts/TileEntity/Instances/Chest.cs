using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName ="New Chest",menuName="Tile Entity/Chest")]
public class Chest : TileEntity, IClickableTileEntity
{
    [Tooltip("Rows of items")]
    public uint rows;
    [Tooltip("Columns of items")]
    public uint columns;
    [Tooltip("GUI Opened when clicked")]
    public GameObject gui;
    protected ItemSlot[,] items;


    public void onClick()
    {
        items = new ItemSlot[rows,columns];
        Debug.Log("I am clicked yay!");
    }
}
