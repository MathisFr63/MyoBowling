using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    // Rigidbody de la boule de bowling.
    private Rigidbody rigidbody;

    // Vélocité de la boule.
    public static Vector3 ballVelocity;

    // Use this for initialization
    void Start()
    {
        // Initialisation de la vélocité de la boule à 0.
        ballVelocity = Vector3.zero;
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // Modification de la vélocité de la boule.
        BallVelocityMovement(ballVelocity);
    }

    // Méthode permettant de modifier la vélocité de la boule.
    void BallVelocityMovement(Vector3 velocity)
    {
        rigidbody.velocity = velocity;
    }
}
