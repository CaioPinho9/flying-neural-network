using System;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

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
    public bool recharged;
    public bool gameOver;
    public int score = 0;

    [Header("Identity")]
    public int id;
    public bool copy;
    public string dna;
    public string rna = "";
    public float opacity = .5f;
    public GameObject missile;
    private Animator anim;
    public LayerMask layerMask;

    //Timer
    private float seconds;
    private float lastSecond;
    private GameObject timer;

    // Start is called before the first frame update
    public void Start()
    {
        timer = GameObject.Find("Timer");
        seconds = timer.GetComponent<Timer>().seconds;
        lastSecond = 0;
        anim = GetComponent<Animator>();
        transform.position = new(-10, 0, 0);
        gameObject.GetComponent<Renderer>().enabled = true;
        gameObject.GetComponent<SpriteRenderer>().color = new(1, 1, 1, opacity);
        GameObject boom = transform.GetChild(0).gameObject;
        boom.GetComponent<Renderer>().enabled = false;
        boom.GetComponent<Animator>().enabled = false;
        gameOver = false;
        speed = .5f;
        degrees = 0;
        recharged = true;
        score = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (copy)
        {
            dna = GetComponentInChildren<NeuralNetwork>().network.Copy();
            copy = false;
        }
        if (rna != "")
        {
            GetComponentInChildren<NeuralNetwork>().network.Paste(rna);
            rna = "";
        }

        seconds = timer.GetComponent<Timer>().seconds;
        if (seconds - lastSecond > .5f)
        {
            GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
            foreach (GameObject obstacle in obstacles)
            {
                if (transform.position.x > obstacle.transform.position.x && !gameOver)
                {
                    score += 100;
                }
            }
            lastSecond = seconds;
            if (Math.Abs(transform.position.x) > 12 || Math.Abs(transform.position.y) > 6)
            {
                Death();
            }
        }

        //Until losing
        if (!gameOver)
        {
            Sensors();

            Fire();
            //Change angle
            Pitch();

            Move();

            Animate(speed, horizontal);
        }
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
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, sensorLenght, ~layerMask);
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
                else if (targetType[sensorIndex].Equals("Wall"))
                {
                    sensorTargetType[sensorIndex] = 6;
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
        //Limiting speed
        if ((horizontal > 0 && speed < 5) || (horizontal < 0 && speed > .5f))
        {
            speed += horizontal / 5;
        }

        //Move
        transform.position += speed * Time.deltaTime * transform.right;
    }

    private void Pitch()
    {
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
        GameObject missileInstantiated = Instantiate(missile, missilePosition, Quaternion.Euler(transform.eulerAngles));
        missileInstantiated.GetComponent<Missile>().plane = transform.gameObject;
    }

    private void Death()
    {
        Debug.Log(GameObject.Find("NeuralController").GetComponent<NeuralController>().planesAlive);
        //Activate explosion animation
        GameObject boom = transform.GetChild(0).gameObject;
        boom.GetComponent<Renderer>().enabled = true;
        boom.GetComponent<Animator>().enabled = true;

        //Disable Plane
        gameObject.GetComponent<Renderer>().enabled = false;

        //Stop the game
        gameOver = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //If collide, player loses, unless in godmode
        if (!gameOver && (collision.gameObject.CompareTag("Obstacle") || 
                          collision.gameObject.CompareTag("Enemy") || 
                          collision.gameObject.CompareTag("Ammo") || 
                          collision.gameObject.CompareTag("Wall")))
        {
            Death();
        }
    }
}
