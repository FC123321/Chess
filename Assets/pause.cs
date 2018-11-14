using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pause : MonoBehaviour {

    private bool paused = false;
    //private GameObject timer;

    // Use this for initialization
    void Start () {
        //timer = this.gameObject.GetComponentsInChildren<GameObject>(false)[0];
    }
	
	// Update is called once per frame
	void Update () {
        //timer
    }

    private void OnGUI()
    {
        if(paused)
        {
            GUILayout.Label("Game is paused");
            if(GUILayout.Button("Click to Unpause"))
            {
                TogglePause();
            }
        }
        else
        {
            if (GUILayout.Button("Pause"))
            {
                TogglePause();
            }
        }
    }

    public bool TogglePause()
    {
        paused = !paused;
        if(paused)
        {
            Time.timeScale = 0.0f;
        }
        else
        {
            Time.timeScale = 1.0f;
        }
        
        return paused;
    }
}
