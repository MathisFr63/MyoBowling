using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe permettant de calculer le nombre de quilles n'étant pas tombées.
/// </summary>
public class PinCount : MonoBehaviour
{

    // Nombre de quilles debouts.
    public static int standingPins;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Réinitialisation du nombre de quilles debout.
        standingPins = 0;

        //Pour chaque quille, si elle est encore debout, incrémenter le nombre de quilles debout.
        foreach (GameObject pins in GameObject.FindGameObjectsWithTag("Pins"))
        {
            if ((pins.transform.localEulerAngles.x < 22f || pins.transform.localEulerAngles.x > 345f && pins.transform.localEulerAngles.y < 24f || pins.transform.localEulerAngles.y > 345f) && pins.transform.position.y >= -0.48)
            {
                standingPins++;
            }
        }
    }
}
