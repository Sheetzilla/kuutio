using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PasswordCheck : MonoBehaviour
{
    public TMP_InputField passwordField;
    public string nextScene = "GameScene";

    const string UnlockKey = "unlocked";

    // "juhannuskuutio7" built from char codes so it isn't a plain literal in the binary
    string GetPass()
    {
        char[] c = {
            (char)106, (char)117, (char)104, (char)97, (char)110,
            (char)110, (char)117, (char)115,                      
            (char)107, (char)117, (char)117, (char)116, (char)105, (char)111, 
            (char)55                                              
        };
        return new string(c);
    }

    void Start()
    {
        // Already unlocked previously? Skip straight to the game.
        if (PlayerPrefs.GetInt(UnlockKey, 0) == 1)
            SceneManager.LoadScene(nextScene);
    }

    public void Submit()
    {
        if (passwordField.text.Trim().ToLower() == GetPass())
        {
            PlayerPrefs.SetInt(UnlockKey, 1);
            PlayerPrefs.Save();
            SceneManager.LoadScene(nextScene);
        }
        else
        {
            Debug.Log("Wrong password");
        }
    }

    // Optional: call this to make the user log in again.
    public void Logout()
    {
        PlayerPrefs.DeleteKey(UnlockKey);
        PlayerPrefs.Save();
    }
}