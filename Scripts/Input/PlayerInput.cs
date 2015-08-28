using UnityEngine;
using System.Collections;
/*
 * Gets input from keyboard and joystick, moves the Player.
 */
public class PlayerInput : MonoBehaviour
{
   private float movementSpeed = 5f;
   public float walkSpeed = 3f;
   public float runSpeed = 10f;
   public float rotationSpeed = 25f;

   // movement & rotation input axes
   private float vSpeed_Left = 0f;
   private float hSpeed_Left = 0f;
   private float vSpeed_Right = 0f;
   private float hSpeed_Right = 0f;
   private bool sprintFlag = false;

   private Animator playerAnim;
   private bool isMovementAllowed = true;

	void Start () 
   {
      playerAnim = GetComponent<Animator>();
      movementSpeed = walkSpeed;

      // hide mouse cursor
      Cursor.visible = false;
	}

   private void GetAxisInput()
   {
      // player movement axis input - WASD / left stick
      vSpeed_Left = Input.GetAxis("Vertical");
      hSpeed_Left = Input.GetAxis("Horizontal");
      sprintFlag = Input.GetButton("Sprint");

      // check if joystick input is available
      if (Input.GetJoystickNames()[0] != "")
      {
         // player rotation axis input - right stick
         vSpeed_Right = Input.GetAxis("JoystickRV");
         hSpeed_Right = Input.GetAxis("JoystickRH");
      }
      // default input - mouse
      if (Input.GetAxis("Mouse Y") !=0 || Input.GetAxis("Mouse X") !=0)
      {
         // player rotation axis input - mouse
         vSpeed_Right = Input.GetAxis("Mouse Y");
         hSpeed_Right = Input.GetAxis("Mouse X");
      }      
   }
	
	void Update ()
   {
      // ------------------ Get axis and keys input --------------------------
      GetAxisInput();

      // set movement speed
      if (sprintFlag)
      {
         movementSpeed = runSpeed;
      }
      else
      {
         movementSpeed = walkSpeed;
      }

      // ------------------ Negative axis input LEFT -------------------------
      // calc vector magnitude to check negative axis input - left stick
      Vector2 vec1 = new Vector2(vSpeed_Left, 0);
      Vector2 vec2 = new Vector2(0, hSpeed_Left);
      // left stick - controls player movement
      float axisInputValue_Left = vec1.magnitude + vec2.magnitude;

      // ------------------ Negative axis input RIGHT -------------------------
      // calc vector magnitude to check negative axis input - right stick
      Vector2 vec3 = new Vector2(vSpeed_Right, 0);
      Vector2 vec4 = new Vector2(0, hSpeed_Right);
      // right stick - controls camera movement
      float axisInputValue_Right = vec3.magnitude + vec4.magnitude;

      if (!CameraCtrl.Instance.invertY)
      {
         CameraCtrl.Instance.elevationAngle += vSpeed_Right;
      }
      else
      {
         CameraCtrl.Instance.elevationAngle -= vSpeed_Right;
      }

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
         transform.RotateAround(transform.position, Vector3.up, hSpeed_Right * rotationSpeed);

         // set player animations
         if (playerAnim)
         {
            // set sum of axis input to include negative values too
            playerAnim.SetFloat("motion", axisInputValue_Left);
            playerAnim.SetFloat("move_LH", hSpeed_Left);
            playerAnim.SetFloat("move_LV", vSpeed_Left);
            playerAnim.SetFloat("rotation", axisInputValue_Right);
            playerAnim.SetFloat("rotate_RH", hSpeed_Right);
            playerAnim.SetBool("sprint", sprintFlag);
         }
      }
	}

   public void SetMovementAllowed(bool isAllowed)
   {
      isMovementAllowed = isAllowed;
   }
}
