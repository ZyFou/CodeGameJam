// BoardManager.cs
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RuleSetSO rules;

    [Header("Board (drag & drop in order)")]
    [SerializeField] private List<ArcadeButton> buttons = new();

    [Header("Debug")]
    public bool playOnStart = true;

    [Tooltip("Tour courant (1 tour = rules.roundsPerTour rounds)")]
    public int currentTourIndex = 1;

    [Tooltip("Round dans le tour (1..rules.roundsPerTour)")]
    public int currentRoundInTour = 1;

    struct ButtonRuntime
    {
        public bool isOn;
        public ButtonKind kind;
        public float offAt;
    }

    ButtonRuntime[] rt;

    float roundEndAt;
    float nextSpawnAt;
    bool roundRunning;

    int score;
    int blackStrikes;

    // COMBO runtime
    int greenStreak;
    int comboStack;
    int comboErrors;
    float comboExpireAt;

    void Awake()
    {
        if (rules == null)
        {
            Debug.LogError("BoardManager: rules (RuleSetSO) is missing. Assign a RuleSet asset in the inspector.");
            return;
        }

        rt = new ButtonRuntime[buttons.Count];

        for (int i = 0; i < buttons.Count; i++)
        {
            if (buttons[i] == null)
            {
                Debug.LogError($"BoardManager: buttons[{i}] is null. Fill the list in inspector.");
                continue;
            }

            buttons[i].Init(this, i);
            SetButton(i, ButtonKind.Neutral, false);
        }
    }

    void Start()
    {
        if (playOnStart)
            StartRound(currentTourIndex, currentRoundInTour);
    }

    void Update()
    {
        if (!roundRunning) return;

        float t = Time.time;

        // fin de round
        if (t >= roundEndAt)
        {
            EndRound();
            return;
        }

        // combo expire
        if (IsComboUnlocked() && comboStack > 0 && t >= comboExpireAt)
        {
            Debug.Log($"[COMBO] expired. stack was {comboStack}");
            ResetCombo();
        }

        // auto-off
        for (int i = 0; i < rt.Length; i++)
        {
            if (rt[i].isOn && t >= rt[i].offAt)
                SetButton(i, ButtonKind.Neutral, false);
        }

        // spawn (avec pauses)
        if (t >= nextSpawnAt)
        {
            nextSpawnAt = t + rules.spawnInterval;

            if (Random.value <= rules.spawnChance)
                TrySpawnOne();
        }
    }

    // -------------------------
    // ROUND CONTROL
    // -------------------------
    public void StartRound(int tourIndex, int roundInTour)
    {
        currentTourIndex = tourIndex;
        currentRoundInTour = Mathf.Clamp(roundInTour, 1, Mathf.Max(1, rules.roundsPerTour));

        score = 0;
        blackStrikes = 0;

        // reset combo runtime
        greenStreak = 0;
        comboStack = 0;
        comboErrors = 0;
        comboExpireAt = 0f;

        // reset board
        for (int i = 0; i < rt.Length; i++)
            SetButton(i, ButtonKind.Neutral, false);

        roundRunning = true;
        roundEndAt = Time.time + rules.roundDuration;
        nextSpawnAt = Time.time + 0.1f;

        Debug.Log($"Round started (Tour {currentTourIndex}, Round {currentRoundInTour}/{rules.roundsPerTour})");
    }

    void EndRound()
    {
        roundRunning = false;
        Debug.Log($"Round ended. Score={score}, BlackStrikes={blackStrikes}, ComboStack={comboStack}");
        // brancher ici plus tard: score->argent, tickets, UI, etc.
    }

    // -------------------------
    // INPUT
    // -------------------------
    public void OnButtonPressed(int index)
    {
        if (!roundRunning) return;
        if (index < 0 || index >= rt.Length) return;

        // clic sur bouton OFF -> miss (optionnel: erreur combo)
        if (!rt[index].isOn)
        {
            if (rules.countOffClickAsComboError)
                RegisterComboError("clicked_off");

            Debug.Log($"[MISS] idx={index} OFF => score={score}");
            return;
        }

        ButtonKind kind = rt[index].kind;
        int gained = 0;

        if (kind == ButtonKind.Green)
        {
            greenStreak++;
            gained = rules.pointsGreen;

            if (IsComboUnlocked())
            {
                if (greenStreak >= rules.comboStartAfterGreens)
                {
                    comboStack = Mathf.Min(comboStack + 1, rules.comboMaxStack);
                    comboErrors = 0;
                    comboExpireAt = Time.time + rules.comboExpireSeconds;

                    if (rules.useMultiplier)
                    {
                        float mult = 1f + comboStack * rules.comboMultiplierStep;
                        gained = Mathf.RoundToInt(gained * mult);
                    }
                    else
                    {
                        gained += rules.comboBonusPerGreen * comboStack;
                    }

                    Debug.Log($"[COMBO] streak={greenStreak} stack={comboStack} gained={gained}");
                }
                else
                {
                    comboExpireAt = Time.time + rules.comboExpireSeconds;
                }
            }
        }
        else if (kind == ButtonKind.Yellow)
        {
            gained = rules.pointsYellow;
            greenStreak = 0;
            RegisterComboError("yellow");
        }
        else if (kind == ButtonKind.Black)
        {
            greenStreak = 0;
            blackStrikes++;
            RegisterComboError("black");
        }
        else
        {
            gained = rules.pointsNeutral;
            greenStreak = 0;
            RegisterComboError("neutral");
        }

        score += gained;

        // le bouton redevient neutral/off après clic
        SetButton(index, ButtonKind.Neutral, false);

        Debug.Log($"[CLICK] idx={index} kind={kind} +{gained} => score={score}");
    }

    // -------------------------
    // SPAWN
    // -------------------------
    void TrySpawnOne()
    {
        // limit simultané
        int onCount = 0;
        for (int i = 0; i < rt.Length; i++)
            if (rt[i].isOn) onCount++;

        if (onCount >= rules.maxOnAtOnce) return;

        // indices OFF
        List<int> off = new List<int>(rt.Length);
        for (int i = 0; i < rt.Length; i++)
            if (!rt[i].isOn)
                off.Add(i);

        if (off.Count == 0) return;

        int idx = off[Random.Range(0, off.Count)];
        ButtonKind kind = RollKind();

        // si on roll Neutral mais allowNeutralOn=false => on ne spawn pas
        if (kind == ButtonKind.Neutral && !rules.allowNeutralOn)
            return;

        SetButton(idx, kind, true);
        rt[idx].offAt = Time.time + rules.onLifetime;
    }

    ButtonKind RollKind()
    {
        bool blackAllowed = rules.allowBlack && (currentTourIndex >= rules.blackUnlockTour);

        int wGreen  = rules.allowGreen ? rules.weightGreen : 0;
        int wYellow = rules.allowYellow ? rules.weightYellow : 0;
        int wNeutral = rules.allowNeutralOn ? rules.weightNeutral : 0;
        int wBlack  = blackAllowed ? rules.weightBlackAfterUnlock : 0;

        int total = wGreen + wYellow + wNeutral + wBlack;
        if (total <= 0) return ButtonKind.Neutral;

        int roll = Random.Range(0, total);

        if (wBlack > 0)
        {
            if (roll < wBlack) return ButtonKind.Black;
            roll -= wBlack;
        }

        if (roll < wGreen) return ButtonKind.Green;
        roll -= wGreen;

        if (roll < wYellow) return ButtonKind.Yellow;
        return ButtonKind.Neutral;
    }

    void SetButton(int index, ButtonKind kind, bool on)
    {
        rt[index].kind = kind;
        rt[index].isOn = on;
        rt[index].offAt = on ? Time.time + rules.onLifetime : 0f;

        if (buttons[index] != null)
            buttons[index].SetVisual(kind, on);
    }

    // -------------------------
    // COMBO HELPERS
    // -------------------------
    bool IsComboUnlocked()
    {
        if (!rules.comboEnabled) return false;

        if (currentTourIndex < rules.comboUnlockTour) return false;
        if (currentTourIndex > rules.comboUnlockTour) return true;

        // même tour
        return currentRoundInTour >= rules.comboUnlockRoundInTour;
    }

    void RegisterComboError(string reason)
    {
        if (!IsComboUnlocked()) return;
        if (rules.comboResetAfterErrors <= 0) return;

        comboErrors++;
        if (comboErrors >= rules.comboResetAfterErrors)
        {
            Debug.Log($"[COMBO] reset by error ({reason}). errors={comboErrors}");
            ResetCombo();
        }
    }

    void ResetCombo()
    {
        greenStreak = 0;
        comboStack = 0;
        comboErrors = 0;
        comboExpireAt = 0f;
    }
}
