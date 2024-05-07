using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WorldModule.Caves;

namespace TileEntityModule.Instances {
    public class CaveTeleporterUIController : MonoBehaviour
    {
        public CaveSelectController caveSelectController;
        public GameObject mListView;
        private Button[] mCaveButtons;
        public GameObject buttonPrefab;
        // Start is called before the first frame update
        void Start()
        {
            caveSelectController.showDefault();
            Cave[] caves = CaveRegistry.getAll();
            mCaveButtons = new Button[caves.Length];
            int index = 0;
            foreach (Cave cave in caves) {
                Button button = CaveTeleporterUIFactory.generateButton(cave,buttonPrefab,mListView.transform);
                mCaveButtons[index] = button;
                button.onClick.AddListener(() => buttonPress(cave));
            }
            

        }
        private void buttonPress(Cave cave) {
            caveSelectController.showCave(cave);
        }
    }
}

