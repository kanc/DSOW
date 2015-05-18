using UnityEngine;
using System.Collections;
using GlobalData;

public class OpenDoor : MonoBehaviour {

    public enum eDoorState
    {
        eOpened,
        eMoving,
        eClosed
    };

    public GameObject   Opener;
    public GameObject   DoorPivot;
    public float        LookingTime;
    public float        OpenningAngle = 90;
    public float        Speed = 5;
    public eDoorState   InitialState = eDoorState.eClosed;
    public GameObject   TextDoor;
    
    private eDoorState  m_eCurrentState;
    private eDoorState  m_eTargetState;
    private Quaternion  m_targetRotation;
    private TimeBar     m_cmpTimer;

    public eDoorState GetDoorState() { return m_eCurrentState; }

    // Use this for initialization
	void Start () {

        m_eCurrentState = InitialState;

        m_cmpTimer = Opener.GetComponent<TimeBar>();

        if (TextDoor != null)
            TextDoor.SetActive(false);

        if (m_eCurrentState == eDoorState.eOpened)
        {
            DoorPivot.transform.Rotate(DoorPivot.transform.up, OpenningAngle);
        }
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (m_eCurrentState == eDoorState.eMoving)
        {
            OnMoving();
        }
	
	}

    void OnTriggerEnter(Collider other)
    {
        if (TextDoor != null && other.gameObject.GetComponent<Player>() != null)
            TextDoor.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (TextDoor != null)
            TextDoor.SetActive(false);
    }



    void OnTriggerStay(Collider other)
    {
        if (m_eCurrentState != eDoorState.eMoving)
        {
            CheckLookOpener();
            
            if (m_cmpTimer.CountdownTime == 0.0f)
            {
                ToogleState();
                m_cmpTimer.StopCount();                
            }
        }
    }

    private void CheckLookOpener()
    {
        RaycastHit hit;

        if (GlobalData.CameraUtil.IsLookingAtInteract(Opener, out hit))
        {
            //if time bar is stoped
            if (m_cmpTimer.CountdownTime == -1.0f)
            {
                m_cmpTimer.TotalTime = LookingTime;
                m_cmpTimer.IniCount();
            }
        }
        else
        {
            m_cmpTimer.StopCount();
        }
        
    }

    public void ToogleState()
    {
        switch (m_eCurrentState)
        {
            case eDoorState.eClosed :             
                m_targetRotation = DoorPivot.transform.rotation * Quaternion.AngleAxis(OpenningAngle, Vector3.up);
                m_eTargetState = eDoorState.eOpened;
                GlobalData.GameEventsCall.TriggerEvent(InteractuableEvents.OpenDoor);
                break;

            case eDoorState.eOpened :
                m_targetRotation = DoorPivot.transform.rotation * Quaternion.AngleAxis(-OpenningAngle, Vector3.up);
                m_eTargetState = eDoorState.eClosed;
                GlobalData.GameEventsCall.TriggerEvent(InteractuableEvents.CloseDoor);
                break;
        }

        m_eCurrentState = eDoorState.eMoving;
        
    }

    private void OnMoving()
    {
        DoorPivot.transform.rotation = Quaternion.Slerp(DoorPivot.transform.rotation, m_targetRotation, Time.deltaTime * Speed);

        if (DoorPivot.transform.rotation == m_targetRotation)
        {
            m_eCurrentState = m_eTargetState;
        }
    }

}
