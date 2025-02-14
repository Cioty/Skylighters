﻿/*=============================================================================
 * Game:        Metallicide
 * Version:     Beta
 * 
 * Class:       JumpPad.cs
 * Purpose:     Launches the player either in Mech or Core in a specified
 *              force and direction.
 * 
 * Author:      Lachlan Wernert & Daniel Cox
 * Team:        Skylighter
 * 
 * Deficiences:
 * 
 *===========================================================================*/
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public Transform forceDirection;
    public float maxCooldown = 0.1f;
    public float coreLaunchForce;
    public float mechLaunchForce;
    private bool hasLaunched = false;
    private Trigger trigger;
    private Animator animator;
    private float cooldownTimer = 0f;
    private bool canLaunch = true;

    public void Awake()
    {
       animator = GetComponentInChildren<Animator>();
       trigger = GetComponentInChildren<Trigger>();
    }

    public void FixedUpdate()
    {
        if (canLaunch)
        {
            GameObject collidedObject = trigger.CollidedGameObject();
            if (collidedObject && collidedObject.tag == "Player")
            {
                PlayerHandler playerHandler = collidedObject.GetComponentInParent<PlayerHandler>();

                if (playerHandler.IsGrounded)
                {
                    hasLaunched = false;
                }

                if (trigger.IsEnabled())
                {
                    hasLaunched = true;
                    animator.SetTrigger("Jump");
                }
                else
                    hasLaunched = false;

                if (hasLaunched)
                {
                    if (playerHandler.CurrentState == StateManager.PLAYER_STATE.Mech)
                    {
                        playerHandler.MechController.ResetGravity();
                        playerHandler.MechImpactRecevier.AddImpact(forceDirection.transform.up, mechLaunchForce);
                    }
                    else
                    {
                        playerHandler.CoreRigidbody.AddForce(forceDirection.transform.up * coreLaunchForce, ForceMode.Impulse);
                    }

                    hasLaunched = false;
                    canLaunch = false;
                }
            }
        }
        else
        {
            // Cool down timer to prevent constant launch:
            cooldownTimer += Time.deltaTime;
            if(cooldownTimer >= maxCooldown)
            {
                cooldownTimer = 0f;
                canLaunch = true;
            }
        }
    }
}
