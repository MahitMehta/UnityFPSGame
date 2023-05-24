using System.Collections.Generic;
using UnityEngine;
using WSMessage;
using Debug = UnityEngine.Debug;

public class Interpolator : MonoBehaviour
{
    public Vector3 targetPosition, lastPosition;
    public Quaternion targetRotation, lastRotation;

    private float interpolationRatioPosition = 0;
    private float deltaTimePosition = 0f;

    private float interpolationRatioRotation = 0;
    private float deltaTimeRotation = 0f;

    public int previousTicks = -1; 

    private readonly Queue<BatchTransform> positionUpdates = new();
    private readonly Queue<BatchTransform> rotationUpdates = new();

    void Start()
    {
        targetRotation = transform.rotation;
        lastRotation = transform.rotation;
    }

    void Update()
    {
        if ((interpolationRatioPosition >= 1 || Vector3.Distance(transform.position, targetPosition) < 0.0001f) && positionUpdates.Count > 0)
        {
            interpolationRatioPosition %= 1;
            BatchTransform bt = positionUpdates.Dequeue();

            lastPosition = transform.position; 
            targetPosition = new Vector3(bt.position[0], bt.position[1], bt.position[2]);

            //var deltaTimePositionPrev = deltaTimePosition;
            deltaTimePosition = (bt.ticks - previousTicks) * Time.fixedDeltaTime; 

            // Not sure if this beneficial 
            //  if (deltaTimePosition != 0) interpolationRatioPosition = interpolationRatioPosition * deltaTimePositionPrev / deltaTimePosition; 
            previousTicks = bt.ticks; 
        } 


        if ((interpolationRatioRotation >= 1 || Quaternion.Angle(transform.rotation, targetRotation) < 0.0001f) && rotationUpdates.Count > 0)
        {
            interpolationRatioRotation %= 1;
            BatchTransform bt = rotationUpdates.Dequeue();

            lastRotation = transform.rotation;
            targetRotation = Quaternion.Euler(new Vector3(bt.rotation[0], bt.rotation[1], bt.rotation[2]));

            deltaTimeRotation = (bt.ticks - previousTicks) * Time.fixedDeltaTime;
        }

        interpolationRatioPosition += Time.deltaTime / (deltaTimePosition != 0 ? deltaTimePosition : Time.deltaTime);
        transform.position = Vector3.Lerp(lastPosition, targetPosition, interpolationRatioPosition);

        interpolationRatioRotation += Time.deltaTime / (deltaTimeRotation != 0 ? deltaTimeRotation : Time.deltaTime);
        transform.rotation = Quaternion.Lerp(lastRotation, targetRotation, interpolationRatioRotation);
       
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
