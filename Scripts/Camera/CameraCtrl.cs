using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// Camera Control - wall occlusion and vertical rotation settings.
/// Author: Julie Maksymova
/// Last Edited Date: August 24 / 2015
/// </summary>
public class CameraCtrl : Singleton<CameraCtrl>
{
   protected CameraCtrl() {} // prevents construction

   // Position settings ---------------------
   [HideInInspector]
   public Dictionary<string, Transform> helperPoints = new Dictionary<string, Transform>();
   private Vector3 desiredPos = Vector3.zero;
   private Transform focusPoint;
   public float elevationAngle = 0f;
   public float currentRadius = 5f;
   // camera position during previous frame
   private Vector3 prevPosition = Vector3.zero;

   // Occlusion settings --------------------
   // don't occlude items with this tag in all Camera States
   public List<string> occludeFilter;
   public float occlusionOffset = 1.0f;
   [HideInInspector]
   public bool occlusionMode = false;
   private RaycastHit hit;

   // Global settings ----------------------
   // invert Y axis input in camera states
   [HideInInspector]
   public bool invertY = false;
   [HideInInspector]
   public float chaseRotationSpeed = 1f;
   [HideInInspector]
   public float zoomRotationSpeed = 1f;
   [HideInInspector]
   public float runRotationSpeed = 1f;

   void Start()
   {
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

         // cache raycast point in helpers
         IsRaycastPoint raycastPoint = FindObjectOfType<IsRaycastPoint>();
         if (raycastPoint != null)
         {
            helperPoints.Add("RaycastPoint", raycastPoint.transform);
         }
         else
         {
            Debug.LogError("Transform 'Spine' with IsRaycastPoint script was not found.");
         }
      }
      currentRadius = (focusPoint.position - helperPoints["ChasePoint"].position).magnitude;
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
      // ensures that character has moved completely in Update
      // before camera tracks its position
      desiredPos = focusPoint.position + (Mathf.Sin(elevationAngle * Mathf.Deg2Rad) * Vector3.up
                             - Mathf.Cos(elevationAngle * Mathf.Deg2Rad) * focusPoint.forward) * currentRadius;
      transform.LookAt(focusPoint.position);
      transform.rotation = Quaternion.LookRotation(focusPoint.position - transform.position);

      Vector3 dir = focusPoint.position - desiredPos;
      Debug.DrawRay(desiredPos, dir, Color.green);

      if (Physics.Raycast(desiredPos, dir, out hit, dir.magnitude + occlusionOffset))
      {
         if (hit.collider.gameObject.GetComponent<MeshRenderer>())
         {
            transform.position = hit.point - dir.normalized * occlusionOffset;
         }
         else
         {
            transform.position = desiredPos;
         }
      }
      else
      {
         transform.position = desiredPos;
      }
   }
}