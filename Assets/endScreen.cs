using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class endScreen : MonoBehaviour
{
    [SerializeField] private int mainmenu = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onStartGame()
    {
        SceneManager.LoadScene(mainmenu);
    }
}
