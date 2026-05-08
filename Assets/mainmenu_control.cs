using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class mainmenu_control : MonoBehaviour
{
    [SerializeField] private string firstSceneName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onStartGame()
    {
        SceneManager.LoadScene(firstSceneName);
    }
}
