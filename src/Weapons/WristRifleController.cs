using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WristRifleController : ArmWeaponController
{

    public GameObject bulletPrefab;

    private float triggerTime = 0f;
    private float triggerCooldown = 0.05f;
    private bool enabled = false;
    private Transform firePoint;
    private GameObject muzzleFlash;
    private Rigidbody playerRb;
    private InputManager input;
    private Animator animator;
    private bool isLeft = false;

    // Start is called before the first frame update
    void Start()
    {
        firePoint = transform.Find("Model").Find("FirePoint");
        muzzleFlash = transform.Find("Model").Find("MuzzleFlash").gameObject;
        playerRb = FindObjectOfType<PlayerController>().GetComponent<Rigidbody>();
        input = FindObjectOfType<InputManager>();
        animator = transform.Find("Model").GetComponent<Animator>();

        // why do I have to do this???
        //transform.Rotate(new Vector3(0f, 0f, 90f), Space.Self);
        transform.Translate(new Vector3(0.1f, 0f, -0.2f), Space.Self);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    override public void setLeft(bool v)
    {
        isLeft = v;
    }

    override public void enable()
    {
        enabled = true;
        animator.SetBool("Clenched", true);
    }

    override public void disable()
    {
        enabled = false;
        animator.SetBool("Clenched", false);
    }

    // user is holding trigger
    override public void shoot()
    {
        // no action if gun is not enabled
        if (!enabled)
        {
            return;
        }

        // fire bullet
        if (triggerTime <= 0.0f)
        {
            triggerTime = triggerCooldown; // restart cooldown process
            muzzleFlash.GetComponent<ParticleSystem>().Play();
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            bullet.GetComponent<AudioSource>().Play();
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.AddForce(firePoint.forward * 100 + playerRb.velocity, ForceMode.VelocityChange);
            Destroy(bullet, 5f);
            if (isLeft)
            {
                input.vibrateLeft(0.1f, 0.05f); // haptic feedback, pretty mild for this weapon}
            }
            else
            {
                input.vibrateRight(0.1f, 0.05f);
            }
        }

        // keep track of time for next shot
        else
        {
            triggerTime -= Time.deltaTime;
        }
    }

    // user is not holding trigger
    override public void release()
    {
        // no action if gun is not enabled
        if (!enabled)
        {
            return;
        }

        // keep track of time so that next shot has correct delay
        if (triggerTime > 0)
        {
            triggerTime -= Time.deltaTime;
        }
    }
}
