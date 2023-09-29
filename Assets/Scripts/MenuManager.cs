using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private OptionsMenu optionsLink;
    [SerializeField] private AudioSource source;
    public VisualElement Root { get; private set; }

    private Label _title;
    
    private Button _playButton;

    private Button _optionsButton;

    private Button _exitButton;

    private void Start()
    {
        Root = document.rootVisualElement;
        SetReferences();
        SetEvents();
    }

    private void SetReferences()
    {
        _playButton = Root.Q<Button>("PlayButton");
        _optionsButton = Root.Q<Button>("OptionsButton");
        _exitButton = Root.Q<Button>("QuitButton");
    }

    private void SetEvents()
    {
        _playButton.clicked += LoadMap;
        _optionsButton.clicked += OptionsButton;
        _exitButton.clicked += ExitButton;
    }

    private void LoadMap()
    {
        SceneManager.LoadScene("Game");
    }

    private void OptionsButton()
    {
        Root.AddToClassList("hidden");
    }

    private void ExitButton()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif

    }
}
