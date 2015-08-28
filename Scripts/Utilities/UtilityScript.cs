using UnityEngine;
using System.Collections;

// Utility helper scripts
public class UtilityScript
{
   public static float floatComparePrecision = 0.005f;

   public static float AngleDir(Vector3 beginDir, Vector3 targetDir, Vector3 up)
   {
      Vector3 cross = Vector3.Cross(beginDir, targetDir);
      float dir = Vector3.Dot(cross, up);

      if (dir > 0f)
      {
         // clockwise
         return 1f;
      }
      else if (dir < 0f)
      {
         // anticlockwise
         return -1f;
      }
      else
      {
         return 0f;
      }
   }

   public static bool CheckParallel(Vector3 A, Vector3 B)
   {
      Vector3 cross = Vector3.Cross(A, B);
      return (Mathf.Abs(cross.magnitude) <= floatComparePrecision) ? true : false;
   }

   public static float GetDegToDirection(Vector3 A, Vector3 B)
   {
      // find angle between playerForward & movementDirection to rotate player in new facing direction
      float angleDeg = Vector3.Angle(A, B);
      //Debug.Log("AngleDeg btw Player & camera: " + angleDeg);
      float rotationSign = UtilityScript.AngleDir(A, B, Vector3.up);
      //Debug.Log("Left/Right Player & camera fwd: " + rotationSign);
      float finalDeg = angleDeg * rotationSign;
      //Debug.Log("FINAL: " + finalDeg);

      return finalDeg;
   }

   public static Transform FindChildByName(Transform parent, string childName)
   {
      // stop search if current is the "childName"
      if (parent.name.Contains(childName))
         return parent;

      // search children for "childName"
      for (int i = 0; i < parent.childCount; ++i)
      {
         Transform found = FindChildByName(parent.GetChild(i), childName);

         // if not null - search was successful
         if (found != null)
            return found;
      }

      // not found
      return null;
   }

   public static Transform FindChildByComponent(Transform parent, string componentName)
   {
      // stop search if current is the "childName"
      if (parent.GetComponent(componentName))
         return parent;

      // search children for "childName"
      for (int i = 0; i < parent.childCount; ++i)
      {
         Transform found = FindChildByComponent(parent.GetChild(i), componentName);

         // if not null - search was successful
         if (found != null)
            return found;
      }

      // not found
      return null;
   }

   // http://forum.unity3d.com/threads/vector3-from-string-solved.46223/
   public static Vector3 parseVector3(string rString)
   {
      string[] temp = rString.Substring(1, rString.Length - 2).Split(',');
      float x = float.Parse(temp[0]);
      float y = float.Parse(temp[1]);
      float z = float.Parse(temp[2]);
      Vector3 rValue = new Vector3(x, y, z);
      return rValue;
   }
}
