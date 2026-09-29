using UnityEditor;
using UnityEngine;

public class PlayerPrefsClearTool
{
    [MenuItem("Tools/Clear PlayerPrefs")]
    public static void ClearPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        
        Debug.Log("<color=cyan>[TOOLS]</color> PlayerPrefs (Tüm Kayıtlar) başarıyla silindi! Oyun 1. Level'dan başlayacak.");
    }
}