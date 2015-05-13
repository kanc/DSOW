using UnityEngine;
using System.Collections;
using GlobalData;

public class Interactuable : MonoBehaviour {
    
    public GameObject           MessageText;
    public float                LookingTime;
    public GameObject           VisibleTrigger;
    public InteractuableEvents  ObjectEvent;

    private bool m_bEventTriggered = false;
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
        if (other.gameObject.GetComponent<Player>() != null)
            MessageText.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        MessageText.SetActive(false);
        m_bEventTriggered = false;
        gameObject.GetComponent<Renderer>().sharedMaterial.color = Color.white;
    }

    void OnTriggerStay(Collider other)
    {        
        CheckLookingAtObject();

        if (m_cmpTimer.CountdownTime == 0.0f)
        {
            GlobalData.GameEventsCall.TriggerEvent(ObjectEvent);
            m_bEventTriggered = true;
            gameObject.GetComponent<Renderer>().sharedMaterial.color = Color.white;
            m_cmpTimer.StopCount();
        }
    }

    private void CheckLookingAtObject()
    {
        RaycastHit hit;

        if (m_bEventTriggered == true) return;

        if (GlobalData.CameraUtil.IsLookingAtInteract(VisibleTrigger, out hit))
        {
            gameObject.GetComponent<Renderer>().sharedMaterial.color = Color.green;            
            
            //if time bar is stoped
            if (m_cmpTimer.CountdownTime == -1.0f)
            {
                m_cmpTimer.TotalTime = LookingTime;
                m_cmpTimer.IniCount();
            }
        }
        else
        {
            gameObject.GetComponent<Renderer>().sharedMaterial.color = Color.white;

            m_cmpTimer.StopCount();
        }

    }
}
