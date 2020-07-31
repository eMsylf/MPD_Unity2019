﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public GameObject Ammunition;
    public float Firepower = 10f;
    [Tooltip("The end of the barrel of the weapon")]
    public Transform FirePoint;

    public virtual void Fire()
    {
        Debug.Log("Fire " + name);
        if (Ammunition != null)
        {
            // Instantiate
            GameObject projectile = Instantiate(Ammunition, FirePoint.position, transform.rotation);

            // Add force
            projectile.GetComponent<Rigidbody>()?.AddForce(GetComponentInParent<Rigidbody>().velocity + FirePoint.forward * Firepower, ForceMode.Impulse);

            // Add an owner
            Projectile projectileComponent = projectile.GetComponent<Projectile>();
            if (projectileComponent == null)
            {
                projectileComponent = projectile.AddComponent<Projectile>();
            }
            projectileComponent.Owner = GetComponentInParent<PlayerController>();
        }
    }
}