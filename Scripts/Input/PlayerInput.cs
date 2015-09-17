using UnityEngine;
using System.Collections;
/*
 * Gets input from keyboard and joystick, moves the Player.
 */
public class PlayerInput : Singleton<PlayerInput>
{
   protected PlayerInput() {}

   // current speed
   private float movementSpeed = 5f;
   public float walkSpeed = 3f;
   public float runSpeed = 10f;
   public float zoomSpeed = 1.5f;
   public float rotationSpeed = 25f;

   // movement & rotation input axes
   private float vSpeed_Left = 0f;
   private float hSpeed_Left = 0f;
   private float vSpeed_Right = 0f;
   private float hSpeed_Right = 0f;
   private bool sprintFlag = false;

   [HideInInspector]
   public float axisInputValue_Left = 0f;
   [HideInInspector]
   public float axisInputValue_Right = 0f;
   private Animator playerAnim;
   private bool isMovementAllowed = true;

   private CameraCtrl_Helper camHelper;
   private Vector3 prevMouseScreenPos = Vector3.zero;
   private float screenWidth = 0f;
   private float screenHeight = 0f;
   private Rect screenRect;

   // based on mouse pos on screen
   private Vector3 lookPos;

	void Start () 
   {
      camHelper = GetComponent<CameraCtrl_Helper>();
      playerAnim = GetComponent<Animator>();
      movementSpeed = walkSpeed;

      screenWidth = Screen.width;
      screenHeight = Screen.height;
      screenRect = new Rect(0f, 0f, screenWidth, screenHeight);
	}

   /// <summary>
   /// Called in each Update
   /// </summary>
   private void GetAxisInput()
   {
      // hide mouse cursor
      Cursor.visible = false;
      Cursor.lockState = CursorLockMode.Confined;

      // player movement axis input - WASD / left stick
      vSpeed_Left = Input.GetAxis("Vertical");
      hSpeed_Left = Input.GetAxis("Horizontal");
      sprintFlag = Input.GetButton("Sprint");

      // check if joystick input is available ---------------------
      if (Input.GetJoystickNames()[0] != "")
      {
         // player rotation axis input - right stick
         vSpeed_Right = Input.GetAxis("JoystickRV");
         hSpeed_Right = Input.GetAxis("JoystickRH");

         if (!CameraCtrl.Instance.invertY)
         {
            CameraCtrl.Instance.elevationAngle -= vSpeed_Right;
         }
         else
         {
            CameraCtrl.Instance.elevationAngle += vSpeed_Right;
         }
      }
      else
      {
         if (Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.01f)
         {
            vSpeed_Right = Input.GetAxis("Mouse Y");
            // mouse input -- increase elevationAngle only when mouse moves            
            if (!CameraCtrl.Instance.invertY)
            {
               CameraCtrl.Instance.elevationAngle -= vSpeed_Right;
            }
            else
            {
               CameraCtrl.Instance.elevationAngle += vSpeed_Right;
            }
         }
      }

      // default input - mouse ------------------------------------
      if (Mathf.Abs(Input.GetAxis("Mouse X")) > 0.01f)
      {
         hSpeed_Right = Input.GetAxis("Mouse X");
      }

      // input values when mouse is not moving
      if (prevMouseScreenPos == Input.mousePosition &&
            Mathf.Abs(Input.GetAxis("Mouse X")) == 0f)
      {
         vSpeed_Right = 0f;
         hSpeed_Right = 0f;
      }

      // rotate player --------------------------------------------
      if (Mathf.Abs(hSpeed_Right) > 0.01f)
      {
         Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);
         lookPos = Camera.main.ScreenToWorldPoint(mousePos);
         //Debug.DrawRay(transform.position, lookPos, Color.red);
         Quaternion targetRotation = Quaternion.LookRotation(lookPos - transform.position, Vector3.up);

         if (Input.mousePosition.x < screenWidth * 0.5f && hSpeed_Right > 0f)
         {
            targetRotation = Quaternion.LookRotation(transform.position - lookPos, Vector3.up);
         }

         if (Input.mousePosition.x > screenWidth * 0.5f && hSpeed_Right < 0f)
         {
            targetRotation = Quaternion.LookRotation(transform.position - lookPos, Vector3.up);
         }

         float step = rotationSpeed * Time.deltaTime;
         transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, step);
      }

      if (Input.GetKey(KeyCode.Escape))
      {
         Application.Quit();
      }
   }
	
	void Update ()
   {
      // ------------------ Get axis and keys input --------------------------
      GetAxisInput();

      if (camHelper.currentState != CameraCtrl_Helper.EntityState.ZOOM)
      {
         // set movement speed
         if (sprintFlag)
         {
            movementSpeed = runSpeed;
            camHelper.currentState = CameraCtrl_Helper.EntityState.RUN;
         }
         else
         {
            movementSpeed = walkSpeed;
            camHelper.currentState = CameraCtrl_Helper.EntityState.WALK;
         }
      }
      else
      {
         movementSpeed = zoomSpeed;
      }

      // ------------------ Negative axis input LEFT -------------------------
      // calc vector magnitude to check negative axis input - left stick
      Vector2 vec1 = new Vector2(vSpeed_Left, 0);
      Vector2 vec2 = new Vector2(0, hSpeed_Left);
      // left stick - controls player movement
      axisInputValue_Left = vec1.magnitude + vec2.magnitude;

      // ------------------ Negative axis input RIGHT -------------------------
      // calc vector magnitude to check negative axis input - right stick
      Vector2 vec3 = new Vector2(vSpeed_Right, 0);
      Vector2 vec4 = new Vector2(0, hSpeed_Right);
      // right stick - controls camera movement
      axisInputValue_Right = vec3.magnitude + vec4.magnitude;

      // ------------------ Player & Camera Forward / Right vectors -----------
      // camera forward on XZ plane in world space
      Vector3 cameraForward = Camera.main.transform.forward;
      cameraForward.y = 0;
      cameraForward.Normalize();

      //Vector3 cameraRight = Camera.main.transform.right;
      //cameraRight.y = 0;
      //cameraRight.Normalize();

      // player's forward on XZ plane in world space
      Vector3 playerForward = transform.forward;
      playerForward.y = 0;
      playerForward.Normalize();

      Vector3 playerRight = transform.right;
      playerRight.y = 0;
      playerRight.Normalize();

      Debug.DrawRay(transform.position, playerForward, Color.green);
      Debug.DrawRay(Camera.main.transform.position, cameraForward, Color.magenta);

      // ----------------------------------------------------------------------
      // --------------- PLAYER MOVEMENT --------------------------------------

      if (isMovementAllowed)
      {
         // player's movement direction
         Vector3 movementDirection = vSpeed_Left * playerForward + hSpeed_Left * playerRight;
         movementDirection.y = 0f;
         movementDirection.Normalize();
         Debug.DrawRay(transform.position, movementDirection, Color.blue);

         // move player if there was input on at least 1 axis on keyboard / joystick
         if (axisInputValue_Left > 0)
         {
            transform.Translate(movementDirection * movementSpeed * Time.deltaTime, Space.World);
         }

         // rotate player based on mouse / controller input
         //transform.RotateAround(transform.position, Vector3.up, hSpeed_Right * rotationSpeed);

         // set player animations
         if (playerAnim)
         {
            // set sum of axis input to include negative values too
            playerAnim.SetFloat("motion", axisInputValue_Left);
            playerAnim.SetFloat("move_LH", hSpeed_Left);
            playerAnim.SetFloat("move_LV", vSpeed_Left);
            playerAnim.SetFloat("rotation", axisInputValue_Right);
            playerAnim.SetFloat("rotate_RH", hSpeed_Right);

            if (camHelper.currentState != CameraCtrl_Helper.EntityState.ZOOM)
            {
               playerAnim.SetBool("sprint", sprintFlag);
            }
         }
      }

      if (screenRect.Contains(Input.mousePosition))
      {
         prevMouseScreenPos = Input.mousePosition;
      }
	}

   public void SetMovementAllowed(bool isAllowed)
   {
      isMovementAllowed = isAllowed;
   }
}
