﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using BobJeltes;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Canvas))]
public class MenuManager : MonoBehaviour
{
    private GameControls controls;
    private GameControls Controls
    {
        get
        {
            if (controls == null)
            {
                controls = new GameControls();
            }
            return controls;
        }
    }

    public Transform Screens;
    public GameObject DebugNavigation;
    public GameObject InputBlocker;
    [Min(0f)]
    public float ScreenTransitionDuration = .5f;
    public bool BlockInputsDuringTransitions = false;

    public StartScreen StartScreen;
    public MenuScreen CurrentScreen = null;
    public List<MenuScreen> PreviousScreens = new List<MenuScreen>();
    public NavigationButton GlobalBackButton;

    public int GetScreenCount()
    {
        return transform.childCount;
    }

    EventSystem activeEventSystem;
    public EventSystem FindActiveEventSystem()
    {
        if (activeEventSystem == null)
        {
            activeEventSystem = FindObjectOfType<EventSystem>();
            if (activeEventSystem == null)
            {
                Debug.LogError("No event system found in scene!");
            }
        }
        return activeEventSystem;
    }

    public void GoToScreen(MenuScreen newScreen)
    {
        if (Screens == null)
        {
            Debug.Log("Screens not assigned");
            return;
        }
        if (newScreen == null)
        {
            Debug.LogError("Screen not assigned");
            return;
        }

        if (ScreenTransitionDuration > 0f)
        {
            if (BlockInputsDuringTransitions)
                InputBlockerActive(true);
            Screens.DOComplete();
            DG.Tweening.Core.TweenerCore<Vector3, Vector3, DG.Tweening.Plugins.Options.VectorOptions> tweenerCore = Screens.DOMove(GetMoveDelta(CurrentScreen.transform, newScreen.transform), ScreenTransitionDuration);
            if (BlockInputsDuringTransitions)
                tweenerCore.OnComplete(() => InputBlockerActive(false));
        }
        else
        {
            Screens.position = GetMoveDelta(CurrentScreen.transform, newScreen.transform);
        }

        PreviousScreens.Add(CurrentScreen);
        CurrentScreen = newScreen;

        if (!GlobalBackButton.gameObject.activeInHierarchy)
        {
            GlobalBackButton.gameObject.SetActive(true);
        }
    }

    public void GoToPreviousScreen()
    {
        if (Screens == null)
        {
            Debug.Log("Screens not assigned");
            return;
        }
        if (PreviousScreens == null || PreviousScreens.Count == 0)
        {
            Debug.Log("No previous screens to go back to");
            GlobalBackButton.gameObject.SetActive(false);
            return;
        }
        MenuScreen previousScreen = PreviousScreens[PreviousScreens.Count - 1];
        if (previousScreen == null)
        {
            Debug.Log("Previous screen is null");
            return;
        }

        if (ScreenTransitionDuration > 0f)
        {
            if (BlockInputsDuringTransitions)
                InputBlockerActive(true);
            Screens.DOComplete();
            DG.Tweening.Core.TweenerCore<Vector3, Vector3, DG.Tweening.Plugins.Options.VectorOptions> tweenerCore = Screens.DOMove(GetMoveDelta(CurrentScreen.transform, previousScreen.transform), ScreenTransitionDuration);
            if (BlockInputsDuringTransitions)
                tweenerCore.OnComplete(() => InputBlockerActive(false));
        }
        else
        {
            Screens.position = GetMoveDelta(CurrentScreen.transform, previousScreen.transform);
        }

        CurrentScreen = previousScreen;
        PreviousScreens.Remove(previousScreen);

        if (PreviousScreens.Count == 0)
        {
            GlobalBackButton.gameObject.SetActive(false);
        }
    }

    public Vector3 GetMoveDelta(Transform current, Transform next)
    {
        return Screens.position + (current.position - next.position);
    }

    public void AddScreenToHistory(MenuScreen screen)
    {
        PreviousScreens.Add(screen);
    }

    void InputBlockerActive(bool enabled)
    {
        if (InputBlocker != null)
        {
            InputBlocker.SetActive(enabled);
        }
        else
            Debug.LogWarning("Input Blocker is not set in " + name + " but was attempted to set active. Have you disabled BlockInputsDuringTransitions?", this);
    }

    public bool IsOpen { get { return GetComponent<Canvas>().enabled; } }

    public void Toggle()
    {
        GetComponent<Canvas>().enabled = !GetComponent<Canvas>().enabled;
    }

    public void Open()
    {
        GetComponent<Canvas>().enabled = true;
    }

    public void Close()
    {
        GetComponent<Canvas>().enabled = false;
    }

    void Start()
    {
        if (StartScreen.AssignAutomatically)
        {
            if (Screens.childCount != 0)
            {
                MenuScreen firstScreen = Screens.GetChild(0).GetComponent<MenuScreen>();
                if (firstScreen != null)
                    StartScreen.Screen = firstScreen;
            }
        }
        CurrentScreen = StartScreen.Screen;
    }

    public bool VisualizeScreenNavigation = true;
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (Screens != null)
        {
            return;
        }

        foreach (NavigationButton button in Screens.GetComponentsInChildren<NavigationButton>())
        {
            if (button.Target != null) 
                Gizmos.DrawLine(button.transform.position, button.Target.transform.position);
        }
    }
#endif
    private void OnEnable()
    {
        Controls.InGame.OpenMenu.performed += _ => Toggle();
        FindActiveEventSystem();
    }

    public void Quit()
    {
        GameManager.Instance.Quit();
    }

    public void MainMenu()
    {
        SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
    }
}

[Serializable]
public class StartScreen
{
    [Tooltip("If set to TRUE, the first screen in the hierarchy will be automatically selected as the start screen of this menu.")]
    public bool AssignAutomatically;
    public MenuScreen Screen;
}
