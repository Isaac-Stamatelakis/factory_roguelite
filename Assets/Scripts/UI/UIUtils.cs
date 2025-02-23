using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI {
    public static class UIUtils
    {
        public static IEnumerator TransitionUIElement(RectTransform rectTransform, Vector3 destination)
        {
            const int updates = 200;
            for (int i = 0; i < updates; i++)
            {
                rectTransform.position = Vector3.Lerp(rectTransform.position, destination, (float)i / updates);
                yield return null;
            }
            yield return null;
        }
    }
}

