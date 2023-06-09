
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    public float mana = 100;
    public float manaRegen = 5; //for powerups
    public float manaCost1 = 10;
    public float manaCost2 = 10;

    public GameObject ammo;
    public Transform indexFinger;
    public string wizardClass = "FireWizard";

    public Vector3 aimingPoint;
    float lastmoveTime = 0;
    bool isGrounded = true;





    private string sceneName; 
    // Start is called before the first frame update
    void Start()
    {

        GetComponent<Rigidbody>().freezeRotation = true;
        sceneName = SceneManager.GetActiveScene().name;
        camera = Camera.main;
        animator = gameObject.GetComponentInChildren<Animator>();

        indexFinger = getIndexFinger(transform);

        ammo = Resources.Load(wizardClass.Substring(0, wizardClass.IndexOf("W")) + "ball") as GameObject;
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
        lastmoveTime += Time.deltaTime;
        if(animator == null) animator = gameObject.GetComponentInChildren<Animator>();

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
            lastmoveTime = 0;
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
            lastmoveTime = 0;

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
            lastmoveTime = 0;


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
            lastmoveTime = 0;

        }
        else
        {
            if (sceneName == "GameScene") MainGameManager.Instance().playerStateRT[2] = 0;
            else if (sceneName == "RoomScene") RoomManager.Instance().playerStateRT[2] = 0;
            animator.SetBool("walkBack", false);
        }




        if (Input.GetMouseButtonDown(0) && sceneName == "GameScene" && mana >= manaCost1)
        {
            GameObject ball = Instantiate(ammo, indexFinger.position, transform.rotation);
            ball.AddComponent<BallMove>().source = gameObject;
            ball.transform.LookAt(aimingPoint);
            animator.SetTrigger("attack1");
            MainGameManager.Instance().playerStateRT[5] = 1;
            mana -= manaCost1;
            MainGameManager.Instance().mana.text = mana.ToString();

            MainGameManager.Instance().manaBarScript.UpdateHealthBar(mana);
            lastmoveTime = 0;
        }

        //will make attack2
        if (Input.GetMouseButtonDown(1) && sceneName == "GameScene" && mana >= manaCost2)
        {
            GameObject ball = Instantiate(ammo, indexFinger.position, transform.rotation);
            ball.AddComponent<BallMove>().source = gameObject;
            ball.transform.LookAt(aimingPoint); 
            animator.SetTrigger("attack2");
            MainGameManager.Instance().playerStateRT[6] = 1;
            mana -= manaCost1;
            MainGameManager.Instance().mana.text = mana.ToString();

            MainGameManager.Instance().manaBarScript.UpdateHealthBar(mana);
            lastmoveTime = 0;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetComponent<Rigidbody>().AddForce(Vector3.up * 5f, ForceMode.Impulse);
            lastmoveTime = 0;
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W))
        {
            Vector3 change = transform.forward * positionSpeed * Time.deltaTime;
            transform.position += change * 1.5f;
            if (sceneName == "GameScene") MainGameManager.Instance().playerStateRT[4] = 1;
            else if (sceneName == "RoomScene") RoomManager.Instance().playerStateRT[4] = 1;
            animator.SetBool("sprint", true);
        }
        else
        {
            if (sceneName == "GameScene") MainGameManager.Instance().playerStateRT[4] = 0;
            else if (sceneName == "RoomScene") RoomManager.Instance().playerStateRT[4] = 0;
            animator.SetBool("sprint", false);
        }

        //manaRegen
        
        if(lastmoveTime > 3 && MainGameManager.Exists() && mana < 100)
        {
            mana = mana + manaRegen > 100 ? 100 : mana + manaRegen;
            MainGameManager.Instance().manaBarScript.UpdateHealthBar(mana);
            MainGameManager.Instance().mana.text = mana.ToString();
            lastmoveTime = 0; 
        }



    }

    private void OnTriggerEnter(Collider other)
    {
        var e = other.GetComponent<PowerUp>();
        if (e != null)
        {
            if (e.type == PowerUp.powerup.HEALTH)
            {
                GameManager.Instance().getUser().hp = Mathf.Clamp(GameManager.Instance().getUser().hp + 40, 0, 100);
                MainGameManager.Instance().takeDamage();
            }
            else if (e.type == PowerUp.powerup.MANA)
            {
                manaRegen += 5;
            }else if (e.type == PowerUp.powerup.SHIELD)
            {
                GameManager.Instance().getUser().shield = Mathf.Clamp(GameManager.Instance().getUser().shield + 40, 0, 100);
                MainGameManager.Instance().takeDamage();
            }
            Destroy(other.gameObject);
        }
    }
}