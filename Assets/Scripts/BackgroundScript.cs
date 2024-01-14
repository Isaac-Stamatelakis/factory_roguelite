using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScript : MonoBehaviour
{
    int yOffset = 1;
    Transform playerTransform;
    // Start is called before the first frame update
    void Start()
    {
        playerTransform = GameObject.Find("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = new Vector3(-playerTransform.position.x/30,-playerTransform.position.y/30+yOffset,transform.localPosition.z);
    }
}
