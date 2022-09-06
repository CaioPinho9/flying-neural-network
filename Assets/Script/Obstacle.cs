using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float speed;

    // Update is called once per frame
    void Update()
    {
        //Speed increase by time, timer is in the controller
        speed = GameObject.Find("GameController").GetComponent<GameController>().speed / -2;

        //Movement
        Vector3 movement = new(speed, 0f, 0f);
        transform.position += Time.deltaTime * movement;

        //Destroy obstacle
        if (transform.position.x < -20)
        {
            Destroy(gameObject);
        }

        
    }
}
