using UnityEngine;

public class GameStateProxy : MonoBehaviour
{
    public GameState GameState { get; private set; }

    public void Initialize<T>(T nGameState) where T : GameState
    {
        GameState = nGameState;
    }
}