//FoodPickup.cs by Azuline Studios© All Rights Reserved
//script for food pickups
using UnityEngine;
using System.Collections;

public class FoodPickup : MonoBehaviour {
	
	private GameObject playerObj;//the GameObject that is a child of FPS Weapons which has the WeaponBehavior script attatched
	private Transform myTransform;
	
	public bool removeOnUse = true;//Does this pickup disappear when used/activated by player?
	
	public AudioClip pickupSound;//sound to play when picking up food
	public AudioClip fullSound;//sound to play when player is full
	
	public int hungerToRemove = 15;//amount of hunger to remove when picking up this food item
	public int healthToRestore = 5;//amount of health to restore when picking up this food item
	
	private FPSPlayer FPSPlayerComponent;

	void Start () {
		myTransform = transform;//manually set transform for efficiency
		playerObj = Camera.main.transform.GetComponent<CameraKick>().playerObj;
		FPSPlayerComponent = playerObj.GetComponent<FPSPlayer>();
	}
	
	public void PickUpItem(){
		//if player is hungry, apply hungerToRemove to hungerPoints
		if (FPSPlayerComponent.hungerPoints > 0.0f && FPSPlayerComponent.usePlayerHunger) {
			
			if(FPSPlayerComponent.hungerPoints - hungerToRemove > 0.0){
				FPSPlayerComponent.UpdateHunger(-hungerToRemove);
			}else{
				FPSPlayerComponent.UpdateHunger(-FPSPlayerComponent.hungerPoints);	
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
				//remove this food pickup
				Object.Destroy(gameObject);
			}
		}else{
			//if player is not hungry, just play beep sound
			if(fullSound){PlayAudioAtPos.instance.PlayClipAt(fullSound, myTransform.position, 0.75f, 1.0f, 1.0f);}	
		}
	}
}