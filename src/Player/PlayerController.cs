using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerController : MonoBehaviour
{
    public enum ArmWeapon {None, WristRifle, Blade}
    public enum ShoulderWeapon {None, Cannon, Missles}
    public GameObject windParticlesObj;
    public GameObject dronePrefab;
    public Animator healthAnimator;


    public Animator[] leftFingers;
    public Animator[] rightFingers;

    [Header("Arm Weapon Prefabs")]
    public GameObject wristRiflePrefab;
    public GameObject bladePrefab;


    private HealthBar healthBar;

    private InputManager input;
    private Rigidbody rb;

    private ParticleSystem windParticles;

    private AudioSource leftParticlesAudio;
    private AudioSource rightParticlesAudio;

    private Animator handAnimatorLeft;
    private Animator handAnimatorRight;

    private Grabber leftGrabber;
    private Grabber rightGrabber;

    private Transform leftArmSlot;
    private Transform rightArmSlot;

    private ArmWeaponController leftArmWeapon;
    private ArmWeaponController rightArmWeapon;
    private ShoulderWeaponController leftShoulderWeapon;
    private ShoulderWeaponController rightShoulderWeapon;

    private ParticleSystem leftParticles;
    private ParticleSystem rightParticles;

    private float health = 100f;

    private float spawnTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        input = GetComponent<InputManager>();
        rb = GetComponent<Rigidbody>();
        leftParticles = input.leftTransform.Find("Hand").Find("Particle System").GetComponent<ParticleSystem>();
        rightParticles = input.rightTransform.Find("Hand").Find("Particle System").GetComponent<ParticleSystem>();
        windParticles = windParticlesObj.GetComponent<ParticleSystem>();
        leftParticlesAudio = leftParticles.gameObject.GetComponent<AudioSource>();
        rightParticlesAudio = rightParticles.gameObject.GetComponent<AudioSource>();
        healthBar = FindObjectOfType<HealthBar>();
        handAnimatorLeft = input.leftTransform.Find("Hand").Find("GauntletOpenAnimation").GetComponent<Animator>();
        handAnimatorRight = input.rightTransform.Find("Hand").Find("GauntletOpenAnimation").GetComponent<Animator>();
        leftGrabber = input.leftTransform.Find("Hand").GetComponent<Grabber>();
        rightGrabber = input.rightTransform.Find("Hand").GetComponent<Grabber>();
        leftGrabber.setOther(rightGrabber);
        rightGrabber.setOther(leftGrabber);
        leftArmSlot = input.leftTransform.Find("Hand").Find("WeaponSlot");
        rightArmSlot = input.rightTransform.Find("Hand").Find("WeaponSlot");
        chooseWeapons(ArmWeapon.WristRifle, ArmWeapon.WristRifle, ShoulderWeapon.None, ShoulderWeapon.Cannon);
        
    }

    // Update is called once per frame
    void Update()
    {
        Clench();
        // recenters rigid body if needed
        //rb.position = input.headTransform.position;
        trackHead();

        //updates wind position
        updateWind();

        // slight gravity
        rb.AddForce(new Vector3(0f, -2f, 0f), ForceMode.Acceleration);

        // drag
        rb.AddForce(-0.002f * rb.velocity, ForceMode.VelocityChange);

        // pass input info to grabbers and arm weapons
        if (input.getLeftGrip())
        {

            //handAnimatorLeft.SetBool("Clenched", true);
            leftGrabber.grip();
            if (leftArmWeapon && !leftGrabber.isHolding())
            {
                leftArmWeapon.enable();
            }
        }
        else
        {

            //handAnimatorLeft.SetBool("Clenched", false);
            leftGrabber.unGrip();
            if (leftArmWeapon)
            {
                leftArmWeapon.disable();
            }

        }
        if (input.getRightGrip())
        {

            //handAnimatorRight.SetBool("Clenched", true);
            rightGrabber.grip();
            if (rightArmWeapon && !rightGrabber.isHolding())
            {
                rightArmWeapon.enable();
            }

        }
        else
        {

            //handAnimatorRight.SetBool("Clenched", false);
            rightGrabber.unGrip();
            if (rightArmWeapon)
            {
                rightArmWeapon.disable();
            }
        }

        // thrusters
        if (!input.getLeftGrip() && input.getLeftTrigger())
        {
            rb.AddForceAtPosition(-input.leftTransform.forward * 2.5f, input.leftTransform.position);
            input.vibrateLeft(0.3f, 0.1f);

            // turns on particle effects
            if (!leftParticles.isPlaying)
            {
                leftParticles.Play();
                leftParticlesAudio.Play();
            }
        }
        else
        {
            if (leftParticles.isPlaying)
            {
                leftParticles.Stop();
                leftParticlesAudio.Stop();
            }
        }
        if (!input.getRightGrip() && input.getRightTrigger())
        {
            rb.AddForceAtPosition(-input.rightTransform.forward * 2.5f, input.rightTransform.position);
            input.vibrateRight(0.3f, 0.1f);

            // turns on particle effects
            if (!rightParticles.isPlaying)
            {
                rightParticles.Play();
                rightParticlesAudio.Play();
            }
        }
        else
        {
            if (rightParticles.isPlaying)
            {
                rightParticles.Stop();
                rightParticlesAudio.Stop();
            }
            
        }


        // shooting test code
        

        // left arm weapon fire button
        if (input.getLeftTrigger())
        {
            leftArmWeapon.shoot();
        } 
        else
        {
            leftArmWeapon.release();
        }

        // right arm weapon fire button
        if (input.getRightTrigger())
        {
            rightArmWeapon.shoot();
        }
        else
        {
            rightArmWeapon.release();
        }

        // spawn enemies
        spawnTime -= Time.deltaTime;
        if (input.getLeftSecondary() && spawnTime <= 0f)
        {
            spawnTime = 0.3f;
            GameObject drone = Instantiate(dronePrefab, new Vector3(UnityEngine.Random.Range(-100, 100), 100, UnityEngine.Random.Range(-100, 100)), Quaternion.identity);
            drone.GetComponent<DroneController>().target = gameObject;
        }


        // check for death
        if (health <= 0)
        {
            die();
        }
    }

    void chooseWeapons(ArmWeapon leftArm, ArmWeapon rightArm, ShoulderWeapon leftShoulder, ShoulderWeapon rightShoulder)
    {
        // remove old weapons
        if (leftArmSlot.childCount == 1)
        {
            Destroy(leftArmSlot.GetChild(0));
        }
        if (rightArmSlot.childCount == 1)
        {
            Destroy(rightArmSlot.GetChild(0));
        }

        // set left arm weapon
        if (leftArm == ArmWeapon.WristRifle)
        {
            GameObject obj = Instantiate(wristRiflePrefab, leftArmSlot.transform);
            leftArmWeapon = obj.GetComponent<ArmWeaponController>();
            leftArmWeapon.setLeft(true);
            //obj.transform.parent = leftArmSlot;
        }
        else
        {
            leftArmWeapon = null;
        }


        // set right arm weapon
        if (rightArm == ArmWeapon.WristRifle)
        {
            GameObject obj = Instantiate(wristRiflePrefab, rightArmSlot.transform);
            rightArmWeapon = obj.GetComponent<ArmWeaponController>();
            rightArmWeapon.setLeft(false);
            //obj.transform.parent = rightArmSlot;
        }
        else
        {
            rightArmWeapon = null;
        }

        // set shoulder weapons to do!!!!!
    }

    void updateWind()
    {
        if (rb.velocity.magnitude < 10 && windParticles.isPlaying)
        {
            windParticles.Stop();
        }
        if (rb.velocity.magnitude >= 10 && !windParticles.isPlaying)
        {
            windParticles.Play();
        }
        windParticles.transform.position = input.headTransform.position + rb.velocity / 2.0f;
        //windParticles.startSpeed = rb.velocity.magnitude;
        windParticles.transform.rotation = Quaternion.FromToRotation(Vector3.forward, rb.velocity);
    }

    void trackHead()
    {

        // recenters player if real player walks
        if (transform.position.x != input.headTransform.position.x || transform.position.z != input.headTransform.position.z)
        {
            // save rig position (it will get moved because it is a child of Player)
            Vector3 temp = input.rigTransform.position;

            // recenter player
            transform.position = new Vector3(input.headTransform.position.x, transform.position.y, input.headTransform.position.z);

            // recover old rig position
            input.rigTransform.position = temp;
        }

        // adjusts height if real player crouches
        /*float newHeight = input.headTransform.localPosition.y;
        if (newHeight < 1.3f)
        {
            newHeight = 1.3f;
        }
        capsuleCollider.height = newHeight;
        capsuleCollider.center = new Vector3(0, newHeight / 2f, 0);*/


    }

    void die()
    {

    }

    public void applyDamage(float damage)
    {
        health -= damage;
        healthAnimator.SetTrigger("Hit");
        healthBar.SetHealth(health);
    }

    void Clench()
    {
        for(int i = 0; i < leftFingers.Length; i++)
        {
            leftFingers[i].SetBool("Clenched", input.getLeftGrip());
            rightFingers[i].SetBool("Clenched", input.getRightGrip());
        }
        
    }

    void Unclench()
    {
        for (int i = 0; i < leftFingers.Length; i++)
        {
            leftFingers[i].SetBool("Clenched", false);
            rightFingers[i].SetBool("Clenched", false);
        }
    }
}
