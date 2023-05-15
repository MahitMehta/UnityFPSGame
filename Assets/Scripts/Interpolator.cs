using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interpolator : MonoBehaviour
{
    public Vector3 targetPos, lastPos, diff;
    public Quaternion targetRot, lastRot;
    public float speed;
    public float timeSinceLastTarget;
    // Start is called before the first frame update
    void Start()
    {
        targetPos = transform.position;
        lastPos = transform.position;
        targetRot = transform.rotation;
        lastRot = transform.rotation;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
        timeSinceLastTarget += Time.deltaTime;
    }

    public void setTargetPos(Vector3 target)
    {
        Debug.Log("target set " + target.ToString());
        targetPos = target;
        lastPos = transform.position;
        diff = target - transform.position;
        speed = diff.magnitude / timeSinceLastTarget;
        timeSinceLastTarget = 0;
    }
    public void setTargetRot(Quaternion target)
    {
        Debug.Log("target set " + target.ToString());
        targetRot = target;
        lastRot = transform.rotation;
        timeSinceLastTarget = 0;
    }
}
