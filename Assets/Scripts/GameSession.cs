using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSession : MonoBehaviour
{
    [SerializeField] Text scores;
    Ball ball;
    Player[] players;
    bool toStart;
    float resetTimeLenght = 1;

    int leftTeamScore;
    int rightTeamScore;
    float scoreTime;
    float resetTime;

    // Start is called before the first frame update
    void Start()
    {
        ball = FindObjectOfType<Ball>();
        players = FindObjectsOfType<Player>();
        leftTeamScore = 0;
        rightTeamScore = 0;
        scores.text = leftTeamScore + " - " + rightTeamScore;
    }

    // Update is called once per frame
    void Update()
    {
        if(toStart && Time.unscaledTime - resetTime >= resetTimeLenght)
        {
            toStart = false;
            Time.timeScale = 1;
        }
    }

    public void ScoreForleftTeam()
    {
        leftTeamScore++;
        scores.text = leftTeamScore + " - " + rightTeamScore;
        reset();
    }

    public void ScoreForRightTeam()
    {
        rightTeamScore++;
        scores.text = leftTeamScore + " - " + rightTeamScore;
        reset();
    }

    void reset()
    {
        resetTime = Time.unscaledTime;
        Time.timeScale = 0;

        ball.reset();
        Player.ballOwner = null;
        Player.holdTime = -1;
        foreach(Player p in players)
            p.reset();

        toStart = true;
    }
}
