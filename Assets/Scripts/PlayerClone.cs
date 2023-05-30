using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClone : MonoBehaviour
{
    public Animator animator;
    public List<int> playerState = new() { 0, 0, 0, 0, 0, 0, 0};
    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerState[0] == 1)
        {
            animator.SetBool("walkFwd", true);
        }
        else
        {
            animator.SetBool("walkFwd", false);
        }


        if (playerState[3] == 1)
        {
            animator.SetBool("walkLeft", true);
        }
        else
        {
            animator.SetBool("walkLeft", false);
        }


        if (playerState[1] == 1)
        {
            animator.SetBool("walkRight", true);
        }
        else
        {
            animator.SetBool("walkRight", false);
        }

        if (playerState[2] == 1)
        {
            animator.SetBool("walkBack", true);
        }
        else
        {
            animator.SetBool("walkBack", false);
        }
        
        if (playerState[4] == 1)
        {
            animator.SetBool("sprint", true);
        }
        else
        {
            animator.SetBool("sprint", false);
        }

        if (playerState[5] == 1)
        {
            animator.SetTrigger("attack1");
            playerState[5] = 0;
        }

        if (playerState[6] == 1)
        {
            animator.SetTrigger("attack2");
            playerState[6] = 0;
        }



    }
}
