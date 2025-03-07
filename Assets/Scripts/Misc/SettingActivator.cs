using System;
using UI.GraphicSettings;
using UnityEngine;

namespace Misc
{
    public class SettingActivator : MonoBehaviour
    {
        public void Start()
        {
            if (GraphicSettingsUtils.APPLY_ON_START)
            {
                GraphicSettingsUtils.ApplyAllGraphicsSettings();
            }
            GameObject.Destroy(gameObject);
        }
    }
}
