using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Mark Rowland
//DoB: 5/22/2018
//Hook logic script. Interacts with corresponding player script.

public class HookLogic : MonoBehaviour
{
  //PROPERTIES

  //GLOBALS
  bool hitSomething; //True when the hook hits something.
  GameObject hit; //Object hit.

  // Use this for initialization
  void Start ()
  {
    //Init values.
    hitSomething = false;
  }

  // Update is called once per frame
  void Update ()
  {
    
  }

  private void OnCollisionEnter(Collision collision)
  {
    //If we hit the player, skip it.
    if (collision.gameObject.tag == "Player")
    {
      return;
    }

    //Set hit, get the object info.
    hitSomething = true;
    hit = collision.transform.gameObject;

    //Lock object in place.
    GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
  }

  public bool GetHitStatus()
  {
    return hitSomething;
  }

  public GameObject GetHitObject()
  {
    return hit;
  }
}
