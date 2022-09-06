using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using Color = UnityEngine.Color;

public class NetworkUI : MonoBehaviour
{
    public float startY;
    public float startX;

    public float neuronDistanceX;
    public float neuronDistanceY;

    public GameObject neuronCircle;
    private List<Neuron> neuronData = new();
    private List<Link> linkData = new();
    private GameObject bestNetwork;
    private Layer[] network;

    public Material material;

    private bool built = false;

    public int gen = 1;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        transform.GetChild(0).GetComponent<TextMeshPro>().text = "Gen " + gen.ToString();

        if (built && bestNetwork != null)
        {
            int renderIndex = 0;
            foreach (Layer layer in bestNetwork.GetComponentInChildren<NeuralNetwork>().network.layer)
            {
                foreach (Neuron neuron in layer.neuron)
                {
                    if (neuron.output > 0)
                    {
                        neuronData[renderIndex].render.GetComponentInChildren<SpriteRenderer>().color = new(1, 0, 0);
                    }
                    else
                    {
                        neuronData[renderIndex].render.GetComponentInChildren<SpriteRenderer>().color = new(1, 1, 1);
                    }
                    renderIndex++;
                }
            }
        }
    }

    public void Build(GameObject bestNetwork)
    {
        this.bestNetwork = bestNetwork;
        this.network = bestNetwork.GetComponentInChildren<NeuralNetwork>().network.layer;

        //Clear
        if (neuronData.Count > 0 && linkData.Count > 0)
        {
            foreach (Neuron neuron in neuronData)
            {
                Destroy(neuron.render);
            }
            foreach (Link link in linkData)
            {
                Destroy(link.render);
            }
        }

        neuronData = new();
        linkData = new();

        float x = startX;
        float y = startY;

        //Create neurons by layer
        foreach (Layer layer in network)
        {
            neuronDistanceX = 6.8f/ layer.neuronCount;
            //Neurons from layer
            for (int neuronIndex = 0; neuronIndex < layer.neuron.Count; neuronIndex++)
            {
                Neuron neuron = layer.neuron[neuronIndex];

                //Centralizing neurons
                if (neuronIndex != 0)
                {
                    x -= neuronDistanceX;
                }

                //Create neuron gameobject
                neuron.render = DrawCircle(new Vector3(x, y, 0));

                //Save neuron to use later
                neuronData.Add(neuron);

                //Red if activated
                if (neuron.output > 0)
                {
                    neuron.render.GetComponentInChildren<SpriteRenderer>().color = Color.red;
                }
                
            }

            //Links from layer
            for (int linkIndex = 0; linkIndex < layer.link.Count; linkIndex++)
            {
                Link link = layer.link[linkIndex];

                //Saves links for later
                linkData.Add(link);
            }

            //Separate neurons
            x = (float)(startX - neuronDistanceX);
            y -= neuronDistanceY;
            built = true;
        }

        int nextLayerIndex = 68;
        for (int linkIndex = 0; linkIndex < network[0].link.Count; linkIndex++)
        {
            ArrayList colorWidth = ColorWidth(linkIndex);

            if (linkIndex < 5)
            {
                linkData[linkIndex].render = DrawLine(neuronData[linkIndex].render.transform.position, neuronData[nextLayerIndex].render.transform.position, (Color)colorWidth[0], (float)colorWidth[1]);
                nextLayerIndex++;
            }
            else
            {
                linkData[linkIndex].render = DrawLine(neuronData[linkIndex].render.transform.position, neuronData[nextLayerIndex].render.transform.position, (Color)colorWidth[0], (float)colorWidth[1]);
                linkIndex++;
                colorWidth = ColorWidth(linkIndex);
                linkData[linkIndex].render = DrawLine(neuronData[linkIndex].render.transform.position, neuronData[nextLayerIndex].render.transform.position, (Color)colorWidth[0], (float)colorWidth[1]);
                linkIndex++;
                colorWidth = ColorWidth(linkIndex);
                linkData[linkIndex].render = DrawLine(neuronData[linkIndex].render.transform.position, neuronData[nextLayerIndex].render.transform.position, (Color)colorWidth[0], (float)colorWidth[1]);
                nextLayerIndex++;
            }
            
        }
        

        //Create links
        int index = 0;
        foreach (Neuron neuron1 in neuronData)
        {
            foreach (Neuron neuron2 in neuronData)
            {
                if (neuron1.layer.layerId == 0)
                {
                    break;
                }
                //Check if neuron2 is in the next layer
                if (neuron1.layer.layerId == neuron2.layer.layerId - 1)
                {
                    if (index >= linkData.Count)
                    {
                        linkData.Add(new(neuron1, neuron1, Random.Range(-100, 100), Random.Range(-100, 100)));
                    }

                    ArrayList colorWidth = ColorWidth(index);

                    //Save render object in the link
                    linkData[index].render = DrawLine(neuron1.render.transform.position, neuron2.render.transform.position, (Color)colorWidth[0], (float)colorWidth[1]);
                    index++;
                }
            }
        }
    }

    private ArrayList ColorWidth(int index)
    {
        //Width is based on the weight of the link
        float weight = Mathf.Abs(linkData[index].weight / 100);
        float width = 0.03f * weight;
        if (width < 0.01f) { width = 0.01f; }

        //Color is based on how positive or negative the weight is
        Color color;
        if (linkData[index].weight < 0)
        {
            color = new(1, 1 - weight, 1 - weight, .8f);
        }
        else if (linkData[index].weight > 0)
        {
            color = new(1 - weight, 1 - weight, 1, .8f);
        }
        else
        {
            color = new(1, 1, 1, .8f);
        }
        ArrayList array = new()
        {
            color,
            width
        };
        return array;
    }

    private GameObject DrawLine(Vector3 start, Vector3 end, Color color, float width)
    {
        GameObject myLine = new();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.sortingOrder = 4;
        lr.material = new Material(material);
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        myLine.transform.parent = transform;
        return myLine;
    }

    private GameObject DrawCircle(Vector3 position)
    {
        GameObject neuron = Instantiate(neuronCircle);
        neuron.transform.position = position;
        neuron.transform.parent = transform;
        return neuron;
    }
}
