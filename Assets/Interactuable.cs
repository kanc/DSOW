using UnityEngine;
using System.Collections;
using GlobalData;

public class Interactuable : MonoBehaviour {
    
    public GameObject           MessageText;
    public float                LookingTime;
    public InteractuableEvents  Event;

    private TimeBar m_cmpTimer;

    // Use this for initialization
    void Start()
    {
        m_cmpTimer = GetComponent<TimeBar>();
        MessageText.SetActive(false);

    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider other)
    {
        MessageText.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        MessageText.SetActive(false);
    }

    void OnTriggerStay(Collider other)
    {        
        CheckLookingAtObject();

        if (m_cmpTimer.CountdownTime == 0.0f)
        {
            //DoorToOpen.GetComponent<OpenDoor>().ToogleState();
            m_cmpTimer.StopCount();
        }
    }

    private void CheckLookingAtObject()
    {
        RaycastHit hit;

        if (GlobalData.CameraUtil.IsLookingAtInteract(gameObject, out hit))
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
}
