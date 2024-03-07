using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace TileEntityModule.Instances.Machines {
    public class ArrowProgressController : MonoBehaviour
    {
        private Sprite[] arrows;
        private Image image;


        // Update is called once per frame
        void Update()
        {
            
        }
        public void setArrow(float val) {

            if (image == null) {
                image = GetComponent<Image>();
                if (image == null) {
                    Debug.LogError("Arrow Progress Controller assigned to object '" + name + "' without Image component ");
                    return;
                }
            }
            if (val < 0 || val > 1) {
                Debug.LogError("val provided to arrow outside range (0,1)");
                return;
            }
            if (arrows == null) {
                arrows = Resources.LoadAll<Sprite>("UI/Arrows/animatedarrow");
            }
            if (val == 0) {
                image.sprite = arrows[0];
                return;
            }
            int index = Mathf.CeilToInt(val*8);
            image.sprite = arrows[index];
        }
    }
}

