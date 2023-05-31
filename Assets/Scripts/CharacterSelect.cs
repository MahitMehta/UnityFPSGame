using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public string selectionName;
    public Button ready;
    void Start()
    {
        Debug.Log("choices" + choicesPF.Count);


        for (int i = 0; i < choicesPF.Count; i++)
        {
            choices.Add(Instantiate(choicesPF[i], transform.position, Quaternion.Euler(new Vector3(0, i * 360 / choicesPF.Count, 0))));
            choices[i].transform.SetParent(transform, true);
            choices[i].transform.position += choices[i].transform.forward * 2;
            Debug.Log("here");
        }


    }

    // Update is called once per frame
    void Update()
    {
        float minD = 100;
        foreach (GameObject choice in choices)
        {
            choice.transform.LookAt(Camera.main.transform);
            if (Mathf.Abs((choice.transform.position - Camera.main.transform.position).magnitude) < minD)
            {
                selectionName = choice.name;
                minD = Mathf.Abs((choice.transform.position - Camera.main.transform.position).magnitude);
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
        Debug.Log(selectionName);
    }
}
