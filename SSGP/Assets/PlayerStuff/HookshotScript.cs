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
  public GameObject Hook; //Hookshot object.

  //GLOBALS
  PlayerController player; //Player controller. Used to disable actions while hooking.
  Rigidbody body; //Rigidbody of the player.
  float travelSpeed; //How fast the hookshot travels. Calculated from HookDistance and TravelTime.
  bool isHooking; //True while the hookshot is firing.
  
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
  }

  // Update is called once per frame
  void Update ()
  {
    
  }
}
