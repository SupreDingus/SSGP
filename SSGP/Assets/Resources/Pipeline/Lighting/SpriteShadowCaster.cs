//Author: Alexzander DeJardin
//DoB: 6/24/2018
//Useless component that signals to the lighting system that this object should cast shadows

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CasterType
{
    Box,
    Sphere,
    Mesh,
}

public class SpriteShadowCaster : MonoBehaviour
{
    public CasterType shadowType = CasterType.Box;

    void Start()
    {

    }
}
