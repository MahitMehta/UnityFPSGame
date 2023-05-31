using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public float rotationSpeed = 30;
    public float positionSpeed = 3.5f;
    public Camera camera;
    public Animator animator;

    public float viewRotationDeltaYSpeed = 6.5f;
    public float viewRotationDeltaXSpeed = -2.5f;

    public float viewRotationXMax = 22.0f;
    public float viewRotationXMin = 0.0f;

    public GameObject ammo;
    public Transform indexFinger;
    public string wizardClass;

    public Vector3 aimingPoint;


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

        //aiming

        Ray ray = new Ray(camera.transform.position, camera.transform.forward);
        RaycastHit hit;
        Physics.Raycast(ray, out hit);
        if (hit.collider == null) aimingPoint = ray.GetPoint(50);
        else aimingPoint = hit.point;

        float viewRotationDeltaY = Input.GetAxis("Mouse X");
        float viewRotationDeltaX = Input.GetAxis("Mouse Y");

        transform.Rotate(new Vector3(0, camera.transform.eulerAngles.y - transform.eulerAngles.y, 0));



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

        if (Input.GetMouseButtonDown(0) && sceneName == "GameScene")
        {
            GameObject ball = Instantiate(ammo, indexFinger.position, transform.rotation);
            ball.AddComponent<BallMove>().source = gameObject;
            ball.transform.LookAt(aimingPoint);
            animator.SetTrigger("attack1");
            //MainGameManager.Instance().playerStateRT[5] = 1;
            
        }

        //will make attack2
        if (Input.GetMouseButtonDown(1) && sceneName == "GameScene")
        {
            Instantiate(ammo, indexFinger.position, transform.rotation).AddComponent<BallMove>().source = gameObject;
            animator.SetTrigger("attack2");

            //MainGameManager.Instance().playerStateRT[6] = 1;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetComponent<Rigidbody>().AddForce(Vector3.up * 5f, ForceMode.Impulse);
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftShift))
        {
            Vector3 change = transform.forward * positionSpeed * Time.deltaTime;
            transform.position += change * 2;
            /*if (sceneName == "GameScene") MainGameManager.Instance().playerStateRT[4] = 1;
            else */if (sceneName == "RoomScene") RoomManager.Instance().playerStateRT[4] = 1;
            animator.SetBool("sprint", true);
        }
        else
        {
            /*if (sceneName == "GameScene") MainGameManager.Instance().playerStateRT[4] = 0;
            else */if (sceneName == "RoomScene") RoomManager.Instance().playerStateRT[4] = 0;
            animator.SetBool("sprint", false);
        }



        


    }
}