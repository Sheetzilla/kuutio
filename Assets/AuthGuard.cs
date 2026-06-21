using UnityEngine;
using UnityEngine.SceneManagement;

public class AuthGuard : MonoBehaviour
{
    public string passwordScene = "pass";

    const string UnlockKey = "unlocked";

    void Awake()
    {
        // No valid unlock flag? Kick back to the password scene.
        if (PlayerPrefs.GetInt(UnlockKey, 0) != 1)
            SceneManager.LoadScene(passwordScene);
    }
}