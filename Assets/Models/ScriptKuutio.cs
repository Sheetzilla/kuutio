using TMPro;
using UnityEngine;

public class ScriptKuutio : MonoBehaviour
{
    public GameObject kuutio;
    public GameObject spawnattuKuutio;
    public Vector3 direction = new Vector3(0, 0, 0);
    public float spinSpeed;
    public bool spinSpeedIsDegrees = false; // set to true if spinSpeed is degrees/sec in the Inspector
    public bool isSpinning = true;
    public TextMeshProUGUI buttonText;

    private timer timerScript;
    private GameManager gameManager;
    private Rigidbody spawnRb;

    void Start()
    {
        timerScript = GetComponent<timer>();
        gameManager = GetComponent<GameManager>();

        spawnattuKuutio = Instantiate(kuutio, direction, kuutio.transform.rotation);
        spawnRb = spawnattuKuutio.GetComponent<Rigidbody>();

        buttonText.text = "Start";
        timerScript.inputField.interactable = true;
        isSpinning = false;
    }

    // Use FixedUpdate for physics changes
    void FixedUpdate()
    {
        if (spawnRb == null)
            return;

        if (isSpinning)
        {
            float angular = spinSpeedIsDegrees ? spinSpeed * Mathf.Deg2Rad : spinSpeed;
            // Negative Y for clockwise rotation (as seen from above) using right-hand rule
            spawnRb.angularVelocity = new Vector3(0f, -angular, 0f);
        }
        else
        {
            spawnRb.angularVelocity = Vector3.zero;
        }
    }

    public void SetSpin()
    {
        if (timerScript.timeLimit == 0)
            return;

        isSpinning = !isSpinning;

        if (isSpinning)
        {
            buttonText.text = "Stop";
            timerScript.ResumeTimer();
            timerScript.inputField.interactable = false;
        }
        else
        {
            buttonText.text = "Start";
            timerScript.StopTimer();
            timerScript.inputField.interactable = true;
            gameManager.StopDropTimer();
        }
    }
}