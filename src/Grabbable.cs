using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
    public Grabber.GrabType type = Grabber.GrabType.Pickup;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerStay(Collider collider)
    {

        // get grabber component
        Grabber grabber = collider.GetComponent<Grabber>();

        // check to see if collider has a grabber
        if (grabber == null)
        {
            return;
        }

        if (grabber.isGrabbing())
        {

            grabber.grab(gameObject, type);
        }
    }
}
