using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabber : MonoBehaviour
{

    public enum GrabType { Pickup, Terrain, Hybrid };

    private float grabTime = 0.0f;
    private GameObject heldObject = null;
    private GrabType heldType;
    private Rigidbody heldRb = null;
    private Vector3 heldPos;
    private InputManager input;

    private Vector3 throwStartPos = new Vector3(0, 0, 0);
    private Vector3 throwEndPos = new Vector3(0, 0, 0);
    private Quaternion throwStartRot = Quaternion.identity;
    private Quaternion throwEndRot = Quaternion.identity;
    private Vector3 throwVel = Vector3.zero;
    private Vector3 launchVel;
    private float throwTime = 0f;
    private PlayerController player;
    private Rigidbody playerRb;
    private Grabber other;

    // Start is called before the first frame update
    void Start()
    {
        input = FindObjectOfType<InputManager>();
        player = FindObjectOfType<PlayerController>();
        playerRb = player.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (grabTime > 0f)
        {
            grabTime -= Time.deltaTime;

        }

        
        if (heldObject)
        {
            if (heldType == GrabType.Pickup)
            {

                // used for throwing
                if (heldRb.position != throwEndPos)
                {
                    throwStartPos = throwEndPos;
                    throwEndPos = heldRb.position;
                    throwStartRot = throwEndRot;
                    throwEndRot = heldRb.rotation;
                    throwTime = Time.deltaTime;
                    throwVel = 0.5f * throwVel + 0.5f * (throwEndPos - throwStartPos) / Time.deltaTime;
                }

            }

            else if (heldType == GrabType.Terrain)
            {
                Vector3 offset = heldPos - transform.position;
                Vector3 vel = offset / Time.deltaTime;
                player.transform.position += offset;
                if (offset != Vector3.zero)
                {
                    launchVel = vel;
                }
            }
        }
        
            
    }

    public void setOther(Grabber g)
    {
        other = g;
    }

    public bool isGrabbing()
    {
        return grabTime > 0f && !heldObject;
    }

    public bool isHolding()
    {
        return heldObject;
    }

    public void grip() {
        grabTime = 0.2f;
    }

    public void unGrip()
    {
        grabTime = 0f;
        if (heldObject)
        {
            release();
        }
    }

    public void release()
    {
        if (heldType == GrabType.Pickup)
        {
            heldObject.transform.parent = null;
            heldRb.isKinematic = false;
            heldRb.detectCollisions = true;
            throwObject();
            throwVel = Vector3.zero;
            heldRb = null;
        }
        else if (heldType == GrabType.Terrain)
        {
            playerRb.isKinematic = false;
            launch();
        }

        heldObject = null;
    }

    // rock climbing grip switch
    public void transferGrip()
    {
        Vector3 offset = heldPos - transform.position;
        player.transform.position += offset;
        heldObject = null;
    }

    public void throwObject()
    {
        Vector3 vel = (throwEndPos - throwStartPos) / throwTime;
        heldRb.AddForce(vel, ForceMode.VelocityChange);
    }

    public void launch()
    {
        playerRb.AddForce(launchVel, ForceMode.VelocityChange);
    }



    public void grab(GameObject obj, GrabType type)
    {
        // cant grab terrain with too much velocity, apply friction instead
        if (type == GrabType.Terrain && playerRb.velocity.magnitude > 4)
        {
            playerRb.AddForce(-playerRb.velocity * 0.1f, ForceMode.VelocityChange);
            grabTime = 0.2f;
            return;
        }
            




        grabTime = 0f;
        heldObject = obj;
        heldType = type;

        if (heldType == GrabType.Pickup)
        {
            heldRb = heldObject.GetComponent<Rigidbody>();
            heldObject.transform.parent = transform;
            heldRb.isKinematic = true;
            heldRb.velocity = Vector3.zero;
            heldRb.detectCollisions = false;
        }

        else if (heldType == GrabType.Terrain)
        {
            // release other hand if on terrain
            if (other.heldObject && other.heldType == GrabType.Terrain)
            {
                other.transferGrip();
            }

            heldPos = transform.position;
            playerRb.velocity = Vector3.zero;

        }
        
    }
}
