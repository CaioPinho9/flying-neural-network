using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class NeuralController : MonoBehaviour
{
    public bool restart;

    private GameObject bestPlane;
    private List<GameObject> bestPlanes;
    private GameObject[] planes;

    public int planesAlive;
    public int planeQuantity = 50;

    public GameObject plane;
    private GameController gameController;

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        Create();
        planes = GameObject.FindGameObjectsWithTag("Player");
        planesAlive = planeQuantity;
        UpdateNetwork();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        planesAlive = planeQuantity;
        planes = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject plane in planes)
        {
            planesAlive -= plane.GetComponent<Plane>().gameOver ? 1 : 0;
        }

        if (planesAlive <= 0)
        {
            Debug.Log("Restart");
            gameController.gameOver = true;
            BestPlane();
            UpdateNetwork();
            planes = GameObject.FindGameObjectsWithTag("Player");
            planesAlive = planeQuantity;
            GameObject.Find("UI").GetComponent<NetworkUI>().gen++;
        }
    }

    private void Create()
    {
        for (int id = 0; id < planeQuantity; id++)
        {
            GameObject instantiated = Instantiate(plane);
            instantiated.transform.SetParent(transform);
            instantiated.GetComponent<Plane>().id = id;
        }
    }

    private void BestPlane()
    {
        planes = GameObject.FindGameObjectsWithTag("Player");
        bestPlane = planes[0];
        bestPlanes = new();
        foreach (GameObject plane in planes)
        {
            if(bestPlane.GetComponent<Plane>().score < plane.GetComponent<Plane>().score)
            {
                bestPlane = plane;
            }

            if (bestPlanes.Count < 30)
            {
                bestPlanes.Add(plane);
            }
            else
            {
                for (int index = 0; index < bestPlanes.Count; index++)
                {
                    if (bestPlanes[index].GetComponent<Plane>().score < plane.GetComponent<Plane>().score)
                    {
                        bestPlanes[index] = plane;
                    }
                }
            }
        }
        GameObject.Find("Window Chart").GetComponent<WindowGraph>().score.Add(bestPlane.GetComponent<Plane>().score);

        foreach (GameObject plane in planes) { plane.GetComponent<Plane>().Start(); }
        DeletePlanes();
    }

    private void UpdateNetwork()
    {
        if (bestPlane == null)
        {
            planes[0].GetComponentInChildren<NeuralNetwork>().Start();
            planes[0].GetComponentInChildren<NeuralNetwork>().network.CreateUI(planes[0]);
            Debug.Log("Done");
        }
        else
        {
            bestPlane.GetComponentInChildren<NeuralNetwork>().network.CreateUI(bestPlane);
        }
    }

    private void DeletePlanes()
    {
        foreach (GameObject plane in planes)
        {
            bool destroy = true;
            for (int index = 0; index < bestPlanes.Count; index++)
            {
                if (plane.GetComponent<Plane>().id == bestPlanes[index].GetComponent<Plane>().id)
                {
                    destroy = false;
                }
            }

            if (destroy)
            {
                Destroy(plane);
            }
        }
        RecreatePlanes();
    }

    private void RecreatePlanes()
    {
        int index = 0;
        int lastIndex = -1;
        string dna = "";
        for (int id = 0; id < planeQuantity; id++)
        {
            bool useId = true;
            foreach (GameObject plane in bestPlanes)
            {
                plane.GetComponent<Plane>().opacity = 1;
                if (plane.GetComponent<Plane>().id == id)
                {
                    useId = false;
                }
            }

            if (useId)
            {
                int bestPlaneIndex = (int)Math.Floor(index / (double)(planeQuantity / 10));
                GameObject planeParent = bestPlanes[bestPlaneIndex];
                index++;
                
                if (bestPlaneIndex != lastIndex)
                {
                    dna = planeParent.GetComponentInChildren<NeuralNetwork>().network.Copy();
                    lastIndex = bestPlaneIndex;
                }
                GameObject instantiated = Instantiate(planeParent);
                instantiated.transform.SetParent(transform);
                instantiated.GetComponentInChildren<NeuralNetwork>().Start();
                instantiated.GetComponentInChildren<NeuralNetwork>().network.Paste(dna);
                instantiated.GetComponentInChildren<NeuralNetwork>().network.Mutate();
                instantiated.GetComponent<Plane>().id = id;
                instantiated.GetComponent<Plane>().opacity = .5f;
                instantiated.GetComponent<Plane>().Start();
            }
        }
    }
}
