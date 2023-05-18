using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using WSMessage;

public class Interpolator : MonoBehaviour
{
    public Vector3 targetPosition, lastPosition;
    public Quaternion targetRotation, lastRotation;

    public float interpolationRatioPosition = 0;
    public float deltaTimePosition = 1f;

    public float interpolationRatioRotation = 0;
    public float deltaTimeRotation = 1f;

    public float previousTSPosition = -1;
    public float previousTSRotation = -1;

    private readonly Queue<BatchTransform> positionUpdates = new();
    private readonly Queue<BatchTransform> rotationUpdates = new();

    void Start()
    {
        targetRotation = transform.rotation;
        lastRotation = transform.rotation;
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, targetPosition) < 0.0001f && positionUpdates.Count > 0)
        {
            interpolationRatioPosition %= 1; 
            BatchTransform bt = positionUpdates.Dequeue();

            lastPosition = targetPosition;
            targetPosition = new Vector3(bt.position[0], bt.position[1], bt.position[2]);

            deltaTimePosition = (bt.ts - previousTSPosition) / 1_000_000_000.0f; 
            previousTSPosition = bt.ts; 
        }

        if (Quaternion.Angle(transform.rotation, targetRotation) < 0.0001f && rotationUpdates.Count > 0)
        {
            interpolationRatioRotation %= 1;
            BatchTransform bt = rotationUpdates.Dequeue();

            lastRotation = targetRotation;
            targetRotation = Quaternion.Euler(new Vector3(bt.rotation[0], bt.rotation[1], bt.rotation[2]));

            deltaTimeRotation = (bt.ts - previousTSRotation) / 1_000_000_000.0f; 
            previousTSRotation = bt.ts;
        }

        interpolationRatioPosition += Time.deltaTime / (deltaTimePosition != 0 ? deltaTimePosition : Time.deltaTime); 
        transform.position = Vector3.Lerp(lastPosition, targetPosition, interpolationRatioPosition);

        interpolationRatioRotation += Time.deltaTime / (deltaTimeRotation != 0 ? deltaTimeRotation : Time.deltaTime);
        transform.rotation = Quaternion.Lerp(lastRotation, targetRotation, interpolationRatioPosition);
    }

    public void AddPosition(BatchTransform bt)
    {
        positionUpdates.Enqueue(bt);
    }

    public void AddRotation(BatchTransform bt)
    {
        rotationUpdates.Enqueue(bt);
    }
}
