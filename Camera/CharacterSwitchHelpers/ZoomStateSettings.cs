using UnityEngine;
using System.Collections;
/// <summary>
/// Contains settings for ZoomState for selected character
/// ----------- Used for character switching -------------
/// Author: Julie Maksymova
/// Last Edited Date: June 28/2015
/// </summary>
public class ZoomStateSettings : BaseCameraState 
{
   //[HideInInspector]
   public Transform zoomPoint;
   //[HideInInspector]
   public float rotationSpeed;
}
