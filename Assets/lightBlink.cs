using UnityEngine;
using System.Collections;

public class lightBlink : MonoBehaviour {

    private float number;

    // Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () 
    {
        number = Random.value;

        if (number <= 0.8)
            GetComponent<Light>().enabled = false;
        else
            GetComponent<Light>().enabled = true;
	}
}
