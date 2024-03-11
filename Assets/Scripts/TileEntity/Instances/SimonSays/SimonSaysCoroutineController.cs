using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEntityModule.Instances.SimonSays {
    public class SimonSaysCoroutineController : MonoBehaviour
    {
        private SimonSaysController controller;
        public void init(SimonSaysController controller) {
            this.controller = controller;
        }

        public void display(List<int> sequence) {
            controller.DisplayingSequence = true;
            StartCoroutine(showSequence(sequence));
        }

        private IEnumerator showSequence(List<int> sequence) {
            foreach (int position in sequence) {
                yield return showTile(controller.ColoredTiles[position]);
            }
            controller.DisplayingSequence = false;
        }

       

        public void showTileClick(SimonSaysColoredTileEntity coloredTile) {
            if (!controller.AllowPlayerPlace) {
                return;
            }
            
            StartCoroutine(showTileOnPlayerPress(coloredTile));
        }
        private IEnumerator showTile(SimonSaysColoredTileEntity coloredTileEntity) {
            coloredTileEntity.setColor(1);
            yield return new WaitForSeconds(0.8f);
            coloredTileEntity.setColor(0);
            yield return new WaitForSeconds(0.2f);
        }

        private IEnumerator showTileOnPlayerPress(SimonSaysColoredTileEntity coloredTileEntity) {
            coloredTileEntity.setColor(1);
            yield return new WaitForSeconds(0.8f);
            coloredTileEntity.setColor(0);
            yield return new WaitForSeconds(0.4f);
            int index = controller.ColoredTiles.IndexOf(coloredTileEntity);
            controller.PlayerSequence.Add(index);
            controller.evaluateSequence();
        }
    }
}

