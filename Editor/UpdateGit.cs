﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

public static class UpdateGit 
{
    [MenuItem("Window/Ishimine/UpdateGameStateMachine")]    
    public static void SelectMe()
    {
        AddRequest request = Client.Add("https://github.com/FelipeIshimine/GameStateMachineCore.git");
    }
}
