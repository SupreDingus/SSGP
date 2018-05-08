using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Mark Rowland
//DoB: 4/16/2018
//Basic player controller.

public class PlayerController : MonoBehaviour
{
  //PROPERTIES
  public float Speed = 2F; //Speed added to the player when moving.
  public float MaxSpeed = 20F; //Maximum speed of the player.
  public float JumpForce = 5F; //Strength of the jump force.
  public float JumpTime = 0.75F; //Holding down jump continues to jump for this long.
  [Range(0.01F, 0.99F)]
  public float SlowStrength = 0.25F; //How quickly the player slows when not inputting movement.
  public float SlowEpsilon = 0.01F; //Epsilon check for when we slow down.

  //GLOBALS
  Rigidbody body; //Rigidbody of this object.
  AttackScript attack; //AttackScript attached to this object.
  bool isGrounded; //True when touching the ground.
  bool canJump; //True if jumping can be done.
  bool facingRight; //True when facing right.
  bool isLocked; //If true, skip update.
  float airTime; //Timer used when jumping.

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
    canJump = false;
    isLocked = false;
	}

  // FixedUpdate is called on specific intervals.
  void FixedUpdate()
  {
    if(isLocked)
    {
      canJump = false;
      return;
    }

   
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
    if (Input.GetKey(KeyCode.Z) && canJump)
    {
      //Check for skip. (Airborne and we're past jumping.
      if (!isGrounded && airTime > JumpTime)
      {
        canJump = false;
      }
      else if(isGrounded)
      {
        airTime = 0F;
      }

      //Do the jump stuff.
      Vector3 temp = body.velocity;
      temp.y = JumpForce;
      body.velocity = temp;
      airTime += Time.fixedDeltaTime;
    }

    //Check for turning off canJump.
    if (!Input.GetKey(KeyCode.Z) && !isGrounded && canJump)
      canJump = false;

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
    //Y-Axis check.
    float bottom = transform.position.y - transform.localScale.y * 0.5F;
    float top = collision.transform.position.y + collision.transform.localScale.y * 0.5F;
    
    if(bottom >= top)
    {
      //X-axis check.
      float epsilon = 0.001F;
      float xScale = transform.localScale.x * 0.5F;
      float thisLeft = transform.position.x - xScale + epsilon;
      float thisRight = transform.position.x + xScale - epsilon;

      xScale = collision.transform.localScale.x * 0.5F;
      float otherLeft = collision.transform.position.x - xScale;
      float otherRight = collision.transform.position.x + xScale;

      if(otherLeft < thisRight && otherRight > thisLeft)
      {
        isGrounded = true;
        canJump = true;
      }
      else
      {
        print("Failed x check");
      }
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

  //Return if we're facing right or not.
  public bool FacingRight()
  {
    return facingRight;
  }

  //Lock / unlock the player controller.
  public void FlipLock()
  {
    isLocked = !isLocked;
  }
}
