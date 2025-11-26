using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SlopFling/Skin Database")]
public class SkinDatabase : ScriptableObject
{
    [Serializable]
    public class SkinItem
    {
        public string skinId;           
        public string skinName;
        public Sprite icon;            
        public int price;               // 0 = free
        public bool unlockedByDefault;  // skin mặc định = true
        public Material material;  
        public Mesh mesh; 
    }

    public SkinItem[] skins;

    private Dictionary<string, SkinItem> _byId;

    private void OnEnable()
    {
        BuildMap();
    }

    private void BuildMap()
    {
        _byId = new Dictionary<string, SkinItem>();
        if (skins == null) return;
        foreach (var s in skins)
        {
            if (s == null || string.IsNullOrEmpty(s.skinId)) continue;
            _byId[s.skinId] = s;
        }
    }

    public int Count => skins != null ? skins.Length : 0;

    public SkinItem GetByIndex(int index)
    {
        if (skins == null || index < 0 || index >= skins.Length) return null;
        return skins[index];
    }

    public SkinItem GetById(string id)
    {
        if (string.IsNullOrEmpty(id) || _byId == null) return null;
        _byId.TryGetValue(id, out var skin);
        return skin;
    }

    public int IndexOf(string id)
    {
        if (skins == null) return -1;
        for (int i = 0; i < skins.Length; i++)
        {
            if (skins[i] != null && skins[i].skinId == id)
                return i;
        }
        return -1;
    }
}
