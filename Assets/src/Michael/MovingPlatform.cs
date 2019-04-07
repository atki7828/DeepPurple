﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour {

    GameObject player;
    public float speed = 1;
    public bool moving = true;
    public bool playerOnPlatform = false;
    public bool OnlyMoveWithPlayer = false;
    public Vector3 home,start,end,direction;

	// Use this for initialization
	void Start () {
        player = GameObject.FindWithTag("Player");
        start = this.transform.position;
        home = start;
        if(OnlyMoveWithPlayer)  moving = false;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if(end == Vector3.zero) {
            end = new Vector3(start.x,start.y*3,start.z);
        }
        if(direction == Vector3.zero)
            direction = (end - start).normalized;
        if(moving) {
            this.transform.position += direction * speed * Time.deltaTime;
            foreach(Collider o in Physics.OverlapBox(this.GetComponent<Collider>().bounds.center,this.GetComponent<Collider>().bounds.size))
                if(o.name == "Coin")
                   o.transform.position += direction * speed * Time.deltaTime;
            if(playerOnPlatform) player.transform.position += direction * speed * Time.deltaTime;
            //this.transform.position = Vector3.Lerp(this.transform.position,end,speed * Time.deltaTime);
            if(Vector3.Distance(this.transform.position,end) < 0.2f) {
                Vector3 s = end;
                end = start;
                start = s;
                direction = (end - start).normalized;
            }
        }
        else if(Vector3.Distance(this.transform.position,home) > 0.1f)
            this.transform.position += (home-this.transform.position).normalized*speed*Time.deltaTime;
	}

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject == player) {
            if(OnlyMoveWithPlayer)
                moving = true;
            playerOnPlatform = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if(other.gameObject == player) {
            if(OnlyMoveWithPlayer)
                moving = false;
            playerOnPlatform = false;
        }
    }

    public void Init(Vector3 home, Vector3 end, float speed = 1, bool OnlyMoveWithPlayer = false) {
        this.home = home;
        this.end = end;
        this.speed = speed;
        this.OnlyMoveWithPlayer = OnlyMoveWithPlayer;
    }

}