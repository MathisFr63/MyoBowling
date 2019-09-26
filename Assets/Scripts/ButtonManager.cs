using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    // Position initiale de la boule (permettant de la replacer correctement afin de rejouer).
    private Vector3 initialBallPosition;

    // Rotation initiale de la boule (permettant de la replacer correctement afin de rejouer).
    private Quaternion initialBallRotation;

    // Liste des positions des quilles (permettant de les replacer correctement afin de rejouer).
    private List<Vector3> pinsPosition = new List<Vector3>();

    // Liste des rotations des quilles (permettant de les replacer correctement afin de rejouer).
    private List<Quaternion> pinsRotation = new List<Quaternion>();

    private new Rigidbody rigidbody;

    public GameObject canvas;

    // Use this for initialization
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();

        // Récupération de la position et de la rotation de la boule pour pouvoir rejouer.
        initialBallPosition = rigidbody.position;
        initialBallRotation = rigidbody.rotation;

        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;
        canvas.SetActive(true);

        // Récupération de la position et de la rotation de chaque quille pour pouvoir rejouer.
        foreach (GameObject pins in GameObject.FindGameObjectsWithTag("Pins"))
        {
            pinsPosition.Add(pins.transform.position);
            pinsRotation.Add(pins.transform.rotation);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    /// <summary>
    /// Méthode permettant de déplacer la boule sur la droite lors du clic sur la fléche de droite.
    /// </summary>
    public void RightArrow()
    {
        if (this.transform.position.x > -0.4f)
            this.transform.Translate(new Vector3(-0.1f, 0f, 0f));
    }

    /// <summary>
    /// Méthode permettant de déplacer la boule sur la gauche lors du  clic sur la flèche de gauche.
    /// </summary>
    public void LeftArrow()
    {
        if (this.transform.position.x < 0.4f)
            this.transform.Translate(new Vector3(0.1f, 0f, 0f));
    }

    /// <summary>
    /// Méthode permettant de soumettre la boule de bowling à la gravité afin de lancer le jeu.
    /// </summary>
    public void DropBall()
    {
        print("Appel de la méthode DropBall");
        canvas.SetActive(false);


        rigidbody.useGravity = true;
        rigidbody.isKinematic = false;
    }


    /// <summary>
    /// Méthode permettant de rejouer.
    /// </summary>
    public void Replay()
    {
        print("Appel de la méthode Replay");
        canvas.SetActive(true);
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;

        // Réinitialisation de la rotation, de la positionm, et de la vélocité de la boule.
        rigidbody.rotation = initialBallRotation;
        rigidbody.position = initialBallPosition;
        rigidbody.velocity = new Vector3(0f, 0f, 0f);

        // Réinitialisation des booléens permettant de savoir si le joueur a joué.
        TouchManager.hasAlreadyPlayed = false;
        ThalmicMyo.hasAlreadyPlayed = false;

        // Réinitialisation de la position, de la rotation et de l'angle de chaque quille.
        int i = 0;
        foreach (GameObject pins in GameObject.FindGameObjectsWithTag("Pins"))
        {
            pins.transform.position = pinsPosition[i];
            pins.transform.rotation = pinsRotation[i];
            pins.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            i++;
        }
    }
}