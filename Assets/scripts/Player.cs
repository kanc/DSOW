using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {


    private bool m_bAccessCard = true;

    // Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public bool HasAccessCard()
    {
        return m_bAccessCard;    
    }

    public void SetAccessCard(bool getted)
    {
        m_bAccessCard = getted;
    }
}
