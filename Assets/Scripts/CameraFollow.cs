using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe permettant à la caméra de suivre la boule.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    //Boule de bowling
    public GameObject ball;

    // Distance entre la balle et la caméra.
    private Vector3 offset;

    // Use this for initialization
    void Start()
    {
        // Calcul de la distance entre la balle et la caméra.
        offset = ball.transform.position - this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Modification de la position de la caméra pour pouvoir suivre la balle.
        if (ball.transform.position.z > -8.861f)
            this.transform.position = ball.transform.position - offset;
    }
}
