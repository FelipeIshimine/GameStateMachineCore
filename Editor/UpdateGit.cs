using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

public static class UpdateGit 
{
    [MenuItem("Window/Ishimine/Update/GameStateMachine")]    
    public static void SelectMe() => Client.Add("https://github.com/FelipeIshimine/GameStateMachineCore.git");
}
