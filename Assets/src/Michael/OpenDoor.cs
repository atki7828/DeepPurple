using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoor : MonoBehaviour {

    private Vector3 OpenPosition, ClosePosition;
    private float moveSpeed = 3;
    private int motionSensor = 4;
    private GameObject Player;
    private bool isOpen = false;
    private bool isLocked = false;
    private AudioClip openSound,closeSound; 
    private AudioSource audioSource;
    public float volume = 0.5f;

	void Start () {
        Player = GameObject.FindWithTag("Player");
        OpenPosition = this.transform.position + new Vector3(0.0f,this.GetComponent<Collider>().bounds.size.y,0.0f);
        ClosePosition = this.transform.position;


        audioSource = gameObject.GetComponent<AudioSource>();
        //audioSource.clip = openSound;
        audioSource.playOnAwake = false;

        if (openSound == null)
            openSound = (AudioClip)Resources.Load("Michael/Audio/electric_door_opening_1");
        if (closeSound == null)
            closeSound = (AudioClip)Resources.Load("Michael/Audio/electric_door_closing_2");
            //closeSound = (AudioClip)Resources.Load("Michael/Audio/sfx-door-open");
	}
	
	void Update () {
        isOpen = false;
        float dx,dz;
        if(!isLocked) {
            foreach(GameObject e in RoomGenerator.EnemyList) {
                if (e != null)
                {
                    Vector3 ePos = e.transform.position;
                    dx = Math.Abs(ePos.x - transform.position.x);
                    dz = Math.Abs(ePos.z - transform.position.z);
                    double eDist = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dz, 2));
                    if (eDist <= motionSensor)
                    {
                        isOpen = true;
                        break;
                    }
                }
            }

            if (!isOpen)
            {
                Vector3 PlayerPos = Player.transform.position;
                dx = Math.Abs(PlayerPos.x - transform.position.x);
                dz = Math.Abs(PlayerPos.z - transform.position.z);
                double distPlayer = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dz, 2));
                if (distPlayer <= motionSensor)
                {
                    isOpen = true;
                    if (!audioSource.isPlaying)
                        audioSource.PlayOneShot(openSound, 1.0f);
                }
            }


            if(isOpen)  Open();
            else        Close();
        }
        else {
            Close();
        }
		
	}

    private void Open() {
        if(this.transform.position.y < OpenPosition.y) {
            this.transform.position += new Vector3(0,moveSpeed*Time.deltaTime,0);
        }
        else
            audioSource.Stop();

    }

    private void Close() {
        if (this.transform.position.y > ClosePosition.y)
        {
            this.transform.position -= new Vector3(0, moveSpeed * Time.deltaTime, 0);
            if (!audioSource.isPlaying)
                audioSource.PlayOneShot(closeSound, 1.0f);
        }
        else
            audioSource.Stop();
    }
    public void Lock() {
        this.isLocked = true;
        this.GetComponent<Renderer>().materials[1].color = new Color(0.984f, 0.313f, 0.156f, 0.309f);
    }
    public void Unlock() {
        this.isLocked = false;
        this.GetComponent<Renderer>().materials[1].color = new Color(0.156f, 0.313f, 0.984f, 0.309f);
    }
}
