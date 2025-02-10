using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEntity.Instances.SimonSays {
    public class SimonSaysCoroutineController : MonoBehaviour
    {
        private SimonSaysControllerInstance controller;
        public void init(SimonSaysControllerInstance controller) {
            this.controller = controller;
        }

        public void Display(List<int> sequence) {
            StartCoroutine(showSequence(sequence));
        }

        public void RestartGame()
        {
            StartCoroutine(RestartGameCoroutine());
        }

        private IEnumerator RestartGameCoroutine()
        {
            controller.PlayingSequence = true;
            yield return new WaitForSeconds(2f);
            controller.InitGame();
        }

        private IEnumerator showSequence(List<int> sequence)
        {
            controller.PlayingSequence = true;
            foreach (int position in sequence) {
                yield return showTile(controller.ColoredTiles[position]);
            }
            controller.PlayingSequence = false;
        }

       

        public void ShowTileClick(SimonSaysColoredTileEntityInstance coloredTile) {
            if (controller.PlayingSequence || controller.Restarting) {
                return;
            }
            
            StartCoroutine(showTileOnPlayerPress(coloredTile));
        }
        private IEnumerator showTile(SimonSaysColoredTileEntityInstance coloredTileEntity) {
            coloredTileEntity.setColor(1);
            yield return new WaitForSeconds(0.8f);
            coloredTileEntity.setColor(0);
            yield return new WaitForSeconds(0.2f);
        }

        private IEnumerator showTileOnPlayerPress(SimonSaysColoredTileEntityInstance coloredTileEntity)
        {
            if (controller.Restarting) yield break;
            coloredTileEntity.setColor(1);
            yield return new WaitForSeconds(0.8f);
            coloredTileEntity.setColor(0);
            yield return new WaitForSeconds(0.4f);
            if (controller.Restarting) yield break;
            int index = controller.ColoredTiles.IndexOf(coloredTileEntity);
            controller.PlayerSequence.Add(index);
            controller.EvaluateSequence();
            
        }
    }
}

