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

    //Timer
    private float seconds;
    private float lastSecond;
    private GameObject timer;

    private GameObject plane;
    private ArrayList input;
    public Network network = null;

    public float xInput;
    public float xNeuron;

    // Start is called before the first frame update
    public void Start()
    {
        //Timer
        timer = GameObject.Find("Timer");
        seconds = timer.GetComponent<Timer>().seconds;
        lastSecond = 0;

        //Plane
        plane = transform.parent.gameObject;
        if (network == null)
        {
            //Data from plane
            InputData();
            //Data to neural network
            network = new(input);
            //Create all neurons
            network.CreateNeurons();
            //Connect layers
            network.LinkLayers();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        seconds = timer.GetComponent<Timer>().seconds;

        if (!plane.GetComponent<Plane>().gameOver)
        {
            if (seconds - lastSecond > .5f)
            {
                RunNetwork();
                lastSecond = seconds;
            }
            Control();
        }
    }

    private void RunNetwork()
    {
        network.Clear();
        InputData();
        network.InputUpdate(input);
        network.Forward();
        Output();
    }

    private void InputData()
    {
        plane = transform.parent.gameObject;

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
        xInput = (float)input[0];
    }

    private void Output()
    {
        /*
        Debug.Log("Input");
        Debug.Log(network.layer[0].neuron[0].output);
        Debug.Log(network.layer[0].neuron[1].output);
        Debug.Log(network.layer[0].neuron[2].output);
        Debug.Log(network.layer[0].neuron[3].output);
        Debug.Log(network.layer[0].neuron[4].output);
        Debug.Log("Output");
        Debug.Log(network.layer[4].neuron[0].output);
        Debug.Log(network.layer[4].neuron[1].output);
        Debug.Log(network.layer[4].neuron[2].output);
        Debug.Log(network.layer[4].neuron[3].output);
        Debug.Log(network.layer[4].neuron[4].output);
        */

        left = (network.layer[4].neuron[0].output > 0) ? 1 : 0;
        right = (network.layer[4].neuron[1].output > 0) ? 1 : 0;
        up = (network.layer[4].neuron[2].output > 0) ? 1 : 0;
        down = (network.layer[4].neuron[3].output > 0) ? 1 : 0;
        shot = (network.layer[4].neuron[4].output > 0) ? 1 : 0;

        /*
        Debug.Log("Control");
        Debug.Log(left);
        Debug.Log(right);
        Debug.Log(up);
        Debug.Log(down);
        Debug.Log(shot);
        */

    }

    private void Control()
    {
        plane.GetComponent<Plane>().vertical = -left + right;
        plane.GetComponent<Plane>().horizontal = -down + up;
        plane.GetComponent<Plane>().shot = (shot > 0);
    }
}
public class Network
{
    //Layers(Number of neurons, index of layer)
    public Layer[] layer = { new(67, 0), new(26, 1), new(52, 2), new(26, 3), new(5, 4) };
    //Data from plane
    public ArrayList input;
    public string dna;
    private float mutate = 10;

    public Network(ArrayList input)
    {
        //Receive data
        this.input = input;

        //Send data to input layer
        InputNeuron();
    }

    public String Copy()
    {
        dna = "";
        foreach (Layer layer in layer)
        {
            foreach (Link link in layer.link)
            {
                dna += link.weight + "/" + link.bias + ";";
            }
        }
        return dna;
    }

    public void Paste(String dna)
    {
        String[] rna = dna.Split(";");
        int index = 0;
        //Debug.Log("Dna");
        //Debug.Log(dna);
        foreach (Layer layer in layer)
        {
            foreach (Link link in layer.link)
            {
                String[] gene = rna[index].Split("/");
                link.weight = float.Parse(gene[0]);
                link.bias = float.Parse(gene[1]);
                //Debug.Log("Gene");
                //Debug.Log(gene[0]);
                //Debug.Log(gene[1]);
                index++;
            }
        }
    }

    public void CreateUI(GameObject plane)
    {
        //Create first UI, after creating the neurons
        GameObject.Find("UI").GetComponent<NetworkUI>().Build(plane);
    }

    public void InputNeuron()
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

    public void InputUpdate(ArrayList input)
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
            if (index == 0)
            {
                layer[index].LinkSensors(layer[index], layer[index + 1]);
            }
            else
            {
                layer[index].LinkNeurons(layer[index], layer[index + 1]);
            }
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

    public void Forward()
    {
        for (int index = 0; index < layer.Length; index++)
        {
            layer[index].Forward(layer[index]);
        }
    }

    public void Mutate()
    {
        foreach (Layer layer in layer)
        {
            foreach (Link link in layer.link)
            {
                float random = UnityEngine.Random.Range(-mutate, mutate);
                if (link.weight < mutate && link.weight > -mutate)
                {
                    link.weight += random;
                } 
                else if (link.weight >= mutate)
                {
                    link.weight -= Math.Abs(random);
                } 
                else
                {
                    link.weight += Math.Abs(random);
                }

                random = UnityEngine.Random.Range(-mutate, mutate);
                if (link.bias < mutate && link.bias > -mutate)
                {
                    link.bias += random;
                }
                else if (link.weight >= mutate)
                {
                    link.bias -= Math.Abs(random);
                }
                else
                {
                    link.bias += Math.Abs(random);
                }
            }
        }
    }

}

public class Layer
{
    //How many neurons there's in this layer
    public readonly int neuronCount;
    //Id
    public int layerId;
    //Neurons in this layer
    public List<Neuron> neuron = new();
    //Connections that start in this layer
    public List<Link> link = new();
    private float randomStart = 100;

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

    public void LinkSensors(Layer thisLayer, Layer nextLayer)
    {

        //Iterates to connect every neuron in layer 1 with each neuron in layer 2
        int nextIndex = 0;
        for (int thisIndex = 0; thisIndex < 26; )
        {
            if (thisIndex < 6)
            {
                link.Add(new Link(thisLayer.neuron[thisIndex], nextLayer.neuron[thisIndex], UnityEngine.Random.Range(-randomStart, randomStart), UnityEngine.Random.Range(-randomStart, randomStart)));
            }
            else
            {
                link.Add(new Link(thisLayer.neuron[thisIndex], nextLayer.neuron[nextIndex], UnityEngine.Random.Range(-randomStart, randomStart), UnityEngine.Random.Range(-randomStart, randomStart)));
                link.Add(new Link(thisLayer.neuron[thisIndex + 21], nextLayer.neuron[nextIndex], UnityEngine.Random.Range(-randomStart, randomStart), UnityEngine.Random.Range(-randomStart, randomStart)));
                link.Add(new Link(thisLayer.neuron[thisIndex + 42], nextLayer.neuron[nextIndex], UnityEngine.Random.Range(-randomStart, randomStart), UnityEngine.Random.Range(-randomStart, randomStart)));
            }
            thisIndex++;
            nextIndex++;
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

        if (layer.layerId < 4)
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
            return input;
        }
        return 0;
    }
}