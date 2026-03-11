using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public float offsetX = 2f;

    public float minX;
    public float maxX;

    private float fixedY;
    private float fixedZ;

    void Start()
    {
        fixedY = transform.position.y;
        fixedZ = transform.position.z;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float targetX = target.position.x + offsetX;
        targetX = Mathf.Clamp(targetX, minX, maxX);

        Vector3 desiredPosition = new Vector3(targetX, fixedY, fixedZ);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}