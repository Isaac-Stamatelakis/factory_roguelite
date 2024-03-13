using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using WaveFunctionCollaps;
public class WFCTest : MonoBehaviour
{
    [SerializeField]
    public GameObject tileMapPrefab;
    protected Tilemap outputImage;
    [SerializeField]
    [Tooltip("For tiles usualy set to 1. If tile contain just a color can set to higher value")]
    public int patternSize = 2;
    [SerializeField]
    [Tooltip("How many times algorithm will try creating the output before quiting")]
    public int maxIterations = 1;
    [SerializeField]
    [Tooltip("Output image width")]
    public int outputWidth = 5;
    [SerializeField]
    [Tooltip("Output image height")]
    public int outputHeight = 5;
    [SerializeField]
    [Tooltip("Don't use tile frequency - each tile has equal weight")]
    public bool equalWeights = false;
    WaveFunctionCollapse wfc;

    // Start is called before the first frame update
    void Start()
    {
        outputImage = gameObject.GetComponent<Tilemap>();
        CreateWFC();
        CreateTilemap();
        SaveTilemap();


    }

    public void CreateWFC()
    {
        wfc = new WaveFunctionCollapse(this.tileMapPrefab.GetComponent<Tilemap>(), this.outputImage, patternSize, this.outputWidth, this.outputHeight, this.maxIterations, this.equalWeights);
    }
    public void CreateTilemap()
    {
        wfc.CreateNewTileMap();
    }

    public void SaveTilemap()
    {
        Tilemap output = wfc.GetOutputTileMap();
        
        string[,] outputStrings = new string[outputWidth,outputHeight];
        BoundsInt bounds = output.cellBounds;
        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);

                if (output.HasTile(tilePosition))
                {
                    TileBase tile = output.GetTile(tilePosition);
                    if (tile is StandardTile) {
                        outputStrings[x-bounds.min.x,y-bounds.min.x]=((StandardTile) tile).id;
                    }
                   
                }
            }
        }
        
    }

}
