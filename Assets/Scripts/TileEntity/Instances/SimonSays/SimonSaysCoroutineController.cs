using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEntityModule.Instances.SimonSays {
    public class SimonSaysCoroutineController : MonoBehaviour
    {
        private List<SimonSaysColoredTileEntity> coloredTiles;
        public void init(List<SimonSaysColoredTileEntity> coloredTiles) {
            this.coloredTiles = coloredTiles;
        }

        public void display(ColorPosition[] sequence) {
            StartCoroutine(show(sequence));
            
        }

        private IEnumerator show(ColorPosition[] sequence) {
            foreach (ColorPosition colorPosition in sequence) {
                coloredTiles[colorPosition.position].setColor(colorPosition.color);
                yield return new WaitForSeconds(0.8f);
                coloredTiles[colorPosition.position].setColor(0);
                yield return new WaitForSeconds(0.2f);
            }
        }

        public IEnumerator showTileClick(SimonSaysColoredTileEntity coloredTile) {
            yield return null;
        }
    }
}

