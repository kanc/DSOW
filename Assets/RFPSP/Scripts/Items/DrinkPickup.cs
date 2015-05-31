//DrinkPickup.cs by Azuline Studios© All Rights Reserved
//script for drink pickups
using UnityEngine;
using System.Collections;

public class DrinkPickup : MonoBehaviour {
	
	private GameObject playerObj;//the GameObject that is a child of FPS Weapons which has the WeaponBehavior script attatched
	private Transform myTransform;
	
	public bool removeOnUse = true;//Does this pickup disappear when used/activated by player?
	
	public AudioClip pickupSound;//sound to play when picking up drink
	public AudioClip fullSound;//sound to play when player isn't thirsty
	
	public int thirstToRemove = 15;//amount of thirst to remove when picking up this drink item
	public int healthToRestore = 5;//amount of health to restore when picking up this drink item
	
	private FPSPlayer FPSPlayerComponent;

	void Start () {
		myTransform = transform;//manually set transform for efficiency
		playerObj = Camera.main.transform.GetComponent<CameraKick>().playerObj;
		FPSPlayerComponent = playerObj.GetComponent<FPSPlayer>();
	}
	
	public void PickUpItem(){
		//if player is thirsty, apply thirstToRemove to thirstPoints
		if (FPSPlayerComponent.thirstPoints > 0.0f && FPSPlayerComponent.usePlayerThirst) {
			
			if(FPSPlayerComponent.thirstPoints - thirstToRemove > 0.0){
				FPSPlayerComponent.UpdateThirst(-thirstToRemove);
			}else{
				FPSPlayerComponent.UpdateThirst(-FPSPlayerComponent.thirstPoints);	
			}
			
			//restore player health by healthToRestore amount
			if(FPSPlayerComponent.hitPoints + healthToRestore < FPSPlayerComponent.maximumHitPoints){
				FPSPlayerComponent.HealPlayer(healthToRestore);	
			}else{
				FPSPlayerComponent.HealPlayer(FPSPlayerComponent.maximumHitPoints - FPSPlayerComponent.hitPoints);
			}
			
			//play pickup sound
			if(pickupSound){PlayAudioAtPos.instance.PlayClipAt(pickupSound, myTransform.position, 0.75f, 1.0f, 1.0f);}
			
			if(removeOnUse){
				//remove this drink pickup
				Object.Destroy(gameObject);
			}
		}else{
			//if player not thirsty, just play beep sound
			if(fullSound){PlayAudioAtPos.instance.PlayClipAt(fullSound, myTransform.position, 0.75f, 1.0f, 1.0f);}	
		}
	}
}