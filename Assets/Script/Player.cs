using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class Player : MonoBehaviour
{
    //Input
    [Header("Input")]
    public float vertical;
    public float horizontal;
    public bool shot;

    //Sensor
    [Header("Sensors")]
    public float[] sensorDistance = new float[24];
    public float sensorCoin = 0;
    private readonly float sensorLenght = 20f;
    private readonly float[] sensorDegrees = { 0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 90, 135, 180 };
    private readonly Vector3[] sensorDirection = new Vector3[24];

    //Movement
    [Header("Movement")]
    public float speed = 2f;
    public float degrees = 0;
    public float angle;

    [Header("Gameplay")]
    //Gameplay
    public float recharged = 1;
    public bool gameOver = false;
    public float score = 0;

    [Header("Identity")]
    public int id;
    public string dna;
    public string rna = "";
    public bool visible = false;

    //Timer
    public float time;
    public float queueTime = .5f;

    //References
    private Rigidbody2D rigidbody;
    public NeuralNetwork network;
    public GameObject missile;
    private Animator anim;
    public LayerMask layerMask;
    public LayerMask layerCoin;

    // Start is called before the first frame update
    public void Start()
    {
        //Control Components
        anim = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody2D>();
        //Start at position
        transform.position = new(-10, 0, 0);
        //Create the neural network
        network = new();
        //Don't rotate without a command
        rigidbody.freezeRotation = true;

        if (dna == "")
        {
            //Set dna in this object
            dna = network.dna;
            rna = dna;
        }
    }

    public void Restart()
    {
        //Change color and sortingOrder
        GetComponent<SpriteRenderer>().color = new(1f, 1f, 1f, visible ? 1f : .2f);
        GetComponent<SpriteRenderer>().sortingOrder = 1;
        GetComponent<Renderer>().enabled = true;

        //Enable Animation
        GetComponent<Animator>().enabled = true;
        //Enable Movement
        rigidbody.isKinematic = false;
        //Reset position and angle
        transform.position = new(-10, 0, 0);
        transform.eulerAngles = new(0, 0, 0);
        //Reset dying explosion
        GameObject boom = transform.GetChild(0).gameObject;
        boom.GetComponent<Renderer>().enabled = false;
        boom.GetComponent<Animator>().enabled = false;
        //Set alive
        gameOver = false;
        //Reset speed, angle, recharge and score
        speed = 2f;
        degrees = 0;
        recharged = 1;
        score = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //Until losing
        if (!gameOver)
        {
            //When dna receives a input, changes the network
            if (rna != dna)
            {
                rna = dna;
                network.Paste(dna);
            }

            //Timer
            if (time > queueTime)
            {
                //Set visibility
                GetComponent<SpriteRenderer>().color = new(1f, 1f, 1f, visible ? 1f : .2f);

                //Die if leaves area
                if (Math.Abs(transform.position.x) > 12 || Math.Abs(transform.position.y) > 6)
                {
                    Death();
                }

                //Check every obstacle
                GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
                if (obstacles != null)
                {
                    foreach (GameObject obstacle in obstacles)
                    {
                        if (obstacle.transform.position.x < transform.position.x)
                        {
                            //Add 100 to score if this player is in obstacle right
                            score += 100;
                        }
                    }
                }

                //Update sensors
                Sensors();

                //Calculate how to move
                RunNetwork();

                Fire();
                //Change angle
                Pitch();

                Move();
                
                time = 0;
            }
            //Increases time and score
            time += Time.deltaTime;
            score += Time.deltaTime;

            //Animate
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
            else if(sensorDegrees[i] == 180)
            {
                //Direction is a Vector2, with range between 0 and 1
                sensorDirection[^1] = new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0);
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
        float[] distance = new float[24];

        //Create a raycast
        int sensorIndex = 0;
        foreach (Vector3 direction in sensorDirection)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, sensorLenght, ~layerMask);
            if (hit.transform != null)
            {
                //Saving raycast hit position and object type
                distance[sensorIndex] = GameController.Distance(hit.point.x, hit.point.y, transform.position.x, transform.position.y);

                Debug.DrawLine(transform.position, hit.point);
            } 
            else
            {
                distance[sensorIndex] = 0;
            }

            sensorIndex++;
        }

        angle = (float)Math.PI * degrees / 180;
        Vector3 coinDirection = new((float)Math.Cos(angle), (float)Math.Sin(angle), 0);
        RaycastHit2D hitCoin = Physics2D.Raycast(transform.position, coinDirection, sensorLenght, layerCoin);
        if (hitCoin.transform != null)
        {
            //Saving raycast hit position and object type
            sensorCoin = GameController.Distance(hitCoin.point.x, hitCoin.point.y, transform.position.x, transform.position.y);

            Debug.DrawLine(transform.position, hitCoin.point);
        }
        else
        {
            sensorCoin = 0;
        }

        //Global Output
        sensorDistance = distance;
    }

    private void Move()
    {
        //Limiting speed
        if ((horizontal > 0 && speed < 10) || (horizontal < 0 && speed > 0f))
        {
            speed += horizontal / 5;
            if (speed < 0) { speed = 0; }
        }

        //Move
        transform.position += speed * Time.deltaTime * transform.right;
    }

    private void Pitch()
    {
        //Increasing the input
        degrees += vertical * 3f;

        //Change angle of plane
        transform.eulerAngles = new(0f, 0f, degrees);
    }

    private void Fire()
    {
        //Manual and automatic input, shot when recharged
        if ((Input.GetAxis("Fire") > 0 || shot) && recharged == 1)
        {
            //Create missile
            recharged = 0;
            Missile();
        }

        //Remove and add missile in animation
        anim.SetBool("recharged", recharged == 1);
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
        missileInstantiated.GetComponent<Missile>().player = transform.gameObject;
    }

    private void Death()
    {
        //Activate explosion animation
        GameObject boom = transform.GetChild(0).gameObject;
        boom.GetComponent<Renderer>().enabled = true;
        boom.GetComponent<Animator>().enabled = true;

        rigidbody.isKinematic = true;
        rigidbody.velocity = Vector3.zero;
        GetComponent<SpriteRenderer>().color = Color.black;
        GetComponent<SpriteRenderer>().sortingOrder = 0;
        GetComponent<Animator>().enabled = false;

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

    private ArrayList InputData()
    {
        //Receive data from plane
        speed = GetComponent<Player>().speed;
        recharged = GetComponent<Player>().recharged;
        sensorDistance = GetComponent<Player>().sensorDistance;
        sensorCoin = GetComponent<Player>().sensorCoin;

        //Organize data in array to sort the inputs
        return new()
        {
            speed,
            recharged,
            sensorCoin,
            sensorDistance,
        };
    }

    private void RunNetwork()
    {
        network.Clear();
        network.Input(InputData());
        network.Forward();

        float left = (network.layer[network.lastLayer - 1].neuron[0].output > 0) ? 1 : 0;
        float right = (network.layer[network.lastLayer - 1].neuron[1].output > 0) ? -1 : 0;
        float up = (network.layer[network.lastLayer - 1].neuron[2].output > 0) ? 1 : 0;
        float down = (network.layer[network.lastLayer - 1].neuron[3].output > 0) ? -1 : 0;
        shot = (network.layer[network.lastLayer - 1].neuron[4].output > 0);

        GetComponent<Player>().vertical = up + down;
        GetComponent<Player>().horizontal = left + down;
        GetComponent<Player>().shot = shot;
    }
}

public class NeuralNetwork
{
    public static int[] neuronsLayer = { 27, 8, 5 };
    public int lastLayer = neuronsLayer.Length;

    public Layer[] layer = new Layer[neuronsLayer.Length];

    public string dna;
    public static int weightLimit = 1000;
    public int mutate = 10;

    public NeuralNetwork()
    {
        for (int i = 0; i < neuronsLayer.Length; i++)
        {
            layer[i] = new()
            {
                //How many neurons in this layer
                neuronCount = neuronsLayer[i],
                //Which layer is this
                layerId = i,
                //Range of link weight
                randomStart = weightLimit,
                //How long is the network
                networkSize = neuronsLayer.Length
            };
        }
        CreateNeurons();
        LinkLayers();
        dna = Copy();
    }

    public string Copy()
    {
        dna = "";
        //Weight/Bias;Weight/Bias;Weight/Bias;
        //";" separates different links, and "/" separates weight and bias
        foreach (Layer layer in layer)
        {
            foreach (Link link in layer.link)
            {
                dna += link.weight.ToString("0.00") + "/" + link.bias.ToString("0.00") + ";";
            }
        }
        return dna;
    }

    public void Paste(string dna)
    {
        string[] rna = dna.Split(";");
        int index = 0;
        //Weight/Bias;Weight/Bias;Weight/Bias;
        //";" separates different links, and "/" separates weight and bias
        foreach (Layer layer in layer)
        {
            foreach (Link link in layer.link)
            {
                string[] gene = rna[index].Split("/");
                link.weight = float.Parse(gene[0]);
                link.bias = float.Parse(gene[1]);
                index++;
            }
        }
    }

    public void Mutate()
    {
        foreach (Layer layer in layer)
        {
            foreach (Link link in layer.link)
            {
                MutateLink(link, RandomNumber(mutate), RandomNumber(mutate));
            }
        }
        dna = Copy();
    }

    public void MutateLink(Link link, float randomW, float randomB)
    {
        if (link.weight < weightLimit && link.weight > -weightLimit)
        {
            link.weight += randomW;
        }
        else if (link.weight >= weightLimit)
        {
            link.weight -= Math.Abs(randomW);
        }
        else
        {
            link.weight += Math.Abs(randomW);
        }

        if (link.bias < mutate && link.bias > -mutate)
        {
            link.bias += randomB;
        }
        else if (link.weight >= mutate)
        {
            link.bias -= Math.Abs(randomB);
        }
        else
        {
            link.bias += Math.Abs(randomB);
        }
    }

    public void Random()
    {
        foreach (Layer layer in layer)
        {
            foreach (Link link in layer.link)
            {
                float random = RandomNumber(weightLimit);
                link.weight = random;

                random = RandomNumber(weightLimit);
                link.bias = random;
            }
        }
    }

    public static float RandomNumber(float limit)
    {
        return (float)UnityEngine.Random.Range(-limit, limit) + (float)UnityEngine.Random.Range(-100, 100) / 100;
    }

    public void CreateNeurons()
    {
        int neuronId = 0;
        //Create neurons to each layer
        foreach (Layer layer in layer)
        {
            neuronId = layer.CreateNeurons(neuronId);
        }
    }

    public void LinkLayers()
    {
        //Layer 1 connects with 2, etc
        for (int index = 0; index < layer.Length - 1; index++)
        {
            layer[index].LinkNeurons(layer[index], layer[index + 1]);
        }
    }

    public void Clear()
    {
        foreach (Layer layer in layer)
        {
            if (layer.layerId > 0)
            {
                //Reset neuron input
                for (int index = 0; index < layer.neuron.Count; index++)
                {
                    layer.neuron[index].input = 0;
                    layer.neuron[index].output = 0;
                }
            }
        }
    }

    public void Input(ArrayList input)
    {
        //totalIndex is inputIndex + sensors arrays indexs
        //Ex: x,x,x,x,x,[x,x,x],[x,x,x],[x,x,x] inputIndex = 8, totalIndex = 14
        int totalIndex = 0;
        for (int inputIndex = 0; inputIndex < input.Count; inputIndex++)
        {
            if (!input[inputIndex].GetType().IsArray)
            {
                //A neuron in input layer receives an input value
                layer[0].neuron[inputIndex].output = (float)input[inputIndex];
                totalIndex++;
            }
            else
            {
                //Iterating the sensors
                float[] sensors = (float[])input[inputIndex];
                for (int sensorIndex = 0; sensorIndex < sensors.Length; sensorIndex++)
                {
                    //A neuron in input layer receives an input value
                    layer[0].neuron[totalIndex].output = sensors[sensorIndex];
                    totalIndex++;
                }
            }
        }
    }

    public void Forward()
    {
        for (int index = 0; index < layer.Length; index++)
        {
            layer[index].Forward(layer[index]);
        }
    }
}

public class Layer
{
    //How many neurons there's in this layer
    public int neuronCount;
    //Id
    public int layerId;
    //Last layer
    public int networkSize;
    //Neurons in this layer
    public List<Neuron> neuron = new();
    //Connections that start in this layer
    public List<Link> link = new();
    public int randomStart;

    public Layer() { }

    public int CreateNeurons(int neuronId)
    {
        //Create neurons, limiting to how many must be
        for (int index = neuron.Count; index < neuronCount; index++)
        {
            neuron.Add(new(this, neuronId));
            neuronId++;
        }
        return neuronId;
    }

    public void LinkNeurons(Layer thisLayer, Layer nextLayer)
    {
        //Iterates to connect every neuron in layer 1 with each neuron in layer 2
        for (int thisIndex = 0; thisIndex < thisLayer.neuronCount; thisIndex++)
        {
            for (int nextIndex = 0; nextIndex < nextLayer.neuronCount; nextIndex++)
            {
                //Create link between neurons
                link.Add(new Link(thisLayer.neuron[thisIndex], nextLayer.neuron[nextIndex], UnityEngine.Random.Range(-randomStart, randomStart), UnityEngine.Random.Range(-randomStart, randomStart)));
            }
        }
    }
    public void Forward(Layer layer)
    {
        if (layer.layerId > 0)
        {
            for (int index = 0; index < layer.neuron.Count; index++)
            {
                //Debug.Log("ReLU");
                //Debug.Log(layer.neuron[index].input);
                layer.neuron[index].output = layer.neuron[index].ReLU();
                //Debug.Log(layer.neuron[index].output);
            }
        }

        if (layer.layerId < networkSize)
        {
            for (int index = 0; index < layer.link.Count; index++)
            {
                //Debug.Log("Weight");
                //Debug.Log(layer.link[index].neuron1.output);
                layer.link[index].neuron2.input += layer.link[index].Weight();
                //Debug.Log(layer.link[index].neuron2.input);
            }
        }
    }

}

public class Link
{
    //UI reference
    public GameObject render;

    //Beginning of link
    public Neuron neuron1;
    //Ending
    public Neuron neuron2;

    //Used to proccess data
    public float weight;
    public float bias;
    public Link(Neuron neuron1, Neuron neuron2, float weight, float bias)
    {
        this.neuron1 = neuron1;
        this.neuron2 = neuron2;
        this.weight = weight;
        this.bias = bias;
    }

    public float Weight()
    {
        return neuron1.output * weight + bias;
    }
}
public class Neuron
{
    //UI reference
    public GameObject render;

    //Where this neuron is located
    public Layer layer;
    public int neuronId;

    //Input and output
    public float input = 0;
    public float output = 0;

    public Neuron(Layer layer, int neuronId)
    {
        this.layer = layer;
        this.neuronId = neuronId;
    }

    //Rectifier
    public float ReLU()
    {
        //Activate if input > 0
        if (input > 0)
        {
            return input;
        }
        return 0;
    }
}
