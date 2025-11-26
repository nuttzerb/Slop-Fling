using UnityEngine;

public class BallSkinController : MonoBehaviour
{
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;

    [SerializeField] private SkinDatabase skinDb;

    private const string PREF_CURRENT_SKIN = "SF_CurrentSkinId";

    private void Awake()
    {
        if (!meshFilter)   meshFilter = GetComponentInChildren<MeshFilter>();
        if (!meshRenderer) meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    private void Start()
    {
        ApplyCurrentSkin();
    }

    public void ApplyCurrentSkin()
    {
        if (skinDb == null || meshFilter == null || meshRenderer == null)
            return;

        string id = PlayerPrefs.GetString(PREF_CURRENT_SKIN, "");
        var skin = skinDb.GetById(id);

        if (skin == null) return;

        if (skin.mesh != null)
            meshFilter.sharedMesh = skin.mesh;

        if (skin.material != null)
            meshRenderer.sharedMaterial = skin.material;
    }
}
