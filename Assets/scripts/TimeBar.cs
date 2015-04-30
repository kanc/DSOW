using UnityEngine;
using System.Collections;

public class TimeBar : MonoBehaviour {

    public float TotalTime;
    public float X_Offset;
    public float Y_Offset = 1.25f;
    public float Z_Offset = -1.25f;
    public float InitialXScale = 3.0f;
    public float InitialYScale = 0.8f;
    public Material TimeBarMaterial;
    private GameObject m_objBar;
    public float CountdownTime = -1.0f;
    private bool m_bShowBar = false;


    // Use this for initialization
    void Start()
    {
        CreateBarObject();        
    }

    void CreateBarObject()
    {
        //create a quad
        m_objBar = GameObject.CreatePrimitive(PrimitiveType.Quad);

        //set as child of this object
        m_objBar.transform.SetParent(transform);

        //change scale, offset position and material
        m_objBar.transform.localScale = new Vector3(InitialXScale, InitialYScale, 1);
        m_objBar.transform.localPosition = new Vector3(X_Offset, Y_Offset, Z_Offset);
        m_objBar.GetComponent<Renderer>().sharedMaterial = TimeBarMaterial;

        m_objBar.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_bShowBar) return;

        //decrease time
        if (CountdownTime > 0.0f)
        {
            CountdownTime -= Time.deltaTime;
        }
        else
        {
            FinishCount();
            return;
        }

        //health bar always orthogonal to camera
        m_objBar.transform.rotation = Quaternion.RotateTowards(m_objBar.transform.rotation, Camera.main.transform.rotation, 90);        
        
        //update size
        UpdateBarSize();

    }

    //update health bar size (x scale proportional to max health)
    private void UpdateBarSize()
    {
        float currentHealthPercent = CountdownTime * 100 / TotalTime;
        float xscale = InitialXScale * currentHealthPercent / 100;

        if (m_objBar != null)
        {
            m_objBar.transform.localScale = new Vector3(xscale, InitialYScale, 1);
        }
    }

    private void FinishCount()
    {
        m_bShowBar = false;
        m_objBar.SetActive(false);
        CountdownTime = 0.0f;
    }

    public void StopCount()
    {
        m_bShowBar = false;
        m_objBar.SetActive(false);
        CountdownTime = -1.0f;        
    }
    public void IniCount()
    {
        CountdownTime = TotalTime;
        m_bShowBar = true;
        m_objBar.SetActive(true);        
    }
}
