using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelect : MonoBehaviour
{
    // Start is called before the first frame update
    public List<GameObject> choicesPF;
    private List<GameObject> choices = new List<GameObject>();
    public float timeTillSnap;
    private float timeSinceLastScroll;
    private Quaternion rotationLastScroll;
    public GameObject selection;
    public GameObject player, camera;
    void Start()
    {
        Debug.Log("choices" + choicesPF.Count);


        for (int i = 0; i < choicesPF.Count; i++)
        {
            choices.Add(Instantiate(choicesPF[i], transform.position, Quaternion.Euler(new Vector3(0, i * 360 / choicesPF.Count, 0))));
            choices[i].transform.SetParent(transform, true);
            choices[i].transform.position += choices[i].transform.forward * 2;
        }


    }

    // Update is called once per frame
    void Update()
    {
        float minD = 100;
        foreach (GameObject choice in choices)
        {
            choice.transform.LookAt(camera.transform);
            if (Mathf.Abs((choice.transform.position - camera.transform.position).magnitude) < minD)
            {
                selection = choice;
                minD = Mathf.Abs((choice.transform.position - camera.transform.position).magnitude);
            }
        }
        if (Input.mouseScrollDelta.y != 0)
        {
            timeSinceLastScroll = 0;
            transform.Rotate(new Vector3(0, Input.mouseScrollDelta.y * 10, 0));
            rotationLastScroll = transform.rotation;
        }
        else
        {
            timeSinceLastScroll += Time.deltaTime;
            if (timeSinceLastScroll > timeTillSnap)
            {
                for (int i = 0; i <= 360; i += 360 / choicesPF.Count)
                {
                    if (Mathf.Abs(transform.rotation.eulerAngles.y - i) < 180 / choicesPF.Count)
                    {
                        transform.rotation = Quaternion.Slerp(rotationLastScroll, Quaternion.Euler(new Vector3(0, i, 0)), (timeSinceLastScroll - 1) / (timeTillSnap));
                    }
                }
            }
        }
    }
    public void select()
    {
        Destroy(player.transform.Find("PolyArtWizardMaskTintMat").gameObject);
        Instantiate(selection.transform.Find("PolyArtWizardMaskTintMat"), player.transform);
        GameManager.Instance().getUser().wizardClass = selection.name.Substring(0, selection.name.IndexOf("("));
        Debug.Log(GameManager.Instance().getUser().wizardClass);
        GameManager.Instance().SendMessages(new List<WSMessage.Message>() {
                    GameManager.Instance().ContructBroadcastMethodCallMessage("updateSkin", new List<string>
                    {
                        GameManager.Instance().userId, GameManager.Instance().getUser().wizardClass
                    }.Cast<object>().ToArray())
            });
    }
}
