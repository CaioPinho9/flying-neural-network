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
    public bool shot;
    
    //Input
    [Header("Input")]
    public float x;
    public float y;
    public float speed;
    public float degrees;
    public bool recharged;

    //Sensor
    public float[] sensorX = new float[21];
    public float[] sensorY = new float[21];
    public int[] sensorTargetType = new int[21];

    private GameObject plane;

    // Start is called before the first frame update
    void Start()
    {
        plane = transform.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        Input();
        Forward();
    }

    private void Input()
    {
        x = plane.transform.position.x;
        y = plane.transform.position.y;
        speed = plane.GetComponent<Plane>().speed;
        degrees = plane.GetComponent<Plane>().degrees;
        recharged = plane.GetComponent<Plane>().recharged;
        sensorX = plane.GetComponent<Plane>().sensorX;
        sensorY = plane.GetComponent<Plane>().sensorY;
        sensorTargetType = plane.GetComponent<Plane>().sensorTargetType;
    }

    private void Forward()
    {
        Rede rede = new(x, y, speed, degrees, recharged, sensorX, sensorY, sensorTargetType);
        rede.Link();
        rede.Forward();

    }
}
class Rede
{
    float x;
    float y;
    float speed;
    float degrees;
    bool recharged;
    float[] sensorX = new float[21];
    float[] sensorY = new float[21];
    int[] sensorTargetType = new int[21];

    public Rede(float x, float y, float speed, float degrees, bool recharged, float[] sensorX, float[] sensorY, int[] sensorTargetType)
    {
        this.x = x;
        this.y = y;
        this.speed = speed;
        this.degrees = degrees;
        this.recharged = recharged;
        this.sensorX = sensorX;
        this.sensorY = sensorY;
        this.sensorTargetType = sensorTargetType;
    }

    Layer[] layer = { new(67), new(5), new(5), new(5) };

    private void Input()
    {

        layer[0].neuron[0].output = x;
        layer[0].neuron[1].output = y;
        layer[0].neuron[2].output = speed;
        layer[0].neuron[3].output = degrees;
        layer[0].neuron[4].output = Convert.ToInt32(recharged);

        for (int index = 5; index < sensorX.Length + sensorY.Length + sensorTargetType.Length; index++)
        {
            if (index < sensorX.Length) {
                layer[0].neuron[index].output = sensorX[index];
            } 
            else if (index < sensorX.Length + sensorY.Length)
            {
                layer[0].neuron[index].output = sensorY[index];
            }
            else
            {
                layer[0].neuron[index].output = sensorTargetType[index];
            }
        }

    }

    public void Link()
    {
        for (int index = 0; index < layer.Length - 1; index++)
        {
            layer[index].Link(layer[index], layer[index + 1]);
        }
    }

    public void Forward()
    {
        for (int index = 0; index < layer.Length - 1; index++)
        {
            layer[index].Forward(layer[index]);
        }
    }
}

class Layer
{
    int neuronCount;
    public List<Neuron> neuron = new();
    public List<Link> link = new();

    public Layer(int neuronCount)
    {
        this.neuronCount = neuronCount;
    }

    public void Link(Layer thisLayer, Layer nextLayer)
    {
        for (int thisIndex = 0; thisIndex < thisLayer.neuronCount; thisIndex++)
        {
            for (int nextIndex = 0; nextIndex < nextLayer.neuronCount; nextIndex++)
            {
                thisLayer.neuron.Add(new());
                nextLayer.neuron.Add(new());

                link.Add(new Link(thisLayer.neuron[thisIndex], nextLayer.neuron[nextIndex], UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-100f, 100f)));
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
class Link
{
    Neuron neuron1;
    Neuron neuron2;
    float weight;
    float bias;
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
class Neuron
{
    public float input;
    public float output;

    public float ReLU()
    {
        if (input > 0)
        {
            Debug.Log(output);

            return output;
        }
        Debug.Log(0);
        return 0;
    }
}


