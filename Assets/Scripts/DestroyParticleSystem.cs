using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyParticleSystem : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<ParticleSystem>().Emit(1);
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<ParticleSystem>().particleCount == 0) Destroy(gameObject);
    }
}
