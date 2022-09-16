using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NeuralController : MonoBehaviour
{
    [Header("Config")]
    public int playerAmmount;
    public int playerSurviveAmmount;
    public float randomAmmount;
    public GameObject prefab;

    [Header("Manage")]
    public int playerAlive;
    public GameObject bestPlayer;
    public GameObject lastBest;
    public List<GameObject> bestPlayers;
    public GameObject[] players;
    private int gen = 0;

    [Header("Timer")]
    public float time;
    public float queueTime = .5f;

    // Start is called before the first frame update
    void Start()
    {
        Create(playerAmmount);
        players = GameObject.FindGameObjectsWithTag("Player");
        players[0].GetComponent<Player>().Start();
        lastBest = players[0];
        GameObject.Find("UI").GetComponent<NetworkUI>().Build(players[0]);
        GameObject.Find("alive").GetComponent<Text>().text = "Birds " + playerAmmount.ToString() + " / " + playerAmmount.ToString();
        Check();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (time > queueTime)
        {
            Check();

            time = 0;
        }
        time += Time.deltaTime;

        /*
        if (nextObstacle == null || nextObstacle.transform.position.x < -5f)
        {
            DetectObstacle();
        }

        ObstacleCoords();
        */

        if (playerAlive <= 0)
        {
            Check();
            Restart();
        }
    }

    /*
    void DetectObstacle()
    {
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        if (obstacles != null)
        {
            foreach (GameObject obstacle in obstacles)
            {
                if (obstacle.GetComponent<Obstacle>() != null && obstacle.GetComponent<Obstacle>().id == obstacleId)
                {
                    nextObstacle = obstacle;
                    obstacleId++;
                    break;
                }
            }
        }
    }

    void ObstacleCoords()
    {
        foreach (GameObject player in players)
        {
            if (nextObstacle != null)
            {
                player.GetComponent<Player>().obstacle = nextObstacle;
            }
        }
    }
    */

    void Create(int ammount)
    {
        for (int i = 0; i < ammount; i++)
        {
            GameObject plane = Instantiate(prefab);
            plane.transform.SetParent(transform, false);
            plane.GetComponent<Player>().id = i;
        }
    }

    void Check()
    {
        playerAlive = playerAmmount;
        bestPlayer = players[0];
        bestPlayers.Clear();
        foreach (GameObject bird in players)
        {
            bird.GetComponent<Player>().visible = false;
            if (bird.GetComponent<Player>().gameOver)
            {
                playerAlive--;
                GameObject.Find("alive").GetComponent<Text>().text = "Player " + playerAlive.ToString() + " / " + playerAmmount.ToString();
            }

            if (bestPlayer.GetComponent<Player>().score < bird.GetComponent<Player>().score)
            {
                bestPlayer = bird;
                bird.GetComponent<Player>().visible = true;
            }

            if (bestPlayers.Count < playerSurviveAmmount)
            {
                bestPlayers.Add(bird);
                bird.GetComponent<Player>().visible = true;
            }
            else
            {
                int index = 0;
                for (int i = 0; i < bestPlayers.Count; i++)
                {
                    for (int j = 0; j < bestPlayers.Count; j++)
                    {
                        if (bestPlayers[i].GetComponent<Player>().score < bestPlayers[j].GetComponent<Player>().score &&
                            bestPlayers[i].GetComponent<Player>().score < bestPlayers[index].GetComponent<Player>().score)
                        {
                            index = i;
                        }
                    }
                }

                if (bestPlayers[index].GetComponent<Player>().score < bird.GetComponent<Player>().score)
                {
                    bestPlayers[index] = bird;
                    bird.GetComponent<Player>().visible = true;
                }
            }
        }

        if (lastBest.GetComponent<Player>().id != bestPlayer.GetComponent<Player>().id)
        {
            GameObject.Find("UI").GetComponent<NetworkUI>().Build(bestPlayer);
        }
        GameObject.Find("Score").GetComponent<TextMeshPro>().text = bestPlayer.GetComponent<Player>().score.ToString("0000000");
        GameObject.Find("best").GetComponent<Text>().text = "Best ID " + bestPlayer.GetComponent<Player>().id.ToString();
        lastBest = bestPlayer;
    }

    void UpdateUI()
    {
        Debug.Log("Gen: " + gen);
        Debug.Log(bestPlayer.GetComponent<Player>().network.Copy());
        gen++;
        GameObject.Find("UI").GetComponent<NetworkUI>().Build(bestPlayer);
        GameObject.Find("Window Chart").GetComponent<WindowGraph>().score.Add((int)bestPlayer.GetComponent<Player>().score);
        GameObject.Find("gen").GetComponent<Text>().text = "Gen " + gen.ToString();
    }

    void Restart()
    {
        int index = 0;
        int lastIndex = -1;
        string dna = "";
        playerAlive = playerAmmount;
        UpdateUI();

        foreach (GameObject bird in players)
        {
            bool isBest = false;
            foreach (GameObject best in bestPlayers)
            {
                if (bird.GetComponent<Player>().id == best.GetComponent<Player>().id)
                {
                    isBest = true;
                }
            }
            if (isBest)
            {
                bird.GetComponent<Player>().Restart();
            }
            else
            {
                int bestBirdIndex = (int)Math.Floor((double)index * (1 + randomAmmount) * (double)(playerSurviveAmmount / (double)(playerAmmount - playerSurviveAmmount)));
                bird.GetComponent<Player>().Restart();
                if (bestBirdIndex < bestPlayers.Count)
                {
                    GameObject birdMother = bestPlayers[bestBirdIndex];

                    if (bestBirdIndex != lastIndex)
                    {
                        dna = birdMother.GetComponent<Player>().network.Copy();
                        lastIndex = bestBirdIndex;
                    }
                    bird.GetComponent<Player>().network.Paste(dna);
                    bird.GetComponent<Player>().network.Mutate();
                }
                else
                {
                    bird.GetComponent<Player>().network.Random();
                }
                index++;
            }
        }
        GameObject.Find("GameController").GetComponent<GameController>().Restart();
    }
}
