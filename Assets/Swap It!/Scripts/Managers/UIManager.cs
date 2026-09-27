using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour, IGameStateListener
{
    [Header("Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Level Popup Elements")]
    [Tooltip("Arka planı (Image) olan Ana Popup Objesi")]
    [SerializeField] private GameObject levelPopupContainer;

    [Tooltip("Sadece yazıyı değiştirmek için Text referansı")]
    [SerializeField] private TextMeshProUGUI levelPopupText;

    // YENİ: OnEnable yerine Start kullanıyoruz. (GameManager.Awake kesin bittikten sonra)
    private void Start()
    {
        GameManager.Instance?.RegisterListener(this);
    }

    // YENİ: OnDisable yerine OnDestroy kullanıyoruz ki obje kapatılsa bile listeden düşmesin, tamamen silinince düşsün.
    private void OnDestroy()
    {
        GameManager.Instance?.UnregisterListener(this);
    }

    public void GameStateChangedCallBack(EGameState gameState)
    {
        if (menuPanel != null) menuPanel.SetActive(gameState == EGameState.MENU);
        if (gamePanel != null) gamePanel.SetActive(gameState == EGameState.GAME);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(gameState == EGameState.LEVELCOMPLETE);
        if (gameOverPanel != null) gameOverPanel.SetActive(gameState == EGameState.GAMEOVER);

        if (gameState == EGameState.GAME)
        {
            ShowLevelPopup();
        }
    }

    private void ShowLevelPopup()
    {
        if (levelPopupContainer == null || levelPopupText == null) return;

        if (LevelManager.Instance != null)
        {
            levelPopupText.text = "LEVEL " + LevelManager.Instance.CurrentLevelNum.ToString();
        }

        LeanTween.cancel(levelPopupContainer);

        levelPopupContainer.SetActive(true);
        levelPopupContainer.transform.localScale = Vector3.zero;

        LeanTween.scale(levelPopupContainer, Vector3.one, 0.4f)
            .setEase(LeanTweenType.easeOutBack)
            .setOnComplete(() =>
            {
                LeanTween.scale(levelPopupContainer, Vector3.zero, 0.3f)
                    .setDelay(2.0f)
                    .setEase(LeanTweenType.easeInBack)
                    .setOnComplete(() =>
                    {
                        levelPopupContainer.SetActive(false);
                    });
            });
    }
}