using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] UIDocument mainMenuDocument;

    public VisualElement ui;

    public Button playButton;
    public Button optionsButton;
    public Button helpButton;
    public Button quitButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        ui = mainMenuDocument.rootVisualElement;
        playButton = ui.Q<Button>("PlayButton");
        playButton.clickable.clicked += OnPlayButtonClicked;
        optionsButton = ui.Q<Button>("OptionsButton");
        optionsButton.clickable.clicked += OnOptionsButtonClicked;
        helpButton = ui.Q<Button>("HelpButton");
        helpButton.clickable.clicked += OnHelpButtonClicked;
        quitButton = ui.Q<Button>("QuitButton");
        quitButton.clickable.clicked += OnQuitButtonClicked;
    }

    private void OnEnable()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    private void OnQuitButtonClicked()
    {
        Application.Quit();
    }

    private void OnOptionsButtonClicked()
    {
        Debug.Log("Options!");
    }

    private void OnHelpButtonClicked()
    {
        Debug.Log("Help!");
    }

    private void OnPlayButtonClicked()
    {
        SceneManager.LoadScene("TestWorld");
        gameObject.SetActive(false);
    }
}
