/*
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private Board board;
    [SerializeField] private ItemPool itemPool;

    private void OnEnable()
    {
        // Board üzerinde bir eşya patladığında beni haberdar et (Havuza iade etmek için)
        if (board != null)
            board.ItemDestroyedEvent += OnItemDestroyedOnBoard;
    }

    private void OnDisable()
    {
        if (board != null)
            board.ItemDestroyedEvent -= OnItemDestroyedOnBoard;
    }

    private void Start()
    {
        GenerateLevel();
    }

    private void GenerateLevel()
    {
        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                Item safeItem = GetSafeItemForPosition(x, y);
                board.PlaceItemAt(safeItem, x, y);
            }
        }
        Debug.Log("<color=cyan>[LEVEL MANAGER]</color> Level dolduruldu.");
    }

    private Item GetSafeItemForPosition(int x, int y)
    {
        int maxAttempts = 100;
        int attempts = 0;
        Item newItem;

        do
        {
            newItem = itemPool.GetRandomItem();
            attempts++;

            // Eğer çektiğimiz bu renk, yatay veya dikeyde 3'lü yaratıyorsa:
            if (CausesMatch(newItem.ItemType, x, y))
            {
                // Bu item işimize yaramadı, onu havuza iade et ve döngüye devam edip yeni renk çek
                itemPool.ReturnItem(newItem);
                newItem = null; // null yaparak while kontrolünün patlamamasını sağlıyoruz
            }

        } while (newItem == null && attempts < maxAttempts);

        if (attempts >= maxAttempts)
        {
            Debug.LogWarning($"[{x},{y}] için güvenli renk bulunamadı! Son çekilen item yerleştiriliyor.");
            newItem = itemPool.GetRandomItem(); // Fail-safe
        }

        return newItem;
    }

    private bool CausesMatch(ItemType type, int x, int y)
    {
        if (x >= 2)
        {
            Item left1 = board.GetItemAt(x - 1, y);
            Item left2 = board.GetItemAt(x - 2, y);
            if (left1 != null && left2 != null && left1.ItemType == type && left2.ItemType == type) return true;
        }

        if (y >= 2)
        {
            Item down1 = board.GetItemAt(x, y - 1);
            Item down2 = board.GetItemAt(x, y - 2);
            if (down1 != null && down2 != null && down1.ItemType == type && down2.ItemType == type) return true;
        }

        return false;
    }

    // Board "Şu obje patladı" dediğinde LevelManager o objeyi alıp havuza koyar.
    private void OnItemDestroyedOnBoard(Item itemToReturn)
    {
        if (itemPool != null && itemToReturn != null)
        {
            itemPool.ReturnItem(itemToReturn);
        }
    }
}
*/

using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private Board board;
    [SerializeField] private ItemPool itemPool;

    private void OnEnable()
    {
        if (board != null)
        {
            board.ItemDestroyedEvent += OnItemDestroyedOnBoard;
            // YENİ: Board'un Gravity sinyalini dinlemeye başla
            board.OnGravityFinished += RefillBoard; 
        }
    }

    private void OnDisable()
    {
        if (board != null)
        {
            board.ItemDestroyedEvent -= OnItemDestroyedOnBoard;
            board.OnGravityFinished -= RefillBoard; 
        }
    }

    private void Start()
    {
        GenerateLevel();
    }

    private void GenerateLevel()
    {
        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                Item safeItem = GetSafeItemForPosition(x, y);
                board.PlaceItemAt(safeItem, x, y);
            }
        }
        Debug.Log("<color=cyan>[LEVEL MANAGER]</color> Level dolduruldu.");
    }

    private Item GetSafeItemForPosition(int x, int y)
    {
        int maxAttempts = 100;
        int attempts = 0;
        Item newItem;

        do
        {
            newItem = itemPool.GetRandomItem();
            attempts++;

            if (CausesMatch(newItem.ItemType, x, y))
            {
                itemPool.ReturnItem(newItem);
                newItem = null; 
            }

        } while (newItem == null && attempts < maxAttempts);

        if (attempts >= maxAttempts)
        {
            newItem = itemPool.GetRandomItem(); 
        }

        return newItem;
    }

    private bool CausesMatch(ItemType type, int x, int y)
    {
        if (x >= 2)
        {
            Item left1 = board.GetItemAt(x - 1, y);
            Item left2 = board.GetItemAt(x - 2, y);
            if (left1 != null && left2 != null && left1.ItemType == type && left2.ItemType == type) return true;
        }

        if (y >= 2)
        {
            Item down1 = board.GetItemAt(x, y - 1);
            Item down2 = board.GetItemAt(x, y - 2);
            if (down1 != null && down2 != null && down1.ItemType == type && down2.ItemType == type) return true;
        }

        return false;
    }

    private void OnItemDestroyedOnBoard(Item itemToReturn)
    {
        if (itemPool != null && itemToReturn != null)
        {
            itemPool.ReturnItem(itemToReturn);
        }
    }

    // --- YENİ: DÜZELTİLMİŞ REFILL VE CASCADE MANTIĞI ---
    private void RefillBoard()
    {
        // Animasyonların bitmesini bekleyebilmek için işlemi Coroutine'e devrediyoruz
        StartCoroutine(RefillRoutine());
    }

    private IEnumerator RefillRoutine()
    {
        bool refilledAny = false;
        
        // Her sütunda (X ekseninde) kaç tane yeni blok spawn ettiğimizi tutacağız.
        // Böylece bloklar image_9240d8.jpg'deki gibi üst üste binmeyecek, yukarı doğru sıraya girecek.
        int[] spawnCounts = new int[board.Width]; 

        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                if (board.GetItemAt(x, y) == null)
                {
                    Item newItem = itemPool.GetRandomItem();
                    board.PlaceItemAt(newItem, x, y); // Mantıksal olarak (x,y)'ye yerleşti

                    // FİZİKSEL DOĞUM YERİ: Tahtanın tavanı + bu sütunda daha önce spawn olanların sayısı
                    float spawnY = board.Height + spawnCounts[x];
                    newItem.transform.position = board.GridToWorld(x, (int)spawnY);

                    // LEANTWEEN İLE DÜŞÜŞ: Gerçek hedefine (y) doğru 0.4 saniyede düşsün
                    LeanTween.move(newItem.gameObject, board.GridToWorld(x, y), 0.4f).setEaseOutQuad();

                    spawnCounts[x]++; // Bu sütuna bir blok daha eklendi, sonrakini daha yukarıdan başlat
                    refilledAny = true;
                }
            }
        }

        if (refilledAny)
        {
            Debug.Log("<color=cyan>[REFILL]</color> Bloklar sırayla yağıyor, düşmeleri bekleniyor...");
            
            // Yeni gelen bloklar havadayken eşleşme (Cascade) kontrolü Y-A-P-I-L-M-A-Z!
            // Animasyon süresi kadar (0.5 saniye) bekliyoruz ki bloklar yerine otursun.
            yield return new WaitForSeconds(0.5f);
            
            // Bloklar yerine oturdu. Şimdi zincirleme reaksiyon (Cascade) var mı kontrol et!
            board.CheckAndResolveMatches(); 
        }
    }
}