using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float rotationSpeed = 30;
    public float positionSpeed = 2.5f;
    public Camera camera;
    public Animator animator;

    public float viewRotationDeltaYSpeed = 6.5f;
    public float viewRotationDeltaXSpeed = 2.5f;

    public float viewRotationXMax = 22.0f;
    public float viewRotationXMin = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        camera = gameObject.GetComponentInChildren<Camera>();
        animator = gameObject.GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float viewRotationDeltaY = Input.GetAxis("Mouse X");
        float viewRotationDeltaX = Input.GetAxis("Mouse Y");

        transform.Rotate(new Vector3(0, viewRotationDeltaY * viewRotationDeltaYSpeed, 0));

        if (camera.transform.rotation.eulerAngles.x + viewRotationDeltaX * viewRotationDeltaXSpeed <= viewRotationXMax &&
            camera.transform.rotation.eulerAngles.x + viewRotationDeltaX * viewRotationDeltaXSpeed >= viewRotationXMin)
            camera.transform.Rotate(new Vector3(viewRotationDeltaX * viewRotationDeltaXSpeed, 0, 0));

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
    }
}