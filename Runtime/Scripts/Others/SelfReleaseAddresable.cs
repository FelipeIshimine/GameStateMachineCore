using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class SelfReleaseAddresable : MonoBehaviour
{
    public GameStateWithAddressableAssets owner;

    public void ReleaseFromGameStateOwner()
    {
        owner.Release(this);
    }

    internal void Initialize(GameStateWithAddressableAssets gameStateWithAddressableAssets)
    {
        owner = gameStateWithAddressableAssets;
    }
}
