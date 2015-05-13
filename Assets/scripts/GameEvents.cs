using UnityEngine;
using System.Collections;

public class GameEvents : MonoBehaviour {

    private float m_fMessageTime = 10.0f;
    private bool m_bShowMsg;
    private string m_sMessage;
    private bool m_bShowImage;
    public GameObject FernandoClue;
    public GameObject ITKey;
    public GameObject ITDoor;
    public GameObject WCDoor;
    public Texture EliasZombieFace;
    public GUIStyle MyGUIStyle;

    public void MoveZombie()
    {
        ZombieBehaviour zombie = (ZombieBehaviour)GameObject.FindObjectOfType(typeof(ZombieBehaviour));
        zombie.MoveForwardAndDisable(6);
    }

    public void GetSecurityCard()
    {

    }

    public void GetReceptionNote()
    {
        m_bShowMsg = true;
        m_fMessageTime = 25;
        m_sMessage = "“Hoy he notado a Fernando, el  jefe de RRHH, un poco nervioso, incluso diría que con miedo. Entré en su despacho para hablar con él y le encontré pálido leyendo un email que rápidamente soltó sobre su mesa. Me pareció que el contenido le afectó sobremanera…”";

        FernandoClue.SetActive(true);
    }

    public void GetFernandoClue()
    {
        m_bShowMsg = true;
        m_fMessageTime = 25;
        m_sMessage = "“Fernando, estoy muy preocupada. Ya sabemos lo hijo de puta que es este tio, pero lo que ocurre últimamente no es normal. Han desaparecido ya varias personas en Barcelona y ahora otra en Madrid. Fíjate que el otro día me pareció como que intento morderme el muy subnormal. Le he pedido a David si podía revisar las cámaras de seguridad y ver algún comportamiento extraño y me ha dejado una nota en su despacho pero esta la puerta cerrada. Creo que la llave esta en la sala de juntas”";
        ITKey.SetActive(true);
    }

    public void GetITKey()
    {
        ITDoor.GetComponent<Collider>().enabled = true;
        ITKey.SetActive (false);
    }

    public void GetITNote()
    {
        m_bShowMsg = true;
        m_fMessageTime = 25;
        m_sMessage = "“Ana, he estado mirando las cámaras de seguridad y me quedado en shock. Tienes que ir al baño de hombres y verlo por ti misma, pero creo que darle de comer al gordo los restos que habían en mi taper caducado no ha sido buena idea, no sé qué hacer con el cuerpo, además de que yo solo no puedo moverlo”";

        WCDoor.GetComponent<Collider>().enabled = true;
    }

    void Start()
    {
        FernandoClue.SetActive(false);
        ITKey.SetActive(false);
        ITDoor.GetComponent<Collider>().enabled = false;
    }

    void Update()
    {        
    }

    public void ShowEliasZombieFace()
    {
        m_bShowImage = true;
        m_fMessageTime = 0.5f;
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

        if (m_bShowImage)
        {
            m_fMessageTime -= Time.deltaTime;

            GUI.DrawTexture(new Rect(0, 0, Screen.width / 2, Screen.height), EliasZombieFace);
            GUI.DrawTexture(new Rect(Screen.width / 2, 0, Screen.width / 2, Screen.height), EliasZombieFace);

            if (m_fMessageTime <= 0)
                m_bShowImage = false;
        }
    }
}

