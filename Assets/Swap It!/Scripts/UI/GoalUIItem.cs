using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GoalUIItem : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;
    
    [Header("Completion Feedback")]
    [Tooltip("Prefab içine eklediğin kapalı haldeki yeşil tik objesini buraya sürükle")]
    [SerializeField] private GameObject checkmarkObject; 

    private bool isCompleted = false;

    public void Setup(ItemType type, int amount, Sprite icon)
    {
        iconImage.sprite = icon;
        amountText.text = amount.ToString();
        
        // Başlangıçta tiki gizle, sayıyı göster ve durumu sıfırla
        checkmarkObject.SetActive(false);
        amountText.gameObject.SetActive(true);
        isCompleted = false;
    }

    public void UpdateAmount(int remainingAmount)
    {
        if (isCompleted) return; // Hedef zaten bittiyse (veya eksiye düşerse) tekrar animasyon oynatmayı engeller

        if (remainingAmount <= 0)
        {
            isCompleted = true;
            amountText.text = "0";

            LeanTween.cancel(gameObject);
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity; // Rotasyonu sıfırla

            // Kendi etrafında (Z ekseninde) 360 derece çevir
            LeanTween.rotateAroundLocal(gameObject, Vector3.forward, -360f, 0.5f)
                .setEase(LeanTweenType.easeOutBack) 
                .setOnComplete(() => 
                {
                    amountText.gameObject.SetActive(false); // Sayıyı gizle
                    checkmarkObject.SetActive(true);        // Yeşil tiki aç
                    
                    checkmarkObject.transform.localScale = Vector3.zero;
                    LeanTween.scale(checkmarkObject, Vector3.one, 0.3f).setEase(LeanTweenType.easeOutBounce);
                });
        }
        else
        {
            amountText.text = remainingAmount.ToString();
            
            LeanTween.cancel(gameObject);
            transform.localScale = Vector3.one;
            LeanTween.scale(gameObject, Vector3.one * 1.15f, 0.15f).setEasePunch();
        }
    }
}