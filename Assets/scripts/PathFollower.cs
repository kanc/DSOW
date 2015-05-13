using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathFollower : MonoBehaviour {

    public float            Speed;
    public float            PointRadius = 0.5f;
    public List<Vector3>    pathPoints;
    public bool             FreezeX = false;
    public bool             FreezeY = false;
    public bool             FreezeZ = false;
        
    private int             m_iCurrentIndex;
    private float           m_fSquareRad;

    // Use this for initialization
	void Start () 
    {
        //get square distance point radius to avoid operations on checks
        m_fSquareRad = PointRadius * PointRadius;

        //get initial path index
        GetInitalIndex();
	
	}

    private void FreezePathPointAxis()
    {
        for (int i = 0; i < pathPoints.Count; ++i)
        {
            Vector3 newPoint = pathPoints[i];

            if (FreezeX)
            {
               newPoint.x = transform.position.x;
            }
            if (FreezeY)
            {
                newPoint.y = transform.position.y;
            }
            if (FreezeZ)
            {
                newPoint.z = transform.position.z;
            }

            pathPoints[i] = newPoint;
        }
    }

    private void GetInitalIndex()
    {
        //get initial point
        if (pathPoints.Count > 0)
        {            
            m_iCurrentIndex = 0;
            FreezePathPointAxis();
        }
        else
        {
            m_iCurrentIndex = -1;
        }
    }

    public void SetPath(List<Vector3> path)
    {
        pathPoints = path;
        GetInitalIndex();
    }
    
	
	// Update is called once per frame
	void Update () 
    {
        if (m_iCurrentIndex >= 0)
        {
            MoveToNextPoint();
            ChangeNextPointTarget();
        }
	}

    private void MoveToNextPoint()
    {
        //get direction to current target
        Vector3 direction = pathPoints[m_iCurrentIndex] - transform.position;
        //get velocity
        Vector3 velocity = direction.normalized * Speed * Time.deltaTime;

        //move it 
        transform.position += velocity;
        //face the point
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 4 * Time.deltaTime);        

    }

    private void ChangeNextPointTarget()
    {
        //vector to target
        Vector3 toTarget = pathPoints[m_iCurrentIndex] - transform.position;

        //inside radius to change waypoint
        if (toTarget.sqrMagnitude <= m_fSquareRad)
        {
            //current point is the last one
            if (m_iCurrentIndex == pathPoints.Count - 1)
            {
                m_iCurrentIndex = -1;
            }
            else
            {
                //get the next path point
                ++m_iCurrentIndex;
            }
        }
    }
}
