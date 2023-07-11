using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed;
    Vector2 moveDirection;
    Rigidbody2D rb;
    void Awake(){
        rb = GetComponent<Rigidbody2D>();
    }
    void Update(){
        moveDirection.x = Input.GetAxisRaw("Horizontal");
        moveDirection.y = Input.GetAxisRaw("Vertical");
    }
    void FixedUpdate(){
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.deltaTime);
    }
}
