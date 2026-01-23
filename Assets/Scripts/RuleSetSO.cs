using UnityEngine;

public enum ButtonKind { Neutral, Green, Yellow, Black }

[CreateAssetMenu(menuName = "ButtonBooth/RuleSet", fileName = "RuleSet_Default")]
public class RuleSetSO : ScriptableObject
{
    [Header("Round")]
    public float roundDuration = 30f;
    public float spawnInterval = 0.35f; // à ajuster selon vitesse voulue
    public float onLifetime = 5f;
    public int maxOnAtOnce = 3;

    [Header("Kinds / Weights")]
    [Range(0,100)] public int weightGreen = 60;
    [Range(0,100)] public int weightYellow = 40;
    [Range(0,100)] public int weightNeutral = 0; // OFF = neutral visuel, mais pas "On"

    [Header("Black buttons (bombes)")]
    public int blackUnlockTour = 3;
    [Range(0,100)] public int weightBlackAfterUnlock = 10;

    [Header("Scoring")]
    public int pointsGreen = 3;
    public int pointsYellow = 2;
    public int pointsNeutral = 0;

    [Header("Black penalty (later)")]
    public int blackMoneyPenalty = 5;
    public int blackStrikesToLoseTourGains = 3;
}
