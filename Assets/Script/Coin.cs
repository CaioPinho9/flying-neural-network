using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public float speed;
    private GameObject collect;
    // Start is called before the first frame update
    void Start()
    {
        //Find object with collected animation
        collect = GameObject.Find("Collect");
        transform.parent = null;
    }
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
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !collision.GetComponent<Player>().gameOver)
        {
            //Coins have a great impact in the score
            collision.transform.GetComponent<Player>().score += 200000;

            //Open loop exit
            Destroy(GameObject.Find("Exit"));

            //Reduce coin animation
            GetComponent<Animator>().Play("Collect Coin", -1, 0f);

            //Collected animation
            collect.GetComponent<Renderer>().enabled = true;
            collect.GetComponent<Animator>().enabled = true;

            //Destroy coin
            Destroy(gameObject, 1f);
        }
    }
}
