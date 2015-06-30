using UnityEngine;
using System.Collections;
/// <summary>
/// BaseCameraState - general camera properties
/// 
/// Author: Julie Maksymova
/// Last Edited Date: June 28/2015
/// </summary>
public class BaseCameraState : MonoBehaviour
{
   protected CameraCtrl fsmCtrl;
   // where the camera should be in each state
   protected Vector3 desiredPos = Vector3.zero;
   [HideInInspector]
   public Vector3 currentPos = Vector3.zero;

   // crosshair specific for a current camera state
   //[HideInInspector]
   public Texture2D crosshairImage; // static center image
   //[HideInInspector]
   public Texture2D crosshair_TL; // top left
   //[HideInInspector]
   public Texture2D crosshair_TR; // top right
   //[HideInInspector]
   public Texture2D crosshair_BL; // bottom left
   //[HideInInspector]
   public Texture2D crosshair_BR; // bottom right

   // target tracked by camera - player, enemy etc.
   // rotate around this target
   //[HideInInspector]
   public Transform target;
   [HideInInspector]
   public Vector3 lastCameraTargetPos = Vector3.zero;
   // camera lookAt - face the focus point while rotating
   //[HideInInspector]
   public Transform targetFocusPoint;
   // raycast camera from player for wall occlusion
   //[HideInInspector]
   public Transform camRaycastPoint;
   // offset from player based on position of helper point - horizontal
   [HideInInspector]
   public float offsetFromTarget = 1f;
   // offset from player based on rotation input - vertical
   [HideInInspector]
   public float offsetFromTargetVert = 1f;
   // transition modifier
   public float lerpSpeed = 5f;
   // scales camera's field of view
   public float maxZoomScale = 60f;
   public float minZoomScale = 25f;

   public virtual void Enter(CameraCtrl fsm)
   {
      fsmCtrl = fsm;
   }

   public virtual void Execute() { }
   public virtual void Exit()
   {
      fsmCtrl.prevState = this;
   }
}
