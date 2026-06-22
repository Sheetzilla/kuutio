using System.Collections;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class timer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public float timeRemaining;
    public float timeLimit;
    public bool timerIsRunning;
    public TMP_InputField inputField;

    private ScriptKuutio cube;
    private GameManager gameManager;
    public GameObject Button;

    public float timerVolume = 1f;

    public float brrInterval = 0.2f;

    [SerializeField]
    private AudioClip Brr;

    // Repeat controls (default to 20 as requested)
    public int brrRepeat = 20;
    public float brrGap = 0f; // extra gap between repeats in seconds

    private Coroutine brrRoutine;
    public bool IsBeeping { get; private set; }

    void Start()
    {
        cube = GetComponent<ScriptKuutio>();
        gameManager = GetComponent<GameManager>();
        inputField.onEndEdit.AddListener(Changed);
    }

    void Update()
    {
        if (!timerIsRunning)
            return;

        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        inputField.text = minutes.ToString("00") + ":" + seconds.ToString("00");

        if (timeRemaining > 0)
        {
            timeRemaining -= Mathf.Min(Time.deltaTime, 0.1f);
        }
        else
        {
            Debug.Log("Time has run out!");
            timeRemaining = 0;
            timerIsRunning = false;
            inputField.text = "00:00";

            // Start beeping: will play up to brrRepeat times (default 20)
            Debug.Log("timer: starting beeping sequence.");
            StartBeeping();

            gameManager.StopDropTimer();
            gameManager.DropAllCubes();
            cube.isSpinning = false;
            cube.buttonText.text = "Start";
            inputField.interactable = true;
            Button.SetActive(false);
            timeLimit = 0;
        }
    }

    // Starts the repeated Brr playback. Playback stops automatically after
    // brrRepeat plays or earlier if StopBeeping() is called (e.g. main cube pressed).
    public void StartBeeping()
    {
        if (IsBeeping)
        {
            Debug.Log("timer: already beeping.");
            return;
        }

        if (brrRoutine != null)
            StopCoroutine(brrRoutine);

        
        IsBeeping = true;
        brrRoutine = StartCoroutine(PlayBrrRepeated());
    }

    // Stops the repeated playback immediately.
    public void StopBeeping()
    {
        Debug.Log("timer: StopBeeping called.");
        if (brrRoutine != null)
        {
            StopCoroutine(brrRoutine);
            brrRoutine = null;
        }
        IsBeeping = false;
    }

    private IEnumerator PlayBrrRepeated()
    {
        if (audioManager.instance == null)
        {
            Debug.LogWarning("timer: audioManager.instance is null; cannot play Brr.");
            IsBeeping = false;
            yield break;
        }

        if (Brr == null)
        {
            Debug.LogWarning("timer: Brr AudioClip is not assigned on the timer component.");
            IsBeeping = false;
            yield break;
        }

        int times = Mathf.Max(1, brrRepeat);
        Debug.Log($"timer: PlayBrrRepeated starting - will attempt {times} plays, clip='{Brr.name}', length={Brr.length}s, gap={brrGap}s");

        for (int i = 0; i < times; i++)
        {
            if (!IsBeeping) // allow external stop
                break;

            Debug.Log($"timer: playing Brr iteration {i+1}/{times}");
            audioManager.instance.PlayAudio(Brr, this.transform, volume: timerVolume);

            // when next beep sound starts
            yield return new WaitForSeconds(brrInterval);
        }

        Debug.Log("timer: PlayBrrRepeated finished.");
        brrRoutine = null;
        IsBeeping = false;
    }

    // Parses "MM:SS" into total seconds. Returns false if the field is empty/invalid.
    private bool TryParseTime(out float totalSeconds)
    {
        totalSeconds = 0f;
        string[] parts = inputField.text.Split(':');
        if (parts.Length != 2)
            return false;
        if (!int.TryParse(parts[0].Trim(), out int minutes))
            return false;
        if (!int.TryParse(parts[1].Trim(), out int seconds))
            return false;
        totalSeconds = minutes * 60 + seconds;
        return true;
    }

    public void StartTimer()
    {
        if (timerIsRunning) return;
        if (!TryParseTime(out float seconds) || seconds <= 0)
            return;

        timeRemaining = seconds;
        timeLimit = seconds;
        timerIsRunning = true;
        inputField.interactable = false;

        cube.isSpinning = true;
        cube.buttonText.text = "Stop";
        gameManager.StartDropTimer();
        Button.SetActive(true);
    }

    public void StopTimer()
    {
        timerIsRunning = false;
        gameManager.StopDropTimer();
    }

    public void ResumeTimer()
    {
        if (inputField.text == "00:00")
            return;

        if (!TryParseTime(out float seconds) || seconds <= 0)
            return;

        timeRemaining = seconds;
        timeLimit = seconds;
        timerIsRunning = true;
        inputField.interactable = false;

        cube.isSpinning = true;
        cube.buttonText.text = "Stop";
        gameManager.StartDropTimer();
    }

    // Called when the player finishes editing the input field.
    public void Changed(string value)
    {
        inputField.text = FormatTime(value);

        if (TryParseTime(out float seconds))
        {
            timeRemaining = seconds;
            timeLimit = seconds;
        }

        // Editing the timer fully re-arms the game: stop running and drop all
        // existing cubes (they fall away visually rather than vanishing). The
        // player must respawn all 300 cubes before the timer starts again.
        timerIsRunning = false;
        gameManager.StopDropTimer();
        gameManager.DropAllCubes();
        cube.isSpinning = false;
        cube.buttonText.text = "Start";
        inputField.interactable = true;
        Button.SetActive(false);
    }

    string FormatTime(string input)
    {
        // keep only numbers
        string digits = System.Text.RegularExpressions.Regex.Replace(input, @"\D", "");

        if (string.IsNullOrEmpty(digits))
            return "00:00";

        // limit to 4 digits (MMSS)
        if (digits.Length > 4)
            digits = digits.Substring(0, 4);

        // pad on the RIGHT so the user types minutes first:
        // "1" -> 0100 -> 01:00, "130" -> 1300 -> 13:00, "0130" -> 01:30
        digits = digits.PadRight(4, '0');

        int minutes = int.Parse(digits.Substring(0, 2));
        int seconds = int.Parse(digits.Substring(2, 2));

        // clamp to a max of 59:59
        if (minutes > 59) minutes = 59;
        if (seconds > 59) seconds = 59;

        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }
}