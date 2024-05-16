using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace UI {
    public class UIVideoPlayer : MonoBehaviour
    {
        public RawImage rawImage;
        public Image placeHolder;
        public VideoPlayer videoPlayer;
        private int counter;
        public int fadeDelay;
        void Start()
        {
            videoPlayer.Play();
            rawImage.enabled = true;
        }

        public void FixedUpdate() {
            if (placeHolder == null) {
                return;
            }
            counter++;
            if (counter < fadeDelay + 255) {
                Color color = placeHolder.color;
                color.a = (255-counter+fadeDelay)/255f;
                placeHolder.color = color;
            } else {
                GameObject.Destroy(placeHolder.gameObject);
                placeHolder = null;
            }
        }
        void Update()
        {
            rawImage.texture = videoPlayer.texture;
        }

    }
}

