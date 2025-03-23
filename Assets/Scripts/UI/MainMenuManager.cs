using UnityEngine;
using UnityEngine.SceneManagement;

namespace SuperRobot
{
    public class MainMenuManager : MonoBehaviour
    {
        public void StartNewGame()
        {
            SceneManager.LoadScene("Scene");
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}