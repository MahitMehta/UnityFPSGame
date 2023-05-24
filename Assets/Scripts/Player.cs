using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public float rotationSpeed = 30;
    public float positionSpeed = 2.5f;
    public Camera camera;
    public Animator animator;

    public float viewRotationDeltaYSpeed = 6.5f;
    public float viewRotationDeltaXSpeed = -2.5f;

    public float viewRotationXMax = 22.0f;
    public float viewRotationXMin = 0.0f;

    public GameObject ammo;
    public Transform indexFinger;
    public string wizardClass;


    private string sceneName; 
    // Start is called before the first frame update
    void Start()
    {
        sceneName = SceneManager.GetActiveScene().name;
        camera = gameObject.GetComponentInChildren<Camera>();
        animator = gameObject.GetComponentInChildren<Animator>();

        indexFinger = getIndexFinger(transform);

    }

    Transform getIndexFinger(Transform parent)
    {
        foreach(Transform child in parent){
            if (child.name == "index_01_r") return child;
            else if(getIndexFinger(child) != null) return getIndexFinger(child);
        }
        return null;
    }

    // Update is called once per frame
    void Update()
    {
        float viewRotationDeltaY = Input.GetAxis("Mouse X");
        float viewRotationDeltaX = Input.GetAxis("Mouse Y");

        transform.Rotate(new Vector3(0, viewRotationDeltaY * viewRotationDeltaYSpeed, 0));

        if (Cursor.lockState != CursorLockMode.None && camera.transform.rotation.eulerAngles.x + viewRotationDeltaX * viewRotationDeltaXSpeed <= viewRotationXMax &&
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
            if (sceneName == "GameScene") MainGameManager.Instance().playerStateRT[0] = 1;
            else if (sceneName == "RoomScene") RoomManager.Instance().playerStateRT[0] = 1;
            animator.SetBool("walkFwd", true);
        }
        else
        {
            if (sceneName == "GameScene") MainGameManager.Instance().playerStateRT[0] = 0;
            else if (sceneName == "RoomScene") RoomManager.Instance().playerStateRT[0] = 0;
            animator.SetBool("walkFwd", false);
        }


        if (Input.GetKey(KeyCode.A))
        {
            Vector3 change = -transform.right * positionSpeed * Time.deltaTime;
            transform.position += change;

            if (sceneName == "GameScene") MainGameManager.Instance().playerStateRT[3] = 1;
            else if (sceneName == "RoomScene") RoomManager.Instance().playerStateRT[3] = 1;
            animator.SetBool("walkLeft", true);
        }
        else
        {
            if (sceneName == "GameScene") MainGameManager.Instance().playerStateRT[3] = 0;
            else if (sceneName == "RoomScene") RoomManager.Instance().playerStateRT[3] = 0;
            animator.SetBool("walkLeft", false);
        }


        if (Input.GetKey(KeyCode.D))
        {
            Vector3 change = transform.right * positionSpeed * Time.deltaTime;
            transform.position += change;

            if (sceneName == "GameScene") MainGameManager.Instance().playerStateRT[1] = 1;
            else if (sceneName == "RoomScene") RoomManager.Instance().playerStateRT[1] = 1;
            animator.SetBool("walkRight", true);

        }
        else
        {
            if (sceneName == "GameScene") MainGameManager.Instance().playerStateRT[1] = 0;
            else if (sceneName == "RoomScene") RoomManager.Instance().playerStateRT[1] = 0;
            animator.SetBool("walkRight", false);
        }

        if (Input.GetKey(KeyCode.S))
        {
            Vector3 change = -transform.forward * positionSpeed * Time.deltaTime;
            transform.position += change;

            if (sceneName == "GameScene") MainGameManager.Instance().playerStateRT[2] = 1;
            else if (sceneName == "RoomScene") RoomManager.Instance().playerStateRT[2] = 1;
            animator.SetBool("walkBack", true);
        }
        else
        {
            if (sceneName == "GameScene") MainGameManager.Instance().playerStateRT[2] = 0;
            else if (sceneName == "RoomScene") RoomManager.Instance().playerStateRT[2] = 0;
            animator.SetBool("walkBack", false);
        }

        if (Input.GetMouseButtonDown(1) && sceneName == "GameScene")
        {
            Instantiate(ammo, indexFinger.position, transform.rotation).AddComponent<BallMove>().source = gameObject;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetComponent<Rigidbody>().AddForce(Vector3.up * 5f, ForceMode.Impulse);
        }
    }
}