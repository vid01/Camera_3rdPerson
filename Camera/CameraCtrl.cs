using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// Camera FSM - states are switched based on current player's actions
/// 
/// Author: Julie Maksymova
/// Last Edited Date: June 30/2015
/// </summary>
public class CameraCtrl : Singleton<CameraCtrl>
{
   protected CameraCtrl() { }     // guarantee that only one instance will be created
   public BaseCameraState currentState;
   public GameObject chaseTarget; // current active Player

   // Occlusion camera settings --------------------
   // don't occlude items with this tag in all Camera States
   public List<string> occludeFilter;
   [HideInInspector]
   public bool occlusionMode = false;
   // obj name in parent model that will be used as a raycast point for occlusion check
   public string occludeColliderName = "Chest";

   [HideInInspector]
   // for zoom in / out position sync
   public BaseCameraState prevState;

   // Global camera settings ----------------------
   // invert Y axis input in camera states
   [HideInInspector]
   public bool invertY = false;

   // Cache helper objs for gizmos display --------
   public struct PlayerHelperPos
   {
      public string characterName;
      public GameObject focusPoint;
      public GameObject zoomPoint;
      public GameObject chasePoint;
      public GameObject camCastPoint;
      public GameObject pivot;
      public GameObject inCoverPoint;
   }

   [HideInInspector]
   public List<PlayerHelperPos> playerHelpers;

   [HideInInspector]
   // Cache playable characters ------------------
   public List<GameObject> playerChars;

   void Start()
   {
      // set init state
      if (currentState != null)
      {
         currentState.Enter(this);
      }
      else
      {
         Debug.Log("Camera init state is not set!");
      }

      // on init enable input control on current chaseTarget
      if (chaseTarget != null)
      {
         //PlayerInput cur_inputCtrl = chaseTarget.GetComponent<PlayerInput>();
         //cur_inputCtrl.enabled = true;
      }
      else
      {
         Debug.Log("Camera chase target is not set!");
      }
   }

   void Awake()
   {
      playerHelpers = new List<PlayerHelperPos>();
      playerChars = new List<GameObject>();
      CacheHelperPoints();
   }

   public void ChangeState(BaseCameraState state)
   {
      // save prev state
      prevState = currentState;

      currentState.Exit();
      currentState = state;
      currentState.Enter(this);
   }

   void StateUpdates()
   {
      if (currentState != null)
      {
         currentState.Execute();
      }
   }

   void LateUpdate()
   {
      // ensure that character has moved completely in Update
      // before camera tracks its position
      StateUpdates();
   }

   /// <summary>
   /// Used in CharacterSwitchController, sets new target for the Camera to follow.
   /// Target is changed in Chase and Zoom States on current Camera.
   /// </summary>
   /// <param name="newChaseTarget">New target Player for the Camera</param>
   public void SetChaseTarget(GameObject newChaseTarget)
   {
      // disable input for prev character
      //PlayerInput prev_inputCtrl = chaseTarget.GetComponent<PlayerInput>();
      //prev_inputCtrl.enabled = false;

      // enable input for current character
      chaseTarget = newChaseTarget;
      //PlayerInput cur_inputCtrl = chaseTarget.GetComponent<PlayerInput>();
      //cur_inputCtrl.enabled = true;

      // write camera settings to the new target
      // ChaseState settings ---------------------------------------------
      ChaseState chaseState = Camera.main.GetComponent<ChaseState>();
      ChaseStateSettings chaseSettings = chaseTarget.GetComponent<ChaseStateSettings>();
      chaseState.target = chaseSettings.target;
      chaseState.targetFocusPoint = chaseSettings.targetFocusPoint;
      chaseState.camRaycastPoint = chaseSettings.camRaycastPoint;
      chaseState.lerpSpeed = chaseSettings.lerpSpeed;
      chaseState.chasePoint = chaseSettings.chasePoint;
      chaseState.rotationSpeed = chaseSettings.rotationSpeed;

      // InCoverState settings -------------------------------------------
      InCoverState coverState = Camera.main.GetComponent<InCoverState>();
      InCoverStateSettings coverSettings = chaseTarget.GetComponent<InCoverStateSettings>();
      coverState.target = coverSettings.target;
      coverState.targetFocusPoint = coverSettings.targetFocusPoint;
      coverState.camRaycastPoint = coverSettings.camRaycastPoint;
      coverState.lerpSpeed = coverSettings.lerpSpeed;
      coverState.inCoverPoint = coverSettings.inCoverPoint;
      coverState.rotationSpeed = coverSettings.rotationSpeed;
      coverState.chasePoint = coverSettings.chasePoint;
      coverState.zoomPoint = coverSettings.zoomPoint;

      // load crosshair for ChaseState & InCoverState --------------------
      chaseState.crosshairImage = chaseSettings.crosshairImage;
      //coverState.crosshairImage = chaseSettings.crosshairImage;
      if (chaseSettings.crosshair_TL != null)
      {
         chaseState.crosshair_TL = chaseSettings.crosshair_TL;
         //coverState.crosshair_TL = chaseSettings.crosshair_TL;
      }
      if (chaseSettings.crosshair_TR != null)
      {
         chaseState.crosshair_TR = chaseSettings.crosshair_TR;
         //coverState.crosshair_TR = chaseSettings.crosshair_TR;
      }
      if (chaseSettings.crosshair_BL != null)
      {
         chaseState.crosshair_BL = chaseSettings.crosshair_BL;
         //coverState.crosshair_BL = chaseSettings.crosshair_BL;
      }
      if (chaseSettings.crosshair_BR != null)
      {
         chaseState.crosshair_BR = chaseSettings.crosshair_BR;
         //coverState.crosshair_BR = chaseSettings.crosshair_BR;
      }

      // ZoomState settings ---------------------------------------------
      ZoomState zoomState = Camera.main.GetComponent<ZoomState>();
      ZoomStateSettings zoomSettings = chaseTarget.GetComponent<ZoomStateSettings>();
      zoomState.target = zoomSettings.target;
      zoomState.targetFocusPoint = zoomSettings.targetFocusPoint;
      zoomState.camRaycastPoint = zoomSettings.camRaycastPoint;
      zoomState.lerpSpeed = zoomSettings.lerpSpeed;
      zoomState.zoomPoint = zoomSettings.zoomPoint;
      zoomState.rotationSpeed = zoomSettings.rotationSpeed;

      // load crosshair for Zoom State -----------------------------------
      zoomState.crosshairImage = zoomSettings.crosshairImage;
   }

   void OnDrawGizmos()
   {
      // display gizmos for helpers --------------------------------------
      if (GameState.Instance.inGame())
      {
         if (playerHelpers.Count > 0)
         {
            foreach (PlayerHelperPos player in playerHelpers)
            {
               // camera lookAt point
               Gizmos.color = Color.green;
               Gizmos.DrawWireSphere(player.focusPoint.transform.position, 0.1f);
               // zoom point
               Gizmos.color = Color.yellow;
               Gizmos.DrawWireSphere(player.zoomPoint.transform.position, 0.1f);
               // chase state point
               Gizmos.color = Color.red;
               Gizmos.DrawWireSphere(player.chasePoint.transform.position, 0.1f);
               // camera raycast point for wall occlusion
               Gizmos.color = Color.white;
               Gizmos.DrawWireSphere(player.camCastPoint.transform.position, 0.05f);
               // camera rotation pivot
               Gizmos.color = Color.cyan;
               Gizmos.DrawWireSphere(player.pivot.transform.position, 0.05f);
               // inCover helper
               Gizmos.color = Color.magenta;
               Gizmos.DrawWireSphere(player.inCoverPoint.transform.position, 0.1f);
            }
         }
      }
   }

   /// <summary>
   /// Returns the facing direction of current character for rotation after InCover State.
   /// </summary>
   /// <returns>Direction where character is looking.</returns>
   public Vector3 GetLookAtDirection()
   {
      Vector3 lookAtDir = Vector3.zero;
      lookAtDir = chaseTarget.GetComponent<ChaseStateSettings>().targetFocusPoint.forward;
      return lookAtDir;
   }

   /// <summary>
   /// Saves helper positions on all playable characters.
   /// </summary>
   public void CacheHelperPoints()
   {
      // get access to all playable chars and their helper objs
      FindPlayableCharacters();

      // edit helper locations
      foreach (GameObject character in playerChars)
      {
         GameObject focusPoint = null;
         GameObject zoomPoint = null;
         GameObject chasePoint = null;
         GameObject camCastPoint = null;
         GameObject pivot = null;
         GameObject inCoverPoint = null;

         focusPoint = UtilityScript.FindChildRecursively(character.transform, "FocusPoint").gameObject;
         if (focusPoint == null)
         {
            Debug.LogError("No FocusPoint on " + character.gameObject.name);
         }

         zoomPoint = UtilityScript.FindChildRecursively(character.transform, "ZoomPoint").gameObject;
         if (zoomPoint == null)
         {
            Debug.LogError("No ZoomPoint on " + character.gameObject.name);
         }

         chasePoint = UtilityScript.FindChildRecursively(character.transform, "ChasePoint").gameObject;
         if (chasePoint == null)
         {
            Debug.LogError("No ChasePoint on " + character.gameObject.name);
         }

         camCastPoint = UtilityScript.FindCamRaycastPoint(character.transform).gameObject;
         if (camCastPoint == null)
         {
            Debug.LogError("No CamCastPoint on " + character.gameObject.name);
         }

         pivot = UtilityScript.FindChildRecursively(character.transform, "RotationPivot").gameObject;
         if (pivot == null)
         {
            Debug.LogError("No RotationPivot on " + character.gameObject.name);
         }

         inCoverPoint = UtilityScript.FindChildRecursively(character.transform, "InCoverPoint").gameObject;
         if (inCoverPoint == null)
         {
            Debug.LogError("No InCoverPoint on " + character.gameObject.name);
         }

         // search for helper objs in next entity
         if (focusPoint == null || zoomPoint == null || chasePoint == null || camCastPoint == null || pivot == null || inCoverPoint == null)
         {
            continue;
         }
         else
         {
            PlayerHelperPos currentPlayerPos = new PlayerHelperPos();
            currentPlayerPos.focusPoint = focusPoint;
            currentPlayerPos.chasePoint = chasePoint;
            currentPlayerPos.zoomPoint = zoomPoint;
            currentPlayerPos.inCoverPoint = inCoverPoint;
            currentPlayerPos.camCastPoint = camCastPoint;
            currentPlayerPos.pivot = pivot;
            currentPlayerPos.characterName = character.name;

            playerHelpers.Add(currentPlayerPos);
         }
      }
   }

   /// <summary>
   /// Fills in the list of playable characters in CameraCtrl for global access.
   /// </summary>
   public void FindPlayableCharacters()
   {
      // get all player characters
      EntityData[] chars = GameObject.FindObjectsOfType<EntityData>();

      // search characters for playable
      foreach (EntityData entity in chars)
      {
         if (entity.GetComponent<IsPlayer>())
         {
            playerChars.Add(entity.gameObject);
         }
      }
   }
}
