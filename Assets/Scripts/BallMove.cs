using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BallMove : MonoBehaviour
{
    // Start is called before the first frame update
    public string projectileType;
    public Rigidbody rb;
    public GameObject source;
    void Start()
    {
        Debug.Log("ballinstantiated");
        rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;
    }

    // Update is called once per frame
    void Update()
    {
        rb.velocity = transform.forward * 100;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == source) return;
        //Destroy(gameObject);
    }
}
