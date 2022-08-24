using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plane : MonoBehaviour
{
    //Input
    [Header("Input")]
    public float vertical;
    public float horizontal;
    public bool shot;

    //Sensor
    [Header("Sensors")]
    public float[] sensorX = new float[21];
    public float[] sensorY = new float[21];
    public int[] sensorTargetType = new int[21];
    private readonly float sensorLenght = 20f;
    private readonly float[] sensorDegrees = { 0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 90 };
    private readonly Vector3[] sensorDirection = new Vector3[21];

    //Movement
    [Header("Movement")]
    public float speed;
    public float degrees;
    public float angle;

    [Header("Gameplay")]
    //Gameplay
    public bool godMode;
    public bool recharged;
    private bool gameOver;
    
    public GameObject missile;
    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        recharged = true;
        gameOver = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Until losing
        if (!gameOver)
        {
            Sensors();

            Fire();
            //Change angle
            Pitch();

            Move();
        }

        //If it goes outside of camera, player loses
        //Time to render the plane
        if (!GetComponent<Renderer>().isVisible && GameObject.Find("Timer").GetComponent<Timer>().seconds > 0.2f)
        {
            Death();
        }

        Animate(speed, horizontal);
    }

    private void Sensors()
    {
        //Direction index has more indexs than sensorDegrees, because direction also has negative values
        int directionIndex = 0;

        //Convert degrees in Vector2 direction
        for (int i = 0; i < sensorDegrees.Length; i++)
        {
            //Degrees to radian, positive and zero
            angle = (float)Math.PI * (sensorDegrees[i] + degrees) / 180;

            //Zero doesn't have a negative value
            if (sensorDegrees[i] == 0)
            {
                //Direction is a Vector2, with range between 0 and 1
                sensorDirection[0] = new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0);
                directionIndex++;
            }
            else
            {
                //Direction is a Vector2, with range between 0 and 1
                sensorDirection[directionIndex] = new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0);

                //Degrees to radian, negative
                angle = (float)Math.PI * (-sensorDegrees[i] + degrees) / 180;
                sensorDirection[directionIndex + 1] = new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0);

                //Index for positive and negative value
                directionIndex += 2;
                
            }
        }

        //Unity didn't allow global arrays
        float[] x = new float[21];
        float[] y = new float[21];
        String[] targetType = new String[21];

        //Create a raycast
        int sensorIndex = 0;
        foreach (Vector3 direction in sensorDirection)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, sensorLenght);
            if (hit.transform != null)
            {
                //Saving raycast hit position and object type
                x[sensorIndex] = hit.point.x;
                y[sensorIndex] = hit.point.y;
                targetType[sensorIndex] = hit.transform.gameObject.tag;

                Debug.DrawLine(transform.position, hit.point);

                //Classifing Target
                if (targetType[sensorIndex].Equals("Obstacle"))
                {
                    sensorTargetType[sensorIndex] = 1;
                }
                else if (targetType[sensorIndex].Equals("Enemy"))
                {
                    sensorTargetType[sensorIndex] = 2;
                }
                else if (targetType[sensorIndex].Equals("Ammo"))
                {
                    sensorTargetType[sensorIndex] = 3;
                }
                else if (targetType[sensorIndex].Equals("Missile"))
                {
                    sensorTargetType[sensorIndex] = 4;
                }
                else if (targetType[sensorIndex].Equals("Coin"))
                {
                    sensorTargetType[sensorIndex] = 5;
                }
                else
                {
                    sensorTargetType[sensorIndex] = 0;
                }
            } 
            else
            {
                x[sensorIndex] = 0;
                y[sensorIndex] = 0;
                targetType[sensorIndex] = "";
                sensorTargetType[sensorIndex] = 0;
            }

            sensorIndex++;
        }

        //Global Output
        sensorX = x;
        sensorY = y;
    }

    private void Move()
    {
        //Manual Input
        horizontal = Input.GetAxis("Horizontal");

        //Limiting speed
        if ((horizontal > 0 && speed < 5) || (horizontal < 0 && speed > -1))
        {
            speed += horizontal / 10;
        }

        //Move
        transform.position += speed * Time.deltaTime * transform.right;
    }

    private void Pitch()
    {
        //Manual Input
        vertical = Input.GetAxis("Vertical");

        //Increasing the input
        degrees += vertical * 2f;

        //Change angle of plane
        transform.eulerAngles = new Vector3(0f, 0f, degrees);
    }

    private void Fire()
    {
        //Manual and automatic input, shot when recharged
        if ((Input.GetAxis("Fire") > 0 || shot) && recharged)
        {
            //Create missile
            recharged = false;
            Missile();
        }

        //Remove and add missile in animation
        anim.SetBool("recharged", recharged);
    }

    private void Animate(float speed, float horizontal)
    {
        //Break animation if plane is moving backwards or breaking.
        if (speed < 0)
        {
            anim.SetBool("break", true);
        }
        else
        {
            anim.SetBool("break", false);
        }

        if (horizontal < 0)
        {
            anim.SetBool("break", true);
        }
    }

    private void Missile()
    {
        //Spawn in front of plane
        Vector3 missilePosition = new(transform.position.x, transform.position.y - 0.1f, transform.position.z);
        Instantiate(missile, missilePosition, Quaternion.Euler(transform.eulerAngles));
    }

    private void Death()
    {
        //Activate explosion animation
        GameObject boom = transform.GetChild(0).gameObject;
        boom.GetComponent<Renderer>().enabled = true;
        boom.GetComponent<Animator>().enabled = true;

        //Disable Plane
        gameObject.GetComponent<Renderer>().enabled = false;

        //Stop the game
        GameObject.Find("GameController").GetComponent<GameController>().gameOver = true;
        gameOver = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //If collide, player loses, unless in godmode
        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Enemy"))
        {
            if(!godMode)
            {
                Death();
            }
        }
    }

    public void Activate()
    {
        transform.position = new(-10, 0, 0);
        gameObject.GetComponent<Renderer>().enabled = true;
        GameObject boom = transform.GetChild(0).gameObject;
        boom.GetComponent<Renderer>().enabled = false;
        boom.GetComponent<Animator>().enabled = false;
        gameOver = false;
        speed = 0;
        degrees = 0;
        recharged = true;
    }
}
