using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 8;
    Rigidbody myRigidbody;
    Vector3 velocity;

    void Start(){
        myRigidbody = GetComponent<Rigidbody>();
    }

    void Update(){
		Vector3 input = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical"));
		Vector3 direction = input.normalized;
		velocity = direction * speed;
    }

    void FixedUpdate(){
        myRigidbody.position += velocity * Time.deltaTime;
    }
}
