using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LevelGoal
{
    public ItemType targetItemType;
    public int targetAmount;
}

[System.Serializable]
public class RowData
{
    public CellType[] columns;
}

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Match3/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    [Tooltip("Bu level'ın kaçıncı seviye olduğunu buraya yazın (Örn: 1, 5, 12)")]
    public int levelNumber = 1;

    [Header("Board Dimensions")]
    [Min(3)] public int width = 8;
    [Min(3)] public int height = 10;

    [Header("Level Conditions")]
    public int maxMoves = 20;
    public List<LevelGoal> levelGoals;

    [Header("Available Colors")]
    public List<ItemType> allowedColors = new List<ItemType>() 
    { 
        ItemType.Red, ItemType.Blue, ItemType.Green 
    };

    [Header("Map Layout (Y=0 En Alt Satırdır)")]
    public List<RowData> layout = new List<RowData>();
     
    public void ValidateLayout()
    {
        while (layout.Count < height)
        {
            layout.Add(new RowData { columns = new CellType[width] });
        }
        while (layout.Count > height)
        {
            layout.RemoveAt(layout.Count - 1);
        }

        for (int y = 0; y < layout.Count; y++)
        {
            if (layout[y].columns == null || layout[y].columns.Length != width)
            {
                CellType[] newColumns = new CellType[width];
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