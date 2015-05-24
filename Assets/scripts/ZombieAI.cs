using UnityEngine;
using System.Collections;

public class ZombieAI : MonoBehaviour {

	// Use this for initialization
	void Start () {


        GetComponent<Animator>().SetFloat("WalkType", Random.value);
        GetComponent<Animator>().SetFloat("Speed", 10);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
