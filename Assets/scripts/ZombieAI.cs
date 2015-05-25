using UnityEngine;
using System.Collections;

public class ZombieAI : MonoBehaviour {

    public float Speed;
    public float InitialHealth;
    public bool RandomSpeed;
    public float AttackRangeSquare = 4;

    private float CloseOffset;
    private GameObject m_Player;
    private Animator m_Animator;
    private GlobalData.ZombieState m_eState;
    private Vector3 m_vGroundPos;
    private Vector3 m_vPlayerGround;
    private float WalkType;
    private float m_fCurrentSpeed;

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
        Speed = (RandomSpeed) ? Random.value : Speed;

        //initial state
        m_eState = GlobalData.ZombieState.Rising;        
        
	}
	
	// Update is called once per frame
	void FixedUpdate () 
    {
        switch (m_eState)
        {
            case GlobalData.ZombieState.Rising: OnUpdateRising(); break;
            case GlobalData.ZombieState.Moving: OnUpdateMoving(); break;
            case GlobalData.ZombieState.Attack: OnUpdateAttacking(); break;
            
        }	
	}

    void ChangeState(GlobalData.ZombieState newState)
    {
        switch (m_eState)
        {            
            case GlobalData.ZombieState.Attack: OnExitAttacking(); break;
        }

        m_eState = newState;

        switch (m_eState)
        {
            case GlobalData.ZombieState.Moving: OnEnterMoving(); break;
            case GlobalData.ZombieState.Attack: OnEnterAttacking(); break;
            case GlobalData.ZombieState.Death: OnEnterDeath(); break;
        }

    }

    void OnUpdateRising()
    {
        AnimatorStateInfo animState = m_Animator.GetCurrentAnimatorStateInfo(0);

        if (!animState.IsName("Rising"))
        {
            ChangeState(GlobalData.ZombieState.Moving);
        }
            
    }
    
    void OnEnterMoving()
    {        
        //send zombie speed to animator controller and walking type
        m_fCurrentSpeed = 0;
        m_Animator.SetFloat("Speed", m_fCurrentSpeed);
        m_Animator.SetFloat("WalkType", WalkType);
    }

    void OnUpdateMoving()
    {
        //increment speed over time until reach zombie speed
        if (m_fCurrentSpeed < Speed)
        {            
            m_fCurrentSpeed += Time.deltaTime;
            m_Animator.SetFloat("Speed", m_fCurrentSpeed);
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
        m_Animator.SetBool("Attacking", true);
    }

    void OnExitAttacking()
    {
        //send attack switch to animator controller
        m_Animator.SetBool("Attacking", false);
    }

    void OnEnterDeath()
    {
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

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("algo entro");

        if (other.gameObject.GetComponent<Player>() != null)
            Debug.Log("daño!");
        
    }

    
}
