using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileEntityContainerController : MonoBehaviour
{
    public IEnumerator fullLoadAllTileEntities() {
        for (int n = 0; n < transform.childCount; n ++) {
            GameObject tileEntity = transform.GetChild(n).gameObject;
            TileEntityFactory.fullLoadTileEntity(tileEntity);
        }
        yield return null;
    }

    public void unFullLoadTileEntities() {
        for (int n = 0; n < transform.childCount; n ++) {
            GameObject tileEntity = transform.GetChild(n).gameObject;
            GameObject fullLoadContainer = Global.findChild(tileEntity.transform,"FullLoadedContainer");
            if (fullLoadContainer != null) {
                GameObject.Destroy(fullLoadContainer);
            }
        }
    }

    public List<List<Dictionary<string, object>>> getDynamicTileEntityOptions(string tileContainerName) {
        List<List<Dictionary<string,object>>> dynamicTileOptions = new List<List<Dictionary<string, object>>>();
        // Create Empty List
        for (int x = 0; x < Global.ChunkSize; x ++) {
            List<Dictionary<string,object>> tempList = new List<Dictionary<string, object>>();
            for (int y = 0; y < Global.ChunkSize; y ++) {
                tempList.Add(new Dictionary<string, object>());
            }
            dynamicTileOptions.Add(tempList);
        }

        for (int n = 0; n < transform.childCount; n ++) {
            GameObject tileEntity = transform.GetChild(n).gameObject;
            TileEntityProperties tileEntityProperties = tileEntity.GetComponent<TileEntityProperties>();
            if (tileEntityProperties.TileContainerName == tileContainerName) {
                dynamicTileOptions[tileEntityProperties.Position.x][tileEntityProperties.Position.y] = tileEntity.GetComponent<TileEntityProperties>().Data;
            }
        }

        return dynamicTileOptions;
    }

    public void saveAllTileEntities() {
        for (int n = 0; n < transform.childCount; n ++) {
            GameObject tileEntity = transform.GetChild(n).gameObject;
            TileEntityProperties tileEntityProperties = tileEntity.GetComponent<TileEntityProperties>();
            foreach (Component component in tileEntity.GetComponents<MonoBehaviour>()) {
                if (component is SeralizableTileOption) {
                    SeralizableTileOption seralizableTileOption = (SeralizableTileOption) component;
                    seralizableTileOption.seralize();
                }
            }
        }
    }
}
