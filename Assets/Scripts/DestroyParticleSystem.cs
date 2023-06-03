using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyParticleSystem : MonoBehaviour
{
    public ParticleSystem particleSystem;
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
        particleSystem.Stop();
        if (particleSystem.particleCount == 0) Destroy(gameObject);
    }
}
