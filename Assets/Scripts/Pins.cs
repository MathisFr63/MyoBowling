using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pins : MonoBehaviour {

    public static int pinStanding;

	// Use this for initialization
	void Start () {
        pinStanding++;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void checkStanding()
    {
        // Vérification pour savoir si la quille est tombée ou non.
        if (this.transform.eulerAngles.x < -42f || this.transform.eulerAngles.x > 42f || this.transform.eulerAngles.z < -16f || this.transform.eulerAngles.z > 24f)
            pinStanding--;
    }
}
