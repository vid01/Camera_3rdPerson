using UnityEngine;
using System.Collections;
/// <summary>
/// ZoomState - camera transition to ZoomIn / Out Mode
/// 
/// Author: Julie Maksymova
/// Last Edited Date: June 28/2015
/// </summary>
public class ZoomState : BaseCameraState
{
   // position of camera in ZoomIn Mode
   [HideInInspector]
   public Transform zoomPoint;
   public Transform pivotPoint;
   //[HideInInspector]
   public float rotationSpeed = 5f;
   // in degrees
   public float maxVerticalAngle = 40f;

   // for state transitions
   private ChaseState chaseState;
   private bool zoomToggle = false;

   // rotation input axes for Zoom Mode
   private float vSpeed_Right = 0f;
   private float hSpeed_Right = 0f;

   // Wall occlusion -------------------------------
   private Vector3 hitPos;
   private RaycastHit hit;
   private bool occludeMode = false;
   private bool occludeTrigger = false;

   // LERP -----------------------------------
   // transition camera to new position from previous state
   float lerpTime = 1f;
   float currentLerpTime = 0f;
   float offsetFromTarget_Start;
   float offsetFromTarget_End;

   public void Start()
   {
      chaseState = gameObject.GetComponent<ChaseState>();
      // set position for wall occlusion
      pivotPoint = chaseState.target.transform;

      if (crosshairImage == null)
      {
         Debug.LogError("Crosshair texture for ZoomState is not set!");
      }
   }

   public override void Enter(CameraCtrl fsm)
   {
      // init base component
      base.Enter(fsm);

      // apply rotation speed settings to all characters
      // vertical rotation -----------------------------
      IsPlayer[] playerChars = GameObject.FindObjectsOfType<IsPlayer>();
      foreach (IsPlayer character in playerChars)
      {
         ControlledMove moveCtrl = character.gameObject.GetComponent<ControlledMove>();
         moveCtrl.rotateSpeed = rotationSpeed;
      }
      // -----------------------------------------------

      // set Lerp values -> offset from zoomPoint
      offsetFromTarget = (chaseState.chasePoint.position - zoomPoint.position).magnitude;
      offsetFromTarget_Start = offsetFromTarget;
      offsetFromTarget_End = 0f;
      // reset lerp timer
      currentLerpTime = 0f;

      // copy vertical offset from previous camera state
      offsetFromTargetVert = fsmCtrl.prevState.offsetFromTargetVert;

      // get direction to target
      Vector3 disp = chaseState.chasePoint.position - zoomPoint.position;
      desiredPos = zoomPoint.position + disp.normalized * offsetFromTarget;

      // enable current player rotation
      ControlledMove curMoveCtrl = fsmCtrl.chaseTarget.GetComponent<ControlledMove>();
      curMoveCtrl.lockRotation = false;
   }

   public override void Exit()
   {
      base.Exit();
   }

   public override void Execute()
   {
      if (GameState.Instance.inGame())
      {
         // PLAYER INPUT -------------------------------
         GetAxisInput();

         // LERP ---------------------------------------
         CameraTransitionLerp();

         // Obstacle occlusion -------------------------
         // sets offset to occlude position
         OccludeObstacle();

         // JOYSTICK ROTATION --------------------------
         if (hSpeed_Right != 0)
         {
            Vector3 disp = chaseState.chasePoint.position - zoomPoint.position;
            Quaternion q = Quaternion.AngleAxis(hSpeed_Right * rotationSpeed * Time.deltaTime, Vector3.up);
            Vector3 rot = q * disp;
            rot = rot.normalized * offsetFromTarget;

            if (!occludeMode)
            {
               desiredPos = rot + zoomPoint.position;
               // camera position on vertical plane         
               desiredPos.y = chaseState.chasePoint.position.y + offsetFromTargetVert;
            }
            else
            {
               desiredPos = rot + pivotPoint.position;
               desiredPos.y = pivotPoint.position.y + offsetFromTargetVert;
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
            // get direction to target
            Vector3 displ = chaseState.chasePoint.position - zoomPoint.position;
            // camera position on horizontal plane
            desiredPos = zoomPoint.position + displ.normalized * offsetFromTarget;
            // camera position on vertical plane
            desiredPos.y = chaseState.chasePoint.position.y + offsetFromTargetVert;
         }
         else
         {
            desiredPos = pivotPoint.position;
         }

         // set current camera position
         currentPos = desiredPos;
         transform.position = currentPos;

         if (!occludeMode)
         {
            transform.LookAt(targetFocusPoint.position);
         }
         else
         {
            Quaternion rotation = Quaternion.LookRotation(fsmCtrl.chaseTarget.transform.forward, Vector3.up);
            transform.rotation = rotation;
         }
      }
   }

   /// <summary>
   /// Transition between states in both ways - Chase and Zoom.
   /// </summary>
   public void CameraTransitionLerp()
   {
      if (offsetFromTarget > 0f)
      {
         // tick timer once per frame
         currentLerpTime += Time.deltaTime;
         if (currentLerpTime > lerpTime)
         {
            currentLerpTime = lerpTime;
         }

         float perc = currentLerpTime / lerpTime;
         offsetFromTarget = Mathf.Lerp(offsetFromTarget_Start, offsetFromTarget_End, perc * lerpSpeed);
         Camera.main.fieldOfView = Mathf.Lerp(maxZoomScale, minZoomScale, perc * lerpSpeed);
      }
      // lerp only fov for wall occlusion
      else if (occludeMode)
      {
         // tick timer once per frame
         currentLerpTime += Time.deltaTime;
         if (currentLerpTime > lerpTime)
         {
            currentLerpTime = lerpTime;
         }

         float perc = currentLerpTime / lerpTime;
         Camera.main.fieldOfView = Mathf.Lerp(maxZoomScale, minZoomScale, perc * lerpSpeed);
      }
   }

   /// <summary>
   /// Obstacle occlusion based on raycast result (from camera to its target).
   /// </summary>
   private void OccludeObstacle()
   {
      RaycastHit hit;
      // prevent camera from looking outside of the world
      Debug.DrawRay(transform.position, transform.right, Color.blue);
      Debug.DrawRay(transform.position, -transform.right, Color.green);
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
      RaycastHit hit;
      // raycast direction - from chasePoint to camRaycastPoint on player
      Vector3 direction = camRaycastPoint.position - zoomPoint.position;
      Debug.DrawRay(zoomPoint.position, direction, Color.green);
      if (Physics.Raycast(zoomPoint.position, direction, out hit))
      {
         // occlude if camera doesn't see the player
         if (hit.collider.tag != "Player" | occludeTrigger)
         {
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

            desiredPos = pivotPoint.position;
            occludeMode = true;
            // set occlusion zoom position
            fsmCtrl.occlusionMode = occludeMode;
            offsetFromTarget = 0f;
         }
         else
         {
            offsetFromTarget = (chaseState.chasePoint.position - zoomPoint.position).magnitude;
            occludeMode = false;
         }

         // stop occlusion if chasePoint is not in the wall
         if (hit.collider.tag == "Player" & occludeTrigger)
         {
            occludeTrigger = false;
         }
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

      // get vector origin for angle check
      Vector3 focusPoint = fsmCtrl.chaseTarget.transform.Find("FocusPoint").transform.position;

      //Debug.DrawLine(focusPoint, tempPosition, Color.red);

      // get angle between vectors target-chasePoint to target-camera
      Vector3 chaseDir;
      Vector3 cameraDir;

      if (occludeMode)
      {
         chaseDir = pivotPoint.position - focusPoint;
         cameraDir = tempPosition - focusPoint;        
      }
      else
      {
         chaseDir = zoomPoint.position - focusPoint;
         cameraDir = tempPosition - focusPoint;
      }

      //Debug.DrawRay(targetFocusPoint.position, chaseDir, Color.white);
      //Debug.DrawRay(targetFocusPoint.position, cameraDir, Color.red);

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
         // above zoomPoint --------------
         if ((tempPosition.y > zoomPoint.position.y)
            & (vSpeed_Right < 0.01f))
         {
            offsetFromTargetVert += vSpeed_Right * rotationSpeed * 0.01f;
         }
         // under zoomPoint ----------------
         if ((tempPosition.y < zoomPoint.position.y)
            & (vSpeed_Right > 0.01f))
         {
            offsetFromTargetVert += vSpeed_Right * rotationSpeed * 0.01f;
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
         // range 0-1
         zoomInput = Input.GetAxis("JoystickLT");
         if (zoomInput <= 0)
         {
            fsmCtrl.ChangeState(chaseState);
         }
      }
      else
      {
         zoomToggle = Input.GetMouseButton(1);
         if (!zoomToggle)
         {
            fsmCtrl.ChangeState(chaseState);
         }
      }
   }
}