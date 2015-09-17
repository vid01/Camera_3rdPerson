using UnityEngine;
using System.Collections;
/// <summary>
/// Zoom Mode for CameraCtrl script
/// Author: Julie Maksymova
/// Last Edited Date: July 21/2015
/// </summary>
public class CameraZoom_Helper : MonoBehaviour
{
   public CameraCtrl_Helper playerState;

   public float zoomSpeed = 7f;
   public float fov_current = 60f;
   public float fov_default = 60f;
   public float fov_zoom = 25f;
   //[HideInInspector]
   //public bool resetZoom = false;

   // LERP -----------------------------------------
   private bool canResetTimer = true;
   private float lerpTime = 1f;
   private float currentLerpTime = 0f;

   void Start()
   {
      playerState = GetComponent<CameraCtrl_Helper>();
   }

   void Update()
   {
         // zooming is disabled while running, during cinematics and menu
         //if (GameState.Instance.inGame() && !resetZoom)
         //{
            GetAxisInput();
            ZoomTransition();
         //}

         // reset zoom for cinematics
         //if (GameState.Instance.inCinematic() || resetZoom)
         //{
         //   fov_current = fov_default;
         //   canResetTimer = true;
         //   ZoomTransition();
         //}
   }

   private void ZoomTransition()
   {
      // tick timer once per frame
      currentLerpTime += Time.deltaTime;
      if (currentLerpTime > lerpTime)
      {
         currentLerpTime = lerpTime;
      }

      float perc = currentLerpTime / lerpTime;

      // zoom-in
      if (fov_current == fov_zoom)
      {
         Camera.main.fieldOfView = Mathf.Lerp(fov_default, fov_zoom, perc * zoomSpeed);
      }

      // zoom-out
      if (fov_current == fov_default)
      {
         Camera.main.fieldOfView = Mathf.Lerp(fov_zoom, fov_default, perc * zoomSpeed);
      }
   }

   private void GetAxisInput()
   {
      // Zoom input ------------------------
      float zoomInput = 0f;
      if (Input.GetJoystickNames()[0] != "")
      {
         // joystick range 0-1
         zoomInput = Input.GetAxis("JoystickLT");
         if (zoomInput > 0)
         {
            fov_current = fov_zoom;
            playerState.currentState = CameraCtrl_Helper.EntityState.ZOOM;
            if (canResetTimer)
            {
               currentLerpTime = 0f;
               canResetTimer = false;
            }
         }
         else
         {
            playerState.currentState = CameraCtrl_Helper.EntityState.WALK;
            fov_current = fov_default;
            canResetTimer = true;
         }
      }
      else
      {
         // mouse input 
         if (Input.GetMouseButton(1))
         {
            fov_current = fov_zoom;
            playerState.currentState = CameraCtrl_Helper.EntityState.ZOOM;
            if (canResetTimer)
            {
               currentLerpTime = 0f;
               canResetTimer = false;
            }
         }
         else
         {
            playerState.currentState = CameraCtrl_Helper.EntityState.WALK;
            fov_current = fov_default;
            canResetTimer = true;
         }
      }
   }
}
