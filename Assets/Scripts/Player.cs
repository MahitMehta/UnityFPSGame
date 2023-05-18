using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float rotationSpeed = 30;
    public float positionSpeed = 2.5f;
    public Camera camera;
    public Animator animator;

    private float previousOffsetX = 0;  
    // Start is called before the first frame update
    void Start()
    {
        camera = gameObject.GetComponentInChildren<Camera>();
        animator = gameObject.GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Input.mousePosition;

        float offsetX = mousePos.x - Screen.width / 2f;
        float offsetY = mousePos.y - Screen.height / 2f;
 
        if (previousOffsetX != 0)
            transform.Rotate(new Vector3(0, (offsetX - previousOffsetX) / 3f, 0));
        previousOffsetX = offsetX;
        //camera.transform.rotation = Quaternion.Euler(new Vector3(-angleY, camera.transform.rotation.y, camera.transform.rotation.z));

        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(new Vector3(0, rotationSpeed, 0) * Time.deltaTime);
        } else if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(new Vector3(0, -rotationSpeed, 0) * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.W))
        {
            Vector3 change = transform.forward * positionSpeed * Time.deltaTime;
            transform.position += change;
            animator.SetBool("walkFwd", true);
        }
        else animator.SetBool("walkFwd", false);

        if (Input.GetKey(KeyCode.A))
        {
            Vector3 change = -transform.right * positionSpeed * Time.deltaTime;
            transform.position += change;
            animator.SetBool("walkLeft", true);
        }
        else animator.SetBool("walkLeft", false);


        if (Input.GetKey(KeyCode.D))
        {
            Vector3 change = transform.right * positionSpeed  * Time.deltaTime;
            transform.position += change;
            animator.SetBool("walkRight", true);
        }
        else animator.SetBool("walkRight", false);

        if (Input.GetKey(KeyCode.S))
        {
            Vector3 change = -transform.forward * positionSpeed * Time.deltaTime;
            transform.position += change;
            animator.SetBool("walkBack", true);
        }
        else animator.SetBool("walkBack", false);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetComponent<Rigidbody>().AddForce(Vector3.up * 5f, ForceMode.Impulse);
        }
        Debug.Log(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle01"));
    }
}