using UnityEngine;
public enum GameStartMode
{
    LoadScene,              // Play → Load scene gameplay
    SingleSceneButton,      // Play → Hide menu → Start game in same scene
    SingleSceneTapToPlay    // Tap anywhere → Hide menu → Start game
}
[CreateAssetMenu(fileName = "GameDefinition", menuName = "Core/Game Definition")]
public class GameDefinition : ScriptableObject
{
    [Header("ID")]
    public string gameId = "default_game";

    [Header("Gameplay Start Mode")]
    public GameStartMode startMode = GameStartMode.LoadScene;
    public string gameplaySceneName;

    [Header("UI Options")]
    public bool hasShop = true;
    public bool hasSkin = true;
    public bool showLevel = true;
    public bool showBottomButtons = true;

    [Header("Visual")]
    public Sprite logo;
    public Sprite background;
    public Color themeColor = Color.white;
}
