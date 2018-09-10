using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour {

    private Rigidbody2D characterRigidbody;

    private float horizontalInput;
    
    private float currentSpeed;
    public float horizontalSpeed;
    
	void Start () {
        characterRigidbody = GetComponent<Rigidbody2D>();
	}
	
	void Update () {
        HandleInputs();

        GetMotionData();

        ApplyMotion();
	}

    private void HandleInputs()
    {
        horizontalInput = Input.GetAxis("Horizontal");
    }

    private void ApplyMotion()
    {
        Vector2 horizontalVelocity = Vector2.right * horizontalInput * horizontalSpeed;
        characterRigidbody.velocity = horizontalVelocity + Vector2.up * characterRigidbody.velocity.y;
    }

    private void GetMotionData()
    {
        currentSpeed = characterRigidbody.velocity.magnitude;
    }
}
