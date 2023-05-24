using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyParticleSystem : MonoBehaviour
{
    public ParticleSystem particleSystem;
    private bool exploded = false;
    // Start is called before the first frame update
    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
        particleSystem.Stop();
        particleSystem.Play();

    }

    // Update is called once per frame
    void Update()
    {
        if (!exploded)
        {
            exploded = true;
            Debug.Log("explosion: "+transform.position);
        }
        //if (particleSystem.particleCount == 0 && exploded) Destroy(gameObject);
    }
}
