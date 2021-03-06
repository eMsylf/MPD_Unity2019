﻿using BobJeltes.Menu;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.VFX;

public class Player : MonoBehaviour
{
    new Rigidbody rigidbody;
    Rigidbody Rigidbody
    {
        get
        {
            if (rigidbody == null)
            {
                rigidbody = GetComponent<Rigidbody>();
            }
            return rigidbody;
        }
    }
    public int ID;
    public Slider Health;
    public ValueText ScoreValue;
    [SerializeField]
    private PlayerController playerController;
    public PlayerController PlayerController
    {
        get
        {
            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
                if (playerController == null)
                    Debug.LogError("Player controller not assigned, and not found on same object as Player");
            }
            return playerController;
        }
    }

    private PlayerClientInterface pci;
    public PlayerClientInterface PlayerClientInterface
    {
        get
        {
            if (pci == null)
            {
                pci = GetComponent<PlayerClientInterface>();
            }
            return pci;
        }
    }
    public List<GameObject> HoverJets;
    public List<GameObject> DeathEffectObjects;
    public EndGameScreen endGameScreen;

    bool disabling = false;
    public int Score = 0;
    private void OnEnable()
    {
        disabling = false;
        Debug.Log("Enable " + name, gameObject);
    }

    private void OnDisable()
    {
        disabling = true;
        Debug.Log("Disable " + name, gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Player player = collision.collider.attachedRigidbody?.GetComponent<Player>();
        if (ServerBehaviour.HasActiveInstance() && player != null && player != this)
        {
            Debug.Log("Found server behaviour: " + ServerBehaviour.Instance.name, ServerBehaviour.Instance);
            ServerBehaviour.Instance.PlayerCollision();
        }
    }

    public void TakeDamage(float damage)
    {
        Health.value -= damage;
        Debug.Log(name + " takes " + damage + " damage", this);
    }

    public void TakeDamage(float damage, string damager)
    {
        Debug.Log(damager + " damages " + name);
        TakeDamage(damage);
    }

    public UnityEvent OnDeath;
    public void Die()
    {
        if (!enabled)
            return;
        ExplodeViolently();
        TurnOffHoverjets();
        //if (GameManager.Instance != null)
        //{
        //    GameManager.Instance.PlayerDeath(this);
        //}
        OnDeath.Invoke();
        if (!disabling)
            enabled = false;

    }

    private void ExplodeViolently()
    {
        Rigidbody.constraints = RigidbodyConstraints.None;
        Rigidbody.useGravity = true;
        if (DeathEffectObjects != null)
        {
            foreach (GameObject obj in DeathEffectObjects)
            {
                Instantiate(obj, transform);
                // Give the visual effect additional velocity
                //if (vfx != null)
                //{
                //    if (vfx.HasVector3("Additional Velocity"))
                //        vfx.SetVector3("Additional Velocity", Rigidbody.velocity);
                //}
            }
        }
    }

    void TurnOffHoverjets()
    {
        if (PlayerController != null)
            PlayerController.enabled = false;
        ConstantForce force = GetComponent<ConstantForce>();
        if (force != null) force.enabled = false;
        foreach (GameObject hoverJet in HoverJets)
        {
            hoverJet.SetActive(false);
        }
    }
}
