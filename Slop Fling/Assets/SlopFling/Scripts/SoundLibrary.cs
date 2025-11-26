using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SlopFling/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public SoundId id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [SerializeField] private Entry[] entries;

    private Dictionary<SoundId, Entry> _map;

    private void OnEnable()
    {
        BuildMap();
    }

    private void BuildMap()
    {
        _map = new Dictionary<SoundId, Entry>();
        if (entries == null) return;

        foreach (var e in entries)
        {
            if (e == null || e.clip == null) continue;
            _map[e.id] = e;
        }
    }

    public AudioClip GetClip(SoundId id, out float volume)
    {
        volume = 1f;
        if (_map == null || !_map.TryGetValue(id, out var entry) || entry.clip == null)
            return null;

        volume = entry.volume;
        return entry.clip;
    }
}
