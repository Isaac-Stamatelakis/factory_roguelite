using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TickClock : MonoBehaviour
{
    bool run = true;
    private static int tickRate = 20; // number of game ticks per second
 

    public IEnumerator startTickClock() {
        this.run = true;
        while (run) {
            Invoke("TickUpdate",1f/tickRate);
            yield return new WaitForSeconds(1/tickRate);
        }
    }

    public void pauseClock() {
        run = false;
    }
    
}
