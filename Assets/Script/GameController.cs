using System;
using UnityEngine;

public class GameController : MonoBehaviour
{
    //Game Over
    public bool gameOver = false;

    //Gameplay
    public static float speed = 2f;
    public int score;

    //Spawn
    public float spawnTime = 0.0f;
    public float waitTime = 0f;
    public float defaultTime;
    public float hardTime;
    private int id = 0;

    //Timer
    private GameObject timer;
    public float seconds;
    public float lastSecond;

    //Objects to spawn
    public GameObject enemy;
    public GameObject crateTower;
    public GameObject loop;

    // Start is called before the first frame update
    void Start()
    {
        timer = transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        //Timer
        seconds = timer.GetComponent<Timer>().seconds;

        //Default obstacles
        defaultTime = 15f / speed;
        //When boss or loop spawns, the next obstacle have a delay to spawn
        hardTime = defaultTime + 20f / speed;

        //After X seconds an obstacle spawns, if player is alive
        if (seconds - spawnTime >= waitTime && !gameOver)
        {
            spawnTime = seconds;
            Spawn();
        }
    }

    public static float Distance(float x1, float y1, float x2, float y2)
    {
        //Euclidian Distance
        return (float)Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
    }

    public void Restart()
    {
        //Delete everything
        GameObject[] enemy = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] ammo = GameObject.FindGameObjectsWithTag("Ammo");
        GameObject[] obstacle = GameObject.FindGameObjectsWithTag("Obstacle");
        GameObject[] missile = GameObject.FindGameObjectsWithTag("Missile");
        GameObject[] coin = GameObject.FindGameObjectsWithTag("Coin");

        foreach (GameObject destroy in enemy) { Destroy(destroy); }
        foreach (GameObject destroy in ammo) { Destroy(destroy); }
        foreach (GameObject destroy in obstacle) { Destroy(destroy); }
        foreach (GameObject destroy in missile) { Destroy(destroy); }
        foreach (GameObject destroy in coin) { Destroy(destroy); }

        //Reset values
        score = 0;
        gameOver = false;
        timer.GetComponent<Timer>().seconds = 0;
        timer.GetComponent<Timer>().StartTime = Time.time;
        seconds = 0;
        lastSecond = 0;
        spawnTime = 0;
        waitTime = 0;
        speed = 1;
        id = 0;
    }

    void Spawn()
    {
        //Choose an obstacle
        GameObject obstacle = RandomObject();

        //Decide the positions and quantity of obstacles
        Vector3[] position = Convoy(obstacle);

        //Spawn
        foreach (Vector3 coord in position)
        {
            //If position was choosed
            if (coord.x != 0)
            {
                //Enemy is fliped
                float side = 0f;
                if (obstacle.name == "Enemy")
                {
                    side = 180;
                }

                GameObject instantiated = Instantiate(obstacle, coord, Quaternion.Euler(new Vector3(0f, side, 0f)));
                if (obstacle.name == "Crate Tower")
                {
                    instantiated.GetComponent<Obstacle>().id = id;
                }
            }
        }
    }

    GameObject RandomObject()
    {
        GameObject obstacle;

        if (id <= 1)
        {
            id++;
            return crateTower;
        }

        //Random
        float randomNumber = UnityEngine.Random.Range(0, 100);

        //Loop 20%
        if (randomNumber < 20 && seconds > 60)
        {
            obstacle = loop;
        }
        //Tower 50%
        else if(randomNumber < 70)
        {
            obstacle = crateTower;
            id++;
        }
        //Enemy 30%
        else
        {
            obstacle = enemy;
        }

        return obstacle;
    }

    Vector3[] Convoy(GameObject obstacle)
    {
        Vector3[] position = new Vector3[9];
        float randomNumber = UnityEngine.Random.Range(0, 100);

        if (obstacle.name == "Crate Tower")
        {
            /*
            //UP 25%
            if (randomNumber < 25)
            {
                position[0] = new Vector3(15f, 2.44f, 0f);
            }
            //DOWN 25%
            else if (randomNumber < 50)
            {
                position[0] = new Vector3(15f, -2.44f, 0f);
            }
            else 
            */

            //DOUBLE UP 25%
            if (id == 1)
            {
                position[0] = new Vector3(15f, -0.11f, 0f);
            }
            //DOUBLE DOWN 25%
            else if (id == 2)
            {
                position[0] = new Vector3(15f, -2.46f, 0f);
            } 
            //DOUBLE UP 25%
            else if (randomNumber <= 50)
            {
                position[0] = new Vector3(15f, -0.11f, 0f);
            }
            //DOUBLE DOWN 25%
            else
            {
                position[0] = new Vector3(15f, -2.46f, 0f);
            }
            //Default spawn time
            waitTime = defaultTime;
        }
        else if (obstacle.name == "Enemy")
        {

            //One 40%
            if (randomNumber < 40)
            {
                position[0] = new Vector3(15f, RandomY(position), 0f);
                //Default spawn time
                waitTime = defaultTime;
            }
            //Two 30%
            else if (randomNumber < 70)
            {
                position[0] = new Vector3(15f, RandomY(position), 0f);
                position[1] = new Vector3(15f, RandomY(position), 0f);
                //Default spawn time
                waitTime = defaultTime;
            }
            //Three 25%
            else if (randomNumber < 95)
            {
                float centerEnemy = RandomY(position);

                //Centralizing convoy
                if (centerEnemy > 2.15)
                {
                    centerEnemy = 2.15f;
                }
                if(centerEnemy < -2.15)
                {
                    centerEnemy = -2.15f;
                }

                position[0] = new Vector3(15f, centerEnemy + 2.5f, 0f);
                position[1] = new Vector3(14f, centerEnemy, 0f);
                position[2] = new Vector3(15f, centerEnemy - 2.5f, 0f);
                //Default spawn time
                waitTime = defaultTime;
            }
            //Boss 5%
            else
            {
                position[0] = new Vector3(23f, 4f, 0f);
                position[1] = new Vector3(21f, 3f, 0f);
                position[2] = new Vector3(19f, 2f, 0f);
                position[3] = new Vector3(17f, 1f, 0f);
                position[4] = new Vector3(15f, 0f, 0f);
                position[5] = new Vector3(17f, -1f, 0f);
                position[6] = new Vector3(19f, -2f, 0f);
                position[7] = new Vector3(21f, -3f, 0f);
                position[8] = new Vector3(23f, -4f, 0f);
                //Delays next spawn
                waitTime = hardTime;
            }
        }
        else if (obstacle.name == "Loop")
        {
            position[0] = new Vector3(20f, 0f, 0f);
            //Delays next spawn
            waitTime = hardTime;
        }

        return position;
    }

    float RandomY(Vector3[] position)
    {
        bool differentPos = true;
        float randomY = 0;

        //Check if two enemies are in the same spot
        while (differentPos)
        {
            randomY = UnityEngine.Random.Range(-4.6f, 4.6f);

            if (position == null)
            {
                break;
            }

            foreach (Vector3 y in position)
            {
                if (Math.Abs(randomY - y.y) < 0.4f)
                {
                    differentPos = true;
                    break;
                }
                differentPos = false;
            }
        }
        return randomY;
    }
}
