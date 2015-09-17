using UnityEngine;
using System.Collections;
/// <summary>
/// Manages player's state transitions.
/// Author: Julie Maksymova
/// Last Edited Date: September 1 / 2015
/// </summary>
public class CameraCtrl_Helper : MonoBehaviour 
{
   public float lerpSpeed = 20f;
   [HideInInspector]
   public float walkDist = 0f;
   [HideInInspector]
   public float runDist = 0f;
   [HideInInspector]
   public float zoomDist = 0f;
   private float desiredRadius = 0f;

   CameraZoom_Helper zoomHelper;

   public enum EntityState
   {
      WALK,
      RUN,
      ZOOM
   };

   public EntityState currentState = EntityState.WALK;

	void Start () 
   {
      CalculateRadius();
      CameraCtrl.Instance.currentRadius = walkDist;
      zoomHelper = GetComponent<CameraZoom_Helper>();
	}
	
   public void CalculateRadius()
   {
      walkDist = (CameraCtrl.Instance.helperPoints["ChasePoint"].position - CameraCtrl.Instance.helperPoints["FocusPoint"].position).magnitude;
      runDist = (CameraCtrl.Instance.helperPoints["RunPoint"].position - CameraCtrl.Instance.helperPoints["FocusPoint"].position).magnitude;
      zoomDist = (CameraCtrl.Instance.helperPoints["ZoomPoint"].position - CameraCtrl.Instance.helperPoints["FocusPoint"].position).magnitude;
   }
	
	void Update () 
   {
      if (currentState == EntityState.WALK)
      {
         desiredRadius = walkDist;
      }
      else if (currentState == EntityState.RUN && 
               PlayerInput.Instance.axisInputValue_Left > 0.01f)
      {
         desiredRadius = runDist;
         zoomHelper.enabled = false;
      }
      else if (currentState == EntityState.ZOOM)
      {
         desiredRadius = zoomDist;
      }

      // enable zooming when player is not running
      if (currentState == EntityState.RUN &&
          PlayerInput.Instance.axisInputValue_Left > 0.01f)
      {
         zoomHelper.enabled = true;
      }

      CameraCtrl.Instance.currentRadius = Mathf.Lerp(CameraCtrl.Instance.currentRadius, desiredRadius, lerpSpeed * Time.deltaTime);
	}
}
