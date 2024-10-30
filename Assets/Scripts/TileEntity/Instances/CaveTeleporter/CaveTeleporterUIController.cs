using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WorldModule.Caves;
using UnityEngine.AddressableAssets;

namespace TileEntityModule.Instances {
    public class CaveTeleporterUIController : MonoBehaviour, ITileEntityUI<CaveTeleporterInstance>
    {
        public CaveSelectController caveSelectController;
        public GameObject mListView;
        public GameObject buttonPrefab;
        private void buttonPress(Cave cave) {
            caveSelectController.showCave(cave);
        }

        public void display(CaveTeleporterInstance tileEntityInstance)
        {
            caveSelectController.showDefault();
            StartCoroutine(loadCaves());
        }

        public IEnumerator loadCaves() {
            var handle = Addressables.LoadAssetsAsync<Cave>("cave",null);
            yield return handle;
            var result = handle.Result;
            foreach (Cave cave in handle.Result) {
                Button button = CaveTeleporterUIFactory.generateButton(cave,buttonPrefab,mListView.transform);
                button.onClick.AddListener(() => buttonPress(cave));
            }
            Addressables.Release(handle);
        }
    }
}

