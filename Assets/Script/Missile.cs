using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    //Movement
    public float speed;
    public float accelaration;
    public float degrees;

    //Gameplay
    private bool destroyed;
    public GameObject plane;

    // Start is called before the first frame update
    void Start()
    {
        destroyed = false;
        accelaration = 1.3f;
        speed = 1.01f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!destroyed)
        {
            //Plane angle
            degrees = transform.eulerAngles.z;

            //Exponencial speed
            accelaration += 1f;
            float speedPow = (float)Math.Pow(speed, accelaration);

            //Move
            transform.position += Time.deltaTime * speedPow * transform.right;
            transform.eulerAngles = new Vector3(0f, 0f, degrees);
        } 
        else
        {
            //Matches speed
            float matchObstacleSpeed = GameObject.Find("GameController").GetComponent<GameController>().speed / -2;
            Vector3 movement = new(matchObstacleSpeed, 0f, 0f);
            transform.position += Time.deltaTime * movement;
        }

        //Destroy missile if it goes outside of camera, and recharges the plane
        if (!GetComponent<Renderer>().isVisible && !destroyed)
        {
            plane.GetComponent<Plane>().recharged = true;
            
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        //Is destroyed if collide with anything, but the player
        if (!collision.gameObject.CompareTag("Player") && !collision.gameObject.CompareTag("Missile"))
        {
            //Recharges plane when destroyed
            plane.GetComponent<Plane>().recharged = true;

            //Activate explosion animation
            GameObject boom = transform.GetChild(0).gameObject;
            boom.GetComponent<Renderer>().enabled = true;
            boom.GetComponent<Animator>().enabled = true;

            //Disable missile
            gameObject.GetComponent<Renderer>().enabled = false;
            destroyed = true;

            Destroy(gameObject, 0.7f);
        }
    }
}
