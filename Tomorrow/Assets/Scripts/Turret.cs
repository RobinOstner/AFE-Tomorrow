using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TurretAudioManager))]
public class Turret : MonoBehaviour {

    private Animator animator;

    private TurretAudioManager audioManager;

    public LayerMask targetLayerMask;

    public Transform target;
    public Transform weaponPivot;
    public GameObject weaponObject;
    public GameObject bulletPrefab;

    public float bulletSpeed;
    public float shootingSpeed;
    private float shootingSpeedTimer;

    public float overshootAngle;

    private Vector2 targetDirection;

    private Vector3 shootingDirection;

    private Vector3 axis;

    private float currentAngle = 270;
    private float angle;

    public float maxDistance;

    public float rotationSpeed;
    public float shootingRotationSpeed;

    public Vector3 kickback;

    public float kickBackAmount;
    public float kickBackSpeed;

    public float shootingTime;

    public float shootingCoolDown;
    public bool isCooledDown;

    public Vector3 lastTargetPosition;

    public float angleSearchOffset;
    public float searchAngle;
    public float searchSpeed;
    private bool flip;
    public float spotLightAngle;

    private Vector3 leftMost;

    public int followDirection;

    public float followTime;

    private float followTimer;

    public float followSpeed;

    public float hitFollowTime;
    public float hitFollowTimer;
    public float hitFollowSpeed;
    
	void Start () {
        animator = GetComponent<Animator>();
        audioManager = GetComponent<TurretAudioManager>();
	}

    void Update()
    {
        CalculateLeftMostDirection();
        CheckArea();

        if (target != null || followTimer > 0)
        {
            if(target != null)
            {
                followTimer = followTime;
            }

            Shoot();

            FollowTarget();
        }
        else {
            SearchLastTargetPos();
            AimAtLastTargetPos();
        }

        HandleCoolDownTimer();
        HandleKickBack();

        followTimer -= Time.deltaTime;
        hitFollowTimer -= Time.deltaTime;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        
        Gizmos.DrawWireSphere(weaponPivot.position, maxDistance);
        
        
        Gizmos.DrawWireSphere(lastTargetPosition, 0.5f);

        Gizmos.color = Color.blue;

        Gizmos.DrawRay(weaponPivot.position, shootingDirection * maxDistance);
        Gizmos.DrawRay(weaponPivot.position, leftMost * maxDistance);
    }

    private float CalculateAimAngle(Vector3 targetPos)
    {
        Vector3 direction = targetPos - weaponPivot.position;
        targetDirection = direction.normalized;

        axis = transform.rotation * Vector3.down;
        angle = Vector3.Angle(targetDirection, axis);
        return angle;
    }

    private void AimAtLastTargetPos()
    {
        float speed = hitFollowTimer <= 0 ? searchSpeed : hitFollowSpeed;

        float angle = CalculateAimAngle(lastTargetPosition);

        angle += hitFollowTimer <= 0 ? angleSearchOffset : 0;

        if (LeftRightTest.CheckLeftRight(axis, Vector3.forward, lastTargetPosition - weaponPivot.position) < 0)
        {
            //Left
            angle = 180 + (180 - angle);
        }

        if (Mathf.Abs(angle - currentAngle) > 10)
        {
            currentAngle += (angle > currentAngle ? speed : -speed) * Time.deltaTime;
            currentAngle = Mathf.Clamp(currentAngle, 90 - overshootAngle, 270 + overshootAngle);
        }
        else if (Mathf.Abs(angle - currentAngle) > 5)
        {
            currentAngle += (angle > currentAngle ? speed / 2f : -speed / 2f) * Time.deltaTime;
            currentAngle = Mathf.Clamp(currentAngle, 90 - overshootAngle, 270 + overshootAngle);
        }
        else if (Mathf.Abs(angle - currentAngle) > 2)
        {
            currentAngle += (angle > currentAngle ? speed / 4f : -speed / 4f) * Time.deltaTime;
            currentAngle = Mathf.Clamp(currentAngle, 90 - overshootAngle, 270 + overshootAngle);
        }
        else if (Mathf.Abs(angle - currentAngle) > 1)
        {
            currentAngle += (angle > currentAngle ? speed / 8f : -speed / 8f) * Time.deltaTime;
            currentAngle = Mathf.Clamp(currentAngle, 90 - overshootAngle, 270 + overshootAngle);
        }

        weaponObject.transform.rotation = Quaternion.identity;
        weaponObject.transform.Rotate(Vector3.forward, currentAngle + transform.rotation.eulerAngles.z + 90);

        shootingDirection = Quaternion.Euler(0, 0, currentAngle) * axis;
    }

    private void SearchLastTargetPos()
    {
        if (!flip)
        {
            angleSearchOffset += Time.deltaTime * searchSpeed;
        }
        else
        {
            angleSearchOffset -= Time.deltaTime * searchSpeed;
        }

        if(Mathf.Abs(angleSearchOffset) >= searchAngle)
        {
            flip = !flip;
        }
    }

    private void CheckArea()
    {
        Collider2D collider = null;
        target = null;

        int firstIndex = -1;
        int lastIndex = -1;

        for(int i=0; i < spotLightAngle; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(weaponPivot.position, Quaternion.Euler(0, 0, -i) * leftMost, maxDistance, targetLayerMask);

            if(hit.collider != null && hit.collider.tag.Equals("Player"))
            {
                if(firstIndex == -1) { firstIndex = i; }
                lastIndex = i;

                if(firstIndex != lastIndex)
                {
                    collider = hit.collider;
                    target = collider.transform;
                }
            }
        }
        if (firstIndex != lastIndex)
        {
            float index = firstIndex + (lastIndex - firstIndex) / 2f;

            if (index <= spotLightAngle * 0.45f)
            {
                followDirection = -1;
            }
            else if (index >= spotLightAngle * 0.55f)
            {
                followDirection = 1;
            }
            else
            {
                followDirection = 0;
            }
        }
    }

    private Vector3 CalculateLeftMostDirection()
    {
        leftMost = Quaternion.Euler(0,0,spotLightAngle/2f) * shootingDirection;
        return leftMost;
    }

    private void FollowTarget()
    {
        float angle = currentAngle;

        switch (followDirection)
        {
            case -1:
                angle += followSpeed * Time.deltaTime ;
                break;
            case 1:
                angle += -followSpeed * Time.deltaTime;
                break;
            default:
                break;
        }
       

        currentAngle = angle;
        currentAngle = Mathf.Clamp(currentAngle, 90 - overshootAngle, 270 + overshootAngle);

        weaponObject.transform.rotation = Quaternion.identity;
        weaponObject.transform.Rotate(Vector3.forward, currentAngle + transform.rotation.eulerAngles.z + 90);

        shootingDirection = Quaternion.Euler(0, 0, currentAngle) * axis;
    }

    private void HandleCoolDownTimer()
    {
        shootingSpeedTimer -= Time.deltaTime;
        shootingSpeedTimer = Mathf.Clamp(shootingSpeedTimer, 0, shootingSpeed);
    }

    private void HandleKickBack()
    {
        weaponObject.transform.position = weaponPivot.position + kickback;

        kickback = Vector3.Lerp(kickback, Vector3.zero, Time.deltaTime * kickBackSpeed);
    }

    private void Shoot()
    {
        if (shootingSpeedTimer == 0)
        {
            BulletController bullet = Instantiate(bulletPrefab, weaponPivot.position + (Vector3)shootingDirection.normalized * 1.5f, Quaternion.identity).GetComponent<BulletController>();
            bullet.Initialize(shootingDirection, bulletSpeed);

            kickback = -shootingDirection * kickBackAmount;

            shootingSpeedTimer = shootingSpeed;

            audioManager.PlayShootingSound();
        }
    }
    public void Hit(Vector3 direction)
    {
        lastTargetPosition = weaponPivot.position - direction.normalized * 10;
        hitFollowTimer = hitFollowTime;
    }
}
