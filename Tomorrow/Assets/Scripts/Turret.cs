using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TurretAudioManager))]
public class Turret : MonoBehaviour {

    private Animator animator;

    private TurretAudioManager audioManager;

    public Transform target;
    public Transform weaponPivot;
    public GameObject weaponObject;
    public GameObject bulletPrefab;

    public float bulletSpeed;
    public float shootingSpeed;
    private float shootingSpeedTimer;

    public bool engaged;

    public float overshootAngle;

    private Vector2 targetDirection;

    private Vector2 shootingDirection;

    private Vector3 axis;

    private float currentAngle = 270;
    private float angle;

    public float rotationSpeed;
    public float shootingRotationSpeed;

    public Vector3 kickback;

    public float kickBackAmount;
    public float kickBackSpeed;

    public bool shoot;

    public float shootingTime;

    public float shootingCoolDown;
    public bool isCooledDown;
    
	void Start () {
        animator = GetComponent<Animator>();
        audioManager = GetComponent<TurretAudioManager>();
	}
	
	void Update () {
        HandleEngaging();

        if (engaged && weaponObject.activeSelf)
        {
            CalculateShootingDirection();

            if (isCooledDown)
            {
                AimAtTarget(rotationSpeed);
            }
            else
            {
                AimAtTarget(shootingRotationSpeed);
            }

            if (shoot)
            {
                Shoot();
            }
        }

        HandleCoolDownTimer();
        HandleKickBack();
	}

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawRay(weaponPivot.position, targetDirection*20);
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

    private void HandleEngaging()
    {
        if (!engaged && currentAngle <= 269)
        {
            if (Mathf.Abs(270 - currentAngle) > 10)
            {
                currentAngle += 270 > currentAngle ? rotationSpeed : -rotationSpeed;
            }
            else if (Mathf.Abs(270 - currentAngle) > 5)
            {
                currentAngle += 270 > currentAngle ? rotationSpeed / 2f : -rotationSpeed / 2f;
            }
            else if (Mathf.Abs(270 - currentAngle) > 2)
            {
                currentAngle += 270 > currentAngle ? rotationSpeed / 4f : -rotationSpeed / 4f;
            }
            else if (Mathf.Abs(270 - currentAngle) > 1)
            {
                currentAngle += 270 > currentAngle ? rotationSpeed / 8f : -rotationSpeed / 8f;
            }
            weaponObject.transform.rotation = Quaternion.identity;
            weaponObject.transform.Rotate(Vector3.forward, currentAngle + transform.rotation.eulerAngles.z + 90);
        }
        else
        {
            animator.SetBool("Engaged", engaged);
        }
    }

    public void OpeningFinished()
    {
        weaponObject.SetActive(true);
    }

    public void ClosingStarted()
    {
        weaponObject.SetActive(false);
    }

    private void CalculateShootingDirection()
    {
        targetDirection = (target.position - weaponPivot.position).normalized;
    }

    private void AimAtTarget(float rotationSpeed)
    {
        axis = transform.rotation * Vector3.down;
        angle = Vector3.Angle(targetDirection, axis);

        if(LeftRightTest.CheckLeftRight(axis, Vector3.forward, target.position - weaponPivot.position) < 0)
        {
            //Left
            angle = 180 + (180-angle);
        }
        
        if (Mathf.Abs(angle - currentAngle) > 10)
        {
            currentAngle += angle > currentAngle ? rotationSpeed : -rotationSpeed;
            currentAngle = Mathf.Clamp(currentAngle, 90 - overshootAngle, 270 + overshootAngle);
        }
        else if (Mathf.Abs(angle - currentAngle) > 5)
        {
            currentAngle += angle > currentAngle ? rotationSpeed/2f : -rotationSpeed/2f;
            currentAngle = Mathf.Clamp(currentAngle, 90 - overshootAngle, 270 + overshootAngle);
        }
        else if(Mathf.Abs(angle - currentAngle) > 2)
        {
            currentAngle += angle > currentAngle ? rotationSpeed/4f : -rotationSpeed/4f;
            currentAngle = Mathf.Clamp(currentAngle, 90 - overshootAngle, 270 + overshootAngle);
        }
        else if (Mathf.Abs(angle - currentAngle) > 1)
        {
            currentAngle += angle > currentAngle ? rotationSpeed/8f : -rotationSpeed/8f;
            currentAngle = Mathf.Clamp(currentAngle, 90 - overshootAngle, 270 + overshootAngle);
        }
        else
        {
            StartCoroutine(ShootAtTarget());
        }

        weaponObject.transform.rotation = Quaternion.identity;
        weaponObject.transform.Rotate(Vector3.forward, currentAngle + transform.rotation.eulerAngles.z + 90);

        shootingDirection = Quaternion.Euler(0, 0, currentAngle) * axis;
    }

    private IEnumerator ShootAtTarget()
    {
        if (isCooledDown)
        {
            isCooledDown = false;
            shoot = true;

            yield return new WaitForSeconds(shootingTime);

            shoot = false;

            yield return new WaitForSeconds(shootingCoolDown);

            isCooledDown = true;
        }
    }
}
