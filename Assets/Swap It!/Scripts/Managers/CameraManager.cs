using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    [Header("Zorunlu Kadraj Ayarları")]
    [Tooltip("Çerçevelerin (Border) ekranın kenarına yapışmaması için fiziksel boşluk")]
    public float padding = 2.0f; 
    
    [Tooltip("Üstte skor/hedef UI'ı varsa tahtayı aşağı kaydırmak için eksi değer gir")]
    public float screenYOffset = -1.0f; 

    private void Awake()
    {
        Instance = this;
    }

    public void FrameBoard(Board board)
    {
        // Çözünürlüğün ve objelerin sahnede tam oluşması için 1 kare (frame) bekletiyoruz.
        StartCoroutine(ForceFrameRoutine(board));
    }

    private IEnumerator ForceFrameRoutine(Board board)
    {
        yield return new WaitForEndOfFrame();

        Camera cam = Camera.main;
        if (cam == null || board == null) yield break;

        // 1. ZORUNLU SINIR HESABI (TÜM OBJELERİ KAPSAR)
        // Board'un altındaki Renderer'a (Mesh) sahip her şeyi (Border, Tile, Item) bul.
        Renderer[] allRenderers = board.GetComponentsInChildren<Renderer>();
        if (allRenderers.Length == 0) yield break;

        // İlk objenin sınırlarıyla bir kutu (Bounds) başlat
        Bounds totalBounds = allRenderers[0].bounds;
        
        // Diğer tüm objeleri bu kutunun içine dahil et (Kutu giderek büyüyecek ve tüm board'u kaplayacak)
        foreach (Renderer r in allRenderers)
        {
            totalBounds.Encapsulate(r.bounds);
        }

        // 2. KAMERAYI ZORLA MERKEZLE
        Vector3 targetCenter = totalBounds.center;

       
        cam.transform.position = targetCenter - (cam.transform.forward * 20f);

        // UI için Y ekseninde (Kameranın kendi yukarı yönünde) offset uygula
        cam.transform.position += cam.transform.up * screenYOffset;

        // 3. EKRAN TAŞMASINI ENGELLEYEN KESİN ZOOM (ORTHOGRAPHIC SIZE)
        // Kapsayıcı kutunun X genişliği ve Z derinliği
        float physicalWidth = totalBounds.size.x + padding;
        float physicalLength = totalBounds.size.z + padding;

        // Kameranın X açısına göre eğik derinliği (Apparent Height) hesaplıyoruz
        float angleRad = cam.transform.eulerAngles.x * Mathf.Deg2Rad;
        float apparentHeight = (physicalLength * Mathf.Sin(angleRad)) + (totalBounds.size.y * Mathf.Cos(angleRad));

        float screenAspect = (float)Screen.width / (float)Screen.height;
        
        // Cihazın ekranına göre gereken X ve Y zoom seviyeleri
        float requiredSizeX = physicalWidth / 2f / screenAspect;
        float requiredSizeY = apparentHeight / 2f;

        // Ekrandan asla taşmaması için büyük olan zoom değerini zorunlu kılıyoruz
        cam.orthographicSize = Mathf.Max(requiredSizeX, requiredSizeY);
        
        Debug.Log($"<color=magenta>[CAMERA MANAGER]</color> Kadraj Zorlandı! Kapsanan Hacim: {totalBounds.size}. Tüm Border'lar ekran içinde.");
    }
}