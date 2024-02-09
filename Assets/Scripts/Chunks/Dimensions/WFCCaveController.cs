using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFCCaveController : DimController
{
    [SerializeField]
    public GameObject tileMapPrefab;
    [SerializeField] 
    public bool generate;
    [SerializeField] 
    public Cave cave;
    [SerializeField] 
    public int patternSize;
    public override void Start() {
        base.Start();
        GameObject closedChunkSystemObject = new GameObject();
        closedChunkSystemObject.name="Cave";
        TileClosedChunkSystem area = closedChunkSystemObject.AddComponent<TileClosedChunkSystem>();
        closedChunkSystems.Add(area);
        if (generate) {
            Debug.Log("New Area Saved At: " + Application.persistentDataPath);
            WFCGenerator caveGenerator = new WFCGenerator(cave,tileMapPrefab,patternSize);
            caveGenerator.generate();
        }
        IntervalVector coveredArea = cave.getCoveredArea();
        area.initalize(transform,coveredArea,-1);

    }
}
