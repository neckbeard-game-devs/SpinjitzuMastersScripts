using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCon : MonoBehaviour
{
    public Rigidbody weaponRb;
    public int ninjaInt;

    public void FireWeapon()
    {
        weaponRb.AddForce(0, 0, 25, ForceMode.Impulse);
        Destroy(gameObject, 3f);
    }

}
