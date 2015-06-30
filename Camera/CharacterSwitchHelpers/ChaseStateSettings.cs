using UnityEngine;
using System.Collections;
/// <summary>
/// Contains settings for ChaseState for selected character
/// ----------- Used for character switching --------------
/// Author: Julie Maksymova
/// Last Edited Date: June 28/2015
/// </summary>
public class ChaseStateSettings : BaseCameraState
{
   //[HideInInspector]
   public Transform chasePoint;
   //[HideInInspector]
   public float rotationSpeed;
}
