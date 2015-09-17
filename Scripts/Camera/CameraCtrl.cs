using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// Camera Control - wall occlusion and vertical rotation settings.
/// Author: Julie Maksymova
/// Last Edited Date: August 30 / 2015
/// </summary>
public class CameraCtrl : Singleton<CameraCtrl>
{
   protected CameraCtrl() {} // prevents construction
   private CameraCtrl_Helper camHelper;

   // Position settings ---------------------
   [HideInInspector]
   public Dictionary<string, Transform> helperPoints = new Dictionary<string, Transform>();
   private Vector3 desiredPos = Vector3.zero;
   // previous position within boundary
   private Vector3 prevPosition = Vector3.zero;
   private Vector3 finalPos = Vector3.zero;
   public float camLerpSpeed = 5f;
   private Transform focusPoint;
   // raycast the player
   private Transform spinePoint;
   [HideInInspector]
   public float elevationAngle = 0f;
   [HideInInspector]
   // previous angle within boundary
   public float prevElevationAngle = 0f;
   public float currentRadius = 5f;
   public float maxAngle = 60f;
   private float curMaxAngle = 60f;

   // Occlusion settings --------------------   
   public float occlusionOffset = 1.0f;
   private RaycastHit hit;
   private RaycastHit hitPlayer;

   // Global settings -----------------------
   // invert Y axis input in camera states
   [HideInInspector]
   public bool invertY = false;

   void Awake()
   {
      // get player state updates
      GameObject player = FindObjectOfType<IsPlayer>().gameObject;
      camHelper = player.GetComponent<CameraCtrl_Helper>();

      // cache camera helper points - in game mode
      if (helperPoints.Count < 4)
      {
         IsCamHelperPoint[] found_helpers = FindObjectsOfType<IsCamHelperPoint>();
         foreach (IsCamHelperPoint helper in found_helpers)
         {
            helperPoints.Add(helper.name, helper.transform);

            if (helper.name.Equals("FocusPoint"))
            {
               focusPoint = helper.transform;
            }
         }
      }
      currentRadius = (focusPoint.position - helperPoints["ChasePoint"].position).magnitude;
      spinePoint = FindObjectOfType<IsSpinePoint>().gameObject.transform;
      curMaxAngle = maxAngle;
   }

   void OnDrawGizmos()
   {
      // cache camera helper points for display - game mode off
      if (helperPoints.Count < 4)
      {
         IsCamHelperPoint[] found_helpers = FindObjectsOfType<IsCamHelperPoint>();
         foreach (IsCamHelperPoint helper in found_helpers)
         {
            helperPoints.Add(helper.name, helper.transform);
         }
      }
      else
      {
         foreach (var helper in helperPoints)
         {
            if (helper.Value != null)
            {
               Gizmos.DrawWireSphere(helper.Value.position, 0.05f);
            }
         }
      }
   }

   void LateUpdate()
   {
      prevPosition = transform.position;

      // ensures that character has moved completely in Update
      // before camera tracks its position
      desiredPos = focusPoint.position + (Mathf.Sin(elevationAngle * Mathf.Deg2Rad) * Vector3.up
                             - Mathf.Cos(elevationAngle * Mathf.Deg2Rad) * focusPoint.forward) * currentRadius;

      if (!BoundaryCheck())
      {
         desiredPos = focusPoint.position + (Mathf.Sin(prevElevationAngle * Mathf.Deg2Rad) * Vector3.up
                             - Mathf.Cos(prevElevationAngle * Mathf.Deg2Rad) * focusPoint.forward) * currentRadius;
      }
      else
      {
         prevElevationAngle = elevationAngle;
      }

      transform.LookAt(focusPoint.position);
      transform.rotation = Quaternion.LookRotation(focusPoint.position - transform.position);

      Vector3 dir = focusPoint.position - desiredPos;
      Vector3 dirPlayer = spinePoint.position - desiredPos;
      Debug.DrawRay(desiredPos, dir, Color.green);
      Debug.DrawRay(desiredPos, dirPlayer, Color.yellow);

      // raycast from focusPoint
      if (Physics.Raycast(desiredPos, dir, out hit, dir.magnitude + occlusionOffset))
      {
         if (hit.collider.gameObject.GetComponent<MeshRenderer>() && 
             !hit.collider.gameObject.name.Equals("FocusPoint"))
         {
            finalPos = hit.point - dir.normalized * occlusionOffset;
         }
         else
         {
            finalPos = desiredPos;
         }
      }
      // raycast from Player
      else if (Physics.Raycast(desiredPos, dirPlayer, out hitPlayer, dirPlayer.magnitude + occlusionOffset))
      {
         if (hitPlayer.collider.gameObject.GetComponent<MeshRenderer>() &&
             !hitPlayer.collider.gameObject.name.Equals("FocusPoint"))
         {
            finalPos = hitPlayer.point - dirPlayer.normalized * occlusionOffset;
         }
         else
         {
            finalPos = desiredPos;
         }
      }
      else
      {
         finalPos = desiredPos;
      }

      transform.position = Vector3.Lerp(prevPosition, finalPos, camLerpSpeed * Time.deltaTime);
   }

   /// <summary>
   /// Checks if next position doesn't exceed the rotation boundary.
   /// </summary>
   private bool BoundaryCheck()
   {
      // get vector from chase point to focus point
      Vector3 origin = focusPoint.position - helperPoints["ChasePoint"].position;
      // get vector from next possible position and focusPoint
      Vector3 nextPos = focusPoint.position - desiredPos;

      if (camHelper.currentState == CameraCtrl_Helper.EntityState.RUN)
      {
         curMaxAngle = maxAngle * 0.5f;
      }
      else
      {
         curMaxAngle = maxAngle;
      }

      if (Vector3.Angle(origin, nextPos) < curMaxAngle * 0.5f)
      {
         return true;
      }

      elevationAngle = prevElevationAngle;
      return false;
   }
}