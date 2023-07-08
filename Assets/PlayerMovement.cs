using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed;
    public float turnSpeed;

    ResourceNode targetObject;
    Vector2 moveDirection;

    void Update()
    {
        moveDirection = (targetObject.transform.position - transform.position);
    }
    void FixedUpdate()
    {
        Debug.Log("Magnitude: " + moveDirection.magnitude);
    }
    public void SetNewTarget(ResourceNode _resource)
    {
        targetObject = _resource;
    }
}
