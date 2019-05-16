using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    float sensitiveDistance = 3;
    float playerVelocity = 2;
    float safeHoldingTime = 0.5f;
    float freezeTimeLenght = 0.5f;
    float freedom = 3;
    float oppnentTriggerDistance = 1.5f;
    float passMinimumDistance = 2;
    float passSpeed = 4;
    float shootSpeed = 8;

    Ball ball;
    Player[] players;
    public static Player ballOwner;
    public static float holdTime = -1;
    Rigidbody2D playerRigidbody2D;
    Rigidbody2D ballRigidbody2D;

    enum State { Stand, GoForBall, GoBackToPosition, HoldTheBall, Freeze };
    [SerializeField] State state;
    State savedState;

    Vector2 defaultPosition;
    Vector2 goalPosition;
    Vector2 velocityDirection;
    Vector2 currentPosition;
    Vector2 ballPosition;
    double distanceToBall;
    double distanceToPosition;
    float freezeTime;

    // Start is called before the first frame update
    void Start()
    {
        ball = FindObjectOfType<Ball>();
        players = FindObjectsOfType<Player>();
        defaultPosition = transform.position;

        if(tag == "LeftTeam")
            goalPosition = new Vector2(8f, 0);
        else
            goalPosition = new Vector2(-8f, 0);
        
        freezeTime = -1;
        playerRigidbody2D = GetComponent<Rigidbody2D>();
        //playerRigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        ballRigidbody2D = ball.GetComponent<Rigidbody2D>();
        state = State.Stand;
        SetPlayerAngle(currentPosition, ballPosition);
    }

    // Update is called once per frame
    void Update()
    {
        currentPosition = transform.position;
        ballPosition = ball.transform.position;
        distanceToBall = Distance(currentPosition, ballPosition);
        distanceToPosition = Distance(currentPosition, defaultPosition);

        switch(state)
        {
            case State.Stand:
                Stand();
                break;
            case State.GoForBall:
                ToBall();
                break;
            case State.GoBackToPosition:
                ToPosition();
                break;
            case State.HoldTheBall:
                Hold();
                break;
            case State.Freeze:
                Freeze();
                break;
        }
    }

    void Stand()
    {
        if(distanceToBall <= sensitiveDistance && (ballOwner == null || tag != ballOwner.tag))
        {
            state = State.GoForBall;
            ToBall();
        }
        else
        {
            SetPlayerAngle(currentPosition, ballPosition);
            playerRigidbody2D.velocity = new Vector2(0, 0);
        }
    }

    void ToBall()
    {
        if(distanceToBall > sensitiveDistance || (ballOwner != null && tag == ballOwner.tag) || distanceToPosition > freedom)
        {
            state = State.GoBackToPosition;
            ToPosition();
        }
        else
        {
            SetPlayerAngle(currentPosition, ballPosition);
            playerRigidbody2D.velocity = playerVelocity * velocityDirection;
        }
    }

    void ToPosition()
    {
        if(distanceToBall <= sensitiveDistance && (ballOwner == null || tag != ballOwner.tag) && distanceToPosition <= freedom / 2)
        {
            state = State.GoForBall;
            ToBall();
        }
        else if(distanceToPosition < 0.1)
        {
            state = State.Stand;
            Stand();
        }
        else
        {
            SetPlayerAngle(currentPosition, defaultPosition);
            playerRigidbody2D.velocity = playerVelocity * velocityDirection;
        }
    }

    void Hold()
    {
        if(Distance(transform.position, goalPosition) < 4)
        {
            Shoot(goalPosition);
            state = State.GoForBall;
            ballOwner = null;
        }
        else
        {
            foreach(Player p in players)
                if(p.tag != tag && Distance(transform.position, p.transform.position) < oppnentTriggerDistance)
                {
                    if(tag == "LeftTeam" && transform.position.x <= -2.5)
                    {
                        Vector2 destination = new Vector2(8, UnityEngine.Random.Range(-4.5f, 4.5f));
                        Shoot(destination);
                        MakeFreezed();
                        ballOwner = null;
                        return;                      
                    }
                    else if(tag == "RightTeam" && transform.position.x >= 2.5)
                    {
                        Vector2 destination = new Vector2(-8, UnityEngine.Random.Range(-4.5f, 4.5f));
                        Shoot(destination);
                        MakeFreezed();
                        ballOwner = null;
                        return;   
                    }
                    else if(Pass())
                    {
                        MakeFreezed();
                        ballOwner = null;
                        return;
                    }
                }

            SetPlayerAngle(currentPosition, goalPosition);
            PlaceTheBall();
            playerRigidbody2D.velocity = playerVelocity * velocityDirection;
        }
    }

    bool Pass()
    {
        Player teammate = null;
        double teammateDistance = -1;
        foreach(Player p in players)
            if(p != this && p.tag == tag)
            {
                double distance = Distance(transform.position, p.transform.position);
                if(distance > passMinimumDistance)
                    if(teammate == null || distance < teammateDistance)
                    {
                        teammate = p;
                        teammateDistance = distance;
                    }
            }
        if(teammate == null)
            return false;

        SetPlayerAngle(transform.position, teammate.transform.position);
        PlaceTheBall();
        ballRigidbody2D.velocity = passSpeed * velocityDirection;
        return true;
    }

    void Shoot(Vector2 destination)
    {
        SetPlayerAngle(currentPosition, destination);
        PlaceTheBall();
        playerRigidbody2D.velocity = playerVelocity * velocityDirection;
        ballRigidbody2D.velocity = shootSpeed * velocityDirection;
    }

    void PlaceTheBall()
    {
        float ballPositionX = transform.position.x + velocityDirection.x / 3;
        float ballPositionY = transform.position.y + velocityDirection.y / 3;
        ball.transform.position = new Vector2(ballPositionX, ballPositionY);
    }

    void Freeze()
    {
        if(Time.time - freezeTime >= freezeTimeLenght)
            state = savedState;
        else
            SetPlayerAngle(currentPosition, ballPosition);
    }

    double Distance(Vector2 origin, Vector2 destination)
    {
        float xDistance = destination.x - origin.x;
        float yDistance = destination.y - origin.y;
        return Math.Sqrt(xDistance * xDistance + yDistance * yDistance);
    }

    void SetPlayerAngle(Vector2 playerPosition, Vector2 destination)
    {
        double playerAngle = Angle(playerPosition, destination);
        velocityDirection = Direction(playerAngle);
        transform.rotation = Quaternion.Euler(0, 0, (float) (playerAngle * 180 / Math.PI));
    }

    double Angle(Vector2 origin, Vector2 destination)
    {
        float xDistance = destination.x - origin.x;
        float yDistance = destination.y - origin.y;
        return Math.Atan2(yDistance, xDistance);
    }

    Vector2 Direction(double angle)
    {
        return new Vector2((float) Math.Cos(angle),(float) Math.Sin(angle));
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.name == "Ball")    
            OwnTheBall();
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.name == "Ball")    
            OwnTheBall();
    }

    void OwnTheBall()
    {
        if(Time.time - holdTime >= safeHoldingTime)
        {
            if(ballOwner != null)
                ballOwner.state = State.GoForBall;
            ballOwner = this;
            state = State.HoldTheBall;
            holdTime = Time.time;
        }
        else
            MakeFreezed();
    }

    void MakeFreezed()
    {
        playerRigidbody2D.velocity = new Vector2(0, 0);
        state = State.Freeze;
        savedState = State.GoForBall;
        freezeTime = Time.time;
    }

    public void reset()
    {
        playerRigidbody2D.velocity = new Vector2(0, 0);
        transform.position = defaultPosition;
        state = State.Stand;
        SetPlayerAngle(currentPosition, ballPosition);
    }

}
