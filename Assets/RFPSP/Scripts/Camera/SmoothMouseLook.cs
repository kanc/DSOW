//SmoothMouseLook.cs by Azuline Studios© All Rights Reserved
//Smoothes mouse input, manages angle limits, enables/unlocks cursor on pause, and compensates for non-recovering weapon recoil. 
using UnityEngine;

using System.Collections;
//using System.Collections.Generic;

public class SmoothMouseLook : MonoBehaviour {
	private InputControl InputComponent;
	private GameObject playerObj;

    public float sensitivity = 4.0f;
	[HideInInspector]
	public float sensitivityAmt = 4.0f;//actual sensitivity modified by IronSights Script

    private float minimumX = -360f;
    private float maximumX = 360f;

    private float minimumY = -85f;
    private float maximumY = 85f;
	[HideInInspector]
    public float rotationX = 0.0f;
	[HideInInspector]
    public float rotationY = 0.0f;
	[HideInInspector]
    public float inputY = 0.0f;
   
	public float smoothSpeed = 0.35f;
	
	private Quaternion originalRotation;
	private Transform myTransform;
	[HideInInspector]
	public float recoilX;//non recovering recoil amount managed by WeaponKick function of WeaponBehavior.cs
	[HideInInspector]
	public float recoilY;//non recovering recoil amount managed by WeaponKick function of WeaponBehavior.cs
	
	void Start(){ 
		playerObj = Camera.main.transform.GetComponent<CameraKick>().playerObj;
		InputComponent = playerObj.GetComponent<InputControl>();

        if (GetComponent<Rigidbody>()){GetComponent<Rigidbody>().freezeRotation = true;}
		
		myTransform = transform;//cache transform for efficiency
		
		originalRotation = myTransform.localRotation;
		//sync the initial rotation of the main camera to the y rotation set in editor
		Vector3 tempRotation = new Vector3(0,Camera.main.transform.eulerAngles.y,0);
		originalRotation.eulerAngles = tempRotation;
		
		sensitivityAmt = sensitivity;//initialize sensitivity amount from var set by player
		
		//Hide the cursor
		Cursor.visible = false;
    }

    void Update(){
		
		if(Time.timeScale > 0.0f && Time.smoothDeltaTime > 0.0f){//allow pausing by setting timescale to 0

			//Hide the cursor
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;

			
			// Read the mouse input axis
			rotationX += InputComponent.lookX * sensitivityAmt * Time.timeScale;//lower sensitivity at slower time settings
			rotationY += InputComponent.lookY * sensitivityAmt * Time.timeScale;
			
			//reset vertical recoilY value if it would exceed maximumY amount 
			if(maximumY - InputComponent.lookY * sensitivityAmt * Time.timeScale < recoilY){
				rotationY += recoilY;
				recoilY = 0.0f;	
			}
			//reset horizontal recoilX value if it would exceed maximumX amount 
			if(maximumX - InputComponent.lookX * sensitivityAmt * Time.timeScale < recoilX){
				rotationX += recoilX;
				recoilX = 0.0f;	
			}
			 
			rotationX = ClampAngle (rotationX, minimumX, maximumX);
			rotationY = ClampAngle (rotationY, minimumY - recoilY, maximumY - recoilY);
			
			inputY = rotationY + recoilY;//set public inputY value for use in other scripts
			 
			Quaternion xQuaternion = Quaternion.AngleAxis (rotationX + recoilX, Vector3.up);
			Quaternion yQuaternion = Quaternion.AngleAxis (rotationY + recoilY, -Vector3.right);
			
			//smooth the mouse input
			myTransform.rotation = Quaternion.Slerp(myTransform.rotation , originalRotation * xQuaternion * yQuaternion, smoothSpeed * Time.smoothDeltaTime * 60 / Time.timeScale);
			//lock mouselook roll to prevent gun rotating with fast mouse movements
			myTransform.rotation = Quaternion.Euler(myTransform.rotation.eulerAngles.x, myTransform.rotation.eulerAngles.y, 0.0f);
			
		}else{
			//Show the cursor
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		
    }
   
    public static float ClampAngle (float angle, float min, float max){
        angle = angle % 360;
        if((angle >= -360F) && (angle <= 360F)){
            if(angle < -360F){
                angle += 360F;
            }
            if(angle > 360F){
                angle -= 360F;
            }         
        }
        return Mathf.Clamp (angle, min, max);
    }
	
}