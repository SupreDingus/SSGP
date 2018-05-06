using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Mark Rowland
//DoB: 5/5/2018
//Does short, horizontal dash.

public class DashScript : MonoBehaviour
{
  //PROPERTIES
  public float DashSpeed = 11F; //Speed of the dash.
  public float DashDuration = 0.75F; //Duration of dash.
  public float CoolDown = 1F; //Minimum time between dashes.
  public float FloatEpsilon = 1F; //Amount of upward force needed to counter gravity.

  //GLOBALS
  PlayerController controller; //Player controller. Used for direction, locking controls.
  Rigidbody body; //Rigidbody of the component.
  Vector3 move; //Movement vector.
  bool isDashing; //True when dashing.
  bool canDash; //True when the player can dash.
  float dashTimer; //Timer used when dashing.
  float cooldownTimer; //Timer used between dashing.
  
	// Use this for initialization
	void Start ()
  {
    //Get components.
    GetComponents();

    //Init values.
    move = Vector3.zero;
    dashTimer = 0F;
    cooldownTimer = 0F;
    isDashing = false;
    canDash = true;
	}
	
	// Update is called once per frame
	void Update ()
  {
    //Get the controller again if we don't have one.
    if (!controller || !body)
      GetComponents();

    //Check for relevent input and if we're not already dashing.
    if(Input.GetKeyDown(KeyCode.C) && canDash)
    {
      //Set relevent bools.
      isDashing = true;
      canDash = false;
      controller.FlipLock();
      dashTimer = 0F;
      cooldownTimer = 0F;
    }

    //If we're dashing...
    if(isDashing)
    {
      //Check to see if we need to set move.
      if (move == Vector3.zero)
      {
        //Check to see if what direction we're going in.
        if(controller.FacingRight())
        {
          move.x = DashSpeed;
        }
        else
        {
          move.x = -DashSpeed;
        }

        move.y = FloatEpsilon;
      }

      //Move it.
      body.velocity = move;

      //Increment timer.
      dashTimer += Time.deltaTime;

      //Check for done.
      if(dashTimer > DashDuration)
      {
        isDashing = false;
        controller.FlipLock();
      }
    }

    //Post-dash cooldown.
    else if (!canDash)
    {
      cooldownTimer += Time.deltaTime;
      
      //Check for done.
      if(cooldownTimer > CoolDown)
      {
        canDash = true;
        move = Vector3.zero;
      }
    }
	}

  //Get relevent components.
  private void GetComponents()
  {
    controller = GetComponent<PlayerController>();
    body = GetComponent<Rigidbody>();
  }
}
