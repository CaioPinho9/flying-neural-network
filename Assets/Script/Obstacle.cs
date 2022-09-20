using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float speed;
    public int id = 0;
    private float time = 0;
    private readonly float queueTime = 1f;
    private GameObject[] players;

    private void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        //Speed increase by time, timer is in the controller
        speed = GameController.speed / -2;

        //Movement
        Vector3 movement = new(speed, 0f, 0f);
        transform.position += Time.deltaTime * movement;

        //Destroy obstacle
        if (transform.position.x < -20)
        {
            Destroy(gameObject);
        }

        //Check every obstacle
        if (time > queueTime)
        {
            foreach (GameObject player in players)
            {
                if (player.transform.position.x > transform.position.x && !player.GetComponent<Player>().gameOver)
                {
                    //Add 100 to score if this player is in obstacle right
                    player.GetComponent<Player>().score += 250;
                }
            }
            time = 0;
        }
        time += Time.deltaTime;

    }
}
