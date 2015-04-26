﻿using UnityEngine;
using System.Collections;

namespace GlobalData
{
    public static class Constants
    {        
        public static string    ENEMY_TAG = "Enemy";
        public static int       INTERACTUABLE_COLLIDER_LAYER = 8;        

    }
    public static class CameraUtil
    {
        public static bool IsLookingAtInteract(GameObject target, out RaycastHit hit)
        {
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);            
            int layerMask = 1 << GlobalData.Constants.INTERACTUABLE_COLLIDER_LAYER;

            if (Physics.Raycast(ray, out hit, 100, layerMask))
            {                
                return (hit.collider.gameObject.GetInstanceID() == target.GetInstanceID()) ? true : false;
            }
            else
            {
                return false;
            }
        }
    }


}