using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace TileEntity.Instances.Machines {
    public class ArrowProgressController : MonoBehaviour
    {
        public Sprite[] arrows;
        public Image image;
        
        public void SetArrowProgress(float val) {
            if (val < 0)
            {
                val = 1;
            }
            if (val > 1)
            {
                val = 1;
            }

            int index = (int)(val * (arrows.Length));
            if (index > arrows.Length - 1) index = arrows.Length - 1;
            if (index < 0) index = 0;
            image.sprite = arrows[index];
        }
    }
}

