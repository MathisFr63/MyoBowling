using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe permettant de jouer avec la souris.
/// </summary>
public class TouchManager : MonoBehaviour
{
    // Position initiale de la souris.
    private Vector2 initialPosition;
    // Position finale de la souris.
    private Vector2 finalPosition;
    
    // Temps initial du lancer.
    private float initialTime;
    // Temps final du lancer.
    private float finalTime;

    // Distance entre la position initiale et la position finale de la souris.
    private Vector2 distance;
    //Temps entre le temps initial et le temps final du lancer.
    private float time;

    // Multiplicateur de la vitesse de la vitesse de la boule
    private float speed = 1f;

    // Multiplicateur de l'orientation x de la boule.
    private float xAxisLimiter = 5f;

    // Permet de savoir si le joueur a joué ou non.
    public static bool hasAlreadyPlayed;

    // Appelé lorsque l'on clique avec la souris (et que l'on maintient appuyé)
    public void onMouseDown()
    {
        // Si le joueur n'a pas encore joué.
        if (!hasAlreadyPlayed)
        {
            // Initialisation de la position initiale de la souris.
            initialPosition = Input.mousePosition;
            // Initialisation du temps initial du lancer.
            initialTime = Time.time;
        }
    }

    // Appelé lorque l'on relâche le clique de la souris
    public void onMouseUp()
    {
        // Si le joueur n'a pas encore joué.
        if (!hasAlreadyPlayed)
        {
            //Initialisation de la position finale de la souris.
            finalPosition = Input.mousePosition;
            
            // Si le joueur a bien tiré vers l'avant.
            if (finalPosition.y > initialPosition.y)
            {
                //Initialisation du temps final du lancer.
                finalTime = Time.time;

                // Calculs de la distance parcourue par la souris et du temps du lancer.
                distance = finalPosition - initialPosition;
                time = finalTime - initialTime;

                // Calcul des trajectoires x et y à ajouter à la boule pour jouer.
                float xPos = (distance.x * Time.deltaTime / time * speed) / xAxisLimiter;
                float yPos = distance.y * Time.deltaTime / time * speed;

                // Modification de la vélocité de la boule.
                Vector3 velocity = new Vector3(-xPos, 0f, -yPos);
                Ball.ballVelocity = velocity;

                // Le joueur a joué.
                hasAlreadyPlayed = true;
            }
        }
    }

}
