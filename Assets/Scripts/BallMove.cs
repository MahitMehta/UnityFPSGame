using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using WSMessage;

public class BallMove : MonoBehaviour
{
    public string projectileType;
    public Rigidbody rb;
    public GameObject source;
    public bool isClone = false;

    void Start()
    {
        rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;

        if (isClone) return;

        GameManager.Instance().AddBTUpdate(gameObject.name, BallBatchTranform);
    }

    [Update(Subscribe = false, TickRate = 1)]
    private void BallBatchTranform()
    {
        BatchTransform bt = new()
        {
            go = name,
            pf = "Fireball",
            type = BTType.Instantiate,
            scene = 2,
            userId = GameManager.Instance().userId,
            position = new List<float>() {
                    transform.position.x,
                    transform.position.y,
                    transform.position.z
            },
            rotation = new List<float>() {
                    transform.eulerAngles.x,
                    transform.eulerAngles.y,
                    transform.eulerAngles.z
                },
        };
        GameManager.Instance().batchTransforms.Add(bt);
    }

    // Update is called once per frame
    void Update()
    {
        rb.velocity = transform.forward * 25;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == source) return;
        //Destroy(gameObject);
    }
}
