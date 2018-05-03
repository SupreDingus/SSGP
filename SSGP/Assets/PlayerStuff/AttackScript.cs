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
  public GameObject AttackArea; //Game object used when attacking.

  //GLOBALS
  GameObject damageObject; //Instantiate / destroy on this object.
  PlayerController player; //PlayerController attached to this object.
  bool isAttacking; //True when attacking.
  float backswingTimer; //Timer used with backswing check.
  float damageTimer; //Timer used with damage check.
  float attackAreaScale; //Half of the width of the total area.

  // Use this for initialization
  void Start ()
  {
    //Get the attack area, if necessary.
    if(!AttackArea)
      AttackArea = Resources.Load("AttackPrefab") as GameObject;

    //Get the controller component.
    player = GetComponent<PlayerController>();

    //Init values.
    backswingTimer = damageTimer = 0F;
    isAttacking = false;
    attackAreaScale = AttackArea.transform.localScale.x * 0.5F + transform.localScale.x * 0.5F;
	}
	
	// Update is called once per frame
	void Update ()
  {
    //Check for input.
    //If we're not attacking, instantiate the object.
    if (Input.GetKeyDown(KeyCode.X) && !isAttacking)
    {
      //Are we facing left or right?
      Vector3 spawnPos = transform.position;
      if (player.FacingRight())
      {
        spawnPos.x += attackAreaScale;
      }
      else
      {
        spawnPos.x -= attackAreaScale;
      }

      //Create the area.
      damageObject = Instantiate(AttackArea, spawnPos, Quaternion.identity);

      //Set variables.
      damageTimer = 0F;
      backswingTimer = 0F;
      isAttacking = true;

      //Go ahead and git outta here.
      return;
    }

    //Check to see if we get rid of the damage area already.
    if(damageObject)
    {
      //Increment timer.
      damageTimer += Time.deltaTime;

      //Destroy if necessary.
      if(damageTimer > DamageTime)
      {
        Destroy(damageObject);
      }
    }

    //Check to see if we can attack again.
    if(isAttacking)
    {
      //Increment timer.
      backswingTimer += Time.deltaTime;
      
      //Reset variable if necessary.
      if(backswingTimer > Backswing)
      {
        isAttacking = false;
      }
    }
	}

  public bool IsAttacking()
  {
    return isAttacking;
  }
}
