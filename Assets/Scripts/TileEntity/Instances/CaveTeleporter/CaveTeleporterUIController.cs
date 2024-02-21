using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
            List<CaveRegion> caves = CaveRegionFactory.getRegionsUnlocked(2);
            mCaveButtons = new Button[caves.Count];
            int index = 0;
            foreach (CaveRegion caveRegion in caves) {
                Button button = CaveTeleporterUIFactory.getButton(caveRegion,buttonPrefab,mListView.transform);
                mCaveButtons[index] = button;
                button.onClick.AddListener(() => buttonPress(caveRegion));
            }
            

        }
        private void buttonPress(CaveRegion caveRegion) {
            caveSelectController.showCave(caveRegion);
        }
    }
}

