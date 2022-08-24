using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static Unity.Burst.Intrinsics.Arm;
using static UnityEditor.Experimental.GraphView.GraphView;
using Color = UnityEngine.Color;

public class NetworkUI : MonoBehaviour
{
    public float startX;
    public float startY;

    public float neuronDistanceX;
    public float neuronDistanceY;

    public GameObject neuronCircle;
    private List<Neuron> neuronData;
    private List<Link> linkData;

    public Material material;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Build(Layer[] network)
    {
        neuronData = new();
        linkData = new();


        float x = startX;
        float y = startY;

        //Create neurons by layer
        foreach (Layer layer in network)
        {
            //Neurons from layer
            for (int neuronIndex = 0; neuronIndex < layer.neuron.Count; neuronIndex++)
            {
                Neuron neuron = layer.neuron[neuronIndex];

                //Show at most 8 neurons, one of each sensor
                if (neuronIndex < 6 || neuronIndex == 26 || neuronIndex == 47)
                {
                    //Centralizing neurons
                    if (neuronIndex != 0)
                    {
                        y -= neuronDistanceY;
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
            }

            //Links from layer
            for (int linkIndex = 0; linkIndex < layer.link.Count; linkIndex++)
            {
                Link link = layer.link[linkIndex];

                //Only one neuron of each sensor, 5 links from each sensor
                if (linkIndex < 31 || (linkIndex > 130 && linkIndex < 136) || (linkIndex > 235 && linkIndex < 241) || linkIndex > 340)
                {
                    //Saves links for later
                    linkData.Add(link);
                }
            }

            //Separate neurons
            x += neuronDistanceX;
            y = (float)(startY - neuronDistanceY * 1.5);
        }


        //Create links
        int index = 0;
        foreach (Neuron neuron1 in neuronData)
        {
            foreach (Neuron neuron2 in neuronData)
            {
                //Check if neuron2 is in the next layer
                if (neuron1.layer.layerId == neuron2.layer.layerId - 1)
                {
                    //Width is based on the weight of the link
                    float weight = Mathf.Abs(linkData[index].weight / 100);
                    float width = 0.06f * weight;
                    if (width < 0.02f) { width = 0.02f; }

                    //Color is based on how positive or negative the weight is
                    Color color;
                    if (linkData[index].weight < 0)
                    {
                        color = new(1, 1 - weight, 1 - weight);
                    } 
                    else if (linkData[index].weight > 0)
                    {
                        color = new(1 - weight, 1 - weight, 1);
                    } 
                    else
                    {
                        color = new(1, 1, 1);
                    }

                    //Save render object in the link
                    linkData[index].render = DrawLine(neuron1.render.transform.position, neuron2.render.transform.position, color, width);
                    index++;
                }
            }
        }
    }

    private GameObject DrawLine(Vector3 start, Vector3 end, Color color, float width)
    {
        GameObject myLine = new GameObject();
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
