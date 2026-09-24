using System.Collections.Generic;
using UnityEngine;

public class ItemPool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private Item[] itemPrefabs; // Tüm renklerin (tiplerin) prefablari
    [SerializeField] private int initialPoolSizePerType = 20; // Her renkten kaç tane yedeklenecek?
    [SerializeField] private Transform poolRoot; // Havuz objelerinin sahnede duracağı yer

    private Dictionary<ItemType, Queue<Item>> poolDictionary;

    private void Awake()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        poolDictionary = new Dictionary<ItemType, Queue<Item>>();

        if (poolRoot == null)
        {
            GameObject root = new GameObject("PoolRoot");
            root.transform.SetParent(this.transform);
            poolRoot = root.transform;
        }

        foreach (Item prefab in itemPrefabs)
        {
            ItemType type = prefab.ItemType;
            Queue<Item> objectPool = new Queue<Item>();

            for (int i = 0; i < initialPoolSizePerType; i++)
            {
                Item newObj = Instantiate(prefab, poolRoot);
                newObj.gameObject.SetActive(false); // Objeyi uyut (Havuza at)
                objectPool.Enqueue(newObj);
            }

            poolDictionary.Add(type, objectPool);
        }
    }

    /// <summary>
    /// Rastgele bir renkte Item çekmek için kullanılır (Refill sırasında).
    /// </summary>
    public Item GetRandomItem()
    {
        if (itemPrefabs.Length == 0) return null;

        int randomIndex = Random.Range(0, itemPrefabs.Length);
        ItemType randomType = itemPrefabs[randomIndex].ItemType;

        return GetItem(randomType);
    }

    /// <summary>
    /// Havuzdan belirli bir tipte (renkte) uyuyan bir objeyi uyandırır ve verir.
    /// Havuzda kalmadıysa acil durum olarak Instantiate eder (Expand).
    /// </summary>
    public Item GetItem(ItemType type)
    {
        if (!poolDictionary.ContainsKey(type))
        {
            Debug.LogError($"[POOL ERROR] Havuzda {type} tipi bulunamadı!");
            return null;
        }

        Queue<Item> pool = poolDictionary[type];

        if (pool.Count > 0)
        {
            Item itemToReuse = pool.Dequeue();
            itemToReuse.gameObject.SetActive(true); // Uykudan uyan
            
            // Havuz kökünden çıkartıp serbest bırakıyoruz ki Board kendi Root'una alabilsin
            itemToReuse.transform.SetParent(null); 
            return itemToReuse;
        }
        else
        {
            // Havuzdaki tüm objeler sahnede aktifse (nadir durum), yeni bir tane yarat (Expand).
            Debug.LogWarning($"[POOL EXPAND] {type} havuzu yetersiz kaldı, yeni yaratılıyor!");
            Item prefabToSpawn = GetPrefabByType(type);
            Item newItem = Instantiate(prefabToSpawn);
            return newItem;
        }
    }

    
    public void ReturnItem(Item itemToReturn)
    {
        itemToReturn.gameObject.SetActive(false); // Objeyi kapat
        itemToReturn.transform.position = Vector3.zero; // Gözden uzaklaştır
        itemToReturn.transform.SetParent(poolRoot); // Hierarchy'de tekrar havuza sok

        ItemType type = itemToReturn.ItemType;
        
        if (poolDictionary.ContainsKey(type))
        {
            poolDictionary[type].Enqueue(itemToReturn);
        }
        else
        {
            // Eğer havuz yapısı bozulduysa mecburen Destroy et.
            Destroy(itemToReturn.gameObject);
        }
    }

    private Item GetPrefabByType(ItemType type)
    {
        foreach (Item prefab in itemPrefabs)
        {
            if (prefab.ItemType == type) return prefab;
        }
        return null;
    }
}