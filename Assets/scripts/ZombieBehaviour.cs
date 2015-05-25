using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GlobalData;

public class ZombieBehaviour : MonoBehaviour {
    
    private Vector3 targetPoint;
    private ZombieState state;

    // Use this for initialization
	void Start () {
        state = ZombieState.Rising;        
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void MoveForwardAndDisable(float distance)
    {
        List<Vector3> path = new List<Vector3>();
        Vector3 destiny = transform.position + (transform.forward * distance);

        path.Add(transform.position);
        path.Add(destiny);

        GetComponent<PathFollower>().SetPath(path);

    }
}
