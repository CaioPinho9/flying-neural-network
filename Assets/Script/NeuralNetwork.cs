using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork : MonoBehaviour
{
    //Output
    [Header("Output")]
    public float left;
    public float right;
    public float up;
    public float down;
    public float shot;
    
    //Input
    [Header("Input")]
    public float x;
    public float y;
    public float speed;
    public float degrees;
    public float recharged;

    //Sensor
    public float[] sensorX = new float[21];
    public float[] sensorY = new float[21];
    public float[] sensorTargetType = new float[21];

    private GameObject plane;
    private ArrayList input;
    public Network network;

    // Start is called before the first frame update
    void Start()
    {
        plane = transform.parent.gameObject;
        //Data from plane
        InputData();
        //Data to neural network
        network = new(input);
        //Create all neurons
        network.CreateNeurons();
        //Connect layers
        network.LinkLayers();
    }

    // Update is called once per frame
    void Update()
    {
        InputData();
    }

    private void InputData()
    {
        //Receive data from plane
        x = plane.transform.position.x;
        y = plane.transform.position.y;
        speed = plane.GetComponent<Plane>().speed;
        degrees = plane.GetComponent<Plane>().degrees;
        recharged = (float)Convert.ToInt32(plane.GetComponent<Plane>().recharged);
        sensorX = plane.GetComponent<Plane>().sensorX;
        sensorY = plane.GetComponent<Plane>().sensorY;
        sensorTargetType = Array.ConvertAll(plane.GetComponent<Plane>().sensorTargetType, item => (float) item);

        //Organize data in array to sort the inputs
        input = new()
        {
            x,
            y,
            speed,
            degrees,
            recharged,
            sensorX,
            sensorY,
            sensorTargetType
        };
    }
}
public class Network
{
    //Layers(Number of neurons, index of layer)
    public Layer[] layer = { new(67, 0), new(5, 1), new(5, 2), new(5, 3) };
    //Data from plane
    private readonly ArrayList input;

    public Network(ArrayList input)
    {
        //Receive data
        this.input = input;

        //Send data to input layer
        InputNeuron();
    }

    private void CreateUI()
    {
        //Create first UI, after creating the neurons
        GameObject.Find("UI").GetComponent<NetworkUI>().Build(layer);
    }

    private void InputNeuron()
    {
        //totalIndex is inputIndex + sensors arrays indexs
        //Ex: x,x,x,x,x,[x,x,x],[x,x,x],[x,x,x] inputIndex = 8, totalIndex = 14
        int totalIndex = 0;
        for (int inputIndex = 0; inputIndex < input.Count; inputIndex++)
        {
            if (!input[inputIndex].GetType().IsArray)
            {
                //A neuron in input layer receives an input value
                layer[0].neuron.Add(new(layer[0]));
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
                    layer[0].neuron.Add(new(layer[0]));
                    layer[0].neuron[totalIndex].output = sensors[sensorIndex];
                    totalIndex++;
                }
            }
        }
    }

    public void CreateNeurons()
    {
        //Create neurons to each layer
        foreach (Layer layer in layer)
        {
            layer.CreateNeurons();
        }
    }

    public void LinkLayers()
    {
        //Layer 1 connects with 2, etc
        for (int index = 0; index < layer.Length - 1; index++)
        {
            layer[index].LinkNeurons(layer[index], layer[index + 1]);
        }
        //Activate UI
        CreateUI();
    }

    public void Forward()
    {
        for (int index = 0; index < layer.Length - 1; index++)
        {
            layer[index].Forward(layer[index]);
        }
    }
}

public class Layer
{
    //How many neurons there's in this layer
    private readonly int neuronCount;
    //Id
    public int layerId;
    //Neurons in this layer
    public List<Neuron> neuron = new();
    //Connections that start in this layer
    public List<Link> link = new();

    public Layer(int neuronCount, int layerId)
    {
        this.neuronCount = neuronCount;
        this.layerId = layerId;
    }

    public void CreateNeurons()
    {
        //Create neurons, limiting to how many must be
        for (int index = neuron.Count; index < neuronCount; index++)
        {
            neuron.Add(new(this));
        }
    }

    public void LinkNeurons(Layer thisLayer, Layer nextLayer)
    {
        //Iterates to connect every neuron in layer 1 with each neuron in layer 2
        for (int thisIndex = 0; thisIndex < thisLayer.neuronCount; thisIndex++)
        {
            for (int nextIndex = 0; nextIndex < nextLayer.neuronCount; nextIndex++)
            {
                //Create link between neurons
                link.Add(new Link(thisLayer.neuron[thisIndex], nextLayer.neuron[nextIndex], UnityEngine.Random.Range(-100f, 100f), UnityEngine.Random.Range(-100f, 100f)));
            }
        }
    }

    public void Forward(Layer layer)
    {
        for (int index = 0; index < layer.link.Count; index++)
        {
            layer.link[index].Weight();
        }

        for (int index = 0; index < layer.neuron.Count; index++)
        {
            layer.neuron[index].ReLU();
        }
    }
}
public class Link
{
    //UI reference
    public GameObject render;

    //Beginning of link
    private Neuron neuron1;
    //Ending
    private Neuron neuron2;

    //Used to proccess data
    public float weight;
    private float bias;
    public Link(Neuron neuron1, Neuron neuron2, float weight, float bias)
    {
        this.neuron1 = neuron1;
        this.neuron2 = neuron2;
        this.weight = weight;
        this.bias = bias;
    }

    public void Weight()
    {
        neuron2.input += neuron1.output * weight + bias;
    }
}
public class Neuron
{
    //UI reference
    public GameObject render;

    //Where this neuron is located
    public Layer layer;

    //Input and output
    public float input = 0;
    public float output = 0;

    public Neuron(Layer layer)
    {
        this.layer = layer;
    }

    //Rectifier
    public float ReLU()
    {
        //Activate if input > 0
        if (input > 0)
        {
            return output;
        }
        return 0;
    }
}