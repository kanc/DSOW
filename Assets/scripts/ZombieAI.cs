using UnityEngine;
using System.Collections;

public class ZombieAI : MonoBehaviour {

    public float    Speed;
    public float    Health;
    public bool     RandomSpeed;
    public float    AttackRangeSquare = 4;
    public float    TimeToDestroyCorpse = 2;
    public GameObject HitBloodSplash;

    private float       CloseOffset;
    private GameObject  m_Player;
    private Animator    m_Animator;    
    private Vector3     m_vGroundPos;
    private Vector3     m_vPlayerGround;
    private float       WalkType;
    private float       m_fCurrentSpeed;
    private float       m_fElapsedTime;
    private GlobalData.ZombieState m_eState;
    private bool m_bHit = false;

    // Use this for initialization
	void Start () {

        //get reference to animator component
        m_Animator = GetComponent<Animator>();

        //get player reference
        Player cmpPlayer = (Player)GameObject.FindObjectOfType(typeof(Player));        
        if (cmpPlayer != null)
        {
            m_Player = cmpPlayer.gameObject;
        }
        else
        {
            m_Player = null;
        }

        //get ground position
        GetGroundPosition();

        //get distance close to player to stop movement
        CloseOffset = AttackRangeSquare - 1;

        //asign random value for movement animations
        WalkType = Random.value;

        //get zombie speed
        Speed = (RandomSpeed) ? Random.Range(0.2f,2.0f) : Speed;

        //initial state
        m_eState = GlobalData.ZombieState.Rising;        
        
	}
	
	void FixedUpdate () 
    {
        switch (m_eState)
        {
            case GlobalData.ZombieState.Rising: OnUpdateRising();       break;
            case GlobalData.ZombieState.Moving: OnUpdateMoving();       break;
            case GlobalData.ZombieState.Attack: OnUpdateAttacking();    break;
            case GlobalData.ZombieState.Death: OnUpdateDeath(); break;            
        }

        //chech when it animation (state) is finished
        if (m_bHit)
        {
            if (!m_Animator.GetNextAnimatorStateInfo(0).IsName("Hit"))
            {
                m_bHit = false;                
            }
        }

        //zombie is death...
        if (Health <= 0 && m_eState != GlobalData.ZombieState.Death)
        {
            Debug.Log("cambiando a estado muerte");
            ChangeState(GlobalData.ZombieState.Death);
        }
	}

    public void DamageDone(float damage, Vector3 hitPosition)
    {

        //we dont want to damage zombie when is rising or is already dead
        if (m_eState != GlobalData.ZombieState.Death && m_eState != GlobalData.ZombieState.Rising)
        {
            //trigger hit animation state
            m_Animator.SetTrigger(GlobalData.Constants.ZOMBIE_HIT_PARAM);
            Health -= damage;
            m_bHit = true;        
        }

        //instantiate blood effect
        if (HitBloodSplash != null)
        {
            Instantiate(HitBloodSplash, hitPosition, Quaternion.identity);
        } 

    }

    void ChangeState(GlobalData.ZombieState newState)
    {
        //manage state transitions
        switch (m_eState)
        {            
            case GlobalData.ZombieState.Attack: OnExitAttacking(); break;
        }

        m_eState = newState;

        switch (m_eState)
        {
            case GlobalData.ZombieState.Moving: OnEnterMoving();    break;
            case GlobalData.ZombieState.Attack: OnEnterAttacking(); break;
            case GlobalData.ZombieState.Death:  OnEnterDeath();     break;
        }

    }

    void OnUpdateRising()
    {
        AnimatorStateInfo animState = m_Animator.GetCurrentAnimatorStateInfo(0);

        //rising is initial state. When state finish go to move
        if (!animState.IsName("Rising"))
        {
            ChangeState(GlobalData.ZombieState.Moving);
        }            
    }
    
    void OnEnterMoving()
    {        
        //send zombie speed to animator controller and walking type
        m_fCurrentSpeed = 0;
        m_Animator.SetFloat(GlobalData.Constants.ZOMBIE_SPEED_PARAM, m_fCurrentSpeed);
        m_Animator.SetFloat(GlobalData.Constants.ZOMBIE_WALKTYPE_PARAM, WalkType);
    }

    void OnUpdateMoving()
    {
        //if zombie has been hit dont move it
        if (m_bHit) return;

        //increment speed over time until reach zombie speed
        if (m_fCurrentSpeed < Speed)
        {            
            m_fCurrentSpeed += Time.deltaTime;
            m_Animator.SetFloat(GlobalData.Constants.ZOMBIE_SPEED_PARAM, m_fCurrentSpeed);
        }
        
        //get player position
        m_vPlayerGround = GetPlayerGroundPosition();

        //check distance
        float distance = GetSquareDistanceToPlayer();

        MoveToPlayer(distance);

        //at attack range, change state to Attack
        if (distance < AttackRangeSquare)
            ChangeState(GlobalData.ZombieState.Attack);
    }

    void OnUpdateAttacking()
    {
        //get player position
        m_vPlayerGround = GetPlayerGroundPosition();

        //check distance
        float distance = GetSquareDistanceToPlayer();

        MoveToPlayer(distance);

        //at attack range, change state
        if (distance > AttackRangeSquare)
            ChangeState(GlobalData.ZombieState.Moving);
    }

    void OnEnterAttacking()
    {
        //send attack switch to animator controller
        m_Animator.SetBool(GlobalData.Constants.ZOMBIE_ATTACK_PARAM, true);
    }

    void OnExitAttacking()
    {
        //send attack switch to animator controller
        m_Animator.SetBool(GlobalData.Constants.ZOMBIE_ATTACK_PARAM, false);
    }

    void OnEnterDeath()
    {
        m_Animator.SetBool(GlobalData.Constants.ZOMBIE_ALIVE_PARAM, false);
        m_Animator.SetTrigger(GlobalData.Constants.ZOMBIE_DEATH_PARAM);
        m_fElapsedTime = 0;
    }

    void OnUpdateDeath()
    {
        Debug.Log(m_fElapsedTime);
        m_fElapsedTime += Time.deltaTime;

        if (m_fElapsedTime > TimeToDestroyCorpse)
        {
            GameObject.Destroy(gameObject);
            Debug.Log("cuerpo destruido");
        }
    }

    Vector3 GetPlayerGroundPosition()
    {
        if (m_Player !=null) 
            return  new Vector3(m_Player.transform.position.x, m_vGroundPos.y, m_Player.transform.position.z);
        else
            return new Vector3();
    }

    private float GetSquareDistanceToPlayer()
    {                
        if (m_Player != null )
        {
            return Vector3.SqrMagnitude (m_vPlayerGround - transform.position);
        }
        else
        {
            return Mathf.Infinity;
        }
        
    }

    private void MoveToPlayer(float distanceToPlayer)
    {
        //get direction to current target
        Vector3 direction = m_vPlayerGround - transform.position;
        //get velocity
        Vector3 velocity = direction.normalized * Speed * Time.deltaTime;

        //only move when zombie is not in close radius
        if (distanceToPlayer > CloseOffset)
        {
            //move it 
            transform.position += velocity;
            transform.position = new Vector3(transform.position.x, m_vGroundPos.y, transform.position.z);
        }

        //face the point
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Speed * 4 * Time.deltaTime);

    }

    private void GetGroundPosition()
    {
        Ray ray = new Ray(transform.position, transform.up * -1);            
        int layerMask = 1 << GlobalData.Constants.GROUND_COLLIDER_LAYER;
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000, layerMask))
        {
            m_vGroundPos = hit.point + new Vector3(0,0.01f,0);
        }

    }

    
}
