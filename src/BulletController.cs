using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float damage = 4f;
    public AudioSource hitSound;

    private Rigidbody rb;

    private Vector3 dir = new Vector3(0, 0, 0);

    private bool isLethal = true;

    // Start is called before the first frame update
    void Start()
    {
        dir = transform.forward;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter(Collision collision)
    {

        // ignore bullets that have already hit something
        if (!isLethal)
        {
            return;
        }

        /*rb.isKinematic = true;
        rb.detectCollisions = false;

        rb.position = collision.GetContact(0).point;
        rb.position += dir * 3f;
        rb.velocity = Vector3.zero;*/
        if (collision.gameObject.CompareTag("Drone"))
        {
            collision.gameObject.GetComponent<DroneController>().applyDamage(damage);
            hitSound.Play();
        }

        else if (collision.gameObject.CompareTag("Player"))
        {
            hitSound.Play();
            collision.gameObject.GetComponent<PlayerController>().applyDamage(damage);
        }
        isLethal = false;
        Destroy(gameObject, 1f);

    }
}
