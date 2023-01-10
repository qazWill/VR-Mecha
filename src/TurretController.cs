using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{

    public GameObject bulletPrefab;

    private Transform pivot;
    private Transform firePoint;
    private Transform player;

    private float shootTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        pivot = transform.Find("Gun Pivot");
        firePoint = pivot.Find("Gun").Find("FirePoint");
        player = GameObject.FindObjectOfType<PlayerController>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 diff = player.position - pivot.position;
        if (diff.magnitude < 50)
        {
            Vector3 dir = Vector3.Normalize(diff);
            if (Vector3.Angle(transform.up, dir) < 110)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir, transform.up);
                
                if(Vector3.Angle(pivot.forward, dir) < 30)
                {
                    pivot.rotation = Quaternion.RotateTowards(pivot.rotation, targetRot, 8 * Time.deltaTime);
                    shoot();
                } else
                {
                    pivot.rotation = Quaternion.RotateTowards(pivot.rotation, targetRot, 30 * Time.deltaTime);
                }
            }
            
        }

        //transform.Find("Cube (1)").GetComponent<MeshRenderer>().

    }

    void shoot()
    {
        return;
        shootTime -= Time.deltaTime;
        if (shootTime <= 0.0f)
        {
            shootTime = 0.1f;

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            Vector3 spreadVector = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
            rb.AddForce(firePoint.forward * 100 + rb.velocity + spreadVector, ForceMode.VelocityChange);
            Destroy(bullet, 5f);
        }
    }
}
