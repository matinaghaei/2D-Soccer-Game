using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{

    Rigidbody2D ballRigidbody2D;
    Vector2 defaultPosition;

    // Start is called before the first frame update
    void Start()
    {
        ballRigidbody2D = GetComponent<Rigidbody2D>();
        //ballRigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        defaultPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void reset()
    {
        ballRigidbody2D.velocity = new Vector2(0, 0);
        transform.position = defaultPosition;
    }
}
