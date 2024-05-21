using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Misc {
    public static class StatUtils
    {
        // Start is called before the first frame update
        public static int  getAmount(int mean, int standardDeviation)
        {
            double u1 = 1.0 - UnityEngine.Random.Range(0,1);
            double u2 = 1.0 - UnityEngine.Random.Range(0,1);
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return Mathf.RoundToInt((float)(mean + standardDeviation * randStdNormal));
        }

    }

}
