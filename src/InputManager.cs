using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class InputManager : MonoBehaviour
{

    // the transforms of the virtual hands
    public Transform leftTransform;
    public Transform rightTransform;
    public Transform headTransform;
    public Transform rigTransform;

    private List<InputDevice> leftDevices = new List<InputDevice>();
    private List<InputDevice> rightDevices = new List<InputDevice>();
    private InputDevice leftController;
    private InputDevice rightController;

    // for reference by rest of program
    private bool leftGrip = false;
    private bool leftTrigger = false;
    private bool leftPrimary = false;
    private bool leftSecondary = false;

    private bool rightGrip = false;
    private bool rightTrigger = false;
    private bool rightPrimary = false;




    public bool getLeftGrip()
    {
        return leftGrip;
    }

    public bool getLeftTrigger()
    {
        return leftTrigger;

    }

    public bool getLeftPrimary()
    {
        return leftPrimary;
    }

    public bool getLeftSecondary()
    {
        return leftSecondary;
    }



    public bool getRightGrip()
    {
        return rightGrip;
    }

    public bool getRightTrigger()
    {
        return rightTrigger;
    }

    public bool getRightPrimary()
    {
        return rightPrimary;
    }




    public void vibrateRight(float strength, float duration)
    {
        uint channel = 0;
        rightController.SendHapticImpulse(channel, strength, duration);
    }

    public void vibrateLeft(float strength, float duration)
    {
        uint channel = 0;
        leftController.SendHapticImpulse(channel, strength, duration);
    }

    private void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        InputDevices.GetDevicesWithRole(InputDeviceRole.LeftHanded, leftDevices);
        if (leftDevices.Count >= 1)
        {
            leftController = leftDevices[0]; // assumming only one device
        }

        InputDevices.GetDevicesWithRole(InputDeviceRole.RightHanded, rightDevices);
        if (leftDevices.Count >= 1)
        {
            rightController = rightDevices[0];
        }

        leftController.TryGetFeatureValue(CommonUsages.triggerButton, out leftTrigger);
        leftController.TryGetFeatureValue(CommonUsages.gripButton, out leftGrip);
        leftController.TryGetFeatureValue(CommonUsages.primaryButton, out leftPrimary);
        leftController.TryGetFeatureValue(CommonUsages.secondaryButton, out leftSecondary);

        rightController.TryGetFeatureValue(CommonUsages.triggerButton, out rightTrigger);
        rightController.TryGetFeatureValue(CommonUsages.gripButton, out rightGrip);
        rightController.TryGetFeatureValue(CommonUsages.primaryButton, out rightPrimary);
    }
}
