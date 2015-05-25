using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public float Health = 100f;
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

    public void DoDamage(float damage)
    {
        Health -= damage;

        Debug.Log(Health);
    }
}
