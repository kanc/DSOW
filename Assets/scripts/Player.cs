using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public float Health = 100f;
    public Animator AnimControl;
    public GameObject PlayerObject;
    public Camera MainCamera;
    public Vector3 CharOffset;
    private bool m_bAccessCard = true;

    // Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void FixedUpdate () {

        Debug.DrawLine(MainCamera.transform.position, MainCamera.transform.forward * 1000);

        //shoot
        if (Input.GetMouseButtonDown(0))
        {
            Ray myRay = new Ray(MainCamera.transform.position, MainCamera.transform.forward);
            RaycastHit hit;
            int layerMask = 1 << GlobalData.Constants.ZOMBIE_COLLIDER_LAYER;

            if (Physics.Raycast(myRay, out hit, 1000, layerMask))
            {                
                if (hit.collider.gameObject.GetComponent<ZombieAI>() != null)
                {                    
                    hit.collider.gameObject.GetComponent<ZombieAI>().DamageDone(30, hit.point);
                }
            }            
        }

        //correct body position (animations despla
        //PlayerObject.transform.localPosition = CharOffset;

        AnimControl.SetFloat("SpeedH", Input.GetAxis("Horizontal"));
        AnimControl.SetFloat("SpeedV", Input.GetAxis("Vertical"));
	
	}

    public bool HasAccessCard()
    {
        return m_bAccessCard;    
    }

    public void SetAccessCard(bool getted)
    {
        m_bAccessCard = getted;
    }

    public void DoDamage(float damage)
    {
        Health -= damage;

        Debug.Log(Health);
    }
}
