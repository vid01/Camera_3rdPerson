using UnityEngine;
using System.Collections;
/// <summary>
/// Camera follows currently selected character in this state
/// at the ChasePoint distance
/// 
/// Author: Julie Maksymova
/// Last Edited Date: June 28/2015
/// </summary>
public class ChaseState : BaseCameraState
{
   // position of camera in Chase Mode
   [HideInInspector]
   public Transform chasePoint;
   //[HideInInspector]
   // must be the same as player's rotation speed
   public float rotationSpeed = 5f;
   // in degrees
   public float maxVerticalAngle = 40f;

   // rotation input axes
   private float vSpeed_Right = 0f;
   private float hSpeed_Right = 0f;

   // State transition -----------------------------
   private ZoomState zoomState;
   private bool zoomToggle = false;
   // Wall occlusion -------------------------------
   private Vector3 hitPos;
   private RaycastHit hit;
   private bool occludeMode = false;
   private bool occludeTrigger = false;
   private bool hitRight = false;
   private bool hitLeft = false;
   // LERP -----------------------------------------
   // transition camera to new position from previous state
   private float lerpTime = 1f;
   private float currentLerpTime = 0f;
   private float offsetFromTarget_Start;
   private float offsetFromTarget_End;
 
   // DEBUG ----------------------------------------
   //public void OnGUI()
   //{
   //   if (GameState.Instance.inGame())
   //   {
   //      GUILayout.Label(Input.mousePosition.ToString());
   //      GUILayout.Label(Input.GetAxis("Mouse X").ToString() + " " + Input.GetAxis("Mouse Y").ToString());
   //   }
   //}

   public void Start()
   {
      // disable mouse cursor
      Cursor.visible = false;
      zoomState = gameObject.GetComponent<ZoomState>();
 
      if (crosshairImage == null)
      {
         Debug.LogError("Crosshair texture for ChaseState is not set!");
      }
   }

   public override void Enter(CameraCtrl fsm)
   {
      // init base component
      base.Enter(fsm);
      // apply rotation speed settings to all characters
      // horizontal rotation ---------------------------
      IsPlayer[] playerChars = GameObject.FindObjectsOfType<IsPlayer>();
      foreach (IsPlayer character in playerChars)
      {
         ControlledMove moveCtrl = character.gameObject.GetComponent<ControlledMove>();
         moveCtrl.rotateSpeed = rotationSpeed;
      }
      // -----------------------------------------------

      // enter init position with default parameters on first enter
      if (currentPos == Vector3.zero)
      {
         // calculate distance offset from target
         offsetFromTarget = (targetFocusPoint.position - chasePoint.position).magnitude;
         // offset is set to max at init, no LERP between states is needed
         offsetFromTarget_End = offsetFromTarget;
         // offset value starts from the chasePoint
         offsetFromTargetVert = 0f;

         // get direction from chasePoint helper to target
         Vector3 disp = chasePoint.position - targetFocusPoint.position;
         desiredPos = targetFocusPoint.position + disp.normalized * offsetFromTarget;
      }
      else
      {
         // transition to ChaseState from ZoomState
         if (fsmCtrl.prevState is ZoomState)
         {
            // set Lerp values -> offset from zoomPoint
            offsetFromTarget_Start = (targetFocusPoint.position - zoomState.zoomPoint.position).magnitude;
            offsetFromTarget_End = (targetFocusPoint.position - chasePoint.position).magnitude;
            offsetFromTarget = offsetFromTarget_Start;
            // reset lerp timer
            currentLerpTime = 0f;
         }
         else
         {
            offsetFromTarget = (targetFocusPoint.position - chasePoint.position).magnitude;
         }

         // copy vertical offset from previous camera state
         offsetFromTargetVert = fsmCtrl.prevState.offsetFromTargetVert;

         // get direction to target
         Vector3 disp = chasePoint.position - targetFocusPoint.position;
         desiredPos = targetFocusPoint.position + disp.normalized * offsetFromTarget;
         // camera position on vertical plane
         desiredPos.y = chasePoint.position.y + offsetFromTargetVert;
      }

      // enable current player rotation
      //ControlledMove curMoveCtrl = fsmCtrl.chaseTarget.GetComponent<ControlledMove>();
      //curMoveCtrl.lockRotation = false;
   }

   public override void Exit()
   {
      base.Exit();
   }

   public override void Execute()
   {
      if (GameState.Instance.inGame())
      {
         // get axis and keys input -------------------------
         GetAxisInput();

         // transition camera from previous state -----------
         if (!(fsmCtrl.prevState is FollowSplineState))
         {
            CameraTransitionLerp();
         }

         // Obstacle occlusion ------------------------------
         // sets offset to occlude position
         OccludeObstacle();

         // move camera related to the player ---------------
         // horizontal rotation -----------------------------
         if (Mathf.Abs(hSpeed_Right) > 0.01f)
         {
            Vector3 disp = chasePoint.position - target.position;
            // rotate around Y axis
            Quaternion q = Quaternion.AngleAxis(hSpeed_Right * Time.deltaTime, Vector3.up);
            Vector3 rot = q * disp;
            rot = rot.normalized * offsetFromTarget;
            desiredPos = rot + target.position;

            if (!occludeMode)
            {
               // camera position on vertical plane
               desiredPos.y = chasePoint.position.y + offsetFromTargetVert;
            }
            else
            {
               desiredPos.y = target.position.y + offsetFromTargetVert;
            }
         }

         // vertical rotation -------------------------------
         if (Mathf.Abs(vSpeed_Right) > 0.01f)
         {
            VerticalRotationClamp();
         }

         // no camera movement input -------------------------
         if (!occludeMode)
         {
            Vector3 displ = chasePoint.position - targetFocusPoint.position;
            // camera position on horizontal plane
            desiredPos = targetFocusPoint.position + displ.normalized * offsetFromTarget;
            // camera position on vertical plane
            desiredPos.y = chasePoint.position.y + offsetFromTargetVert;
         }
         else
         {
            desiredPos.y = target.position.y + offsetFromTargetVert;
         }

         currentPos = desiredPos;
         transform.position = currentPos;
         transform.LookAt(targetFocusPoint.position);
      }
   }

   /// <summary>
   /// Clamps the angle during vertical camera rotation.
   /// </summary>
   private void VerticalRotationClamp()
   {
      // set invert Y axis
      if (!fsmCtrl.invertY)
      {
         vSpeed_Right = -vSpeed_Right;
      }

      Vector3 tempPosition = Camera.main.transform.position;
      tempPosition.y = currentPos.y + vSpeed_Right * rotationSpeed * 0.01f;
      //Debug.DrawLine(target.position, tempPosition, Color.red);

      // get angle between vectors target-chasePoint to target-camera
      Vector3 chaseDir;
      Vector3 cameraDir;

      if (occludeMode)
      {
         chaseDir = targetFocusPoint.position - target.position;
         cameraDir = targetFocusPoint.position - tempPosition;
      }
      else
      {
         chaseDir = chasePoint.position - target.position;
         cameraDir = tempPosition - target.position;
      }

      //Debug.DrawRay(target.position, chaseDir, Color.white);
      //Debug.DrawRay(target.position, cameraDir, Color.red);

      float angleEstimate = Vector3.Angle(chaseDir, cameraDir);
      //Debug.Log("angleEstimate: " + angleEstimate);
      //Debug.Log("vSpeed_Right " + vSpeed_Right);

      // allow rotation within limit ---------------
      if (angleEstimate <= maxVerticalAngle)
      {
         // set the offset
         offsetFromTargetVert += vSpeed_Right * rotationSpeed * 0.01f;
      }
      // if rotation exceeds the limit -------------
      else
      {
         // above chasePoint --------------
         if ((tempPosition.y > chasePoint.position.y)
            & (vSpeed_Right < 0.01f))
         {
            offsetFromTargetVert += vSpeed_Right * rotationSpeed * 0.01f;
         }
         // under chasePoint ----------------
         if ((tempPosition.y < chasePoint.position.y)
            & (vSpeed_Right > 0.01f))
         {
            offsetFromTargetVert += vSpeed_Right * rotationSpeed * 0.01f;
         }
      }
   }

   /// <summary>
   /// Transition between states in both ways - Chase and Zoom.
   /// </summary>
   private void CameraTransitionLerp()
   {
      bool lerpFlag = false;

      if (fsmCtrl.prevState is ZoomState)
      {
         if (offsetFromTarget <= offsetFromTarget_End)
         {
            lerpFlag = true;
         }

      }

      if (lerpFlag)
      {
         // tick timer once per frame
         currentLerpTime += Time.deltaTime;
         if (currentLerpTime > lerpTime)
         {
            currentLerpTime = lerpTime;
         }

         float perc = currentLerpTime / lerpTime;
         offsetFromTarget = Mathf.Lerp(offsetFromTarget_Start, offsetFromTarget_End, perc * 1f);
         Camera.main.fieldOfView = Mathf.Lerp(minZoomScale, maxZoomScale, perc * lerpSpeed);
      }
   }

   /// <summary>
   /// Obstacle occlusion based on raycast result (from camera to its target).
   /// </summary>
   private void OccludeObstacle()
   {
      // prevent camera from looking outside of the world
      //Debug.DrawRay(transform.position, transform.right, Color.blue);
      //Debug.DrawRay(transform.position, -transform.right, Color.green);
      if (Physics.Raycast(transform.position, transform.right, out hit, 0.3f))
      {
         occludeTrigger = true;
      }
      else if (Physics.Raycast(transform.position, -transform.right, out hit, 0.3f))
      {
         occludeTrigger = true;
      }

      OccludePosition();
   }

   private void OccludePosition()
   {
      // raycast direction - from chasePoint to camRaycastPoint on player
      Vector3 direction = camRaycastPoint.position - chasePoint.position;
      //Debug.DrawRay(chasePoint.position, direction, Color.green);
      if (Physics.Raycast(chasePoint.position, direction, out hit))
      {
         // occlude if camera doesn't see the player
         if (hit.collider.tag != "Player" | occludeTrigger)
         {
            // camera is pushed by the obstacle
            //hitPos = hit.point;
            //// calculate distance offset from hitPoint to target
            //offsetFromTarget = (target.position - hitPos).magnitude;

            // don't occlude if the hit obj is listed in OccludeFilter
            for (int i = 0; i < fsmCtrl.occludeFilter.Count; i++)
            {
               if (hit.collider.tag == fsmCtrl.occludeFilter[i])
               {
                  return;
               }
            }

            desiredPos = target.position;
            occludeMode = true;
            // set occlusion zoom position
            fsmCtrl.occlusionMode = occludeMode;
            offsetFromTarget = 0f;
         }
         else
         {
            offsetFromTarget = (targetFocusPoint.position - chasePoint.position).magnitude;
            occludeMode = false;
            fsmCtrl.occlusionMode = occludeMode;
         }

         // stop occlusion if chasePoint is not in the wall AND
         // occludeTrigger is ON
         if (hit.collider.tag == "Player" & occludeTrigger)
         {
            if (Physics.Raycast(transform.position, transform.right, out hit, 0.3f))
            {
               //Debug.Log("right hit");
               hitRight = true;
            }
            else
            {
               hitRight = false;
            }

            if (Physics.Raycast(transform.position, -transform.right, out hit, 0.3f))
            {
               //Debug.Log("left hit");
               hitLeft = true;
            }
            else
            {
               hitLeft = false;
            }

            // check left / right border
            if (!hitRight & !hitLeft)
            {
               occludeTrigger = false;
            }
         }
      }
   }

   /// <summary>
   /// Axis input for both joystick and mouse.
   /// </summary>
   private void GetAxisInput()
   {
      // check if joystick input will be present
      if (Input.GetJoystickNames()[0] != "")
      {
         // player rotation axis input - right stick
         vSpeed_Right = Input.GetAxis("JoystickRV");
         hSpeed_Right = Input.GetAxis("JoystickRH");
      }
      else
      {
         // player rotation axis input - mouse
         vSpeed_Right = Input.GetAxis("Mouse Y");
         hSpeed_Right = Input.GetAxis("Mouse X");
      }


      // Zoom input ------------------------
      float zoomInput = 0f;
      if (Input.GetJoystickNames()[0] != "")
      {
         // joystick range 0-1
         zoomInput = Input.GetAxis("JoystickLT");
         if (zoomInput > 0)
         {
            fsmCtrl.ChangeState(zoomState);
         }
      }
      else
      {
         // mouse input 
         zoomToggle = Input.GetMouseButton(1);
         if (zoomToggle)
         {
            fsmCtrl.ChangeState(zoomState);
         }
      }
   }
}
