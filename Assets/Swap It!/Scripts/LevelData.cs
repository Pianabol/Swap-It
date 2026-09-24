using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Match3/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Board Dimensions")]
    [Min(3)] public int width = 8;
    [Min(3)] public int height = 10;

    [Header("Rules")]
    [Min(1)] public int availableMoves = 20;

    [Header("Available Colors (Pool Tespiti İçin)")]
    public List<ItemType> allowedColors = new List<ItemType>() 
    { 
        ItemType.Red, ItemType.Blue, ItemType.Green 
    };

    // Inspector'da 2D bir grid çizebilmek için bir satır (Row) sınıfı tanımlıyoruz.
    [System.Serializable]
    public class RowData
    {
        public CellType[] columns;
    }

    [Header("Map Layout (Y=0 En Alt Satırdır)")]
    // Haritanın topolojisini tutan liste. Her eleman bir yatay satırı temsil eder.
    public List<RowData> layout = new List<RowData>();

     
    public void ValidateLayout()
    {
        // Eğer layout listesi yükseklikten farklıysa, listeyi ayarla
        while (layout.Count < height)
        {
            layout.Add(new RowData { columns = new CellType[width] });
        }
        while (layout.Count > height)
        {
            layout.RemoveAt(layout.Count - 1);
        }

        // Her satırın (Row) genişliğini ayarla
        for (int y = 0; y < layout.Count; y++)
        {
            if (layout[y].columns == null || layout[y].columns.Length != width)
            {
                CellType[] newColumns = new CellType[width];
                // Eski verileri kopyalamaya çalış
                if (layout[y].columns != null)
                {
                    for (int x = 0; x < Mathf.Min(width, layout[y].columns.Length); x++)
                    {
                        newColumns[x] = layout[y].columns[x];
                    }
                }
                layout[y].columns = newColumns;
            }
        }
    }

    private void OnValidate()
    {
        ValidateLayout();
    }
}