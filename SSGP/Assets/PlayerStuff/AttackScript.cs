using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackScript : MonoBehaviour
{
  //PROPERTIES
  [Range(1, 10)]
  public int Damage = 1; //Damage dealt per attack.
  public float Backswing = 1.1F; //How long before the player can attack again.
  public float DamageTime = 1F; //How long the damage area stays up. Must be lower than Backswing.

  //GLOBALS
  GameObject attackArea; //Game object used when attacking. Childed on the player.
  PlayerController player; //PlayerController attached to this object.
  bool isAttacking; //True when attacking.
  float backswingTimer; //Timer used with backswing check.
  float damageTimer; //Timer used with damage check.
  float offset; //Use this to offset the area from the player.

  // Use this for initialization
  void Start ()
  {
    //Get the attack area and set it to disabled.
    attackArea = this.gameObject.transform.GetChild(0).gameObject;
    attackArea.SetActive(false);

    //Get the controller component.
    player = GetComponent<PlayerController>();

    //Setup the offset vector.
    offset = transform.localScale.x * 0.5F + attackArea.transform.localScale.x * 0.5F;

    //Init values.
    backswingTimer = damageTimer = 0F;
    isAttacking = false;
  }
  
  // Update is called once per frame
  void Update ()
  {
    //Flip which side the attack area is on.
    if(player.FacingRight() && !isAttacking)
    {
      attackArea.transform.position = new Vector3(offset + transform.position.x, transform.position.y, transform.position.z);
    }
    else if (!player.FacingRight() && !isAttacking)
    {
      attackArea.transform.position = new Vector3(-offset + transform.position.x, transform.position.y, transform.position.z);
    }

    //Check for attack input.
    //If we're not attacking, instantiate the object.
    if (Input.GetKeyDown(KeyCode.X) && !isAttacking)
    {
      //Enable the area.
      attackArea.SetActive(true);

      //Set variables.
      damageTimer = 0F;
      backswingTimer = 0F;
      isAttacking = true;

      //Go ahead and git outta here.
      return;
    }

    //Check to see if we can attack again.
    if(isAttacking)
    {
      //Increment timer.
      backswingTimer += Time.deltaTime;
      
      //Reset variable if necessary.
      if(backswingTimer > Backswing)
      {
        attackArea.SetActive(false);
        isAttacking = false;
      }
    }
	}

  public bool IsAttacking()
  {
    return isAttacking;
  }
}
