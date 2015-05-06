using UnityEngine;
using System.Collections;

public class DoorAccess : MonoBehaviour {

    public GameObject   DoorToOpen;
    public GameObject   LookAtObject;
    public Material     ClosedMaterial;
    public Material     OpenedMaterial;
    public GameObject   MessageText;
    public float        LookingTime;

    private TimeBar m_cmpTimer;

	// Use this for initialization
	void Start () {

        m_cmpTimer = GetComponent<TimeBar>();
        MessageText.SetActive(false);
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerExit(Collider other)
    {
        MessageText.SetActive(false);
    }

    void OnTriggerStay(Collider other)
    {

        if (DoorToOpen.GetComponent<OpenDoor>().GetDoorState() == OpenDoor.eDoorState.eMoving) return;
                
        CheckLookOpener();

        if (m_cmpTimer.CountdownTime == 0.0f)
        {
            if (!CheckAccess())
            {
                MessageText.SetActive(true);
                return;
            }

            if (DoorToOpen.GetComponent<OpenDoor>().GetDoorState() == OpenDoor.eDoorState.eOpened)
            {
                transform.GetChild(0).gameObject.GetComponent<Renderer>().sharedMaterial = ClosedMaterial;
            }
            else
            {
                transform.GetChild(0).gameObject.GetComponent<Renderer>().sharedMaterial = OpenedMaterial;
            }

            DoorToOpen.GetComponent<OpenDoor>().ToogleState();
            m_cmpTimer.StopCount();
        }
        
    }

    private bool CheckAccess()
    {
        Player cmpPlayer = (Player)GameObject.FindObjectOfType(typeof(Player));

        if (cmpPlayer == null) return false;

        return cmpPlayer.HasAccessCard();
    }

    private void CheckLookOpener()
    {
        RaycastHit hit;

        if (GlobalData.CameraUtil.IsLookingAtInteract(LookAtObject, out hit))
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
