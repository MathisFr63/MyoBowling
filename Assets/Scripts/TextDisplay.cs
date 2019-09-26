using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextDisplay : MonoBehaviour {
    private Text displayText;
	// Use this for initialization
	void Start () {
        displayText = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        displayText.text = Pins.pinStanding.ToString();
	}
}
