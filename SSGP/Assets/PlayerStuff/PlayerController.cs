using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Mark Rowland
//DoB: 4/16/2018
//Basic player controller.

public class PlayerController : MonoBehaviour
{
  //PROPERTIES
  public float Speed = 2F; //Speed added to the player when moving..
  public float MaxSpeed = 20F; //Maximum speed of the player.
  public float JumpForce = 5F; //Strength of the jump force.
  [Range(0.01F, 0.99F)]
  public float SlowStrength = 0.25F; //How quickly the player slows when not inputting movement.
  public float SlowEpsilon = 0.01F; //Epsilon check for when we slow down.

  //GLOBALS
  Rigidbody body; //Rigidbody of this object.
  AttackScript attack; //AttackScript attached to this object.
  bool isGrounded; //True when touching the ground.
  bool facingRight; //True when facing right.

	// Use this for initialization
	void Start ()
  {
    //Get the necessary components.
    body = GetComponent<Rigidbody>();
    attack = GetComponent<AttackScript>();

    //Lock rotation.
    body.freezeRotation = true;

    //Init necessary values.
    isGrounded = false;
    facingRight = true;
	}

  // FixedUpdate is called on specific intervals.
  void FixedUpdate()
  {
    //Only take movement input if we're not attacking.
    if (!attack.IsAttacking())
    {
      //Left / right input.
      if (Input.GetAxisRaw("Horizontal") != 0F)
      {
        if (Input.GetAxisRaw("Horizontal") > 0F)
        {
          body.AddForce(Vector3.right * Speed);
          facingRight = true;
        }
        if (Input.GetAxisRaw("Horizontal") < 0F)
        {
          body.AddForce(-Vector3.right * Speed);
          facingRight = false;
        }

        //Don't go too fast now.
        if(Mathf.Abs(body.velocity.x) > MaxSpeed)
        {
          //Clamp the speed as necessary.
          Vector3 temp = new Vector3(0, body.velocity.y, 0);
          if (body.velocity.x > 0F)
          {
            temp.x = MaxSpeed;
          }
          else
          {
            temp.x = -MaxSpeed;
          }
          body.velocity = temp;
        }
      }

      //Jump input.
      if (Input.GetAxisRaw("Vertical") > 0F && isGrounded)
      {
        body.AddForce(Vector3.up * JumpForce);
        isGrounded = false;
      }
    }

    //If we attack and we're grounded, stop movement.
    else if (isGrounded)
    {
      Vector3 temp = new Vector3(0, body.velocity.y, 0);
      body.velocity = temp;
    }

    //If we didn't get any x-input, slow it down, if necessary.
    if (Input.GetAxisRaw("Horizontal") == 0 && Mathf.Abs(body.velocity.x) > SlowEpsilon)
    {
      //Calculate slow strength.
      float slow;
      if (body.velocity.x > 0F)
      {
        slow = -SlowStrength;
      }
      else
      {
        slow = SlowStrength;
      }
      
      //Apply slow.
      body.AddForce(Vector3.right * Speed * slow);
    }

    //Check to see if we're too slow..
    else if (Mathf.Abs(body.velocity.x) < SlowEpsilon)
    {
      //If we did, stop it.
      Vector3 temp = new Vector3(0, body.velocity.y, 0);
      body.velocity = temp;
    }
  }

  private void OnCollisionEnter(Collision collision)
  {
    //If the colliding object is below us, we're on the ground.
    float bottom = transform.position.y - transform.localScale.y;
    float top = collision.transform.position.y + collision.transform.localScale.y;

    if(bottom <= top)
    {
      isGrounded = true;
    }
    else
    {
      isGrounded = false;
    }
  }

  private void OnCollisionExit(Collision collision)
  {
    //Always reset to false, just in case.
    isGrounded = false;
  }

  public bool FacingRight()
  {
    return facingRight;
  }
}
