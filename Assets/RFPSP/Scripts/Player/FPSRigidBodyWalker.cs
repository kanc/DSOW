//FPSRigidBodyWalker.cs by Azuline Studios© All Rights Reserved
//Manages player movement controls, sets player movement speed, plays certain sound effects 
//determines player movement state, and sets player's rigidbody velocity.
using UnityEngine;
using System.Collections;

public class FPSRigidBodyWalker : MonoBehaviour {
	
	//set up external script references
	private SmoothMouseLook SmoothMouseLookComponent;
	private FPSPlayer FPSPlayerComponent;
	private InputControl InputComponent;
	private Footsteps FootstepsComponent;
	private WeaponBehavior WeaponBehaviorComponent;
	//private WorldRecenter WorldRecenterComponent;
	
	//objects accessed by this script
	[HideInInspector]
	public GameObject mainObj;
	[HideInInspector]
	public GameObject weaponObj;
	[HideInInspector]
	public GameObject CameraObj;
	[HideInInspector]
	public Transform myTransform;
	private Transform mainCamTransform;
	private Animation CameraAnimationComponent;
	private Rigidbody RigidbodyComponent;

	[HideInInspector]
	public bool playerParented;
	[Tooltip("The Game Object that has the VisibleBody.cs script attached.")]
	public GameObject VisibleBody;
	
	//track player input
	[HideInInspector]
	public float inputXSmoothed = 0.0f;//binary inputs smoothed using lerps
	[HideInInspector]
	public float inputYSmoothed = 0.0f;
	[HideInInspector]
	public float inputX = 0;//1 = button pressed 0 = button released
	[HideInInspector]
	public float inputY = 0;
	private float InputYLerpSpeed;//to allow quick deceleration when running into walls
	private float InputXLerpSpeed;
	private float InputLerpSpeed = 12.0f;//movement lerp speed
	
	//player movement speed amounts
	public float walkSpeed = 4.0f;
	public float sprintSpeed = 9.0f;
	
	//sprinting
	public enum sprintType{//Sprint mode
		hold,
		toggle,
		both
	}
	public sprintType sprintMode = sprintType.both;
	private float sprintDelay = 0.4f;
	public bool limitedSprint = true;//true if player should run only while staminaForSprint > 0 
	public bool sprintRegenWait = true;//true if player should wait for stamina to fully regenerate before sprinting
	public float sprintRegenTime = 3.0f;//time it takes to fully regenerate stamina if sprintRegenWait is true
	private bool breathFxState;
	public float staminaForSprint = 5.0f;//duration allowed for sprinting when limitedSprint is true
	private float staminaForSprintAmt;//actual duration amt allowed for sprinting modified by scripts
	public bool catchBreathSound = true;//true if the catch breath sound effect should be played when staminaForSprint is depleted
	private bool staminaDepleted;//true when player has run out of stamina and must wait for it to regenerate if sprintRegenWait is also true
	
	//prone
	public bool allowProne = true;//true if player is allowed to use prone stance 
	
	//leaning
	public bool allowLeaning = true;//true if player should be allowed to lean
	private float leanDistance = 0.75f;
	[HideInInspector]
	public GameObject leanObj;
	[HideInInspector]
	public CapsuleCollider leanCol;
	[HideInInspector]
	public float leanAmt = 0.0f;
	[HideInInspector]
	public float leanPos;
	private float leanVel;
	private Vector3 leanCheckPos;
	private Vector3 leanCheckPos2;
	[HideInInspector]
	public bool leanState;
	
	public float jumpSpeed = 4.0f;//vertical speed of player jump
	public float climbSpeed = 4.0f;//speed that player moves vertically when climbing
	
	public bool lowerGunForClimb = true;//if true, gun will be lowered when climbing surfaces
	public bool lowerGunForSwim = true;//if true, gun will be lowered when swimming
	public bool lowerGunForHold = true;//if true, gun will be lowered when holding object
	[HideInInspector]
	public bool holdingObject;
	[HideInInspector]
	public bool hideWeapon;//true when weapon should be hidden from view and deactivated
	
	//swimming customization
	public float swimSpeed = 4.0f;//speed that player moves vertically when swimming
	public float holdBreathDuration = 15.0f;//amount of time before player starts drowning
	public float drownDamage = 7.0f;//rate of damage to player while drowning
	
	//player speed limits
	private float limitStrafeSpeed = 0.0f;
	[Tooltip("Percentage to decrease movement speed when strafing diagonally.")]
	public float diagonalStrafeAmt = 0.7071f;
	[Tooltip("Percentage to decrease movement speed when moving backwards.")]
	public float backwardSpeedPercentage = 0.6f;//percentage to decrease movement speed while moving backwards
	[Tooltip("Percentage to decrease movement speed when crouching.")]
	public float crouchSpeedPercentage = 0.55f;//percentage to decrease movement speed while crouching
	private float crouchSpeedAmt = 1.0f;
	[Tooltip("Percentage to decrease movement speed when prone.")]
	public float proneSpeedPercentage = 0.45f;//percentage to decrease movement speed while prone
	private float proneSpeedAmt = 1.0f;
	[Tooltip("Percentage to decrease movement speed when strafing directly left or right.")]
	public float strafeSpeedPercentage = 0.8f;//percentage to decrease movement speed while strafing directly left or right
	private float speedAmtY = 1.0f;//current player speed per axis which is applied to rigidbody velocity vector
	private float speedAmtX = 1.0f;
	[HideInInspector]
	public bool zoomSpeed;//to control speed of movement while zoomed, handled by Ironsights script and true when zooming
	[Tooltip("Percentage to decrease movement speed when zoomed.")]
	public float zoomSpeedPercentage = 0.6f;//percentage to decrease movement speed while zooming
	private float zoomSpeedAmt = 1.0f;
	private float speed;//combined axis speed of player movement
		
	//rigidbody physics settings
	[HideInInspector]
	public float standingCamHeight = 0.9f;
	[HideInInspector]
	public float crouchingCamHeight = 0.45f;//y position of camera while crouching
	[Tooltip("Y position/height of camera when prone.")]
	public float proneCamHeight = -0.2f;//y position of camera while prone
	private float standingCapsuleheight = 2.0f;//height of capsule while standing
	private float crouchingCapsuleHeight = 1.25f;//height of capsule while crouching
	private float capsuleCastHeight = 0.75f;//height of capsule cast above player to check for obstacles before standing from crouch
	private float capsuleCastDist = 0.4f;
	private float rayCastHeight = 2.6f;
	[Tooltip("Percent of standing height to move camera to when crouching.")]
	public float crouchHeightPercentage = 0.5f;//percent of standing height to move camera to when crouching
	[Tooltip("Amount to add to player height.")]
	public float playerHeightMod = 0.0f;//amount to add to player height (proportionately increases player height, radius, and capsule cast/raycast heights)
	private int maxVelocityChange = 5;//maximum rate that player velocity can change
	private Vector3 moveDirection = Vector3.zero;//movement velocity vector, modified by other speed factors like walk, zoom, and crouch states
	private Vector3 velocityChange = Vector3.zero;//actual velocity vector applied to rigidbody;
	
	//grounded and slopelimit checks
	[Tooltip("Angle of ground surface that player won't be allowed to move over.")]
	[Range(0.0f, 90.0f)]
	public int slopeLimit = 40;//the maximum allowed ground surface/normal angle that the player is allowed to climb
	//[HideInInspector]
	public bool grounded;//true when capsule cast hits ground surface
	private bool rayTooSteep;//true when ray from capsule origin hits surface/normal angle greater than slopeLimit, compared with capsuleTooSteep
	private bool capsuleTooSteep;//true when capsule cast hits surface/normal angle greater than slopeLimit, compared with rayTooSteep
	
	//player movement states
	[HideInInspector]
	public Vector3 velocity = Vector3.zero;//total movement velocity vector
	[HideInInspector]
	public CapsuleCollider capsule;
	private	Vector3 sweepHeight;
	private bool parentState;//only set parent once to prevent rapid parenting and de-parenting that breaks functionality
	[HideInInspector]
	public bool inWater;//true when player is touching water collider/trigger
	[HideInInspector]
	public bool holdingBreath;//true when player view/camera is under the waterline
	[HideInInspector]
	public bool belowWater;//true when player is below water movement threshold and is not treading water (camera/view slightly above waterline)
	[HideInInspector]
	public bool swimming;//set by WaterZone.cs script, if true, player will start using swimming movement methods instead of ground/walking methods
	[HideInInspector]
	public bool canWaterJump = true;//to make player release and press jump button again to jump if surfacing from water by holding jump button
	private float swimmingVerticalSpeed;
	[HideInInspector]
	public float swimStartTime;
	[HideInInspector]
	public float diveStartTime;
	[HideInInspector]
	public bool drowning;//true when player has stayed under water for longer than holdBreathDuration
	private float drownStartTime = 0.0f;
	private Vector3 sweepBase;//base of capsule for forward movement checks
		
	//falling
	[HideInInspector]
	public float airTime = 0.0f;//total time that player is airborn
	private bool airTimeState;
	[Tooltip("Number of units the player can fall before taking damage.")]
	public float fallDamageThreshold = 5.5f;//Units that player can fall before taking damage
	[Tooltip("Multiplier of unit distance fallen to convert to hitpoint damage for the player.")]
	public float fallDamageMultiplier = 2.0f;//Multiplier of unit distance fallen to convert to hitpoint damage for the player  
	private float fallStartLevel;//the y coordinate that the player lost grounding and started to fall
	[HideInInspector]
	public float fallingDistance;//total distance that player has fallen
	private bool falling;//true when player is losing altitude
		
	//climbing (ladders or other climbable surfaces)
	[HideInInspector]
	public bool climbing;//true when playing is in contact with ladder trigger or edge climbing trigger
	[HideInInspector]
	public bool noClimbingSfx;//true when playing is in contact with edge climbing trigger or ladder with false Play Climbing Audio value
	[HideInInspector]
	public float verticalSpeedAmt = 4.0f;//actual rate that player is climbing
	
	//jumping
	public float antiBunnyHopFactor = 0.35f;//to limit the time between player jumps
	[HideInInspector]
	public bool jumping;//true when player is jumping
	private float jumpTimer = 0.0f;//track the time player began jump
	private bool jumpfxstate = true;
	[HideInInspector]
	public bool jumpBtn = true;//to control jump button behavior
	[HideInInspector]
	public float landStartTime = 0.0f;//time that player landed from jump
		
	//sprinting
	[HideInInspector]
	public bool canRun = true;//true when player is allowed to sprint
	[HideInInspector]
	public bool sprintActive;//true when sprint button is ready
	private bool sprintBtnState;
	private float sprintStartTime;
	private float sprintStart = -2.0f;
	private float sprintEnd;
	private bool sprintEndState;
	private bool sprintStartState;
	[HideInInspector]
	public bool cancelSprint;//true when sprint is canceled by other player input
	[HideInInspector]
	public float sprintStopTime = 0.0f;//track when sprinting stopped for control of item pickup time in FPSPlayer script 
	private bool sprintStopState = true;
	
	//crouching	
	[HideInInspector]
	public float midPos = 0.9f;//camera vertical position which is passed to VerticalBob.cs and HorizontalBob.cs
	[HideInInspector]
	public bool crouched;//true when player is crouching
	[HideInInspector]
	public bool crouchRisen;//player has risen from crouch postition
	[HideInInspector]
	public bool crouchState;
	
	//prone
	[HideInInspector]
	public bool prone;//true if player is in prone stance
	[HideInInspector]
	public bool proneRisen;//true if player has risen from prone stance
	private bool proneState;
	[HideInInspector]
	public bool proneMove;//true if the player is prone and moving
	[HideInInspector]
	public bool moving;
	
	[HideInInspector]
	public float camDampSpeed;//variable damp speed for vertical camera smoothing for stance changes
	private float lowpos;//the lowest collision on the capsule, used to check if bottom of capsule is not grounded
	
	//sound effects
	[HideInInspector]
	public AudioClip landfx;//audiosource attached to this game object with landing sound effect
	private bool landState;
	
	public LayerMask clipMask;//mask for reducing the amount of objects that ray and capsule casts have to check

	void Start (){

		//set up external script references
		SmoothMouseLookComponent = CameraObj.GetComponent<SmoothMouseLook>();
		FPSPlayerComponent = GetComponent<FPSPlayer>();
		InputComponent = GetComponent<InputControl>();
		FootstepsComponent = GetComponent<Footsteps>();
		//WorldRecenterComponent = GetComponent<WorldRecenter>();

		CameraAnimationComponent = Camera.main.GetComponent<Animation>();
		RigidbodyComponent = GetComponent<Rigidbody>();

		//Initialize rigidbody
		RigidbodyComponent.freezeRotation = true;
		RigidbodyComponent.useGravity = true;
		capsule = GetComponent<CapsuleCollider>();
		leanCol = leanObj.GetComponent<CapsuleCollider>();
		
		myTransform = transform;//cache transforms for efficiency
		mainCamTransform = Camera.main.transform;
		
		//clamp movement modifier percentages
		Mathf.Clamp01(backwardSpeedPercentage);
		Mathf.Clamp01(crouchSpeedPercentage);
		Mathf.Clamp01(proneSpeedPercentage);
		Mathf.Clamp01(strafeSpeedPercentage);
		Mathf.Clamp01(zoomSpeedPercentage);
		
		staminaForSprintAmt = staminaForSprint;//initialize sprint duration counter
		
		//Set capsule dimensions to default if they have been changed in the inspector
		//because the capsule cast for detecting obstacles in front of player scales
		//its sweep distance based on these dimensions and the playerHeightMod var.
		capsule.height = 2.0f;
		capsule.radius = 0.5f;
		//initialize player height variables
		capsule.height = capsule.height + playerHeightMod;
		capsule.radius = capsule.height * 0.25f;
		leanCol.radius = capsule.radius;
		leanDistance = capsule.height * 0.375f;//keep lean distance in proportion with player height
		//initilize capsule heights
		standingCapsuleheight = 2.2f + playerHeightMod;
		crouchingCapsuleHeight = (crouchHeightPercentage * 1.25f) * standingCapsuleheight;
		//initialize camera heights
		standingCamHeight = 0.42f * standingCapsuleheight;
		crouchingCamHeight = crouchHeightPercentage * standingCamHeight;
		//initialize rayCast and capsule cast heights
		capsuleCastHeight = capsule.height * 0.45f;
		rayCastHeight = capsule.height * 1.3f;
		//scale up jump speed to height addition made by playerHeightMod
		jumpSpeed = jumpSpeed / (1 - (playerHeightMod / capsule.height));
		//initialize var used to store lowest collision point on player capsule
		lowpos = myTransform.position.y;

		VisibleBody.SetActive(true);
		
		//set sprint mode to toggle, hold, or both, based on inspector setting
		switch (sprintMode){
			case sprintType.both:
				sprintDelay = 0.4f;
			break;
			case sprintType.hold:
				sprintDelay = 0.0f;
			break;
			case sprintType.toggle:
				sprintDelay = 999.0f;//time allowed between button down and release to activate toggle
			break;
		}	
	}
	
	//to show points used for Physics.CheckCapsule in editor
//	void OnDrawGizmosSelected() {
//		CapsuleCollider c1 = GetComponent<CapsuleCollider>();
//		Vector3 p1 = transform.position - Vector3.up * ((transform.position.y - 0.1f) - (c1.bounds.min.y + c1.radius));//bottom point
//		Vector3 p2 = p1 + Vector3.up * ((c1.height - 0.1f) - c1.radius * 2);//top point
//		Gizmos.color = Color.yellow;
//		Gizmos.DrawLine(p1, p2);
//	}
	
	void Update(){
		
		//set the vertical bounds of the capsule used to detect player collisions
		//bottom point for capsule cast (raise by 0.1 to prevent collisions with ground)
		Vector3 p1 = myTransform.position - Vector3.up * ((myTransform.position.y - 0.1f) - (capsule.bounds.min.y + capsule.radius));
		//top point for capsule cast when standing
		Vector3 p2 = p1 + Vector3.up * ((standingCapsuleheight - 0.1f)- (capsule.radius * 2));//subtract 2 radii because one is added for p1
		//top point for capsule cast when crouching
		Vector3 p3 = p1 + Vector3.up * ((crouchingCapsuleHeight - 0.1f)- (capsule.radius * 2));

		//update player object angles in Update() loop for smooth rotation
		//align localEulerAngles and eulerAngles y values with cameras' to make player walk in the direction the camera is facing 
		Vector3 tempLocalEulerAngles = new Vector3(0.0f,CameraObj.transform.localEulerAngles.y,0.0f);//store angles in temporary vector
		myTransform.localEulerAngles = tempLocalEulerAngles;//apply angles from temporary vector to player object
		Vector3 tempEulerAngles = new Vector3(0.0f, CameraObj.transform.eulerAngles.y,0.0f);//store angles in temporary vector
		myTransform.eulerAngles = tempEulerAngles;//apply angles from temporary vector to player object

    	//set crouched variable that other scripts will access to check for crouching
		//do this in Update() instead of FixedUpdate to prevent missed button presses between fixed updates
    	if(InputComponent.crouchHold && !swimming && !climbing){
    		if(!crouchState){
    			if(!crouched){
					if(prone){//rise from prone to crouch
						if(!Physics.CheckCapsule(p1, p3, capsule.radius * 0.9f, clipMask.value)){
							camDampSpeed = 0.25f;//slower uncrouching speed
							crouchRisen = false;
		    				crouched = true;
							prone = false;
						}
					}else{
						camDampSpeed = 0.1f;//faster moving down to crouch (not moving up against gravity)
	    				crouched = true;
					}
    				sprintActive = false;//cancel sprint if crouch button is pressed
    			}else{//only uncrouch if the player has room above them to stand up
					if(!Physics.CheckCapsule(p1, p2, capsule.radius * 0.9f, clipMask.value)){
						camDampSpeed = 0.2f;
    					crouched = false;
					}
    			}
    			crouchState = true;
    		}
    	}else{
    		crouchState = false;
    		if((sprintActive || climbing || swimming) && !Physics.CheckCapsule(p1, p2, capsule.radius * 0.9f, clipMask.value)){
				camDampSpeed = 0.2f;
    			crouched = false;//cancel crouch if sprint button is pressed and there is room above the player to stand up
    		}
    	}
		
		//set prone variable that other scripts will access to check for prone stance
		if(allowProne){
			if(InputComponent.proneHold && !swimming && !climbing){
				if(!proneState){
					if(!prone){
						camDampSpeed = 0.1f;
						prone = true;
						crouched = false;
						sprintActive = false;//cancel sprint if prone button is pressed
					}else{//cancel prone if sprint button is pressed and there is room above the player to stand up
						if(!Physics.CheckCapsule(p1, p2, capsule.radius * 0.9f, clipMask.value)){
							camDampSpeed = 0.25f;
	    					prone = false;
						}
					}
					crouchRisen = false;
					proneRisen = false;
					proneState = true;
				}
			}else{
				proneState = false;
	    		if((sprintActive || climbing || swimming) && !Physics.CheckCapsule(p1, p2, capsule.radius * 0.9f, clipMask.value)){
					camDampSpeed = 0.25f;
	    			prone = false;//cancel prone if sprint button is pressed and there is room above the player to stand up
	    		}
			}
			
	    	//cancel crouch or prone if jump button is pressed
	    	if(InputComponent.jumpHold 
			&& (crouched || prone)
			//only uncrouch if the player has room above them to stand up
			&& !Physics.CheckCapsule(p1, p2, capsule.radius * 0.9f, clipMask.value)){
				if(crouched){
					camDampSpeed = 0.2f;
				}else{
					camDampSpeed = 0.25f;
				}
    			crouched = false;
				prone = false;
    			landStartTime = Time.time;//set land time to time jump is pressed to prevent uncrouching and then also jumping
    		}
			
			//determine if player has risen from prone state
			if(mainCamTransform.position.y - myTransform.position.y > (standingCamHeight * 0.95f)){
				camDampSpeed = 0.1f;
				if(!proneRisen && !prone){
					proneRisen = true;	
				}
			}
			
			//determine if player is moving while prone
			if(moving && prone){
				proneMove = true;	
			}else{
				proneMove = false;
			}
		}
		
		//determine if player has risen from crouch state
		if(mainCamTransform.position.y - myTransform.position.y > (crouchingCamHeight * 0.5f)){
			if(!crouchRisen){
				camDampSpeed = 0.1f;
				crouchRisen = true;	
			}
		}

		//Determine if player is moving. This var is accessed by other scripts. 
		if(Mathf.Abs(inputY) > 0.15f || Mathf.Abs(inputX) > 0.15f){
			moving = true;	
		}else{
			moving = false;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//Jumping
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		if(grounded){
			if(jumping){
				//play landing sound effect after landing from jump and reset jumpfxstate
				if(jumpTimer + 0.1f < Time.time){
					//play landing sound
					if(!inWater && landfx && !landState){
						PlayAudioAtPos.instance.PlayClipAt(landfx, mainCamTransform.position, 1.0f, 0.0f, 1.0f);
						landState = true;
					}
					if (CameraAnimationComponent.IsPlaying("CameraLand")){
						//rewind animation if already playing to allow overlapping playback
						CameraAnimationComponent.Rewind("CameraLand");
					}
					CameraAnimationComponent.CrossFade("CameraLand", 0.35f,PlayMode.StopAll);
					
					jumpfxstate = true;
					
					jumpBtn = false;
					
					//reset jumping var (this check must be before jumping var is set to true below)
					jumping = false;
				}
				
			}else{
				
				//determine if player is jumping and set jumping variable
				if (InputComponent.jumpPress
				    && !FPSPlayerComponent.zoomed
				    && jumpBtn//check that jump button is not being held
				    && !crouched
				    && !prone
				    && !belowWater
				    && canWaterJump
				    && !climbing
				    && landStartTime + antiBunnyHopFactor < Time.time//check for bunnyhop delay before jumping
				    && (!rayTooSteep || inWater)){//do not jump if ground normal is greater than slopeLimit and not in water
					
					jumpTimer = Time.time;
					
					if(jumpfxstate){
						PlayAudioAtPos.instance.PlayClipAt(FPSPlayerComponent.jumpfx, mainCamTransform.position, 1.0f, 0.0f, 1.0f);
						jumpfxstate = false;
					}
					
					if (CameraAnimationComponent.IsPlaying("CameraJump")){
						//rewind animation if already playing to allow overlapping playback
						CameraAnimationComponent.Rewind("CameraJump");
					}
					CameraAnimationComponent.CrossFade("CameraJump", 0.35f,PlayMode.StopAll);
					
					//apply the jump velocity to the player rigidbody
					RigidbodyComponent.velocity = new Vector3(velocity.x, Mathf.Sqrt(2.0f * jumpSpeed * -Physics.gravity.y), velocity.z);
					jumping = true;
				}
				
				//reset jumpBtn to prevent continuous jumping while holding jump button.
				if(!InputComponent.jumpHold && landStartTime + antiBunnyHopFactor < Time.time){
					jumpBtn = true;
				}
				
			}
		}else{
			if(airTime + 0.2f < Time.time){
				landState = false;
			}
		}
	
	}
	
	void FixedUpdate(){
		RaycastHit rayHit;
		RaycastHit capHit;
		RaycastHit hit2;

		if(Time.timeScale > 0){//allow pausing by setting timescale to 0
			WeaponBehaviorComponent = FPSPlayerComponent.WeaponBehaviorComponent;
			//set the vertical bounds of the capsule used to detect player collisions
			Vector3 p1 = myTransform.position;//middle of player capsule
			Vector3 p2 = p1 + Vector3.up * ((capsule.height / 2) - (capsule.radius * 1.25f));//top of player capsule
			
			//manage the CapsuleCast size and distance for frontal collision check based on player stance
			if(!prone){
				if(!crouched){//longer capsule check distance if standing and taller capsule check height for smoother jumping over obstacles
					capsuleCastDist = 0.4f + (playerHeightMod * 0.2f);
					sweepBase = myTransform.position - Vector3.up * (capsule.radius * 0.66f);
				}else{//shorter capsule check distance and height if crouching
					capsuleCastDist = capsule.radius + 0.01f;
					sweepBase = myTransform.position;
				}
			}else{//shorter capsule check distance and height if prone	
				capsuleCastDist = (capsule.radius * 0.5f) + 0.01f;
				sweepBase = myTransform.position;
			}
			
			sweepHeight = myTransform.position + Vector3.up * ((capsule.height * 0.66f) - capsule.radius);
			
			//track rigidbody velocity
			velocity = RigidbodyComponent.velocity;
				
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			//Player Input
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
			if(FPSPlayerComponent.hitPoints > 1.0f){
				//Sweep a capsule in front of player to detect walls or other obstacles and stop Y input if there is a detected
				//collision to prevent player capsule from overlapping into world collision geometry in between fixed updates. 
				//This allows smoother jumping over obstacles when the player is walking into them. 

				if(!Physics.CapsuleCast (sweepBase, sweepHeight, capsule.radius * 0.5f, myTransform.forward, out hit2, capsuleCastDist, clipMask.value) || climbing){
					
					//force maximum forward movement input if sprinting
					if(sprintActive){
						inputYSmoothed = 1.0f;
						inputY = 1.0f;
					}else{
						if(prone && SmoothMouseLookComponent.inputY > 25){//cancel forward movement if player is looking up while prone
							inputY = 0.0f;
							inputYSmoothed = 0.0f;
						}else{	
							inputY = InputComponent.moveY;
						}
					}
				
					//decrease y input lerp speed to allow the player to slowly come to rest when forward button is pressed
					if(!swimming){
						InputYLerpSpeed = InputLerpSpeed;
					}else{
						InputYLerpSpeed = 3.0f;//player accelerates and decelerates slower in water
					}
					
				}else{
					
					if((!hit2.rigidbody && grounded) || ((airTime + 0.5f) > Time.time)){//allow the player to run into rigidbodies
						//zero out player y input if an object is detected in front of player
						//allow player to backpedal away from obstacle, but still prevent forward movement
						if(InputComponent.moveY < 0.0f){
							if(prone && SmoothMouseLookComponent.inputY > 25){//cancel forward movement if player is looking up while prone
								inputY = 0.0f;
							}else{	
								inputY = InputComponent.moveY;	
							}
						}else{//don't let player move completely forward into obstacles to prevent them from getting stuck
							inputY = 0.0f;
						}
	
						//increase the y input lerp speed to allow the player to stop quickly and prevent overlap of colliders
						InputYLerpSpeed = 128.0f;
						
					}else{
						
						if(prone && SmoothMouseLookComponent.inputY > 25){//cancel forward movement if player is looking up while prone
							inputY = 0.0f;
						}else{	
							inputY = InputComponent.moveY;	
						}
						
					}
				}
				
				if(!swimming){
					InputXLerpSpeed = InputLerpSpeed;
				}else{
					InputXLerpSpeed = 3.0f;//player accelerates and decelerates slower in water
				}
				
				//cancel horizontal movement if player is looking up while prone
				if(prone && SmoothMouseLookComponent.inputY > 25){
				 	inputX = 0.0f;
					inputXSmoothed = 0.0F;
				}else{
					inputX = InputComponent.moveX;
				}

				if(FPSPlayerComponent.restarting){
					inputX = 0.0f;
					inputY = 0.0f;
				}
				
			
				//Smooth our movement inputs using Mathf.Lerp
				inputXSmoothed = Mathf.Lerp(inputXSmoothed,inputX,Time.deltaTime * InputXLerpSpeed);
			    inputYSmoothed = Mathf.Lerp(inputYSmoothed,inputY,Time.deltaTime * InputYLerpSpeed);
					
			}
			
			//set hideWeapon var in WeaponBehavior.cs to true if weapon should be hidden when climbing, swimming, or holding object
			if((holdingBreath && lowerGunForSwim) || (climbing && lowerGunForClimb) || (holdingObject && lowerGunForHold)){
				hideWeapon = true;	
			}else{
				hideWeapon = false;	
			}
			
			
			//This is the start of the large block that performs all movement actions while grounded	
			if (grounded) {
					
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				//Landing
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				
				//reset airTimeState var so that airTime will only be set once when player looses grounding
				airTimeState = true;
				
				if (falling){//reset falling state and perform actions if player has landed from a fall
					
					fallingDistance = 0;
					landStartTime = Time.time;//track the time when player landed
			       	
			        
			        if((fallStartLevel - myTransform.position.y) > 2.0f){
			        	//play landing sound effect when falling and not landing from jump
						if(!inWater && landfx && !landState){
							//play landing sound
							PlayAudioAtPos.instance.PlayClipAt(landfx, mainCamTransform.position, 1.0f, 0.0f, 1.0f);
							landState = true;
						}
							
						//make camera jump when landing for better feeling of player weight	
						if (CameraAnimationComponent.IsPlaying("CameraLand")){
							//rewind animation if already playing to allow overlapping playback
							CameraAnimationComponent.Rewind("CameraLand");
						}
						CameraAnimationComponent.CrossFade("CameraLand", 0.35f,PlayMode.StopAll);
			        }
			        
			        //track the distance of the fall and apply damage if over falling threshold
			        if (myTransform.position.y < fallStartLevel - fallDamageThreshold && !inWater){
			        	CalculateFallingDamage(fallStartLevel - myTransform.position.y);
			        }
					
					falling = false;
		    	}
					
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				//Sprinting
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				
				//toggle or hold sprinting state by determining if sprint button is pressed or held
				if(Mathf.Abs(inputY) > 0.0f){
					if(InputComponent.sprintHold){
						if(!sprintStartState){
							sprintStart = Time.time;//track time that sprint button was pressed
							sprintStartState = true;//perform these actions only once
							sprintEndState = false;
							if(sprintEnd - sprintStart < sprintDelay * Time.timeScale){//if button is tapped, toggle sprint state
								if(!sprintActive){
									if(!sprintActive){//only allow sprint to start or cancel crouch if player is not under obstacle
										sprintActive = true;
									}else{
										sprintActive = false;//pressing sprint button again while sprinting stops sprint
									}
								}else{
									sprintActive = false;	
								}
							}
						}
					}else{
						if(!sprintEndState){
							sprintEnd = Time.time;//track time that sprint button was released
							sprintEndState = true;
							sprintStartState = false;
							if(sprintEnd - sprintStart > sprintDelay * Time.timeScale){//if releasing sprint button after holding it down, stop sprinting
								sprintActive = false;	
							}
						}
					}
				}else{
					if(!InputComponent.sprintHold){
						sprintActive = false;
					}
				}
				
				//cancel a sprint in certain situations
				if((sprintActive && InputComponent.fireHold)//cancel sprint if fire button is pressed
				|| (sprintActive && InputComponent.reloadPress)//cancel sprint if reload button is pressed
				|| (sprintActive && InputComponent.zoomHold && WeaponBehaviorComponent.canZoom)//cancel sprint if zoom button is pressed
				|| (FPSPlayerComponent.zoomed && InputComponent.fireHold)
				|| (inputY <= 0.0f && Mathf.Abs(inputX) > 0.0f)//cancel sprint if player sprints into a wall and strafes left or right
				|| InputComponent.moveY <= 0.0f//cancel sprint if joystick is released
				|| inputY < 0.0f //cancel sprint if player moves backwards
				|| jumping
				|| swimming
				|| climbing
				//cancel sprint if player runs out of breath
				|| (limitedSprint && staminaForSprintAmt <= 0.0f)){
					cancelSprint = true;
					sprintActive = false;
				}
				
				//play catching breath sound if sprinting stamina is depleted
				if(limitedSprint){
					if(staminaForSprintAmt <= 0.0f){
						if(!breathFxState && FPSPlayerComponent.catchBreath && catchBreathSound){
							PlayAudioAtPos.instance.PlayClipAt(FPSPlayerComponent.catchBreath, mainCamTransform.position, 1.0f, 0.0f, 1.0f);
							breathFxState = true;
						}
					}
				}
				
				//reset cancelSprint var so it has to pressed again to sprint
				if(!sprintActive && cancelSprint){
					if(!InputComponent.zoomHold){
						cancelSprint = false;
					}
				}
				
				//determine if stamina has been fully depleted and set staminaDepleted to true
				//to disable sprinting until stamina fully regenerates, if sprintRegenWait is true
				if(limitedSprint && staminaForSprintAmt <= 0.0f){
					staminaDepleted = true;
				}
			
				//determine if player can run
				if(inputY > 0.0f
				&& sprintActive
				&& !crouched
				&& !prone
				&& !cancelSprint
				&& grounded){
				 	canRun = true;
					FPSPlayerComponent.zoomed = false;//cancel zooming when sprinting
					sprintStopState = true;
					if(staminaForSprintAmt > 0.0f && limitedSprint){
						staminaForSprintAmt -= Time.deltaTime;//reduce stamina when sprinting
					}
				}else{
					if(sprintStopState){
						sprintStopTime = Time.time;
						sprintStopState = false;
					}
				 	canRun = false;
					if(limitedSprint){
						if(sprintRegenWait){//determine if player should not be allowed to run unless they have full stamina
							if(!staminaDepleted){
								if(staminaForSprintAmt < staminaForSprint){
									staminaForSprintAmt += Time.deltaTime/* * 1.1f*/;//recover stamina when not sprinting (multiply this by a value to increase recover rate) 
								}
							}else{//stamina fully depleted, wait for it to regenerate before allowing player to sprint again
								if(sprintStopTime + sprintRegenTime < Time.time){
									staminaForSprintAmt = staminaForSprint;//recover full stamina when not sprinting and sprintRegenTime has elapsed
									staminaDepleted = false;
									breathFxState = false;
								}
							}
						}else{//option to allow player to run as soon as any stamina amount has regenerated 
							if(staminaForSprintAmt < staminaForSprint){
								staminaForSprintAmt += Time.deltaTime/* * 1.1f*/;//recover stamina when not sprinting (multiply this by a value to increase recover rate) 
								breathFxState = false;
							}
						}
						
					}
				}
					
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				//Player Movement Speeds
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			
				//check that player can run and set speed 
				if(canRun){
					if(speed < sprintSpeed - 0.1f){//offset speeds by 0.1f to prevent small oscillation of speed value
						speed += 12 * Time.deltaTime;//gradually accelerate to run speed
					}else if(speed > sprintSpeed + 0.1f){
						speed -= 12 * Time.deltaTime;//gradually decelerate to run speed
					}
				}else{
					if(!swimming){
						if(speed > walkSpeed + 0.1f){
							speed -= 16 * Time.deltaTime;//gradually decelerate to walk speed
						}else if(speed < walkSpeed - 0.1f){
							speed += 16 * Time.deltaTime;//gradually accelerate to walk speed
						}
					}else{
						if(speed > swimSpeed + 0.1f){
							speed -= 16 * Time.deltaTime;//gradually decelerate to swim speed
						}else if(speed < swimSpeed - 0.1f){
							speed += 16 * Time.deltaTime;//gradually accelerate to swim speed
						}		
					}
				}
						
				//check if player is zooming and set speed 
				if(zoomSpeed){
					if(zoomSpeedAmt > zoomSpeedPercentage){
						zoomSpeedAmt -= Time.deltaTime;//gradually decrease zoomSpeedAmt to zooming limit value
					}
				}else{
					if(zoomSpeedAmt < 1.0f){
						zoomSpeedAmt += Time.deltaTime;//gradually increase zoomSpeedAmt to neutral
					}
				}
				
				//check that player can crouch and set speed
				//also check midpos because player can still be under obstacle when crouch button is released 
				if(crouched || midPos < standingCamHeight){
					if(crouchSpeedAmt > crouchSpeedPercentage){
						crouchSpeedAmt -= Time.deltaTime;//gradually decrease crouchSpeedAmt to crouch limit value
					}
				}else{
					if(crouchSpeedAmt < 1.0f){
						crouchSpeedAmt += Time.deltaTime;//gradually increase crouchSpeedAmt to neutral
					}
				} 
				
				if(prone && midPos < standingCamHeight){
					if(proneSpeedAmt > proneSpeedPercentage){
						proneSpeedAmt -= Time.deltaTime;//gradually decrease proneSpeedAmt to crouch limit value
					}
				}else{
					if(proneSpeedAmt < 1.0f){
						proneSpeedAmt += Time.deltaTime;//gradually increase proneSpeedAmt to neutral
					}
				} 
				
				//limit move speed if backpedaling
				if (inputY >= 0){
					if(speedAmtY < 1.0f){
						speedAmtY += Time.deltaTime;//gradually increase speedAmtY to neutral
					}
				}else{
					if(speedAmtY > backwardSpeedPercentage){
						speedAmtY -= Time.deltaTime;//gradually decrease speedAmtY to backpedal limit value
					}
				}
				
				//allow limiting of move speed if strafing directly sideways and not diagonally
				if (inputX == 0 || inputY != 0){
					if(speedAmtX < 1.0f){
						speedAmtX += Time.deltaTime;//gradually increase speedAmtX to neutral
					}
				}else{
					if(speedAmtX > strafeSpeedPercentage){
						speedAmtX -= Time.deltaTime;//gradually decrease speedAmtX to strafe limit value
					}
				}
						
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				//Crouching & Prone
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
					
				if(Time.timeSinceLevelLoad > 0.5f){
					if(prone || crouched || FPSPlayerComponent.hitPoints < 1.0f){//also lower to crouch position if player dies
						if(crouched && !prone){
							if(FPSPlayerComponent.hitPoints < 1.0f){
								midPos = proneCamHeight;
								capsule.height = crouchingCapsuleHeight / 1.15f;
							}else{
								midPos = crouchingCamHeight;
								capsule.height = crouchingCapsuleHeight;	
							}
						}else if(prone || FPSPlayerComponent.hitPoints < 1.0f){
							midPos = proneCamHeight;
							capsule.height = crouchingCapsuleHeight / 1.15f;
						}
					}else{
						midPos = standingCamHeight;
						//capsule.height = standingCapsuleheight;
						capsule.height = Mathf.Min(capsule.height + 4 * Time.deltaTime, standingCapsuleheight);
					}
				}
					
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			//Leaning
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				
			if(allowLeaning){
				if(leanDistance > 0.0f){	
					if(Time.timeSinceLevelLoad > 0.5f 
					&& FPSPlayerComponent.hitPoints > 0.0f
					&& !sprintActive
					&& !prone
					&& grounded){
						if(InputComponent.leanLeftHold && !InputComponent.leanRightHold){//lean left
							//set up horizontal capsule cast to check if player would be leaning into an object
							if(!crouched){
								//move up bottom of lean capsule check when standing to allow player to lean over short objects like boxes 
								leanCheckPos = myTransform.position + Vector3.up * capsule.radius;
							}else{
								leanCheckPos = myTransform.position;
							}
							//define upper point of capsule (1/2 height minus radius)
							leanCheckPos2 = myTransform.position + Vector3.up * ((capsule.height / 2) - capsule.radius);
							if(!Physics.CapsuleCast (leanCheckPos, leanCheckPos2, capsule.radius * 0.8f, -myTransform.right, out capHit, leanDistance, clipMask.value)){
								leanAmt = -leanDistance;
							}else{
								leanAmt = 0.0f;
							}
						}else if(InputComponent.leanRightHold && !InputComponent.leanLeftHold){//lean right
							//set up horizontal capsule cast to check if player would be leaning into an object
							if(!crouched){
								//move up bottom of lean capsule check when standing to allow player to lean over short objects like boxes 
								leanCheckPos = myTransform.position + Vector3.up * capsule.radius;
							}else{
								leanCheckPos = myTransform.position;
							}
							//define upper point of capsule (1/2 height minus radius)
							leanCheckPos2 = myTransform.position + Vector3.up * ((capsule.height / 2) - capsule.radius);
							if(!Physics.CapsuleCast (leanCheckPos, leanCheckPos2, capsule.radius * 0.8f, myTransform.right, out capHit, leanDistance, clipMask.value)){
								leanAmt = leanDistance;
							}else{
								leanAmt = 0.0f;
							}
						}else{
							leanAmt = 0.0f;
						}
					}else{
						leanAmt = 0.0f;
					}
					
					//smooth position between leanAmt values
					leanPos = Mathf.SmoothDamp(leanPos, leanAmt, ref leanVel, 0.1f, Mathf.Infinity, Time.deltaTime);
					
						//activate or deactivate leaning collider so NPCs can detect and attack player when they are leaning
					if(Mathf.Abs(leanAmt) > 0.1f){
						leanCol.enabled = true;
					}else{
						leanCol.enabled = false;
					} 
					
					//shorten lean collider if player is crouched and leaning
					if(crouched){
						leanCol.height = 0.75f;
					}else{
						leanCol.height = 1.5f;
					}
				}	
			}
				
				
			}else{//Player is airborn////////////////////////////////////////////////////////////////////////////////////////////////////////////
				
				
				
				//keep track of the time that player lost grounding for air manipulation and moving gun while jumping
				if(airTimeState){
					airTime = Time.time;
					airTimeState = false;
				}
				
					
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				//Falling
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
					
				//subtract height we began falling from current position to get falling distance
				fallingDistance = fallStartLevel - myTransform.position.y;//this value referenced in other scripts
			
				if(!falling){
				    falling = true;			
				    //start tracking altitude (y position) for fall check
				    fallStartLevel = myTransform.position.y;
				}	
			}
				
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			//Climbing and Swimming
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			
			//make player climb up climbable surfaces, or swim up and down based on vertical mouslook angle
			if((climbing || swimming) && !jumping){//climbing var is managed by a ladder script attatched to a trigger that is placed near a ladder
				if(InputComponent.moveY > 0.1f){//only climb if player is moving forward
					if(climbing){//make player climb up or down based on the pitch of the main camera (check mouselook script pitch)
						verticalSpeedAmt = 0.2f + (climbSpeed/4 * (SmoothMouseLookComponent.inputY / 48));
						verticalSpeedAmt = Mathf.Clamp(verticalSpeedAmt, -climbSpeed, climbSpeed);//limit vertical speed to climb speed
					}else{
						if(swimming){
							verticalSpeedAmt = Mathf.Clamp(verticalSpeedAmt, -swimSpeed, swimSpeed);//limit vertical speed to swim speed
							if(!belowWater){
								if(SmoothMouseLookComponent.inputY < -55){//only swim downwards with look angle if player is treading water and looking down
									verticalSpeedAmt = 1 + (swimSpeed * -Mathf.Abs((SmoothMouseLookComponent.inputY / 48)));
								}else{
									verticalSpeedAmt = 0.0f;	
								}
							}else{
								if(SmoothMouseLookComponent.inputY > -15){//normal upwards swimming if not looking down too far
									verticalSpeedAmt = 1 + (swimSpeed * (SmoothMouseLookComponent.inputY / 48));
								}else{//prevent player from dragging against bottom if looking downwards and moving
									if(!Physics.CapsuleCast (p1, p2, capsule.radius * 0.9f, -myTransform.up, out capHit, capsuleCastHeight, clipMask.value)){//detect if player is close to bottom with a capsuleCast
										verticalSpeedAmt = 1 + (swimSpeed * (SmoothMouseLookComponent.inputY / 48));
									}else{
										verticalSpeedAmt = 0.0f;//prevent downward movement if player is at the bottom floor of water zone	
									}
								}
							}
						}
					}
					
					//apply climbing or swimming speed to the player's rigidbody velocity
					RigidbodyComponent.velocity = new Vector3(velocity.x, verticalSpeedAmt, velocity.z);
					
				}else{
					//if not moving forward, do not add extra upward velocity, but allow the player to move off the ladder
					RigidbodyComponent.velocity = new Vector3(velocity.x, 0, velocity.z);
				}
			}
			
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			//Holding Breath
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			
			if(holdingBreath){
				//determine if player will gasp for air when surfacing
				if(Time.time - diveStartTime > holdBreathDuration / 1.5f){
					drowning = true;	
				}
				//determine if player is drowning
				if(Time.time - diveStartTime > holdBreathDuration){
					if(drownStartTime < Time.time){
						FPSPlayerComponent.ApplyDamage(drownDamage); 
						drownStartTime = Time.time + 1.75f;
					}
				}
				
			}else{
				if(drowning){//play gasping sound if player needed air when surfacing
					PlayAudioAtPos.instance.PlayClipAt(FPSPlayerComponent.gasp, mainCamTransform.position, 0.75f, 0.0f, 1.0f);
					drowning = false;
				}
			}
				
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			//Player Ground Check
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
			//cast capsule shape down to see if player is about to hit anything or is resting on the ground
		    if (Physics.CapsuleCast (p1, p2, capsule.radius, -myTransform.up, out capHit, capsuleCastHeight, clipMask.value) 
			|| climbing 
			|| (swimming)){
			
		        grounded = true;
				
				if(!climbing){
					if(!inWater && !swimming){//do not play landing sound if player is in water
						//determine what kind of surface the player is landing on and set landfx to that surface's landing sound effect
						switch(capHit.collider.gameObject.tag){
						case "Water":
							if(FootstepsComponent.waterLand){
								landfx = FootstepsComponent.waterLand;
							}
							break;
						case "Dirt":
							if( FootstepsComponent.dirtLand){
								landfx = FootstepsComponent.dirtLand;
							}
							break;
						case "Wood":
							if(FootstepsComponent.woodLand){
								landfx = FootstepsComponent.woodLand;
							}
							break;
						case "Metal":
							if(FootstepsComponent.metalLand){
								landfx = FootstepsComponent.metalLand;
							}
							break;
						case "Stone":
							if(FootstepsComponent.stoneLand){
								landfx = FootstepsComponent.stoneLand;
							}
							break;
						default:
							if(FootstepsComponent.dirtLand){
								landfx = FootstepsComponent.dirtLand;
							}
							break;	
						}
					}
				}else{
					if(FootstepsComponent.dirtLand){
						landfx = FootstepsComponent.dirtLand;
					}
				}
				
		    }else{
		    	grounded = false;
		    }
			
			//check that angle of the normal directly below the capsule center point is less than the movement slope limit 
			if (Physics.Raycast(myTransform.position, -myTransform.up, out rayHit, rayCastHeight, clipMask.value)) {
				if(Vector3.Angle ( rayHit.normal, Vector3.up ) > slopeLimit && !inWater){
					//perform second raycast down from movement direction to compare with center raycast and determine if player is moving uphill or downhill
					RaycastHit rayHit2;
					Physics.Raycast(myTransform.position + (moveDirection.normalized * capsule.radius), -myTransform.up, out rayHit2, rayCastHeight, clipMask.value);
					if(rayHit2.point.y - rayHit.point.y >= 0.0f){//player moving uphill
						rayTooSteep = true;	
					}else{//player moving downhill
						rayTooSteep = false;		
					}
				}else{
					rayTooSteep = false;	
				}
				//pass the material/surface type tag player is on to the Footsteps.cs script
				FootstepsComponent.materialType = rayHit.collider.gameObject.tag;
				
			}
				
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			//Player Velocity
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		    
			//limit speed if strafing diagonally
			limitStrafeSpeed = (Mathf.Abs(inputX) > 0.5f && Mathf.Abs(inputY) > 0.5f)? diagonalStrafeAmt : 1.0f;
			
			//apply velocity to player rigidbody and allow a small amount of air manipulation
			//to allow jumping up on obstacles when jumping from stationary position with no forward velocity
			if((grounded || climbing || swimming || ((airTime + 0.3f) > Time.time)) && FPSPlayerComponent.hitPoints > 1.0f && !FPSPlayerComponent.restarting){
	
				// We are grounded, so recalculate movedirection directly from axes	
				moveDirection = new Vector3(inputXSmoothed * limitStrafeSpeed, 0.0f, inputYSmoothed * limitStrafeSpeed);
				//realign moveDirection vector to world space
				moveDirection = myTransform.TransformDirection(moveDirection);
				//apply speed limits to moveDirection vector
				moveDirection = moveDirection * speed * speedAmtX * speedAmtY * crouchSpeedAmt * proneSpeedAmt * zoomSpeedAmt;

				//Check both capsule center point and capsule base slope angles to determine if the slope is too high to climb.
				//If so, bypass player control and apply some extra downward velocity to help capsule return to more level ground.
				if(!capsuleTooSteep || climbing || swimming || (capsuleTooSteep && !rayTooSteep)){
			
					//apply a force that attempts to reach target velocity
					velocityChange = moveDirection - velocity;
					//limit max speed
					velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
					velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
				
					//apply ladder climbing speed to velocityChange vector and set y velocity to zero if not climbing ladder
					if(climbing){
						if(InputComponent.moveY > 0.1f){//move player up climbable surface if pressing forward button
							velocityChange.y = verticalSpeedAmt;
						}else if(InputComponent.jumpHold){//move player up climbable surface if pressing jump button
							inputY = 1;//to cycle bobbing effects
							velocityChange.y = climbSpeed * 0.75f;
						}else if(InputComponent.crouchHold){//move player down climbable surface if pressing crouch button
							inputY = -1;//to cycle bobbing effects
							velocityChange.y = -climbSpeed * 0.75f;
						}else{
							velocityChange.y = 0;
						}
						
					}else{
						velocityChange.y = 0;
					}
					
					//finally, add movement velocity to player rigidbody velocity
					RigidbodyComponent.AddForce(velocityChange, ForceMode.VelocityChange);
					
				//}else{//is gravity enough, with low friction physics materials?
					//If slope is too high below both the center and base contact point of capsule, apply some downward velocity to help
					//the capsule fall to more level ground.
					//RigidbodyComponent.AddForce(new Vector3 (0, -0.1f, 0), ForceMode.VelocityChange);
				}
			}else{
				if(FPSPlayerComponent.hitPoints < 1.0f || FPSPlayerComponent.restarting){		
					RigidbodyComponent.freezeRotation = false;//allow player's rigidbody to be pushed by forces and explosions after death
					SmoothMouseLookComponent.enabled = false;//disable mouselook on player death
				}
			}
			
			if(!climbing){
				if(!swimming){
					//apply gravity
					RigidbodyComponent.AddForce(new Vector3 (0, Physics.gravity.y * RigidbodyComponent.mass, 0));
			    	RigidbodyComponent.useGravity = true;
				}else{
					if(swimStartTime + 0.2f > Time.time){//make player sink under surface for a short time if they jumped in deep water 
						//dont make player sink if they are close to bottom
						if(landStartTime + 0.3f > Time.time){//make sure that player doesn't try to sink into the ground if wading into water
							if(!Physics.CapsuleCast (p1, p2, capsule.radius * 0.9f, -myTransform.up, out capHit, capsuleCastHeight, clipMask.value)){
								RigidbodyComponent.AddForce(new Vector3 (0, -6.0f, 0), ForceMode.VelocityChange);//make player sink into water after jump
							}
						}
					}else{	
						//make player rise to water surface if they hold the jump button
						if(InputComponent.jumpHold){
							
							if(belowWater){
								swimmingVerticalSpeed = Mathf.MoveTowards(swimmingVerticalSpeed, 3.0f, Time.deltaTime * 4);
								if(holdingBreath){
									canWaterJump = false;//don't also jump if player just surfaced by holding jump button
								}
							}else{
								swimmingVerticalSpeed = 0.0f;	
							}
						//make player dive downwards if they hold the crouch button
						}else if(InputComponent.crouchHold){
							
							swimmingVerticalSpeed = Mathf.MoveTowards(swimmingVerticalSpeed, -3.0f, Time.deltaTime * 4);
							
						}else{
							//make player sink slowly when underwater due to the weight of their gear
							if(belowWater){
								swimmingVerticalSpeed = Mathf.MoveTowards(swimmingVerticalSpeed, -0.2f, Time.deltaTime * 4);
							}else{
								swimmingVerticalSpeed = 0.0f;	
							}
							
						}
						//allow jumping when treading water if player has released the jump button after surfacing 
						//by holding jump button down to prevent player from surfacing and immediately jumping
						if(!belowWater && !InputComponent.jumpHold){
							canWaterJump = true;	
						}
						//apply the vertical swimming speed to the player rigidbody
						RigidbodyComponent.AddForce(new Vector3 (0, swimmingVerticalSpeed, 0), ForceMode.VelocityChange);
						
					}
					RigidbodyComponent.useGravity = false;//don't use gravity when swimming	
				}
			}else{
				RigidbodyComponent.useGravity = false;
			}
		}
		
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Rigidbody Collisions
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
	void OnCollisionExit ( Collision col  ){
		
		if(col.gameObject.layer == 15){
			//unparent if we are no longer standing on a platform or elevator
			if(/*WorldRecenterComponent.removePrefabRoot &&*/ FPSPlayerComponent.removePrefabRoot){
				//prefab root was removed, so make player object's parent null when not on elevator or platform
				myTransform.parent = null;
			}else if(/*!WorldRecenterComponent.removePrefabRoot &&*/ !FPSPlayerComponent.removePrefabRoot){
				//prefab root exists, so make player object's parent the mainObj, or !!!FPS Main object
				myTransform.parent = mainObj.transform;	
			}
			playerParented = false;
		}
		//return parentState to false so we may check for collisions with elevators or platforms again
		parentState = false;
		capsuleTooSteep = false;	
		inWater = false;
		lowpos = myTransform.position.y;
	}
	
	void OnCollisionStay (Collision col){
	    //define a height of about a fourth of the capsule height to check for collisions with platforms
		float maximumHeight = (capsule.bounds.min.y + capsule.radius);
		//check the collision points within our predefined height range  
		foreach(ContactPoint c in col.contacts){
			if(c.point.y < maximumHeight){
				//check that we want to collide with this object (check for "Moving Platforms" layer) and that its surface is not too steep 
				if(!parentState && col.gameObject.layer == 15 && Vector3.Angle ( c.normal, Vector3.up ) < 70){
					//set player object parent to platform transform to inherit it's movement
					myTransform.parent = col.transform;
					playerParented = true;
					parentState = true;//only set parent once to prevent rapid parenting and de-parenting that breaks functionality
				}
				
				//if this is the lowest collision point on the capsule, set lowpos to its y coord
				if(c.point.y < lowpos){
					lowpos = c.point.y;	
				}
				
				//check that angle of the surface that the capsule base is touching is less than the movement slope limit  
				if(Vector3.Angle ( c.normal, Vector3.up ) > slopeLimit 
				&& !inWater 
				//if lowest collision point is higher up on the capsule, let player move because they might be in a small, concave space
				&& lowpos < capsule.bounds.min.y - 0.2f){
					capsuleTooSteep = true;	
				}else{
					capsuleTooSteep = false;	
				}
			}
		}
		
		
	}
	
	void CalculateFallingDamage ( float fallDistance  ){
		if(fallDamageMultiplier > 0.0f){
	    	FPSPlayerComponent.ApplyDamage(fallDistance * fallDamageMultiplier);   
		}
	}
}