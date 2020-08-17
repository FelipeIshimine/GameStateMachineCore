using System;
using System.Collections.Generic;
using UnityEngine;
//using ScreenTransitionSystem;

public class RootState : GameStateWithAddressableAssets
{
    public static bool PrintDebug = true;

    private static RootState instance;
    public static RootState Instance
    {
        get 
        {
            if (instance == null)
                instance = new RootState();
            return instance; 
        }
    }

    [RuntimeInitializeOnLoadMethod]
    public static void Initialize()
    {
        if (PrintDebug) Debug.Log($"<Color=green> RootState->Initialize() </color>");
        Instance.SwitchState(Instance);
        //OnInstantiationProgress += ScreenTransitionSystem.ScreenTransition.SetProgress;
    }

    public override void OnEnter()
    {
        if (PrintDebug) Debug.Log("<Color=green> GameManager State ENTER </color>");
    }

    public override void OnExit()
    {
        //OnInstantiationProgress -= ScreenTransitionSystem.ScreenTransition.SetProgress;
    }
}
