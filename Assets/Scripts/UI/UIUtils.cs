using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI {
    public static class UIUtils
    {
        public static IEnumerator TransitionUIElement(RectTransform rectTransform, Vector3 destination, bool moveLocal = false)
        {
            const int updates = 200;
            for (int i = 0; i < updates; i++)
            {
                Vector3 newPosition = Vector3.Lerp(moveLocal ? rectTransform.localPosition : rectTransform.position, destination, (float)i / updates);
                if (moveLocal)
                {
                    rectTransform.localPosition = newPosition;
                }
                else
                {
                    rectTransform.position = newPosition;
                }
                
                yield return null;
            }
            yield return null;
        }
    }
}

