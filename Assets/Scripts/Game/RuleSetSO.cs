// RuleSetSO.cs
using UnityEngine;

public enum ButtonKind { Neutral, Green, Yellow, Black }

[CreateAssetMenu(menuName = "ButtonBooth/RuleSet", fileName = "RuleSet_Default")]
public class RuleSetSO : ScriptableObject
{
    [Header("Tour / Round structure")]
    public int roundsPerTour = 3; // par défaut: 1 tour = 3 rounds

    // -----------------------
    // ROUND GAMEPLAY
    // -----------------------
    [Header("Round")]
    public float roundDuration = 30f;
    public float spawnInterval = 1f;
    public float onLifetime = 5f;
    public int maxOnAtOnce = 3;

    [Header("Spawn gaps (pauses)")]
    [Range(0f, 1f)] public float spawnChance = 0.6f; // 60% spawn, 40% pause

    [Header("Allowed kinds at spawn (rule-driven)")]
    public bool allowGreen = true;
    public bool allowYellow = true;
    public bool allowNeutralOn = false; // si tu veux des boutons "gris allumés"
    public bool allowBlack = true;      // désactive en mode si besoin

    [Header("Kinds / Weights")]
    [Range(0,100)] public int weightGreen = 60;
    [Range(0,100)] public int weightYellow = 40;
    [Range(0,100)] public int weightNeutral = 0;

    [Header("Black buttons (bombes)")]
    public int blackUnlockTour = 3;           // pas de noir avant tour 3
    [Range(0,100)] public int weightBlackAfterUnlock = 8;

    [Header("Scoring")]
    public int pointsGreen = 3;
    public int pointsYellow = 1;
    public int pointsNeutral = 0;

    // -----------------------
    // COMBO (100% configurable)
    // -----------------------
    [Header("Combo - Enable / Unlock (Tour + Round-in-Tour)")]
    public bool comboEnabled = true;

    // Exemple: Tour 2 Round 1 => combo dès le début du Tour 2
    public int comboUnlockTour = 2;
    [Range(1, 10)] public int comboUnlockRoundInTour = 1;

    [Header("Combo - Start condition")]
    public int comboStartAfterGreens = 3;  // combo démarre après X verts d'affilée
    public int comboMaxStack = 20;

    [Header("Combo - Scoring")]
    public int comboBonusPerGreen = 1;     // bonus ajouté sur un green quand combo actif
    public bool useMultiplier = false;
    public float comboMultiplierStep = 0.1f;

    [Header("Combo - Expire / Reset")]
    public float comboExpireSeconds = 1.5f; // expire si pas de vert pendant X sec

    [Tooltip("Combien d'erreurs pour reset le combo (jaune, noir, neutral, clic sur off...)")]
    public int comboResetAfterErrors = 1;

    [Tooltip("Compter un clic sur un bouton OFF comme une erreur combo ?")]
    public bool countOffClickAsComboError = true;

    // -----------------------
    // ECONOMY
    // -----------------------
    [Header("Economy - Start")]
    public int startMoney = 10;
    public int startTickets = 2;

    [Header("Economy - Entry cost")]
    [Tooltip("Coût d'entrée par round dans le tour (index 0=round1). Ex: 5/12/20")]
    public int[] entryCostBaseByRoundInTour = new int[] { 5, 12, 20 };

    [Tooltip("Augmentation ajoutée à TOUS les coûts à chaque tour passé. Ex: +3 => Tour2 = base+3")]
    public int entryCostAddPerTour = 3;

    [Header("Economy - Score -> Money")]
    [Tooltip("moneyGain = floor(score / scoreToMoneyK)")]
    public int scoreToMoneyK = 10;

    // -----------------------
    // TICKETS
    // -----------------------
    [Header("Tickets - per played round")]
    public int ticketsPerPlayedRound = 1; // +1 ticket à la fin d'un round joué

    [Header("Tour bonus ticket pool")]
    [Tooltip("Pool de tickets au début du tour (ex 6)")]
    public int tourBonusTicketsStart = 6;

    [Tooltip("Combien le pool baisse par round consommé (joué OU skip). Ex 2 => 6/4/2/0")]
    public int tourBonusTicketsDecayPerRound = 2;
}
