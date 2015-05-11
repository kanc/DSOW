using UnityEngine;
using System.Collections;

public class GameEvents : MonoBehaviour {

    private float m_fMessageTime = 5.0f;
    private bool m_bShowMsg;
    private string m_sMessage;
    public GUIStyle MyGUIStyle;

    public void GetSecurityCard()
    {
        m_bShowMsg = true;
        m_fMessageTime = 25;
        m_sMessage = "“Hoy he notado a Fernando, el  jefe de RRHH, un poco nervioso, incluso diría que con miedo. Entre en su despacho para hablar con él y le encontré pálido leyendo un email que rápidamente soltó sobre su mesa. Me pareció que el contenido le afectó sobremanera…”";
    }

    void Update()
    {        
    }

    void OnGUI()
    {
        if (m_bShowMsg)
        {
            m_fMessageTime -= Time.deltaTime;

            GUI.TextArea(new Rect(0, 0, Screen.width / 2, Screen.height), m_sMessage, MyGUIStyle);
            GUI.TextArea(new Rect(Screen.width / 2, 0, Screen.width / 2, Screen.height), m_sMessage, MyGUIStyle);

            if (m_fMessageTime <= 0)
                m_bShowMsg = false;
        }
    }
}
