using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ArmWeaponController : MonoBehaviour
{
    public abstract void enable();
    public abstract void disable();
    public abstract void shoot();
    public abstract void release();
    public abstract void setLeft(bool v);
}
