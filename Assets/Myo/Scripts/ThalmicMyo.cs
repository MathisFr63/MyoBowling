using UnityEngine;
using System.Collections;

using Arm = Thalmic.Myo.Arm;
using XDirection = Thalmic.Myo.XDirection;
using VibrationType = Thalmic.Myo.VibrationType;
using Pose = Thalmic.Myo.Pose;
using UnlockType = Thalmic.Myo.UnlockType;
using System;
using System.Collections.Generic;
using System.Linq;

// Represents a Myo armband. Myo's orientation is made available through transform.localRotation, and other properties
// like the current pose are provided explicitly below. All spatial data about Myo is provided following Unity
// coordinate system conventions (the y axis is up, the z axis is forward, and the coordinate system is left-handed).
public class ThalmicMyo : MonoBehaviour
{

    // True if and only if Myo has detected that it is on an arm.
    public bool armSynced;

    // Returns true if and only if Myo is unlocked.
    public bool unlocked;

    // The current arm that Myo is being worn on. An arm of Unknown means that Myo is unable to detect the arm
    // (e.g. because it's not currently being worn).
    public Arm arm;

    // The current direction of Myo's +x axis relative to the user's arm. A xDirection of Unknown means that Myo is
    // unable to detect the direction (e.g. because it's not currently being worn).
    public XDirection xDirection;

    // The current pose detected by Myo. A pose of Unknown means that Myo is unable to detect the pose (e.g. because
    // it's not currently being worn).
    public Pose pose = Pose.Unknown;

    // Myo's current accelerometer reading, representing the acceleration due to force on the Myo armband in units of
    // g (roughly 9.8 m/s^2) and following Unity coordinate system conventions.
    public Vector3 accelerometer;

    // Myo's current gyroscope reading, representing the angular velocity about each of Myo's axes in degrees/second
    // following Unity coordinate system conventions.
    public Vector3 gyroscope;


    //------------------------------------------------------------------------------------------------------------------------------------------------------------------


    // Valeur initiale de l'orientation Z (permet de modifier l'orientation la boule sur la droite ou la gauche lors du lancer).
    float OrientationZI;
    // Valeur finale de l'orientation Z (permet de modifier l'orientation la boule sur la droite ou la gauche lors du lancer).
    float OrientationZF;

    // Valeur initiale de l'orientation Y (permet de calibrer la hauteur à laquelle lancer la boule).
    float OrientationYI;

    // Valeur maximale de l'accélération de X durant le lancer.
    float AccelXMax;

    // Booléen permettant de savoir si la dernière position de le joueur était un double tap (permet de relancer le jeu si le joueur double tap deux fois d'affilé)
    bool hasDoubleTap;

    // Booléen permettant de vérifier que le joueur n'a pas encore jouer (pour ne jouer qu'une fois et ne pas modifier la trajectoire de la boule).
    public static bool hasAlreadyPlayed;

    // Booléen permettant de vérifier que le joueur n'a pas encore calibrer son bracelet.
    public bool hasAlreadyCalibrate = false;

    // Permet de stocker la dernière pose faite par le joueur.
    Pose lastPose;

    //Canvas permettant d'afficher ou non les boutons.
    GameObject canvas;

    //Boule de bowling.
    GameObject ball;
    //Rigidbody de la boule de bowling.
    Rigidbody rbBall;

    // GameObject représentant le myo dans le jeu.
    GameObject myo = null;


    // Vecteur contenant la position initiale de la boule (pour la replacer lorsque le joueur souhaite rejouer).
    private Vector3 initialBallPosition;
    //Quaternion contenant la rotation initiale de la boule (pour la replacer lorsque le joueur souhaite rejouer).
    private Quaternion initialBallRotation;

    //Liste contenant la position initiale de chaque quille.
    private List<Vector3> pinsPosition = new List<Vector3>();
    // Liste contenant la rotation initiale de chaque quille.
    private List<Quaternion> pinsRotation = new List<Quaternion>();


    //------------------------------------------------------------------------------------------------------------------------------------------------------------------


    // True if and only if this Myo armband has paired successfully, at which point it will provide data and a
    // connection with it will be maintained when possible.
    public bool isPaired
    {
        get { return _myo != null; }
    }

    // Vibrate the Myo with the provided type of vibration, e.g. VibrationType.Short or VibrationType.Medium.
    public void Vibrate(VibrationType type)
    {
        _myo.Vibrate(type);
    }

    // Cause the Myo to unlock with the provided type of unlock. e.g. UnlockType.Timed or UnlockType.Hold.
    public void Unlock(UnlockType type)
    {
        _myo.Unlock(type);
    }

    // Cause the Myo to re-lock immediately.
    public void Lock()
    {
        _myo.Lock();
    }

    /// Notify the Myo that a user action was recognized.
    public void NotifyUserAction()
    {
        _myo.NotifyUserAction();
    }

    void Start()
    {
        //Vibration indiquant au joueur que le jeu est lancé.
        Vibrate(VibrationType.Short);

        ball = GameObject.FindGameObjectWithTag("BowlingBall");
        myo = GameObject.FindGameObjectWithTag("MYO");
        rbBall = ball.GetComponent<Rigidbody>();

        // Initialisation de la position de la boule.
        initialBallPosition = ball.transform.position;
        // Initialisation de la rotation de la boule.
        initialBallRotation = ball.transform.rotation;

        //Pour chaque quille, stockage de sa position et de sa rotation initiale.
        foreach (GameObject pins in GameObject.FindGameObjectsWithTag("Pins"))
        {
            pinsPosition.Add(pins.transform.position);
            pinsRotation.Add(pins.transform.rotation);
        }

        canvas = GameObject.FindGameObjectWithTag("Canvas");
    }

    void Update()
    {
        lock (_lock)
        {
            armSynced = _myoArmSynced;
            arm = _myoArm;
            xDirection = _myoXDirection;
            if (_myoQuaternion != null)
            {
                transform.localRotation = new Quaternion(_myoQuaternion.Y, _myoQuaternion.Z, -_myoQuaternion.X, -_myoQuaternion.W);
            }
            if (_myoAccelerometer != null)
            {
                accelerometer = new Vector3(_myoAccelerometer.Y, _myoAccelerometer.Z, -_myoAccelerometer.X);
            }
            if (_myoGyroscope != null)
            {
                gyroscope = new Vector3(_myoGyroscope.Y, _myoGyroscope.Z, -_myoGyroscope.X);
            }
            unlocked = _myoUnlocked;

            // Récupération de l'ancienne pose.
            lastPose = pose;
            // Récupération de la pose courant.
            pose = _myoPose;

            // Calibration de la position du Myo lorsque le joueur écarte les doigts.
            if (pose == Pose.FingersSpread && !hasAlreadyCalibrate)
            {
                OrientationYI = myo.transform.forward.y;
                OrientationZI = myo.transform.forward.z;
                hasAlreadyCalibrate = true;

                Debug.Log($"Initialisation des valeurs de l'orientation X({OrientationZI}) et Y({OrientationYI})");
            }

            if (pose == Pose.DoubleTap)
            {
                // Si le joueur fait 2 double tap à la suite --> rejouer.
                if (hasDoubleTap && lastPose == Pose.Rest)
                {
                    Replay();
                }
                hasDoubleTap = true;
            }
            else if (pose != Pose.Rest)
                hasDoubleTap = false;

            // Si le joueur n'a pas encore joué.
            if (!hasAlreadyPlayed)
            {
                // Si la position du joueur est le poing
                if (pose == Pose.Fist)
                {
                    // Si l'accélération maximale est égale à 0 et donc que le joueur n'est pas entrain de lancer --> Début du lancer
                    if (AccelXMax == 0f)
                    {
                        // Vibration indiquant au joueur que le lancer à commencé.
                        _myo.Vibrate(VibrationType.Short);
                    }

                    // Si la position courante Y du myo est supérieure à la position Y initiale lors de la calibration --> Lancer le jeu.
                    if (myo.transform.forward.y > OrientationYI)
                    {
                        AvantLancer();
                    }
                    // Récupération
                    if (AccelXMax < _myoAccelerometer.X)
                        AccelXMax = _myoAccelerometer.X;
                }
                // Sinon, si l'accélération maximale est supérieure à 0, et donc que le joueur est dans un lancé => le joueur a finit son lancé donc on lance le jeu.
                else if (AccelXMax > 0f)
                {
                    AvantLancer();
                }
                // Si le joueur est en extension: déplacement de la boule vers la droite
                else if (pose == Pose.WaveOut && lastPose != Pose.WaveOut)
                {
                    if (ball.transform.position.x > -0.4f)
                        ball.transform.Translate(new Vector3(-0.1f, 0f, 0f));
                }

                // Si le joueur est en flexion: déplacement de la boule vers la gauche
                else if (pose == Pose.WaveIn && lastPose != Pose.WaveIn)
                {
                    if (ball.transform.position.x < 0.4f)
                        ball.transform.Translate(new Vector3(0.1f, 0f, 0f));
                }
            }
        }
    }

    /// <summary>
    /// Méthode permettant d'initialiser les valeurs avant de lancer le jeu.
    /// </summary>
    private void AvantLancer()
    {
        // Vibration indiquant au joueur que le lancer est terminé.
        _myo.Vibrate(VibrationType.Short);
        _myo.Vibrate(VibrationType.Short);

        //Récupération de la valeur de l'orientation de Z.
        OrientationZF = myo.transform.forward.z;

        // Lancement du jeu.
        LancerJeu();
    }

    /// <summary>
    /// Méthode permettant de rejouer.
    /// </summary>
    public void Replay()
    {
        print("Appel de la méthode Replay");
        
        // Affichage des boutons sur l'écran.
        canvas.SetActive(true);
        hasDoubleTap = false;
        hasAlreadyCalibrate = false;

        // Réinitialisation de la rotation de la boule.
        rbBall.rotation = initialBallRotation;
        // Réinitialisation de la position de la boule.
        rbBall.position = initialBallPosition;
        //réinitialisation de la vélocité de la balle.
        rbBall.velocity = Vector3.zero;

        rbBall.isKinematic = true;
        rbBall.useGravity = false;

        // Réinitialisation des booléens permettant de savoir si le joueur a joué.
        TouchManager.hasAlreadyPlayed = false;
        hasAlreadyPlayed = false;

        int i = 0;
        // Réinitialisation de la position, de la rotation et de l'angle de chaque quille.
        foreach (GameObject pins in GameObject.FindGameObjectsWithTag("Pins"))
        {
            pins.transform.position = pinsPosition[i];
            pins.transform.rotation = pinsRotation[i];
            pins.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            i++;
        }

        //Réinitialisation de l'accélération maximale sachant que la partie recommence.
        AccelXMax = 0f;
    }

    /// <summary>
    /// Méthode permettant de lancer le jeu.
    /// </summary>
    private void LancerJeu()
    {
        // Calcul de la vitesse du lancer.
        float VitesseX = AccelXMax * 3;
        // Calcul de la différence entre l'orientation z finale et l'orientation z initiale afin de modifier la direction de la boule.
        float diff = (OrientationZF - OrientationZI) * 3;

        print("Jeu lancé:\tVitesse: " + VitesseX + " Diff: " + diff + " | " + OrientationZF + " | " + OrientationZI);

        // Changement de la vélocité de la boule.
        Vector3 velocity = new Vector3(diff, 0f, -VitesseX);
        Ball.ballVelocity = velocity;

        hasAlreadyPlayed = true;
        rbBall.isKinematic = false;
        rbBall.useGravity = true;
        canvas.SetActive(false);
    }

    void myo_OnArmSync(object sender, Thalmic.Myo.ArmSyncedEventArgs e)
    {
        lock (_lock)
        {
            _myoArmSynced = true;
            _myoArm = e.Arm;
            _myoXDirection = e.XDirection;
        }
    }

    void myo_OnArmUnsync(object sender, Thalmic.Myo.MyoEventArgs e)
    {
        lock (_lock)
        {
            _myoArmSynced = false;
            _myoArm = Arm.Unknown;
            _myoXDirection = XDirection.Unknown;
        }
    }

    void myo_OnOrientationData(object sender, Thalmic.Myo.OrientationDataEventArgs e)
    {
        lock (_lock)
        {
            _myoQuaternion = e.Orientation;
        }
    }

    void myo_OnAccelerometerData(object sender, Thalmic.Myo.AccelerometerDataEventArgs e)
    {
        lock (_lock)
        {
            _myoAccelerometer = e.Accelerometer;
        }
    }

    void myo_OnGyroscopeData(object sender, Thalmic.Myo.GyroscopeDataEventArgs e)
    {
        lock (_lock)
        {
            _myoGyroscope = e.Gyroscope;
        }
    }

    void myo_OnPoseChange(object sender, Thalmic.Myo.PoseEventArgs e)
    {
        lock (_lock)
        {
            _myoPose = e.Pose;
        }
    }

    void myo_OnUnlock(object sender, Thalmic.Myo.MyoEventArgs e)
    {
        lock (_lock)
        {
            _myoUnlocked = true;
        }
    }

    void myo_OnLock(object sender, Thalmic.Myo.MyoEventArgs e)
    {
        lock (_lock)
        {
            _myoUnlocked = false;
        }
    }

    public Thalmic.Myo.Myo internalMyo
    {
        get { return _myo; }
        set
        {
            if (_myo != null)
            {
                _myo.ArmSynced -= myo_OnArmSync;
                _myo.ArmUnsynced -= myo_OnArmUnsync;
                _myo.OrientationData -= myo_OnOrientationData;
                _myo.AccelerometerData -= myo_OnAccelerometerData;
                _myo.GyroscopeData -= myo_OnGyroscopeData;
                _myo.PoseChange -= myo_OnPoseChange;
                _myo.Unlocked -= myo_OnUnlock;
                _myo.Locked -= myo_OnLock;
            }
            _myo = value;
            if (value != null)
            {
                value.ArmSynced += myo_OnArmSync;
                value.ArmUnsynced += myo_OnArmUnsync;
                value.OrientationData += myo_OnOrientationData;
                value.AccelerometerData += myo_OnAccelerometerData;
                value.GyroscopeData += myo_OnGyroscopeData;
                value.PoseChange += myo_OnPoseChange;
                value.Unlocked += myo_OnUnlock;
                value.Locked += myo_OnLock;
            }
        }
    }

    private UnityEngine.Object _lock = new UnityEngine.Object();

    private bool _myoArmSynced = false;
    private Arm _myoArm = Arm.Unknown;
    private XDirection _myoXDirection = XDirection.Unknown;
    private Thalmic.Myo.Quaternion _myoQuaternion = null;
    private Thalmic.Myo.Vector3 _myoAccelerometer = null;
    private Thalmic.Myo.Vector3 _myoGyroscope = null;
    private Pose _myoPose = Pose.Unknown;
    private bool _myoUnlocked = false;

    private Thalmic.Myo.Myo _myo;
}
