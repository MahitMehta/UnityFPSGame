using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{


    public float jumpStrength;
    private bool isGrounded, isDead;
    public float maxSpeed;
    public float slipperyness;
    private float accelerationz, accelerationx;
    float speedz = 0;
    float speedx = 0;
    float time, deathTime;
    private Rigidbody rigidbody;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = gameObject.AddComponent<Rigidbody>();
        rigidbody.useGravity = true;
        
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;


        //if (Input.GetKeyDown(KeyCode.Space) && isGrounded) jump();

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            moveX(true);
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            moveX(false);
        }
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            moveZ(true);
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            moveZ(true);
        }


    }

    void moveX(bool pos)
    {

    }
    void moveZ(bool pos)
    {

    }
}
