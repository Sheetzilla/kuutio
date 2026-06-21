using TMPro;
using UnityEngine;

public class ScriptKuutio : MonoBehaviour
{
    public GameObject kuutio;
    public GameObject spawnattuKuutio;
    public Vector3 direction = new Vector3(0, 0, 0);
    public float spinSpeed;
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

    void Update()
    {
        if (spawnRb == null)
            return;

        if (isSpinning)
            spawnRb.angularVelocity = new Vector3(spinSpeed, 0, 0);
        else
            spawnRb.angularVelocity = Vector3.zero;
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