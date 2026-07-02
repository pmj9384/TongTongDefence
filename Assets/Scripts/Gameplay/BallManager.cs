using UnityEngine;

public class BallManager : InGameManager
{
    public GameManager.GameState CurrentGameState => GameManager.CurrentState;
    public ObjectPoolManager ObjectPool => GameManager.ObjectPool;
    public FieldManager Field => GameManager.FieldManager;
}
