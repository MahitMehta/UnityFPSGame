using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    // Start is called before the first frame update

    public enum powerup
    {
        HEALTH, SHIELD, MANA
    }

    public powerup type;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MainGameManager.dropped = true;
        if(transform.position.y > 1)
        {
            transform.position += Vector3.down * Time.deltaTime * 2;
        }

        
    }
    private void OnDestroy()
    {
        MainGameManager.dropped = false;
    }
}
