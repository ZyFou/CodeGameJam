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
    public int currentTour = 1;

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

    void Awake()
    {
        if (rules == null)
        {
            Debug.LogError("BoardManager: rules (RuleSetSO) is missing.");
            enabled = false;
            return;
        }

        rt = new ButtonRuntime[buttons.Count];

        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].Init(this, i);
            SetButton(i, ButtonKind.Neutral, false);
        }
    }

    void Start()
    {
        if (playOnStart)
            StartRound();
    }

    void Update()
    {
        if (!roundRunning) return;

        float t = Time.time;

        // fin de manche
        if (t >= roundEndAt)
        {
            EndRound();
            return;
        }

        // auto-off
        for (int i = 0; i < rt.Length; i++)
        {
            if (rt[i].isOn && t >= rt[i].offAt)
                SetButton(i, ButtonKind.Neutral, false);
        }

        // spawn
        if (t >= nextSpawnAt)
        {
            nextSpawnAt = t + rules.spawnInterval;
            TrySpawnOne();
        }
    }

    public void StartRound()
    {
        score = 0;
        blackStrikes = 0;

        // reset board
        for (int i = 0; i < rt.Length; i++)
            SetButton(i, ButtonKind.Neutral, false);

        roundRunning = true;
        roundEndAt = Time.time + rules.roundDuration;
        nextSpawnAt = Time.time + 0.1f;

        Debug.Log($"Round started (Tour {currentTour})");
    }

    void EndRound()
    {
        roundRunning = false;
        Debug.Log($"Round ended. Score={score}, BlackStrikes={blackStrikes}");
        // Ici tu brancheras: score -> argent, tickets, UI, etc.
    }

    public void OnButtonPressed(int index)
    {
        if (!roundRunning) return;
        if (index < 0 || index >= rt.Length) return;
        if (!rt[index].isOn) return;

        var kind = rt[index].kind;

        // scoring MVP (tu brancheras argent/tickets plus tard)
        switch (kind)
        {
            case ButtonKind.Green:  score += rules.pointsGreen; break;
            case ButtonKind.Yellow: score += rules.pointsYellow; break;
            case ButtonKind.Black:  blackStrikes++; break;
        }

        // consomme le bouton
        SetButton(index, ButtonKind.Neutral, false);
    }

    void TrySpawnOne()
    {
        // limit simultané
        int onCount = 0;
        for (int i = 0; i < rt.Length; i++)
            if (rt[i].isOn) onCount++;

        if (onCount >= rules.maxOnAtOnce) return;

        // build list of OFF indices
        List<int> off = new List<int>(rt.Length);
        for (int i = 0; i < rt.Length; i++)
            if (!rt[i].isOn)
                off.Add(i);

        if (off.Count == 0) return;

        int idx = off[Random.Range(0, off.Count)];
        var kind = RollKind();

        SetButton(idx, kind, true);
        rt[idx].offAt = Time.time + rules.onLifetime;
    }


    ButtonKind RollKind()
    {
        bool blackAllowed = currentTour >= rules.blackUnlockTour;
        int blackW = blackAllowed ? rules.weightBlackAfterUnlock : 0;

        int g = rules.weightGreen;
        int y = rules.weightYellow;
        int n = rules.weightNeutral;

        int total = g + y + n + blackW;
        if (total <= 0) return ButtonKind.Neutral;

        int roll = Random.Range(0, total);

        if (blackAllowed)
        {
            if (roll < blackW) return ButtonKind.Black;
            roll -= blackW;
        }

        if (roll < g) return ButtonKind.Green;
        roll -= g;

        if (roll < y) return ButtonKind.Yellow;
        return ButtonKind.Neutral;
    }

    void SetButton(int index, ButtonKind kind, bool on)
    {
        rt[index].kind = kind;
        rt[index].isOn = on;
        rt[index].offAt = on ? Time.time + rules.onLifetime : 0f;

        buttons[index].SetVisual(kind, on);
    }
}
