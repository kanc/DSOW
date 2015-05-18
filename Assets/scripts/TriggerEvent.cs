using UnityEngine;
using System.Collections;
using GlobalData;

public class TriggerEvent : MonoBehaviour {

    public GlobalData.InteractuableEvents EventToTrigger;
    public bool OnlyOnce = false;

    private bool triggered = false;

    // Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    void OnTriggerEnter(Collider other)
    {
        if (OnlyOnce && triggered) return;

        GlobalData.GameEventsCall.TriggerEvent(EventToTrigger);
        triggered = true;
        
    }


}
