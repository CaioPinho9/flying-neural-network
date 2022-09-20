using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NeuralController : MonoBehaviour
{
    [Header("Config")]
    public int playerAmount;
    public int playerSurviveAmount;
    public float randomAmount;
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
    public float queueTime = 2f;

    // Start is called before the first frame update
    void Start()
    {
        Create(playerAmount);
        players = GameObject.FindGameObjectsWithTag("Player");
        players[0].GetComponent<Player>().Start();
        lastBest = players[0];
        GameObject.Find("UI").GetComponent<NetworkUI>().Build(players[0]);
        GameObject.Find("alive").GetComponent<Text>().text = "Birds " + playerAmount.ToString() + " / " + playerAmount.ToString();
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
        playerAlive = playerAmount;
        bestPlayer = players[0];
        bestPlayers.Clear();
        foreach (GameObject bird in players)
        {
            bird.GetComponent<Player>().visible = false;
            if (bird.GetComponent<Player>().gameOver)
            {
                playerAlive--;
                GameObject.Find("alive").GetComponent<Text>().text = "Player " + playerAlive.ToString() + " / " + playerAmount.ToString();
            }

            if (bestPlayer.GetComponent<Player>().score < bird.GetComponent<Player>().score)
            {
                bestPlayer = bird;
                bird.GetComponent<Player>().visible = true;
            }

            if (bestPlayers.Count < playerSurviveAmount)
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
        playerAlive = playerAmount;
        UpdateUI();

        foreach (GameObject player in players)
        {
            bool isBest = false;
            foreach (GameObject best in bestPlayers)
            {
                if (player.GetComponent<Player>().id == best.GetComponent<Player>().id)
                {
                    isBest = true;
                }
            }
            if (isBest)
            {
                player.GetComponent<Player>().Restart();
            }
            else
            {
                int randomPlayerAmount = (int)Math.Floor((double)playerAmount * randomAmount);
                int bestPlayerIndex = (int)Math.Floor((double)(((index - randomPlayerAmount) * (playerSurviveAmount)) / (playerAmount - randomPlayerAmount)));
                player.GetComponent<Player>().Restart();

                if (index < randomPlayerAmount)
                {
                    player.GetComponent<Player>().network.Random();
                }
                else if (bestPlayerIndex < bestPlayers.Count)
                {
                    GameObject playerMother = bestPlayers[bestPlayerIndex];
                    float score = playerMother.GetComponent<Player>().score;
                    if (score >= 30000) {
                        score = 30000;
                    }

                    score = 30000 - score;

                    float learningRate = 1 + (((score - 1) * (5 - 1)) / 30000);

                    if (bestPlayerIndex != lastIndex)
                    {
                        dna = playerMother.GetComponent<Player>().network.Copy();
                        lastIndex = bestPlayerIndex;
                    }
                    player.GetComponent<Player>().network.Paste(dna);
                    player.GetComponent<Player>().network.Mutate(learningRate);
                }
                else
                {
                    GameObject playerMother = bestPlayers[bestPlayerIndex];
                    float score = playerMother.GetComponent<Player>().score;
                    if (score >= 10000)
                    {
                        score = 10000;
                    }

                    score = 10000 - score;
                    float learningRate = 1 + (((score - 1) * (5 - 1)) / 10000);
                    GameObject birdMother = bestPlayers[^1];
                    dna = birdMother.GetComponent<Player>().network.Copy();
                    player.GetComponent<Player>().network.Paste(dna);
                    player.GetComponent<Player>().network.Mutate(learningRate);
                    Debug.Log(learningRate);
                }
                index++;
            }
        }
        GameObject.Find("GameController").GetComponent<GameController>().Restart();
    }
}
