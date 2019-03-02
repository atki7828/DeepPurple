﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public int height = 8;
    public int zOffset = -12;
    private GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = player.transform.position;
        position.y = height;
        position.z += zOffset;
        transform.position = position;
    }
}