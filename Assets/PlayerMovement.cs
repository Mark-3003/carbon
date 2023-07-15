using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed;
    public float turnSpeed;
    Vector2 moveDirection;
    Rigidbody2D rb;
    Transform target;
    void Awake(){
        rb = GetComponent<Rigidbody2D>();
    }
    void Update(){
        moveDirection.x = Input.GetAxisRaw("Horizontal");
        moveDirection.y = Input.GetAxisRaw("Vertical");

        if(moveDirection.magnitude > 0.1f){
            target = null;
        }
    }
    void FixedUpdate(){
        if(target == null){
            rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.deltaTime);
        }
        else{
            Vector2 _moveVector = target.position - transform.position;
            rb.MovePosition(rb.position + _moveVector.normalized * moveSpeed * Time.deltaTime);

            Debug.Log(_moveVector.magnitude);
        }
    }
    void OnCollisionEnter2D(Collision2D col){
        if(col.gameObject.CompareTag("tree")){
            target = null; 
        }
    }
    public void SetNewTarget(Transform _target, LayerMask _resourceLayer){
        RaycastHit2D hit = new RaycastHit2D();

        Vector2 _moveVector = (_target.position - transform.position).normalized;
        hit = Physics2D.Raycast(transform.position, _moveVector, Mathf.Infinity, _resourceLayer, -10000f, 10000f);

        if(hit.collider == _target)
            target = _target;
    }
}
