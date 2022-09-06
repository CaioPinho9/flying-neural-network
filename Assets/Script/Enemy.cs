using UnityEngine;

public class Enemy : MonoBehaviour
{
    //Movement
    public float speed;

    //Gameplay
    public bool recharged;

    //Time
    public float time;
    public float shotTime;
    private GameObject controller;

    //Animation
    public GameObject shot;

    //Bullet
    public GameObject ammo;
    private GameObject ammoInstantiated;

    // Start is called before the first frame update
    void Start()
    {
        controller = GameObject.Find("GameController");
        shot = transform.GetChild(0).gameObject;
        recharged = true;
    }

    // Update is called once per frame
    void Update()
    {
        //Speed increases by time, timer in controller
        speed = controller.GetComponent<GameController>().speed / -2;
        time = GameObject.Find("Timer").GetComponent<Timer>().seconds;
       
        //If ammo doesn't exist, enemy is recharged
        if (!ammoInstantiated)
        { 
            recharged = true;
        }

        //Shot animation stops after .5s
        if (time - shotTime > 0.5f)
        {
            shot.GetComponent<Renderer>().enabled = false;
        }

        //Enemy is recharged after 4.5s
        if (time - shotTime > 4.5f)
        {
            recharged = true;
        }

        //While visible and recharged, enemy will shot
        if (GetComponent<Renderer>().isVisible && recharged)
        {
            recharged = false;
            Shot();
        }

        //Enemy is destroyed after leaving area
        if (transform.position.x < -20)
        {
            Death();
        }

        //Move
        Vector3 movement = new(speed, 0f, 0f);
        transform.position += Time.deltaTime * movement;

    }

    void Shot()
    {
        //Spawn in front of the enemy, at the bottom
        Vector3 ammoPosition = new(transform.position.x - 0.2f, transform.position.y - 0.15f, transform.position.z);
        
        //Used to disable shot animation and recharge the enemy
        shotTime = time;
        
        //Spawn ammo, facing left
        ammoInstantiated = Instantiate(ammo, ammoPosition, Quaternion.Euler(new Vector3(0f, 180f, 0f)));

        //Detach from parente
        ammoInstantiated.transform.parent = null;

        //Start shot animation
        shot.GetComponent<Renderer>().enabled = true;
    }

    void Death()
    {
        //Activate explosion animation
        GameObject boom = transform.GetChild(1).gameObject;
        boom.GetComponent<Renderer>().enabled = true;
        boom.GetComponent<Animator>().enabled = true;

        //Disable enemy
        gameObject.GetComponent<Renderer>().enabled = false;

        //Destroy itself
        Destroy(gameObject, 0.53f);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        //When hit by missile, it's destroyed and player receive a bonus score
        if (collision.gameObject.CompareTag("Missile"))
        {
            collision.transform.GetComponent<Missile>().plane.GetComponent<Plane>().score += 50;
            Death();
        }
    }
}
