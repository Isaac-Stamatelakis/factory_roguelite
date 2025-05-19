using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace World
{
    public class AutoSaveUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI mTextElement;
        [SerializeField] private Image mImage;

        public IEnumerator DisplayCountdown()
        {
            const int WARNING_TIME = 5;
            var delay = new WaitForSecondsRealtime(1f);
            for (int i = WARNING_TIME; i > 0; i--)
            {
                mTextElement.text = $"Auto Saving in {i}...";
                yield return delay;
            }
            mTextElement.text = "Auto Save in Progress...";
        }

    

        public IEnumerator CompletionFade()
        {
            mTextElement.text = "Auto Save Complete!";
            const float FADE_TIME = 5f;
            float time = 0;
            float initialAlpha = mImage.color.a;
            while (time < FADE_TIME)
            {
                float alpha = Mathf.Lerp(1, 0, time / FADE_TIME);
                time += Time.deltaTime;
                var color = mImage.color;
                color.a = initialAlpha * alpha;
                mImage.color = color;

                var color1 = mTextElement.color;
                color1.a = alpha;
                mTextElement.color = color1;
                yield return null;
            } 
            GameObject.Destroy(gameObject);
        }
    }
}
