using UnityEngine;
using System.Collections;

public class lightBlink : MonoBehaviour {

    public float BlinkRate = 10.0f;
    public float BlinkDuration = 2.0f;
    public bool EnabledOutOfTime = true;
    public Texture AlternativeCookie;

    private Texture m_originalCookie;
    private float number;
    private float m_fElapsedRate;
    private float m_fElapsedBlink;
    private bool m_bChangeCookie = false;

    // Use this for initialization
	void Start () {

        m_fElapsedRate = BlinkRate;
        m_fElapsedBlink = BlinkDuration;

        if (AlternativeCookie != null)
        {
            m_originalCookie = GetComponent<Light>().cookie;
            m_bChangeCookie = true;
        }
	}
	
	// Update is called once per frame
	void FixedUpdate () 
    {
        m_fElapsedRate -= Time.deltaTime;

        if (m_fElapsedRate <= 0)
        {
            m_fElapsedBlink -= Time.deltaTime;
            
            number = Random.value;

            if (number <= 0.8)
            {
                GetComponent<Light>().enabled = false;

                if (Time.time % 2 == 0 && m_bChangeCookie)
                {
                    if (GetComponent<Light>().cookie == m_originalCookie)
                        GetComponent<Light>().cookie = AlternativeCookie;
                    else
                        GetComponent<Light>().cookie = m_originalCookie;
                }
                
            }
            else
            {
                GetComponent<Light>().enabled = true;
            }

            if (m_fElapsedBlink <= 0)
            {
                m_fElapsedRate = BlinkRate;
                m_fElapsedBlink = BlinkDuration;
                GetComponent<Light>().enabled = EnabledOutOfTime;
            }
        }


	}
}
