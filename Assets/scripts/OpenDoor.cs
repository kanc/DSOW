﻿using UnityEngine;
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

    private float       m_fElapsed = 0;
    private eDoorState  m_eCurrentState;
    private eDoorState  m_eTargetState;
    private Quaternion  m_targetRotation;

    // Use this for initialization
	void Start () {

        m_eCurrentState = InitialState;

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

    void OnTriggerStay(Collider other)
    {
        if (m_eCurrentState != eDoorState.eMoving)
        {
            CheckLookOpener();

            if (m_fElapsed >= LookingTime)
            {                
                ToogleState();
                m_fElapsed = 0;
            }
        }
    }

    private void CheckLookOpener()
    {
        RaycastHit hit;

        if (GlobalData.CameraUtil.IsLookingAtInteract(Opener, out hit))        
            m_fElapsed += Time.deltaTime;
        else
            m_fElapsed = 0;

        
    }

    private void ToogleState()
    {
        switch (m_eCurrentState)
        {
            case eDoorState.eClosed :             
                m_targetRotation = DoorPivot.transform.rotation * Quaternion.AngleAxis(OpenningAngle, Vector3.up);
                m_eTargetState = eDoorState.eOpened;
                break;

            case eDoorState.eOpened :
                m_targetRotation = DoorPivot.transform.rotation * Quaternion.AngleAxis(-OpenningAngle, Vector3.up);
                m_eTargetState = eDoorState.eClosed;
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