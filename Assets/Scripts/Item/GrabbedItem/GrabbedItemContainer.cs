using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbedItemContainer
{
    private static GrabbedItemContainer instance;
    private static GrabbedItemProperties grabbedItemProperties;
    private GrabbedItemContainer() {
        grabbedItemProperties = GameObject.Find("GrabbedItem").GetComponent<GrabbedItemProperties>();
    }
    public static GrabbedItemProperties getGrabbedItem() {
        if (instance == null) {
            instance = new GrabbedItemContainer();
        }
        return grabbedItemProperties;
    }
}
