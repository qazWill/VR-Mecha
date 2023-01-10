using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneController : MonoBehaviour
{
    public GameObject target;
    public float maxSpeed;
    public float maxAccel;
    public float maxTorque;
    public float maxHealth;
    public GameObject explosionPrefab;
    public LayerMask rayLayerMask;
    public GameObject bulletPrefab;
    public Transform firePointLeft;
    public Transform firePointRight;
    public Material hitMaterial;

    private Rigidbody rb;
    private float health;
    private float maxClimbSpeed = 4f;

    private Vector3[] rayCastDirs;

    private float shootTime = 0.0f;
    private bool shootLeft = false;

    private float hitTime = 0.0f;

    
    private Material originalMaterial;
    private MeshRenderer hitMesh;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        health = maxHealth;

        rayCastDirs = new Vector3[2];
        rayCastDirs[0] = new Vector3(0, 1, 0);
        rayCastDirs[1] = new Vector3(0, -1, 0);

        Transform model = transform.Find("DroneMK1");
        hitMesh = model.Find("Body").GetComponent<MeshRenderer>();
        originalMaterial = hitMesh.material;
    }

    // Update is called once per frame
    void Update()
    {

        // stabalize drone and seek target
        if (stabalize())
        {
            avoidOtherDrones();

            if (seekTarget()) // if found target
            {
                if (rb.velocity.magnitude > 1f)
                {
                    brake();
                }
                shoot();
            }
            faceTarget();



        }
        limitSpeed();
        //fixRotation();

        if (health <= 0f)
        {
            die();
        }

        if (hitTime > 0)
        {
            hitTime -= Time.deltaTime;
        } else
        {
            hitMesh.material = originalMaterial;
        }
    }

    public void applyDamage(float damage)
    {
        health -= damage;

        // swap out material for a bit
        hitMesh.material = hitMaterial;
        hitTime = 0.01f;
    }

    void die()
    {
        GameObject explosion = Instantiate(explosionPrefab, transform.position, transform.rotation);
        Destroy(explosion, 4);
        /*float radius = 50.0f;
        float power = 20.0f;
        Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
        foreach (Collider hit in colliders)
        {
            Rigidbody otherRb = hit.GetComponent<Rigidbody>();

            if (otherRb != null)
                otherRb.AddExplosionForce(power, explosionPos, radius, 3.0f);
        }*/
        Destroy(gameObject);
    }

    void brake()
    {
        Vector3 dir = Vector3.Normalize(rb.velocity);
        rb.AddForce(-dir * maxAccel, ForceMode.Acceleration);
    }


    // ensures drone doesn't accelerate past max speed
    void limitSpeed()
    {
        float excessSpeed = rb.velocity.magnitude - maxSpeed;
        if (excessSpeed > 0)
        {
            Vector3 dir = rb.velocity;
            dir.Normalize();
            rb.AddForce(-dir * excessSpeed, ForceMode.VelocityChange);
        }

        /*excessSpeed = rb.velocity.y - maxClimbSpeed;
        if (excessSpeed > 0)
        {
            Vector3 dir = new Vector3(0, rb.velocity.y, 0);
            dir.Normalize();
            rb.AddForce(-dir * excessSpeed, ForceMode.VelocityChange);
        }*/

    }


    void faceTarget()
    {
        /*Vector3 forward = new Vector3(transform.forward.x, 0, transform.forward.z);
        Vector3 dir = new Vector3(targetDir.x, 0, targetDir.z);
        float angle = Vector3.SignedAngle(forward, dir, new Vector3(0, 1, 0));
        rb.AddTorque(axis, ForceMode.Acceleration);*/
        Vector3 dir = Vector3.Normalize(target.transform.position - transform.position);
        Vector3 axis = Vector3.Cross(transform.forward, dir);
        rb.AddTorque(axis * maxTorque, ForceMode.Acceleration);

    }

    void faceHeading()
    {
        Vector3 dir = Vector3.Normalize(rb.velocity);
        Vector3 axis = Vector3.Cross(transform.forward, dir);
        rb.AddTorque(axis * maxTorque, ForceMode.Acceleration);

    }

    bool seekTarget()
    {
        Vector3 a = new Vector3(target.transform.position.x, 0, target.transform.position.z);
        Vector3 b = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 dir = target.transform.position - transform.position;
        if (Vector3.Distance(a, b) > 25)
        {
            dir.Normalize();
            rb.AddForce(dir * maxAccel, ForceMode.Acceleration);
            
        } else
        {
            float heightDiff = target.transform.position.y - transform.position.y;
            if (heightDiff > 10) 
            {
                rb.AddForce(Vector3.up * maxAccel, ForceMode.Acceleration);
            } else if (heightDiff < -10)
            {
                rb.AddForce(-Vector3.up * maxAccel, ForceMode.Acceleration);
            }
            else
            {
                return true;
            }

        }
        return false;
    }

    // applies forces to level out drone
    bool stabalize()
    {
        // level out
        /*Vector3 dir = new Vector3(transform.forward.x, 0, transform.forward.z);
        Vector3 axis = Vector3.Cross(transform.forward, dir);
        axis.Normalize();
        float angle = Vector3.SignedAngle(transform.forward, dir, axis);
        rb.AddTorque(axis * 0.3f, ForceMode.Acceleration);

        // level roll
        dir = new Vector3(transform.right.x, 0, transform.right.z);
        axis = Vector3.Cross(transform.right, dir);
        float rollAngle = Vector3.SignedAngle(transform.right, dir, axis);
        if (rollAngle < 0f)
        {
            axis = -axis;
        }
        rb.AddTorque(axis * 0.3f, ForceMode.Acceleration);*/

        if (Vector3.Angle(transform.up, Vector3.up) > 20.0f)
        {
            Vector3 axis = Vector3.Cross(transform.up, Vector3.up);
            axis.Normalize();
            rb.AddTorque(axis * maxTorque, ForceMode.Acceleration);
            return false;
        } else
        {
            if (Vector3.Angle(Vector3.up, transform.right) < 90)
            {
                rb.AddTorque(transform.forward * -maxTorque, ForceMode.Acceleration);
            } else
            {
                rb.AddTorque(transform.forward * maxTorque, ForceMode.Acceleration);
            }
        }

        return true;
    }


    void avoidOtherDrones()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Drone"))
        {
            if (obj != gameObject)
            {
                avoidObject(obj.transform.position);
            }
        }

        RaycastHit hit;
        foreach (Vector3 dir in rayCastDirs)
        {
            if (Physics.Raycast(transform.position, dir, out hit, 10f, rayLayerMask))
            {
                avoidObject(hit.point);
                Debug.DrawLine(transform.position, hit.point, Color.blue);
            }
        }

        if (Physics.Raycast(transform.position, rb.velocity, out hit, 10f, rayLayerMask))
        {
            avoidObject(hit.point);
            Debug.DrawLine(transform.position, hit.point, Color.blue);
        }

        if (Physics.Raycast(transform.position, -rb.velocity, out hit, 10f, rayLayerMask))
        {
            avoidObject(hit.point);
            Debug.DrawLine(transform.position, hit.point, Color.blue);
        }

        if (Physics.Raycast(transform.position, transform.forward, out hit, 10f, rayLayerMask))
        {
            avoidObject(hit.point);
            Debug.DrawLine(transform.position, hit.point, Color.blue);
        }

        if (Physics.Raycast(transform.position, -transform.forward, out hit, 10f, rayLayerMask))
        {
            avoidObject(hit.point);
            Debug.DrawLine(transform.position, hit.point, Color.blue);
        }

        /*RaycastHit hit;
        if (Physics.Raycast(transform.position, rb.velocity, 10f, rayLayerMask))
        {
            Vector3 dir = -Vector3.Normalize(rb.velocity);
            rb.AddForce(dir * maxAccel, ForceMode.Acceleration);
        }
        if (Physics.Raycast(transform.position, -rb.velocity, 10f, rayLayerMask))
        {
            Vector3 dir = Vector3.Normalize(rb.velocity);
            rb.AddForce(dir * maxAccel, ForceMode.Acceleration);
        }*/
    }

    void avoidObject(Vector3 pos)
    {
        Vector3 a = new Vector3(pos.x, 0, pos.z);
        Vector3 b = new Vector3(transform.position.x, 0, transform.position.z);
        Vector2 planeDiff = b - a;
        float heightDiff = transform.position.y - pos.y;
        Vector3 diff = transform.position - pos;
        if (diff.magnitude < 10 && Mathf.Abs(heightDiff) < 5)
        {
            Vector3 dir = Vector3.Normalize(diff);
            float strength = 125 / diff.sqrMagnitude;
            if (strength > 50)
            {
                strength = 50;
            }
            //float strength = maxAccel;
            rb.AddForce(dir * strength, ForceMode.Acceleration);
        }
        /*else if (diff.magnitude < 4)
        {
            Vector3 dir = Vector3.Normalize(diff);
            rb.AddForce(dir * maxAccel * 0.5f, ForceMode.Acceleration);
        }*/
    }

    void shoot()
    {
        shootTime -= Time.deltaTime;
        if (shootTime <= 0.0f)
        {
            shootTime = 0.05f;
            Transform firePoint = firePointRight;
            if (shootLeft)
            {
                firePoint = firePointLeft;
            }
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            Vector3 spreadVector = new Vector3(Random.Range(-6, 6), Random.Range(-6, 6), Random.Range(-6, 6));
            rb.AddForce(firePoint.forward * 100 + rb.velocity + spreadVector, ForceMode.VelocityChange);
            Destroy(bullet, 5f);

            // alternate shooting sides
            shootLeft = !shootLeft;
        }
    }

}
