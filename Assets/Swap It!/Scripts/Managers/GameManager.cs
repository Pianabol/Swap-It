using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private EGameState initialGameState = EGameState.MENU;
    private EGameState gameState;

    // Sahneyi taramayıp listener'ları hafızada tutuyoruz (Sıfır GC, O(1) maliyet)
    private readonly List<IGameStateListener> listeners = new List<IGameStateListener>();

    public static event Action<EGameState> OnGameStateChanged;

    public EGameState CurrentState => gameState;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetGameState(initialGameState);
    }

    #region Listener Registration (Observer Pattern)
    public void RegisterListener(IGameStateListener listener)
    {
        if (!listeners.Contains(listener))
        {
            listeners.Add(listener);
            listener.GameStateChangedCallBack(gameState);
        }
    }

    public void UnregisterListener(IGameStateListener listener)
    {
        if (listeners.Contains(listener))
        {
            listeners.Remove(listener);
        }
    }
    #endregion

    public void SetGameState(EGameState newState)
    {
        if (gameState == newState && Time.frameCount > 0) return;

        gameState = newState;
        Debug.Log($"<color=magenta>[GAME STATE]</color> Durum değişti: <b>{gameState}</b>");

        // Interface dinleyicilerini tetikle
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i]?.GameStateChangedCallBack(gameState);
        }

        // C# Event dinleyicilerini tetikle
        OnGameStateChanged?.Invoke(gameState);
    }

    #region Flow & UI Callbacks
    public void StartGame()
    {
        SetGameState(EGameState.GAME);
    }

    public void NextButtonCallBack()
    {
        // Sonsuz döngü: Bir sonraki seviyeye geçerken sahneyi yeniler
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RetryButtonCallBack()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RestartGameCallBack()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public bool IsGame() => gameState == EGameState.GAME;
    #endregion
}