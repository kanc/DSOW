using UnityEngine;
using System.Collections;

public class CursorManager : MonoBehaviour {

    public enum CursorEye
    {
        left,
        rigth
    };
    public Texture2D NormalCursorTex = null;
    public Texture2D OpenningCursorTex = null;
    public CursorEye Eye;

    private float leftPos;    

    // Use this for initialization
	void Start () 
    {
        Cursor.visible = true;	
	}
	
    void OnGUI()
    {
        if (NormalCursorTex != null)
        {
            leftPos = (Eye == CursorEye.left) ? (Screen.width / 4) - (NormalCursorTex.width / 2) : ((Screen.width / 4) * 3) - (NormalCursorTex.width / 2);
            
            GUI.DrawTexture(new Rect(leftPos, Screen.height / 2 - (NormalCursorTex.height / 2), NormalCursorTex.width, NormalCursorTex.height), NormalCursorTex);
        }
    }

	// Update is called once per frame
	void Update () {
	
	}
}
