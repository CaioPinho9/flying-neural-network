using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo : MonoBehaviour
{
    //Movement
    public float speed;

    //Time
    private GameObject controller;

    // Start is called before the first frame update
    void Start()
    {
        controller = GameObject.Find("GameController");
    }

    // Update is called once per frame
    void Update()
    {
        //Speed increases by time, timer in controller
        speed = GameController.speed * -1.5f;

        //Movement
        Vector3 movement = new(speed, 0f, 0f);
        transform.position += Time.deltaTime * movement;

        //If it goes outside of camera, it's destroyed
        if (!GetComponent<Renderer>().isVisible)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        //Destroyed when collide
        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Missile"))
        {
            collision.transform.GetComponent<Missile>().player.GetComponent<Player>().score += 500;
            Destroy(gameObject);
        }
    }
}
