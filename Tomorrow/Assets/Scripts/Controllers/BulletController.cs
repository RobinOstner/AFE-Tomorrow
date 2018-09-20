using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour {

    [SerializeField]
    private float timer;

    [SerializeField]
    private GameObject particles;

    private new Rigidbody2D rigidbody;

    private new BoxCollider2D collider;

    private Vector2 direction;
    private float speed;

	// Use this for initialization
	void Start () {
        rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.velocity = direction * speed;

        RotateInDirection();
    }
	
	// Update is called once per frame
	void Update () {
        SelfDestructTimer();
	}

    public void Initialize(Vector2 direction, float speed)
    {
        this.direction = direction;
        this.speed = speed;
    }

    private void RotateInDirection()
    {
        float angle = Vector3.Angle(Vector3.right, direction);
        angle *= direction.y < 0 ? -1 : 1;
        transform.Rotate(0, 0, angle);
    }

    private void SelfDestructTimer()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            Destroy(gameObject);
        }
    }
    
    void OnCollisionEnter2D(Collision2D col)
    {
        Instantiate(particles, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
