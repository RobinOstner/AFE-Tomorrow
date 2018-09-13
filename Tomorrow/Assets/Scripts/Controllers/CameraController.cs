using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    
    public static CameraController instance;
    
    private new Rigidbody rigidbody;
    
    private Vector3 cleanPosition;
    private Vector3 startPosition;
    
    public Transform target;

    public Vector3 offset;
    public Vector3 lookAhead;
    public float speed;
    
    public float zOffset;
    
    private float originalSeed;
    private float continousSeed;
    private float xShakeOffset;
    private float yShakeOffset;
    private float offsetRoll;
    private Vector3 shakeOffset;
    public float shakeSpeed;
    public float shakeMagnitude;
    public float shakeRoll;
    
    public float trauma;
    
	void Start () {
        instance = this;
        
        rigidbody = target.GetComponent<Rigidbody>();
        
        startPosition = cleanPosition = transform.position;
        zOffset = transform.position.z;
        
        originalSeed = Time.time;
        continousSeed = originalSeed;
	}
	
	void Update () {
    }

    void FixedUpdate()
    {
        CameraFollow();
        CameraShake();

        // Add shake to camera
        transform.position = cleanPosition + shakeOffset;
    }

    // Makes the camera follow the character
    void CameraFollow()
    {
        //Vector3 zoom = offset * rigidbody.velocity.magnitude * rigidbody.velocity.magnitude * speedZoom * Time.deltaTime;
        //Vector3 zoom = offset;


        cleanPosition = Vector3.Lerp(cleanPosition, target.position + lookAhead + offset + Vector3.forward*zOffset, Time.deltaTime * speed);
    }

    // Makes the camera shake
    void CameraShake()
    {
        // Decrease Trauma Level every Frame
        trauma -= Time.deltaTime/1.5f;
        // Add Trauma on Demand
        if (Input.GetKeyDown(KeyCode.T)) { trauma += .3f; }
        if (Input.GetKeyDown(KeyCode.G)) { trauma += .5f; }
        if (Input.GetKeyDown(KeyCode.V)) { trauma += .7f; }
        // Clamp Trauma Level
        trauma = Mathf.Clamp01(trauma);


        // Add Time To Seed
        continousSeed += Time.deltaTime*(shakeSpeed*trauma);
        // Produce Random values
        xShakeOffset = Mathf.PerlinNoise(continousSeed, originalSeed) - 0.5f;
        yShakeOffset = Mathf.PerlinNoise(originalSeed, continousSeed) - 0.5f;
        offsetRoll = Mathf.PerlinNoise(continousSeed, continousSeed) - 0.5f;


        // Calculate Offset
        shakeOffset = Vector3.right * xShakeOffset + Vector3.up * yShakeOffset;
        // Magnify Offset
        shakeOffset *= shakeMagnitude * trauma * trauma;
        // Roll the camera
        transform.eulerAngles = Vector3.back * offsetRoll * shakeRoll * trauma * trauma;
    }

    // Add Trauma to the Camera
    public static void AddTrauma(float value)
    {
        // Clamp the value between 0-1
        value = Mathf.Clamp01(value);

        instance.trauma += value;
    }
}
