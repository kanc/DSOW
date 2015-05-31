//WeaponBehavior.cs by Azuline Studios© All Rights Reserved
//Runs weapon animations and initializes, fires, and reloads weapons. 
using UnityEngine;
using System.Collections;

public class WeaponBehavior : MonoBehaviour {

	//Other objects accessed by this script
	[HideInInspector]
	public GameObject playerObj;
	[HideInInspector]
	public GameObject weaponObj;
	public GameObject weaponDropObj;//reference to weapon pickup object to drop for this weapon
//	[HideInInspector]
//	public GameObject cameraObj;

	public GameObject weaponMesh;
	private Animation AnimationComponent;
	private Animation WeaponAnimationComponent;
	private Animation CameraAnimationComponent;
	public bool haveWeapon = false;//true if player has this weapon in their inventory
	[HideInInspector]
	public int weaponNumber = 0;//number of this weapon in the weaponOrder array in playerWeapons script
	public int ammo = 150;//ammo amount for this weapon in player's inventory
	public int bulletsPerClip  = 30;//maximum amount of bullets per magazine
	public int bulletsToReload  = 50;//number of bullets to reload per reload cycle (when < bulletsPerClip, allows reloading one or more bullets at a time)
	public int maxAmmo = 999;//maximum ammo amount player's inventory can hold for this weapon
	public int damage  = 10;//damage to inflict on objects with ApplyDamage(); function
	public int force = 200;//amount of physics push to apply to rigidbodies on contact
	public float range  = 100;//range that weapon can hit targets
	public float fireRate = 0.097f;//time between shots

	//Shooting
	public int projectileCount  = 1;//amount of projectiles to be fired per shot ( > 1 for shotguns)
	private int hitCount = 0;//track number of hits on a surface to optimize/reduce impact effects played for shotguns
	public bool fireModeSelectable;//true if weapon can switch between burst and semi-auto
	private bool fireModeState;
	public bool semiAuto;//true when weapon is in semi-auto mode
	private bool semiState;
	public bool burstFire;//true when weapon is in burst mode
	public bool burstAndAuto;//if true, weeapon will cycle all three burst modes
	public int burstShots = 3;//amount of bullets to fire per burst
	private bool burstState;
	private int burstShotsFired;
	private bool burstHold;
	public bool silentShots;//if true, enemies will not hear weapon shooting
	public bool fireableUnderwater = true;//true if weapon can be fired underwater
	//true when a weapon that cant be fired under water has attempted to fire
	//used to make player have to press fire button again when surfacing to fire instead of holding button down
	private bool waterFireState;
	public bool unarmed;//should this weapon be null/unarmed?
	public float meleeSwingDelay = 0.0f;//this weapon will be treated as a melee weapon when this value is > 0
	private bool swingSide;//to control which direction to swing melee weapon
	[HideInInspector]
	public float shootStartTime = -16.0f;//time that shot started
	
	[HideInInspector]
	public GameObject ammoGuiObj;
	private AmmoText AmmoText1;//set reference for main color element of ammo GUIText
	private AmmoText[] AmmoText2;//set reference for shadow background color element of heath GUIText
	
	private Transform myTransform;
	private Transform mainCamTransform;
	
	private FPSRigidBodyWalker FPSWalkerComponent;
	private PlayerWeapons PlayerWeaponsComponent;
	private Ironsights IronsightsComponent;
	private FPSPlayer FPSPlayerComponent;
	private InputControl InputComponent;
	private	SmoothMouseLook MouseLookComponent;
	private	WeaponEffects WeaponEffectsComponent;
	
	public bool showAimingCrosshair = true;//to determine if aiming reticule should be displayed when not zoomed, used for weapons like sniper rifles
	public bool droppable = true;//Can this weapon be dropped? False for un-droppable weapons like fists or sidearm.
	public bool addsToTotalWeaps = true;//Does this weapon count toward weapon total? False for weapons like fists or sidearm.
	[HideInInspector]
	public bool dropWillDupe;//true when weapon has been picked up from an item that is not destroyed on use to prevent dropping this weapon 
	//and creating duplicated ammo by picking up the weapon again from the non-destroyable pickup item
		
	//Gun Position Amounts
	public float weaponUnzoomXPosition = -0.02f;//horizontal modifier of gun position when not zoomed
	public float weaponUnzoomYPosition = 0.0127f;//vertical modifier of gun position when not zoomed
	public float weaponUnzoomZPosition = 0.0f;//forward modifier of gun position when not zoomed
	public float weaponZoomXPosition = -0.07f;//horizontal modifier of gun position when zoomed
	public float weaponZoomYPosition = 0.032f;//vertical modifier of gun position when zoomed
	public float weaponZoomZPosition = 0.0f;//forward modifier of gun position when zoomed
	public float weaponSprintXPosition = 0.075f;//horizontal modifier of gun position when sprinting
	public float weaponSprintYPosition = 0.0075f;//vertical modifier of gun position when sprinting
	public float weaponSprintZPosition = 0.0f;//forward modifier of gun position when sprinting
	public bool canZoom = true;//true if zoom can be used with this weapon
	public float zoomFOV = 55.0f;//FOV value to use when zoomed, lower values  can be used with scoped weapons for higher zoom
	public float zoomSensitivity = 0.3f;//sensitivity of view movement when zooming
	public float swayAmountUnzoomed = 1.0f;//sway amount for this weapon when not zoomed
	public float swayAmountZoomed = 1.0f;//sway amount for this weapon when zoomed
	public bool PistolSprintAnim ;//set to true to use alternate sprinting animation with pistols
	public float sprintBobAmountX = 1.0f;//to fine tune horizontal weapon sprint bobbing amounts
	public float sprintBobAmountY = 1.0f;//to fine tune vertical weapon sprint bobbing amounts
	public float walkBobAmountX = 1.0f;//to fine tune horizontal weapon walking bobbing amounts
	public float walkBobAmountY = 1.0f;//to fine tune vertical weapon walking bobbing amounts
	public float proneBobAmountX = 1.0f;//to fine tune horizontal weapon walking bobbing amounts
	public float proneBobAmountY = 1.0f;//to fine tune vertical weapon walking bobbing amounts
	
	//Weapon Animation Smoothing
	[HideInInspector]
	public Vector3 gunAnglesTarget = Vector3.zero;
	[HideInInspector]
	public Vector3 gunAngles = Vector3.zero; 
	private Vector3 gunAngleVel = Vector3.zero;
		
	//Sprinting and Player States
	private bool canShoot = true;//true when player is allowed to shoot
	[HideInInspector]
	public bool shooting;//true when shooting
	[HideInInspector]
	public bool sprintAnimState;//to control playback of sprinting animation
	[HideInInspector]
	public bool sprintState;//to control timing of weapon recovery after sprinting
	private float recoveryTime = 0.000f;//time that sprint animation started playing
	private float horizontal = 0;//player movement
	private float vertical = 0;//player movement
		

	public float fireAnimSpeed = 1.0f;//speed to play the firing animation
	public LayerMask bulletMask = 0;//only layers to include in bullet hit detection (for efficiency)
	private LayerMask liquidMask = 0;//only layers to include in underwater bullet hit detection (for efficiency)
		
	//Reloading
	private int bulletsNeeded = 0;//number of bullets absent in magazine
	public int bulletsLeft = 0;//bullets left in magazine
	[HideInInspector]
	public int bulletsReloaded = 0;//number of bullets reloaded during this reloading cycle
	public float reloadTime = 1.75f;//time per reload cycle, should be shorter if reloading one bullet at a time and longer if reloading magazine
	private	float reloadStartTime = 0.0f;
	[HideInInspector]
	public bool reloadState;
	private bool sprintReloadState;
	private	float reloadEndTime = 0.0f;//used to allow fire button to cancel a reload if not reloading a magazine and bulletsLeft > 1
	public float reloadAnimSpeed = 1.15f;//speed of reload animation playback
	public float shellRldAnimSpeed = 0.7f;//speed of single shell/bullet reload animation playback
	public float readyAnimSpeed = 1.0f;//speed of ready animation playback
	public float readyTime = 0.6f;//amount of time needed to finish the ready anim after weapon has just been switched to/selected
	private float recoveryTimeAmt = 0.0f;//amount of time needed to recover weapon center position after sprinting
	private float startTime = 0.0f;//track time that weapon was selected to calculate readyTime
	[HideInInspector]
	public float reloadLastTime = 1.2f;//to track when last bullet is reloaded if not reloading magazine, to play chambering animation and sound
	[HideInInspector]
	public	float reloadLastStartTime = 0.0f;
	[HideInInspector]
	public bool lastReload;//true when last bullet of a non -magazine reload is being loaded, to play chambering animation and sound 
	private bool cantFireState;//to track ammo depletion and to play out of ammo sound	
	
	//Muzzle Flash
	public Transform muzzleFlash;//the game object that will be used as a muzzle flash
	private float muzzleFlashReduction = 6.5f;//value to control time that muzzle flash is on screen (lower value makes muzzle flash fade slower)
	[HideInInspector]
	public Color muzzleFlashColor = new Color(1, 1, 1, 0.0f);
	public GameObject muzzleLightObj;//the game object with a light component that will be used for muzzle light
	public float muzzleLightDelay = 0.1f;//time to wait until the muzzle light starts fading out
	public float muzzleLightReduction = 100.0f;//rate of muzzle light fading
	private Renderer muzzleRendererComponent;
	private Light muzzleLightComponent;
	private Renderer muzzleSmokeComponent;
	
	//Barrel Smoke
	public bool useBarrelSmoke = true;//true if barrel smoke should be emitted
	public int barrelSmokeShots;//number of consecutive shots required for barrel smoke to emit
	public ParticleEmitter barrelSmokeParticles;//particle effect for smoke rising from barrel after firing more bullets than barrelSmokeShots amount
	public Transform barrelSmokePos;//position to emit the barrel smoke particles, made a child of gun mesh to allow smoke to follow gun animations
	private int bulletsJustFired;//amount of bullets that were recently fired
	private bool emitBarrelSmoke;//barrel smoke emission state
	private float barrelSmokeTime;//time that barrel smoke emission should start
	
	//Muzzle Smoke
	public ParticleEmitter muzzleSmokeParticles;//particle effect for puff of smoke when firing
	private Color muzzleSmokeColor = Color.white;//initialize muzzle smoke color
	public float muzzleSmokeAlpha = 0.25f;//alpha transparency of muzzle smoke
	public bool useTracers = true;
	
	//View Kick
	public float shotSpread = 0.0f;//defines accuracy cone of fired bullets
	private	float shotSpreadAmt = 0.0f;//actual accuracy amount
	[HideInInspector]
	public Quaternion kickRotation;//rotation used for screen kicks
	public float kickUp = 7.0f;//amount to kick view up when firing (set in editor)
	public float kickSide = 2.0f;//amount to kick view sideways when firing (set in editor)
	private float kickUpAmt = 0.0f;//actual amount to kick view up when firing
	private float kickSideAmt = 0.0f;//actual amount to kick view sideways when firing
	public float kickBackAmtUnzoom = -0.025f;//distance that gun pushes back when firing and not zoomed
	public float kickBackAmtZoom = -0.0175f;//distance that gun pushes back when firing and zoomed
	public bool useViewClimb;//true if view should use non-recovering recoil climb with weapon fire
	public float viewClimbUp = 1.0f;//amount that view climbs upwards with non-recovering recoil
	public float viewClimbSide = 1.0f;//amount that view moves side to side with non-recovering recoil
	public float viewClimbRight = 0.75f;//amount that view moves right with non-recovering recoil
	public bool useRecoilIncrease;//true if weapon accuracy should decrease with sustained fire
	public int shotsBeforeRecoil = 4;//number of shots before weapon recoil increases with sustained fire
	public float viewKickIncrease = 1.75f;//amount of sustained fire recoil for view angles/input
	public float aimDirRecoilIncrease = 2.0f;//amount of sustained fire recoil for weapon accuracy
		
	//Shell Ejection
	public GameObject shellPrefabRB;//game object to use as empty casing and eject from shellEjectPosition
	public GameObject shellPrefabMesh;//game object to use as empty casing and eject from shellEjectPosition

	private GameObject shell;
	private GameObject shell2;

	public Vector3 shellEjectDirection = new Vector3(0.0f, 0.0f, 0.0f);//direction of ejected shell casing
	public Transform shellEjectPosition;//position shell is ejected from (use origin of ShellEjectPos object attatched to weapon)
	public Transform shellEjectPositionZoom;
	private Transform shellEjectPos;
	public Vector3 shellScale = new Vector3(1.0f, 1.0f, 1.0f);//scale of shell, can be used to make different shaped shells from one model
	public float shellEjectDelay = 0.0f;//delay before ejecting shell (used for bolt action rifles and pump shotguns)
	public float shellForce = 0.2f;//overall movement force of ejected shell
	public float shellUp = 0.75f;//random vertical direction to apply to shellForce
	public float shellSide = 1.0f;//random horizontal direction to apply to shellForce
	public float shellForward = 0.1f;//random forward direction to apply to shellForce
	public float shellRotateUp = 0.25f;//amount of vertical shell rotation
	public float shellRotateSide = 0.25f;//amount of horizontal shell rotation
	public int shellDuration = 5;//time in seconds that shells persist in the world before being removed

	//Audio Sources
	private AudioSource []aSources;//Initialize audio sources
	private AudioSource firefx;//use multiple audio sources to play weapon sfx without skipping
	[HideInInspector]
	public AudioSource otherfx;
	public AudioClip fireSnd;
	public AudioClip reloadSnd;
	public AudioClip reloadLastSnd;//usually shell reload sound + shotgun pump or rifle chambering sound
	public AudioClip noammoSnd;
	public AudioClip readySnd;
	
	private CapsuleCollider capsule;
	
	void Start (){

		myTransform = transform;//cache transform for efficiency
		mainCamTransform = Camera.main.transform;

		playerObj = mainCamTransform.GetComponent<CameraKick>().playerObj;
		weaponObj = mainCamTransform.GetComponent<CameraKick>().weaponObj;

		AnimationComponent = GetComponent<Animation>();
		WeaponAnimationComponent = weaponMesh.GetComponent<Animation>();
		CameraAnimationComponent = Camera.main.GetComponent<Animation>();

		//define external script references (grab from FPSPlayer.cs to reduce GetComponent calls on init)
		FPSPlayerComponent = playerObj.GetComponent<FPSPlayer>();
		FPSWalkerComponent = FPSPlayerComponent.FPSWalkerComponent;
		IronsightsComponent = FPSPlayerComponent.IronsightsComponent;
		PlayerWeaponsComponent = FPSPlayerComponent.PlayerWeaponsComponent;
		InputComponent = FPSPlayerComponent.InputComponent;
		MouseLookComponent = FPSPlayerComponent.MouseLookComponent;
		WeaponEffectsComponent = FPSPlayerComponent.WeaponEffectsComponent;

		//Access GUIText instance that was created by the PlayerWeapons script
		ammoGuiObj = PlayerWeaponsComponent.ammoGuiObjInstance;
		AmmoText1 = ammoGuiObj.GetComponent<AmmoText>();//set reference for main color element of ammo GUIText
		AmmoText2 = ammoGuiObj.GetComponentsInChildren<AmmoText>();//set reference for shadow background color element of heath GUIText
		//Initialize audio sources
		aSources = weaponObj.GetComponents<AudioSource>();
		otherfx = aSources[1];
		firefx = aSources[0];
		firefx.clip = fireSnd;//play fire sound
		capsule = playerObj.GetComponent<CapsuleCollider>();

		if(muzzleFlash){
			muzzleRendererComponent = muzzleFlash.GetComponent<Renderer>();
			if(muzzleLightObj){
				muzzleLightComponent = muzzleLightObj.GetComponent<Light>();
			}
		}

		if(muzzleSmokeParticles){
			muzzleSmokeComponent = muzzleSmokeParticles.GetComponent<Renderer>();
		}
		
		//do not perform weapon actions if this is an unarmed/null weapon
		if(!unarmed){
			
			if(meleeSwingDelay == 0){//initialize muzzle flash color if not a melee weapon
			    muzzleRendererComponent.enabled = false;
			    muzzleFlashColor = muzzleRendererComponent.material.GetColor("_TintColor");
				if(muzzleLightObj){
					muzzleLightComponent.enabled = false;
				}
				//clamp initial ammo amount in clip for non melee weapons
				bulletsLeft = Mathf.Clamp(bulletsLeft,0,bulletsPerClip);
			}else{
				//initial ammo amount in clip for melee weapons
				bulletsLeft = bulletsPerClip;	
			}
			
			if(semiAuto){//make muzzle flash fade out slower when gun is semiAuto
				if(projectileCount < 2){//make muzzle flash last slightly longer for shotguns
					muzzleFlashReduction = 3.5f;
				}else{
					muzzleFlashReduction = 2.0f;		
				}
			}else{
				if(projectileCount < 2){//make muzzle flash last slightly longer for shotguns
					muzzleFlashReduction = 6.5f;
				}else{
					muzzleFlashReduction = 2.0f;		
				}
			}
			
			//exclude layer 18, the liquidCollision layer, from bulletMask so second raycast 
			//of shots fired from above water don't continue to collide with water trigger
			liquidMask = bulletMask & ~(1 << 18);
			
		    //initialize shot timers and animation settings
			shootStartTime = -1.0f;
		    shotSpreadAmt = shotSpread;
						
			gunAngles = gunAnglesTarget;//initialize gun position damp angles
			myTransform.localEulerAngles = gunAngles;
			
			AnimationComponent["RifleSprinting"].speed = -1.5f;//init at this speed for correct rifle switching anim
			if(PistolSprintAnim){AnimationComponent["PistolSprinting"].speed = -1.5f;}//init at this speed for correct pistol switching anim
			WeaponAnimationComponent["Fire"].speed = fireAnimSpeed;//initialize weapon mesh animation speeds
			WeaponAnimationComponent["Reload"].speed = reloadAnimSpeed;
			WeaponAnimationComponent["Ready"].speed = readyAnimSpeed;
			//If weapon reloads one bullet at a time, use anim called "Neutral" of hand returning to idle position
			//from reloading position to allow smooth anims when single bullet reloading is cancelled by sprinting.
			//The "Neutral" animation's wrap mode also needs to be set to "clamp forever" in the animation import settings. 
			if(bulletsToReload != bulletsPerClip){
				WeaponAnimationComponent["Neutral"].speed = 1.5f;
			}
			
			//limit ammo to maxAmmo value
			ammo = Mathf.Clamp(ammo, 0, maxAmmo);
			//limit bulletsToReload value to bulletsPerClip value
			bulletsToReload = Mathf.Clamp(bulletsToReload, 0, bulletsPerClip);
		}
		
//		//disable zoom for melee weapons and if unarmed
//		if(unarmed || meleeSwingDelay > 0){
//			canZoom = false;	
//		}
		
	}
	
	void OnEnable () {
		myTransform = transform;//cache transform for efficiency
		mainCamTransform = Camera.main.transform;
		
		playerObj = mainCamTransform.GetComponent<CameraKick>().playerObj;
		weaponObj = mainCamTransform.GetComponent<CameraKick>().weaponObj;
		//define external script references
		FPSPlayerComponent = playerObj.GetComponent<FPSPlayer>();
		IronsightsComponent = FPSPlayerComponent.IronsightsComponent;
		PlayerWeaponsComponent = weaponObj.GetComponent<PlayerWeapons>();
		WeaponAnimationComponent = weaponMesh.GetComponent<Animation>();

		//Initialize audio sources
		aSources = weaponObj.GetComponents<AudioSource>();
		otherfx = aSources[1];
		firefx = aSources[0];
		firefx.clip = fireSnd;//play fire sound
		capsule = playerObj.GetComponent<CapsuleCollider>();

		//do not perform weapon actions if this is an unarmed/null weapon
		if(!unarmed){
			
			if(Time.timeSinceLevelLoad > 2 && PlayerWeaponsComponent.switching){//don't ready weapon on level load, just when switching weapons
				
				StopCoroutine("Reload");//stop reload coroutine if interrupting a non-magazine reload
				IronsightsComponent.reloading = false;//update reloading var in Ironsights script if cancelling reload to fire
					
				//play weapon readying sound
				otherfx.volume = 1.0f;
				otherfx.clip = readySnd;
				otherfx.PlayOneShot(otherfx.clip, 1.0f / otherfx.volume);
				
				//track time that weapon was made active to calculate readyTime for syncing ready anim with weapon firing
				startTime = Time.time - Time.deltaTime;
				
				myTransform = transform;//cache transforms for efficiency
				gunAngles = gunAnglesTarget;//initialize gun position damp angles
				myTransform.localEulerAngles = gunAngles;
				
				//play weapon readying animation after it has just been selected
				WeaponAnimationComponent["Ready"].speed = readyAnimSpeed;
				WeaponAnimationComponent.CrossFade("Ready",0.35f,PlayMode.StopSameLayer);
			}
		}
		
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//FixedUpdate Actions
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	void FixedUpdate (){
	
		if(Time.timeScale > 0 && Time.deltaTime > 0){//allow pausing by setting timescale to 0
				
			horizontal = FPSWalkerComponent.inputX;//Get input from player movement script
			vertical = FPSWalkerComponent.inputY;
			
			//pass ammo amounts to the ammo GuiText object if not a melee weapon or unarmed
			if(meleeSwingDelay == 0 && !unarmed){
				//pass ammo amount to Gui object to be rendered on screen
			    AmmoText1.ammoGui = bulletsLeft;//main color
				AmmoText1.ammoGui2 = ammo;
				AmmoText2[1].ammoGui = bulletsLeft;//shadow background color
				AmmoText2[1].ammoGui2 = ammo;
				AmmoText1.horizontalOffsetAmt = AmmoText1.horizontalOffset;//normal position on screen
				AmmoText1.verticalOffsetAmt = AmmoText1.verticalOffset;
				AmmoText2[1].horizontalOffsetAmt = AmmoText2[1].horizontalOffset;
				AmmoText2[1].verticalOffsetAmt = AmmoText2[1].verticalOffset;
			}else{
				//pass ammo amount to Gui object to be rendered on screen
			    AmmoText1.ammoGui = bulletsLeft;//main color
				AmmoText1.ammoGui2 = ammo;
				AmmoText2[1].ammoGui = bulletsLeft;//shadow background color
				AmmoText2[1].ammoGui2 = ammo;
				AmmoText1.horizontalOffsetAmt = 5;//make ammo GUIText move off screen if using a melee weapon
				AmmoText1.verticalOffsetAmt = 5;
				AmmoText2[1].horizontalOffsetAmt = 5;
				AmmoText2[1].verticalOffsetAmt = 5;	
			}
			
			//do not perform weapon actions if this is an unarmed/null weapon
			if(!unarmed){
				
				if(FPSPlayerComponent.hitPoints >= 1.0f){
				    
				    //Determine if player is reloading last round during a non-magazine reload. 
					if(reloadLastStartTime + reloadLastTime > Time.time){
						lastReload = true;	
					}else{
						lastReload = false;		
					}
					
				    //cancel auto and manual reload if player starts sprinting
				    if(FPSWalkerComponent.sprintActive
					&& !InputComponent.fireHold
					&& !lastReload//allow player to finish chambering last round of a non-magazine reload
					&& !FPSWalkerComponent.cancelSprint
					&& FPSWalkerComponent.moving
					//cancel auto and manual reload if player presses fire button during a non magazine reload and player has loaded at least 2 shells/bullets 	
					||(InputComponent.fireHold
					&& bulletsToReload != bulletsPerClip
					//&& IronsightsComponent.reloading
					&& bulletsReloaded >= (bulletsToReload * 2.0f))
					|| FPSWalkerComponent.hideWeapon){//cancel reload if player is climbing, swimming, or holding object and weapon is lowered
				    	if(IronsightsComponent.reloading){
							IronsightsComponent.reloading = false;
							//use StopCoroutine to completely stop reload() function and prevent
							//"yield return new WaitForSeconds(reloadTime);" from continuing to excecute
							StopCoroutine("Reload");
							//reset reloading vars for non-magazine reloads
							if(bulletsToReload != bulletsPerClip){
								bulletsReloaded = 0;//reset bulletsReloaded value 
								//rewind Neutral animation when sprinting
								WeaponAnimationComponent["Neutral"].speed = 1.5f;
								WeaponAnimationComponent.Play("Neutral", PlayMode.StopSameLayer);//play Neutral animation	
							}	
							//fast forward camera animations to stop playback if sprinting
							CameraAnimationComponent["CameraReloadMP5"].normalizedTime = 1.0f;
							CameraAnimationComponent["CameraReloadAK47"].normalizedTime = 1.0f;
							CameraAnimationComponent["CameraReloadPistol"].normalizedTime = 1.0f;
							CameraAnimationComponent["CameraReloadSingle"].normalizedTime = 1.0f;
							CameraAnimationComponent["CameraSwitch"].normalizedTime = 1.0f;
							//if sprint interrupts reload more than half-way through, just give bulletsNeeded
							if(bulletsToReload == bulletsPerClip && reloadStartTime + reloadTime / 2 < Time.time && !sprintReloadState){
								bulletsNeeded = bulletsPerClip - bulletsLeft;
								//we have ammo left to reload
								if(ammo >= bulletsNeeded){
									ammo -= bulletsNeeded;//subtract bullets needed from total ammo
									bulletsLeft = bulletsPerClip;//add bullets to magazine 
								}else{
									bulletsLeft += ammo;//if ammo left is less than needed to reload, just load all remaining bullets
									ammo = 0;//out of ammo for this weapon now
								}
								sprintReloadState = true;//only preform this action once at beginning of sprint/reload check
							}else{//if we are less than half way through reload before sprint interrupted, cancel reload
								//stop reload sound from playing
								otherfx.clip = null;
								if(bulletsToReload == bulletsPerClip){
									//rewind reload animation when sprinting
									WeaponAnimationComponent["Reload"].speed = -reloadAnimSpeed * 1.5f;
									WeaponAnimationComponent.CrossFade("Reload", 0.35f, PlayMode.StopSameLayer);//play reloading animation backwards
								}		
							}
						}
					}else{
						//Start automatic reload if player is out of ammo and firing time has elapsed to allow finishing of firing animation and sound
						if (bulletsLeft <= 0 
						&& shootStartTime + fireRate < Time.time 	 
						&& canShoot){
							if( ammo > 0 
							&& !IronsightsComponent.reloading 
							&& !PlayerWeaponsComponent.switching 
							&& ((startTime + readyTime) < Time.time)){
								StartCoroutine("Reload");
								//set animation speeds
								//make this check to prevent slow playing of non magazine anim for last bullet in inventory
								if(bulletsToReload == bulletsPerClip){
									WeaponAnimationComponent["Reload"].speed = reloadAnimSpeed;	
								}
								WeaponAnimationComponent["Ready"].speed = readyAnimSpeed;
							}
						}	
					}
					
					//don't spawn shell if player started sprinting to avoid unrealistic movement of shell if sprint stops
					if(FPSWalkerComponent.canRun){
						StopCoroutine("SpawnShell");	
					}
					
					if(!FPSWalkerComponent.hideWeapon){//dont shoot if player is climbing
						IronsightsComponent.climbMove = 0.0f;
						//enable/disable shooting based on various player states
						if(((!FPSWalkerComponent.sprintActive && !FPSWalkerComponent.prone) || IronsightsComponent.reloading)
						|| FPSWalkerComponent.crouched
						|| (FPSPlayerComponent.zoomed && meleeSwingDelay == 0)
						|| ((Mathf.Abs(horizontal) > 0.75f) && (Mathf.Abs(vertical) < 0.0f) && !FPSWalkerComponent.prone)
						|| FPSWalkerComponent.cancelSprint
						|| (!FPSWalkerComponent.grounded && FPSWalkerComponent.jumping)//don't play sprinting anim while jumping
						|| (FPSWalkerComponent.fallingDistance > 0.75f)//don't play sprinting anim while falling  
						|| (InputComponent.fireHold && !FPSWalkerComponent.prone)){
							//not sprinting
							//set sprint recovery timer so gun only shoots after returning to neutral
							if(!sprintState){
								recoveryTime = Time.time;
								sprintState = true;
							}
							canShoot = true;
							sprintReloadState = false;//reset sprintReloadState to allow another sprint reload cancel check
						}else{
							//sprinting
							if((Mathf.Abs(vertical) > 0.0f)
							|| (Mathf.Abs(horizontal) > 0.0f && FPSWalkerComponent.prone)){
								sprintState = false;
								if(IronsightsComponent.reloading){
									canShoot = false;
								}else{
									if(FPSPlayerComponent.zoomed && meleeSwingDelay == 0){
										canShoot = true;
									}else{
										canShoot = false;
									}
								}
							}else{
								//set sprint recovery timer so gun only shoots after returning to center
								if(!sprintState){
									recoveryTime = Time.time;
									sprintState = true;
								}
								canShoot = true;
							}
						}
					}else{
						if(!FPSWalkerComponent.lowerGunForClimb){
							if(!sprintState){
								recoveryTime = Time.time;
								sprintState = true;
							}
							canShoot = true;
						}else{
							if(meleeSwingDelay == 0){
								IronsightsComponent.climbMove = -0.4f;
							}else{
								IronsightsComponent.climbMove = -1.4f;
							}
							canShoot = false;
							sprintState = false;
						}
					}
				
					//Play noammo sound and manage other states where player can't fire weapon
					if (InputComponent.fireHold){
						if(cantFireState
						&& canShoot
						&& bulletsLeft <= 0
						&& ammo <= 0
						&& ((!PistolSprintAnim && AnimationComponent["RifleSprinting"].normalizedTime < 0.35f)//only play sound when weapon is centered
						 ||(PistolSprintAnim && AnimationComponent["PistolSprinting"].normalizedTime < 0.35f))){
							otherfx.volume = 1.0f;
							otherfx.clip = noammoSnd;
							otherfx.PlayOneShot(otherfx.clip, 1.0f / otherfx.volume);
							shooting = false;
							cantFireState = false;
						}
					}else{
						cantFireState = true;
						waterFireState = false;//allow player to fire again after surfacing from underwater if they released the fire button
					}
					
					//Change fire mode
					if (InputComponent.fireModePress || InputComponent.xboxDpadUpPress){
						if(fireModeState
						&& canShoot
						&& !IronsightsComponent.reloading
						&& ((!PistolSprintAnim && AnimationComponent["RifleSprinting"].normalizedTime < 0.35f)//only change fire mode when weapon is centered
						 ||(PistolSprintAnim && AnimationComponent["PistolSprinting"].normalizedTime < 0.35f))){
							
							burstState = false;
							burstShotsFired = 0;
							
							if(fireModeSelectable && semiAuto){
								
								semiAuto  = false;
								fireModeState = false;
								if(projectileCount < 2){//make muzzle flash last slightly longer for semiAuto
									muzzleFlashReduction = 6.5f;
								}else{
									muzzleFlashReduction = 2.0f;		
								}
								otherfx.volume = 1.0f;
								otherfx.clip = noammoSnd;
								otherfx.PlayOneShot(otherfx.clip, 1.0f / otherfx.volume);
								
								if(burstAndAuto){//if all three fire modes are available, choose burst fire after semi auto
									if(!burstFire){
										burstFire = true;
									}
								}
								
							}else if(fireModeSelectable && !semiAuto){
								
								if(!burstAndAuto){
									semiAuto  = true;
								}else{//if all three fire modes are available, choose auto fire after burst
									if(burstFire){
										burstFire = false;	
									}else{
										semiAuto  = true;//if auto mode is active, switch back to semi auto
									}
								}
								
								fireModeState = false;
								if(projectileCount < 2){//make muzzle flash last slightly longer for shotguns
									muzzleFlashReduction = 3.5f;
								}else{
									muzzleFlashReduction = 2.0f;		
								}
								otherfx.volume = 1.0f;
								otherfx.clip = noammoSnd;
								otherfx.PlayOneShot(otherfx.clip, 1.0f / otherfx.volume);	
							}
						}
					}else{
						fireModeState = true;
					}
					
					//emit barrel smoke particles after firing if player has fired more than barrelSmokeShots
					if(useBarrelSmoke && barrelSmokePos && !FPSWalkerComponent.holdingBreath){
						if(!emitBarrelSmoke){
							if(shootStartTime + 0.25f < Time.time){					
								if(barrelSmokeParticles && bulletsJustFired >= barrelSmokeShots ){
									emitBarrelSmoke = true;
									barrelSmokeTime = Time.time;
								}
							}
						}else{
							if(barrelSmokeTime + 0.5f > Time.time){
								if(!InputComponent.fireHold 
								|| IronsightsComponent.reloading
								//emit particles if player is still holding fire button and fire time has elapsed for semi auto
								|| ((shootStartTime + fireRate < Time.time) && InputComponent.fireHold)){
									barrelSmokeParticles.transform.position = barrelSmokePos.position;
									barrelSmokeParticles.Emit();
								}
							}else{
								emitBarrelSmoke = false;	
							}
						}
					}
					
					//reset bulletsJustFired value when not firing after a time
					if(shootStartTime + 0.35f < Time.time){	
						bulletsJustFired = 0;	
					}
						
				}
				
				//Run weapon sprinting animations
				if (((canShoot || FPSWalkerComponent.hideWeapon)//allow gun to stay centered and lowered when climbing, swimming, or holding object
				|| FPSWalkerComponent.crouched
				|| (FPSWalkerComponent.midPos < FPSWalkerComponent.standingCamHeight && FPSWalkerComponent.proneRisen)//player is crouching
				|| IronsightsComponent.reloading
				|| FPSWalkerComponent.cancelSprint)
				|| !FPSWalkerComponent.moving){
					if(sprintAnimState){//animate weapon up
						//store time that sprint anim started to disable weapon switching during transition
						PlayerWeaponsComponent.sprintSwitchTime = Time.time;
		
						if(!PistolSprintAnim){
							//keep playback at last frame of animation to prevent it from being interrupted and to allow 
							//instant reversal of playback intstead of continuing past an animation playback time of 1
							if(AnimationComponent["RifleSprinting"].normalizedTime > 1){AnimationComponent["RifleSprinting"].normalizedTime = 1;}
							//reverse animation speed for smooth changing of direction/reversal
							//animation will need to finish before recoveryTime has elapsed to prevent twisting of view when recovering from sprint
							AnimationComponent["RifleSprinting"].speed = -2.0f;
							AnimationComponent.CrossFade("RifleSprinting", 0.35f,PlayMode.StopSameLayer);
						}else{
							if(AnimationComponent["PistolSprinting"].normalizedTime > 1){AnimationComponent["PistolSprinting"].normalizedTime = 1;}
							//reverse animation speed for smooth changing of direction/reversal
							//animation will need to finish before recoveryTime has elapsed to prevent twisting of view when recovering from sprint
							AnimationComponent["PistolSprinting"].speed = -4.0f;
							AnimationComponent.CrossFade("PistolSprinting", 0.35f,PlayMode.StopSameLayer);	
						}
						//set sprintAnimState to false to only perform these actions once per change of sprinting state checks
						sprintAnimState = false;
					}
				}else{
					if(!sprintAnimState){//animate weapon down
						//store time that sprint anim started to disable weapon switching during transition
						PlayerWeaponsComponent.sprintSwitchTime = Time.time;
						
						if(!PistolSprintAnim){
							//keep playback at first frame of animation to prevent it from being interrupted and to allow 
							//instant reversal of playback intstead of continuing past an animation playback time of 0 into negative values
							if(AnimationComponent["RifleSprinting"].normalizedTime < 0){AnimationComponent["RifleSprinting"].normalizedTime = 0;}
							//reverse animation speed for smooth changing of direction
							AnimationComponent["RifleSprinting"].speed = 2.0f;
							AnimationComponent.CrossFade("RifleSprinting", 0.35f,PlayMode.StopSameLayer);
						}else{
							//keep playback at first frame of animation to prevent it from being interrupted and to allow 
							//instant reversal of playback intstead of continuing past an animation playback time of 0 into negative values
							if(AnimationComponent["PistolSprinting"].normalizedTime < 0){AnimationComponent["PistolSprinting"].normalizedTime = 0;}
							//reverse animation speed for smooth changing of direction
							AnimationComponent["PistolSprinting"].speed = 4.0f;
							AnimationComponent.CrossFade("PistolSprinting", 0.35f,PlayMode.StopSameLayer);	
						}
						
						//set sprintAnimState to true to only perform these actions once per change of sprinting state checks
						sprintAnimState = true;
						//rewind reloading animation if reload is interrupted by sprint
						if(PlayerWeaponsComponent.switching && IronsightsComponent.reloading){
							WeaponAnimationComponent.CrossFade("Reload", 0.35f,PlayMode.StopSameLayer);
						}
					}
				}
			}
		}
		
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Update Actions
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	void Update (){

		otherfx.pitch = Time.timeScale;

		if(Time.timeScale > 0){//allow pausing by setting timescale to 0
			
			//do not perform weapon actions if this is an unarmed/null weapon
			if(!unarmed && FPSPlayerComponent.hitPoints > 0){
				//Fade out muzzle flash alpha 
				if(muzzleFlash){
					if (muzzleRendererComponent.enabled){
						if(muzzleFlashColor.a > 0.0f){
							muzzleFlashColor.a -= muzzleFlashReduction * Time.deltaTime;
							muzzleRendererComponent.material.SetColor("_TintColor", muzzleFlashColor);
						}else{
							muzzleRendererComponent.enabled = false;//disable muzzle flash object after alpha has faded
						}	
					}
				}
				
				//activate muzzle light for muzzle flash
				if(muzzleLightObj){
					if(muzzleLightComponent.enabled){
						if(muzzleLightComponent.intensity > 0.0f){
							if(shootStartTime + muzzleLightDelay < Time.time){
								muzzleLightComponent.intensity -= muzzleLightReduction * Time.deltaTime;	
							}
						}else{
							muzzleLightComponent.enabled = false;
						}
					}
				}
				
				//start reload if reload button is pressed
				if (InputComponent.reloadPress
				&& !IronsightsComponent.reloading 
				&& ammo > 0 
				&& bulletsLeft < bulletsPerClip
				&& shootStartTime + fireRate < Time.time
				&& !InputComponent.fireHold){
					sprintReloadState = true;
					StartCoroutine("Reload");
				}
				
				//Detect firemode (auto or semi auto) and call fire function
				if(InputComponent.fireHold && !waterFireState){
					if(semiAuto){
						if(!semiState){
							Fire();
							semiState = true;
						}
					}else{
						if(!burstFire){//fire weapon in auto mode
							Fire();
						}else{//fire weapon in burst mode
							if(!burstState && !burstHold){
								burstState = true;
								if(burstShotsFired < burstShots){
									burstHold = true;//stop firing if in burst has finished, but fire button is still held
								}
							}
						}
					}
				}else{
					semiState = false;
					burstHold = false;
				}
				
				//fire shots in burst mode
				if(burstState){
					if(!canShoot//cancel burst fire under these conditions
					|| IronsightsComponent.reloading
					|| PlayerWeaponsComponent.switching 
					|| AnimationComponent["RifleSprinting"].normalizedTime > 0.35f//weapon not centered after sprint
					|| ((startTime + readyTime) > Time.time)){
						burstState = false;
						burstShotsFired = 0;	
					}
					if(burstShotsFired < burstShots){//fire burst shots
						Fire();	
					}else{
						if(!InputComponent.fireHold){//allow another burst to fire if fire button is released
							burstState = false;
							burstShotsFired = 0;
						}
						
					}
				}
				
				//set shooting var to false
				if(shootStartTime + 0.2f < Time.time){
					shooting = false;	
				}
				
			}
				
			//smooth gun angle animation amounts 
			if(Time.smoothDeltaTime > 0.0f && !unarmed){
				gunAngles.x = Mathf.SmoothDampAngle(gunAngles.x, gunAnglesTarget.x, ref gunAngleVel.x, 0.14f, Mathf.Infinity, Time.smoothDeltaTime);
				gunAngles.y = Mathf.SmoothDampAngle(gunAngles.y, gunAnglesTarget.y, ref gunAngleVel.y, 0.14f, Mathf.Infinity, Time.smoothDeltaTime);
				gunAngles.z = Mathf.SmoothDampAngle(gunAngles.z, gunAnglesTarget.z, ref gunAngleVel.z, 0.14f, Mathf.Infinity, Time.smoothDeltaTime);
				myTransform.localEulerAngles = gunAngles;
			}
			
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Weapon Muzzle Flash
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	void MuzzFlash (){

		//enable muzzle flash
		if (muzzleFlash){
		
			if(!FPSWalkerComponent.holdingBreath){
				if(muzzleLightObj){
					muzzleLightComponent.enabled = true;
					muzzleLightComponent.intensity = 8.0f;
				}
				
				//emit smoke particle effect from muzzle
				if(muzzleSmokeParticles){
					muzzleSmokeColor.a = muzzleSmokeAlpha;
					muzzleSmokeComponent.material.SetColor("_TintColor", muzzleSmokeColor);
					muzzleSmokeParticles.transform.position = muzzleFlash.position;
					muzzleSmokeParticles.Emit();
				}
			}
			
			//set muzzle flash color
			if(!FPSWalkerComponent.holdingBreath){
				muzzleFlashColor.r = 1.0f;
				muzzleFlashColor.g = 1.0f;
				muzzleFlashColor.b = 1.0f;
			}else{
				//set muzzle flash to underwaterFogColor from WaterZone.cs
				muzzleFlashColor.r = PlayerWeaponsComponent.waterMuzzleFlashColor.r;
				muzzleFlashColor.g = PlayerWeaponsComponent.waterMuzzleFlashColor.g;
				muzzleFlashColor.b = PlayerWeaponsComponent.waterMuzzleFlashColor.b;
			}
			
			muzzleFlashColor.a = Random.Range(0.4f, 0.5f);
			muzzleRendererComponent.material.SetColor("_TintColor", muzzleFlashColor);
			//add random rotation to muzzle flash
			muzzleFlash.localRotation = Quaternion.AngleAxis(Random.value * 360, Vector3.forward);
			muzzleRendererComponent.enabled = true;
				
		}
	
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Shell Ejection
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	IEnumerator SpawnShell (){

		if(shellEjectDelay > 0.0f){//delay shell ejection for shotguns and bolt action rifles by shellEjectDelay amount
			yield return new WaitForSeconds(shellEjectDelay);
		}

		if(shellEjectPositionZoom && FPSPlayerComponent.zoomed){
			shellEjectPos = shellEjectPositionZoom;
		}else{
			shellEjectPos = shellEjectPosition;//
		}

		//instantiate rigidbody shell object to calculate position and rotation (invisible w/ no mesh)
		shell = Instantiate(shellPrefabRB,shellEjectPos.position,shellEjectPos.transform.rotation) as GameObject;
		//instantiate mesh shell object to smooth/lerp position and rotation from rigidbody shell object
		shell2 = Instantiate(shellPrefabMesh,shellEjectPos.position,shellEjectPos.transform.rotation) as GameObject;
		//shell.transform.localScale = shellScale;//scale size of RB shell object by shellScale amount
		shell2.transform.localScale = shellScale;//scale size of mesh shell object by shellScale amount
		//direction of ejected shell casing, adding random values to direction for realism
		shellEjectDirection = new Vector3((shellSide * 0.7f) + (shellSide * 0.4f * Random.value), 
									  	 (shellUp * 0.6f) + (shellUp * 0.5f * Random.value),
									  	 (shellForward * 0.4f) + (shellForward * 0.2f * Random.value));
		//Apply velocity to shell
		if(shell.GetComponent<Rigidbody>()){
			shell.GetComponent<Rigidbody>().AddForce((transform.TransformDirection(shellEjectDirection) * shellForce), ForceMode.Impulse);
		}
		ShellEjection ShellEjectionComponent = shell.GetComponent<ShellEjection>();
		//Initialize object references for instantiated shell object
		ShellEjectionComponent.playerObj = playerObj;
		ShellEjectionComponent.PlayerRigidbodyComponent = playerObj.GetComponent<Rigidbody>();
		ShellEjectionComponent.RigidbodyComponent = ShellEjectionComponent.gameObject.GetComponent<Rigidbody>();
		ShellEjectionComponent.FPSPlayerComponent = FPSPlayerComponent;
		ShellEjectionComponent.lerpShell = shell2.transform;//pass reference of mesh shell object to RB shell object's ShellEjection.cs component
		ShellEjectionComponent.gunObj = transform.gameObject;
		shell.transform.parent = shellEjectPosition.transform.parent;
		shell2.transform.parent = shellEjectPosition.transform.parent;

		Mesh shellMesh = shell2.GetComponent<MeshFilter>().mesh;
		Bounds shellBounds = shellMesh.bounds;
		shellBounds.Expand(60.0f);


	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Set Up Fire Event
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	void Fire(){
		//do not proceed to fire if out of ammo, have already fired in semi-auto mode, or chambering last round
		if (bulletsLeft <= 0 || (semiAuto && semiState) || lastReload){
			return;
		}
		//only fire at fireRate value
		if(shootStartTime + fireRate > Time.time){ 
			return;	
		}
		
		//don't fire this weapon underwater if fireableUnderwater var is false
		if(FPSWalkerComponent.holdingBreath && (!fireableUnderwater || FPSWalkerComponent.lowerGunForSwim)){ 
			if(cantFireState){
				otherfx.volume = 1.0f;
				otherfx.clip = noammoSnd;
				otherfx.PlayOneShot(otherfx.clip, 1.0f / otherfx.volume);
				cantFireState = false;//only play noammo sound once if this is an automatic weapon
				waterFireState = true;//make player have to press fire button again after surfacing to fire
			}
			return;	
		}
	
		//fire weapon
		//don't allow fire button to interrupt a magazine reload
		if((bulletsToReload == bulletsPerClip && !IronsightsComponent.reloading)
		//allow normal firing when weapon does not reload by magazine
		|| (!IronsightsComponent.reloading && bulletsToReload != bulletsPerClip && bulletsLeft > 0)
		//allow fire button to interrupt a non-magazine reload if there are at least 2 shells loaded
		|| (IronsightsComponent.reloading && bulletsToReload != bulletsPerClip && bulletsLeft >= bulletsToReload * 2.0f && reloadEndTime + reloadTime < Time.time)){
			if (canShoot && !PlayerWeaponsComponent.switching){//don't allow shooting when reloading, sprinting, or switching
					
				//make weapon recover faster from sprinting if using the pistol sprint anim 
				//because the gun/rifle style anims have more yaw movement and take longer to return to center
				if(!PistolSprintAnim){recoveryTimeAmt = 0.4f;}else{recoveryTimeAmt = 0.2f;}
				//reset bullets reloaded for non magazine reloading weapons
				if(bulletsToReload != bulletsPerClip){bulletsReloaded = 0;}
				//Check sprint recovery timer so gun only shoots after returning to center.
				//NOTE: If this is set before view rotation can return to neutral (too small a value) 
				//the view recoil while shooting just after sprinting will "twist" strangely for the first shot.
				if((recoveryTime + recoveryTimeAmt < Time.time) && (startTime + readyTime < Time.time)){
						
					StartCoroutine("FireOneShot");//fire bullet
					StopCoroutine("Reload");//stop reload coroutine if interrupting a non-magazine reload
					IronsightsComponent.reloading = false;//update reloading var in Ironsights script if cancelling reload to fire
					otherfx.clip = null;//stop playing reload sound effect if cancelling reload to fire
						
					if(meleeSwingDelay == 0){//eject shell and perform muzzle flash if not a melee weapon
						MuzzFlash();
						if(shellPrefabRB && shellPrefabMesh){
							StartCoroutine("SpawnShell");
						}
						bulletsJustFired ++;//track number of shots fired recently to determine when to emit barrel smoke particles
					}
					
					//track time that we started firing and keep fire rate as frame rate independent as possible
					shootStartTime = Time.time - (Time.deltaTime/20.0f);
					shooting = true;
				}
			}
		}
	
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Fire Projectile
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	IEnumerator FireOneShot (){
		//do not allow shooting when sprinting
	   	if (canShoot){

			firefx.pitch = Random.Range(0.96f * Time.timeScale, 1.0f * Time.timeScale);//add slight random value to firing sound pitch for variety
			firefx.pitch = 1.0f * Time.timeScale;
			firefx.spatialBlend = 0.0f;
			firefx.panStereo = 0.0f;
			firefx.velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;
			firefx.PlayOneShot(firefx.clip, 0.9f / firefx.volume);//play fire sound
			
			hitCount = 0;//reset hitCount so flesh impacts are counted from zero again for this shot
				
			if(meleeSwingDelay == 0){//if this is not a melee weapon
				//rewind firing animation and set speed
				WeaponAnimationComponent.Rewind("Fire");
				WeaponAnimationComponent["Fire"].speed = fireAnimSpeed;
				WeaponAnimationComponent.CrossFade("Fire", 0.35f,PlayMode.StopSameLayer);//play firing animation
				//make view recoil with shot
				WeaponKick();
				bulletsLeft -= 1;//subtract fired bullet from magazine amount	
				if(burstFire){
					burstShotsFired++;
				}

			}else{
		
				if(swingSide){//determine which side to swing melee weapon
					CameraAnimationComponent.Rewind("CameraMeleeSwingRight");//rewind camera swing animation 
					CameraAnimationComponent["CameraMeleeSwingRight"].speed = 1.7f;//set camera animation speed
					CameraAnimationComponent.CrossFade("CameraMeleeSwingRight", 0.35f,PlayMode.StopSameLayer);//play camera view animation
					
					WeaponAnimationComponent.Rewind("MeleeSwingRight");
					WeaponAnimationComponent["MeleeSwingRight"].speed = fireAnimSpeed;//set weapon swing animation speed
					WeaponAnimationComponent.CrossFade("MeleeSwingRight", 0.1f,PlayMode.StopSameLayer);//play weapon swing animation
						
					swingSide = false;//set swingSide to false to make next swing from other direction 
				}else{
					CameraAnimationComponent.Rewind("CameraMeleeSwingLeft");//rewind camera swing animation 
					CameraAnimationComponent["CameraMeleeSwingLeft"].speed = 1.6f;//set camera animation speed
					CameraAnimationComponent.CrossFade("CameraMeleeSwingLeft", 0.35f,PlayMode.StopSameLayer);//play camera view animation
					
					WeaponAnimationComponent.Rewind("MeleeSwingLeft");
					WeaponAnimationComponent["MeleeSwingLeft"].speed = fireAnimSpeed;//set weapon swing animation speed
					WeaponAnimationComponent.CrossFade("MeleeSwingLeft",0.1f,PlayMode.StopSameLayer);//play weapon swing animation
						
					swingSide = true;//set swingSide to true to make next swing from other direction 
				}
				//wait for the meleeSwingDelay amount while swinging forward before hitting anything
				yield return new WaitForSeconds(meleeSwingDelay);		
			}
			//fire the number of projectiles defined by projectileCount 
			for(float i = 0; i < projectileCount; i++){
				Vector3 direction = SprayDirection();
				RaycastHit hit;
				
				if(meleeSwingDelay == 0){
					//check for ranged weapon hit
					if(Physics.Raycast(mainCamTransform.position, direction, out hit, range, bulletMask)){
						HitObject(hit, direction);
						
						//Perform a second ray cast from impact point if bullet collided with water, so bullets will hit underwater targets. 
						if(hit.transform.tag == "Water" && Physics.Raycast(hit.point, direction, out hit, range, liquidMask)){
							HitObject(hit, direction);	
						}
					}

					if(useTracers){
						//Emit tracers for fired bullet
						WeaponEffectsComponent.BulletTracers(direction, muzzleFlash);
					}
				
				}else{
					//check for melee weapon hit
					//use SphereCast instead of Raycast to simulate swinging arc where melee weapon may contact objects
					if(Physics.SphereCast(mainCamTransform.position, capsule.radius / 3, direction, out hit, range + (range * FPSWalkerComponent.playerHeightMod/2.0f), bulletMask)){
						HitObject(hit, direction);
					//if sphereCast hits nothing, try a rayCast to hit trigger colliders like the surface of water
					}else if(Physics.Raycast(mainCamTransform.position, direction, out hit, range, bulletMask)){
						HitObject(hit, direction);
					}	
				}
			}
			yield break;
		}
		
	}
	
	//weapon or projectile damage and effects for collider that is hit
	void HitObject ( RaycastHit hit, Vector3 direction ){
		// Apply a force to the rigidbody we hit
		if (hit.rigidbody && hit.rigidbody.useGravity){
			hit.rigidbody.AddForceAtPosition(force * direction / (Time.fixedDeltaTime * 100.0f), hit.point);//scale the force with the Fixed Timestep setting
		}
		
		//call the ApplyDamage() function in the script of the object hit
		switch(hit.collider.gameObject.layer){
			case 1://hit object is an object with transparent effects like a window
				if(hit.collider.gameObject.GetComponent<BreakableObject>()){
					hit.collider.gameObject.GetComponent<BreakableObject>().ApplyDamage(damage);
				}	
				break;
			case 13://hit object is an NPC
				if(hit.collider.gameObject.GetComponent<CharacterDamage>()){
					hit.collider.gameObject.GetComponent<CharacterDamage>().ApplyDamage(damage, direction, mainCamTransform.position, myTransform, true);
				}
				break;
			case 9://hit object is an apple
				if(hit.collider.gameObject.GetComponent<AppleFall>()){
					hit.collider.gameObject.GetComponent<AppleFall>().ApplyDamage(damage);
				}	
				break;
			case 19://hit object is a breakable or explosive object
				if(hit.collider.gameObject.GetComponent<BreakableObject>()){
					hit.collider.gameObject.GetComponent<BreakableObject>().ApplyDamage(damage);
				}else if(hit.collider.gameObject.GetComponent<ExplosiveObject>()){
					hit.collider.gameObject.GetComponent<ExplosiveObject>().ApplyDamage(damage);
				}else if(hit.collider.gameObject.GetComponent<MineExplosion>()){
					hit.collider.gameObject.GetComponent<MineExplosion>().ApplyDamage(damage);
				}
				break;
			default:
				break;	
		}
		
		//emit impact particle effects and leave bullet marks by calling the ImpactEfects() and BulletMarks() function of WeaponEffects.cs 
		if(hit.collider.gameObject.tag == "Flesh"){
			hitCount ++;
			if(hitCount < 2){//only draw one flesh impact effect if this is a shotgun for optimization
				WeaponEffectsComponent.ImpactEffects(hit, false);//draw flesh impact effects where the weapon hit NPC
			}
		}else{
			WeaponEffectsComponent.ImpactEffects(hit, false);//draw impact effects where the weapon hit
			WeaponEffectsComponent.BulletMarks(hit);//draw a bullet mark where the weapon hit
		}

	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Reload Weapon
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	IEnumerator Reload (){
		
		if(Time.timeSinceLevelLoad > 2){//prevent any reloading behavior at level start 
		
		    if((!(FPSWalkerComponent.sprintActive && (Mathf.Abs(horizontal) > 0.75f || Mathf.Abs(vertical) > 0.75f))//allow reload while walking
		     //allow auto reload when sprint button is held, even if stationary
			|| FPSWalkerComponent.cancelSprint)
			&& !FPSWalkerComponent.hideWeapon){//dont allow reloading if gun is lowered while climbing, swimming, or holding object
				
				if(ammo > 0){//if player has no ammo in their inventory for this weapon, do not proceed with reload
					
					//cancel zooming when reloading
					FPSPlayerComponent.zoomed = false;
					
					//if loading by magazine, start these reloading actions immediately and wait for reloadTime before adding ammo and completing reload
					if(bulletsToReload == bulletsPerClip){
						//play reload sound once at start of reload
						otherfx.volume = 1.0f;
						otherfx.clip = reloadSnd;
						otherfx.Play();//OneShot(otherfx.clip, 1.0f / otherfx.volume);//play magazine reload sound effect
						
						//determine which weapon is selected and play camera view reloading animation
						if(!PistolSprintAnim){
							if(weaponNumber == 5 || weaponNumber == 6 || weaponNumber == 7){
								//rewind animation if already playing to allow overlapping playback
								CameraAnimationComponent.Rewind("CameraReloadAK47");
								//set camera reload animation speed to positive value to play forward because
								//it might have been reversed if we canceled a reload by sprinting
								CameraAnimationComponent["CameraReloadAK47"].speed = 1.0f;
								CameraAnimationComponent.CrossFade("CameraReloadAK47", 0.35f,PlayMode.StopSameLayer);
							}else{
								CameraAnimationComponent.Rewind("CameraReloadMP5");
								CameraAnimationComponent["CameraReloadMP5"].speed = 1.0f;
								CameraAnimationComponent.CrossFade("CameraReloadMP5", 0.35f,PlayMode.StopSameLayer);
							}
						}else{
							CameraAnimationComponent.Rewind("CameraReloadPistol");
							CameraAnimationComponent["CameraReloadPistol"].speed = 1.0f;
							CameraAnimationComponent.CrossFade("CameraReloadPistol", 0.35f,PlayMode.StopSameLayer);
						}
						
						//Rewind reloading animation, set speed, and play animation. This can cause sudden/jerky start of reload anim
						//if sprinting very briefly, but is necessary to keep reload animation and sound synchronized.
						WeaponAnimationComponent.Rewind("Reload");
						WeaponAnimationComponent["Reload"].speed = reloadAnimSpeed;
						WeaponAnimationComponent.CrossFade("Reload", 0.35f,PlayMode.StopSameLayer);//play reloading animation
					}
					
					//set reloading var in ironsights script to true
					IronsightsComponent.reloading = true;
					reloadStartTime = Time.time;
					
					burstState = false;
					burstShotsFired = 0;
					
					//do not wait for reloadTime if this is not a magazine reload and this is the first bullet/shell to be loaded,
					//otherwise, adding of ammo and finishing reload will wait for reloadTime while animation and sound plays
					if((bulletsToReload != bulletsPerClip && bulletsReloaded > 0) || bulletsToReload == bulletsPerClip){
						// Wait for reload time first, then proceed
						yield return new WaitForSeconds(reloadTime);
					}
					
					//determine how many bullets need to be reloaded
					bulletsNeeded = bulletsPerClip - bulletsLeft;	
					
					//if loading a magazine, update bullet amount and set reloading var to false after reloadTime has elapsed
					if(bulletsToReload == bulletsPerClip){
							
						//set reloading var in ironsights script to false after reloadTime has elapsed
						IronsightsComponent.reloading = false;
				
						//we have ammo left to reload
						if(ammo >= bulletsNeeded){
							ammo -= bulletsNeeded;//subtract bullets needed from total ammo
							bulletsLeft = bulletsPerClip;//add bullets to magazine 
						}else{
							bulletsLeft += ammo;//if ammo left is less than needed to reload, so just load all remaining bullets
							ammo = 0;//out of ammo for this weapon now
						}
							
					}else{//If we are reloading weapon one bullet at a time (or bulletsToReload is less than the magazine amount) run code below
						//determine if bulletsToReload var needs to be changed based on how many bullets need to be loaded						
						if(bulletsNeeded >= bulletsToReload){//bullets needed are more or equal to bulletsToReload amount, so add bulletsToReload amount
							if(ammo >= bulletsToReload){
								bulletsLeft += bulletsToReload;//add bulletsToReload amount to magazine
								ammo -= bulletsToReload;//subtract bullets needed from total ammo
								bulletsReloaded += bulletsToReload;//increment bulletsReloaded so we can track our progress in this non-magazine reload 
							}else{
								bulletsLeft += ammo;//if ammo left is less than needed to reload, just load all remaining bullets
								ammo = 0;//out of ammo for this weapon now
							}
						}else{//if bullets needed are less than bulletsToReload amount, just add the ammo that is needed
							if(ammo >= bulletsNeeded){
								bulletsLeft += bulletsNeeded;	
								ammo -= bulletsNeeded;//subtract bullets needed from total ammo
								bulletsReloaded += bulletsToReload;//increment bulletsReloaded so we can track our progress in this non-magazine reload 
							}else{
								bulletsLeft += ammo;//if ammo left is less than needed to reload, just load all remaining bullets
								ammo = 0;//out of ammo for this weapon now	
							}
						}
							
						if(bulletsNeeded > 0){//if bullets still need to be reloaded and we are not loading a magazine
							StartCoroutine("Reload");//start reload coroutine again to load number of bullets defined by bulletsToReload amount			
						}else{
							IronsightsComponent.reloading = false;//if magazine is full, set reloading var in ironsights script to false
							bulletsReloaded = 0;
							yield break;//also stop coroutine here to prevent sound from playing below
						}
							
						if(bulletsNeeded <= bulletsToReload || ammo <= 0){//if reloading last round, play normal reloading sound and also chambering effect
							otherfx.clip = reloadLastSnd;//set otherfx audio clip to reloadLastSnd
							WeaponAnimationComponent["Reload"].speed = 1.0f;
							//track time we started reloading last bullet to allow for additional time to chamber round before allowing weapon firing		
							reloadLastStartTime = Time.time;
							IronsightsComponent.reloading = false;
						}else{
							otherfx.clip = reloadSnd;//set otherfx audio clip to reloadSnd
							WeaponAnimationComponent["Reload"].speed = shellRldAnimSpeed;
	
						}
						
						//play reloading sound effect	
						otherfx.volume = 1.0f;
						otherfx.pitch = Random.Range(0.95f * Time.timeScale, 1 * Time.timeScale);
						otherfx.PlayOneShot(otherfx.clip, 1.0f / otherfx.volume);
						//play reloading animation
						WeaponAnimationComponent.Rewind("Reload");
						WeaponAnimationComponent.CrossFade("Reload", 0.35f,PlayMode.StopSameLayer);
							
						//play camera reload animation 
						CameraAnimationComponent.Rewind("CameraReloadSingle");
						//set camera reload animation speed to positive value to play forward because
						//it might have been reversed if we canceled a reload by sprinting
						CameraAnimationComponent["CameraReloadSingle"].speed = 1.0f;
						CameraAnimationComponent.CrossFade("CameraReloadSingle", 0.35f,PlayMode.StopSameLayer);
							
						reloadEndTime = Time.time;//track time that we finished reload to determine if this reload can be interrupted by fire button
						
					}	
				}
			}
		}
		
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Calculate angle of bullet fire from muzzle
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	private Vector3 SprayDirection (){
		//increase weapon accuracy if player is crouched
		float crouchAccuracy = 1.0f;
		float spreadPerShot = 1.2f;
		
		if(FPSWalkerComponent.crouched){
			crouchAccuracy = 0.75f;	
		}else{
			crouchAccuracy = 1.0f;	
		}
		//make firing more accurate when sights are raised and/or in semi auto
		if(FPSPlayerComponent.zoomed && meleeSwingDelay == 0){
			if(fireModeSelectable && semiAuto){
				shotSpreadAmt = shotSpread / 5 * crouchAccuracy;
			}else{
				shotSpreadAmt = shotSpread / 3 * crouchAccuracy;
			}
		}else{
			if(fireModeSelectable && semiAuto){
				shotSpreadAmt = shotSpread / 2 * crouchAccuracy;
			}else{
				shotSpreadAmt = shotSpread * crouchAccuracy;
			}
		}
		
		//if using sustained fire recoil, increase aim angles exponentially after shotsBeforeRecoil have been fired
		if(useRecoilIncrease){
			if(bulletsJustFired > shotsBeforeRecoil){
				spreadPerShot = Mathf.Pow(bulletsJustFired - (shotsBeforeRecoil - 1), spreadPerShot / aimDirRecoilIncrease); 
			}else{
				spreadPerShot = 1.2f;	
			}
		}else{
			spreadPerShot = 1.0f;	
		}
		
		//apply accuracy spread amount to weapon facing angle
		float vx = (1 - 2 * Random.value) * shotSpreadAmt * spreadPerShot;
		float vy = (1 - 2 * Random.value) * shotSpreadAmt * spreadPerShot;
		float vz = 1.0f;
		return myTransform.TransformDirection(new Vector3(vx,vy,vz));
		
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Camera Recoil Kick
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	void WeaponKick (){

		float randXkick;
		float randYkick;
		float spreadPerShot = 1.2f;
		//make recoil less when zoomed or prone and more when zoomed out
		if((FPSPlayerComponent.zoomed || FPSWalkerComponent.prone) && meleeSwingDelay == 0.0f){
			kickUpAmt = kickUp;//set kick amounts to those set in the editor
			kickSideAmt = kickSide;
		}else{
			if(!FPSWalkerComponent.crouched
			//normal view kick when crouching and not moving
			||(FPSWalkerComponent.crouched && Mathf.Abs(horizontal) == 0.0f && Mathf.Abs(vertical) == 0.0f)){
				kickUpAmt = kickUp * 1.75f;
				kickSideAmt = kickSide * 1.75f;
			}else{
				//increase view kick to offset increased bobbing 
				//amounts when crouching and moving 
				kickUpAmt = kickUp * 2.75f;
				kickSideAmt = kickSide * 2.75f;
			}
		}

		randXkick = Random.Range(-kickSideAmt * 2.0f, kickSideAmt * 2.0f);
		randYkick = Random.Range(kickUpAmt * 1.5f, kickUpAmt * 2.0f);
		
		//Set rotation quaternion to random kick values
		kickRotation = Quaternion.Euler(mainCamTransform.localRotation.eulerAngles - new Vector3(randYkick, randXkick, 0.0f));
		
		//smooth current camera angles to recoil kick up angles using Slerp
		mainCamTransform.localRotation = Quaternion.Slerp(mainCamTransform.localRotation, kickRotation, 0.1f);
		
		if(useRecoilIncrease){
			
			if(bulletsJustFired > shotsBeforeRecoil && !FPSWalkerComponent.prone){
				//increase spreadPerShot exponentially for more realistic feel
				spreadPerShot = Mathf.Pow(bulletsJustFired - (shotsBeforeRecoil - 1), spreadPerShot / viewKickIncrease); 
			}else{
				if(!FPSWalkerComponent.prone){
					spreadPerShot = 1.2f;	
				}else{//less recoil when prone
					spreadPerShot = 0.5f;
				}
			}
			
			if(useViewClimb){//apply non-recoverable view climb to mouse input with sustained fire recoil
				if(viewClimbUp > 0.0f){
					MouseLookComponent.recoilY += ((randYkick / 8.0f * viewClimbUp) * (spreadPerShot / 6.0f ));
				}
				if(viewClimbSide > 0.0f || viewClimbRight > 0.0f){
					MouseLookComponent.recoilX += ((randXkick / 4.0f * viewClimbSide) + viewClimbRight) * (spreadPerShot / 2.0f);
				}
			}	
		}else{	
			
			if(useViewClimb){//apply non-recoverable view climb to mouse input without sustained fire recoil
				if(viewClimbUp > 0.0f){
					MouseLookComponent.recoilY += randYkick / 8.0f * viewClimbUp;
				}
				if(viewClimbSide > 0.0f || viewClimbRight > 0.0f){
					MouseLookComponent.recoilX += (randXkick / 4.0f * viewClimbSide) + viewClimbRight;
				}
			}	
		}
	}
	
}