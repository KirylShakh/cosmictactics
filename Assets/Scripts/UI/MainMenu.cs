using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenu : MonoBehaviour
{
    void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            transform.Find("PlayButton").gameObject.SetActive(false);
            transform.Find("RestartButton").gameObject.SetActive(true);
        }
    }

    public void OnPlay()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void OnRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnQuit()
    {
        Application.Quit();
    }
}
