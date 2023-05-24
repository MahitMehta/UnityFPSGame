using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using WSMessage;

public class BallMove : MonoBehaviour
{
    public string projectileType;
    public Rigidbody rb;
    public GameObject source;
    public bool isClone = false;
    public GameObject explosion;

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
        if ((transform.position - source.transform.position).magnitude > 20) vanish(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == source.name) return;
        vanish(true);
    }

    private void vanish(bool explode)
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<ParticleSystem>() != null)
            {
                if (!explode && child.gameObject.name.Equals("Impact02")) Destroy(child);
                var e = child.GetComponent<ParticleSystem>().emission;
                e.enabled = false;
                child.parent = null;
                child.AddComponent<DestroyParticleSystem>();

            }
        }
        Debug.Log("collided: " + transform.position);

        Instantiate(Resources.Load("Impact02"), transform.position - transform.forward, transform.rotation);
        Destroy(gameObject);
    }

}
