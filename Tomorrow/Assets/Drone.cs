using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour {

    private Rigidbody2D rigidbody;

    [Header("General")]
    public LayerMask floorLayer;

    public float acceleration;
    public float speed;
    public float dropSpeed;
    public float floatSpeed;

    public Vector2 direction;

    private bool grounded;

    public float groundedDistance;

    public bool lookingRight = false;

    [Header("Navigation")]
    public Transform targetTransform;

	// Use this for initialization
	void Start () {
        rigidbody = GetComponentInChildren<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        ResetValues();

        CheckGround();

        Navigation();

        ApplyMotion();

        if (direction.x > 0) {
            lookingRight = true;
        }
        else if (direction.x < 0)
        {
            lookingRight = false;
        }
	}

    void ApplyMotion()
    {
        Vector2 targetVelocity = direction * speed;
        rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, targetVelocity, Time.deltaTime * acceleration);
    }

    void ResetValues()
    {
        direction = Vector2.zero;
    }

    void CheckGround()
    {
        grounded = Physics2D.Raycast(transform.position, Vector2.down, groundedDistance, floorLayer);
    }

    void Navigation()
    {
        if(targetTransform == null) { return; }

        Vector2 targetDirection = targetTransform.position - transform.position;
        
        if (!grounded)
        {
            direction += Vector2.up * Mathf.Clamp(targetDirection.y, -1, 1) * dropSpeed;
        }
        else
        {
            direction += Vector2.up * Mathf.Clamp(targetDirection.y, 0, 1) * dropSpeed + Vector2.up * floatSpeed;
        }

        if (targetDirection.magnitude < 8) { return; }

        direction += Vector2.right * Mathf.Clamp(targetDirection.x, -1, 1) * speed;
    }
}
