using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Mark Rowland
//DoB: 5/8/2018
//Hookshot. Fires horizontally or vertically, depending on input.
//If it hits a wall, pulls to wall.
//If it hits an enmey, pulls player and enemy together.
//If it hits nothing, it retracts.

public class HookshotScript : MonoBehaviour
{
  //PROPERTIES
  public float HookDistance = 10F; //Maximum distance the hookshot should travel.
  public float TravelTime = 1F; //Time it takes to reach the maximum distance.
  public float Cooldown = 5F; //Wait this long before you can fire again.
  public GameObject Hook; //Hookshot object.

  //GLOBALS
  PlayerController player; //Player controller. Used to disable actions while hooking, get direction.
  Rigidbody body; //Rigidbody of the player.
  GameObject hookObject; //Hookshot object.
  Vector3 move; //Movement vector for the hookshot object.
  float travelSpeed; //How fast the hookshot travels. Calculated from HookDistance and TravelTime.
  float xOffset; //Spawn the hookshot this far way from the player on the x axis.
  float yOffset; //Spawn this far awy on the y axis.
  bool isHooking; //True while the hookshot is firing.
  bool goingOut; //True while the hookshot is moving away from the player.
  bool canHook; //True when the hookshot can be fired.
  float timer; //Used with Cooldown to determine when the hookshot can be fired again.
  
  // Use this for initialization
  void Start ()
  {
    //Get the hookshot, if necessary.
    if(!Hook)
    {
      Hook = Resources.Load("Hook") as GameObject;
    }

    //Get components.
    player = GetComponent<PlayerController>();
    body = GetComponent<Rigidbody>();

    //Init values.
    travelSpeed = HookDistance / TravelTime;
    xOffset = transform.localScale.x * 0.5F + Hook.transform.localScale.x * 0.5F;
    yOffset = transform.localScale.y * 0.5F + Hook.transform.localScale.y * 0.5F;
    isHooking = false;
    goingOut = false;
  }

  // Update is called once per frame
  void Update ()
  {
    //Check for relevent input.
    if(Input.GetKey(KeyCode.V) && !isHooking)
    {
      //Start hooking.
      isHooking = true;
      goingOut = true;

      //Spawn the hookshot object in the correct spot.
      Vector3 spawnPos = Vector3.zero;
      
      //Check to see if we're facing up first.
      if(Input.GetAxis("Vertical") > 0F)
      {
        //Put it above the player.
        spawnPos.x = transform.position.x;
        spawnPos.y = transform.position.y + yOffset;

        //Set the movement vector.
        move = new Vector3(0, travelSpeed, 0);
      }

      //If we're not going up, check the direction we're looking.
      else if(player.FacingRight())
      {
        //Put it in front of the player.
        spawnPos.x = transform.position.x + xOffset;
        spawnPos.y = transform.position.y;

        //Set the movement vector.
        move = new Vector3(travelSpeed, 0, 0);
      }
      else
      {
        //Put it in front of the player.
        spawnPos.x = transform.position.x - xOffset;
        spawnPos.y = transform.position.y;

        //Set the movement vector.
        move = new Vector3(-travelSpeed, 0, 0);
      }

      //Create the hookshot at the relevent position and move it along.
      hookObject = Instantiate(Hook, spawnPos, Quaternion.identity);
      hookObject.GetComponent<Rigidbody>().velocity = move;
    }
  }
}
