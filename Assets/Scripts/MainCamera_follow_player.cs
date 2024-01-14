using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera_follow_player : MonoBehaviour
{
    GameObject player;
    Transform cameraTransform;
    private float yOffset = 1;
    // Start is called before the first frame update
    void Start()
    {
        cameraTransform = GetComponent<Transform>();
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        cameraTransform.position = new Vector3(player.transform.position.x, player.transform.position.y+yOffset, cameraTransform.position.z);
    }
}
