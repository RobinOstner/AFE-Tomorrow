using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneAnimator : MonoBehaviour {

    private Animator animator;

    private Drone drone;

	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();
        drone = GetComponentInParent<Drone>();
	}
	
	// Update is called once per frame
	void Update () {
        SetDirection();
	}

    void SetDirection()
    {
        animator.SetBool("LookingRight", drone.lookingRight);
    }
}
