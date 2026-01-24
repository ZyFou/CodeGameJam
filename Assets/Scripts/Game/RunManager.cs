using UnityEngine;

public class RunManager : MonoBehaviour
{
    public enum RunState
    {
        Idle, // pas commencé
        WaitingToPay, // entre rounds : appuyer => payer et lancer
        InRound, // round en cours
        GameOver,
    }

    [Header("Insert money behavior")]
    public bool afterEnoughOnlyAddOne = true;

    [Header("Refs")]
    [SerializeField]
    private RuleSetSO rules;

    [SerializeField]
    private BoardManager board;

    [SerializeField]
    private HUDController hud;

    [Header("State")]
    public RunState state = RunState.Idle;

    public int money;
    public int tickets;

    public int tourIndex = 1;
    public int roundInTour = 1;

    public int tourBonusPool;
    public int lastRoundScore;

    public int roundsPlayedThisTour;
    public int debtTotal;
    public int debtRemaining;
    public int depositedThisTour;

    [Header("Debt Grace")]
    [SerializeField] private float debtGraceSeconds = 10f;
    private bool debtGraceActive;
    private float debtGraceEndsAt;

    void OnEnable()
    {
        if (board != null)
            board.OnRoundEndedScore += HandleRoundEnded;
    }

    void OnDisable()
    {
        if (board != null)
            board.OnRoundEndedScore -= HandleRoundEnded;
    }

    void Update()
    {
        // Mettre à jour le HUD en temps réel pendant le round
        if (state == RunState.InRound && board != null && hud != null)
        {
            RefreshHUDDuringRound();
        }

        if (debtGraceActive && Time.time >= debtGraceEndsAt)
        {
            debtGraceActive = false;
            if (debtRemaining > 0)
            {
                state = RunState.GameOver;
                RefreshHUD("Game Over : dette non remboursée à temps.");
            }
        }
    }

    void RefreshHUDDuringRound()
    {
        int currentScore = board.GetCurrentScore();
        int comboCount = board.GetComboCount();
        float comboMultiplier = board.GetComboMultiplier();
        float timeRemaining = board.GetTimeRemaining();

        hud.SetState(
            currentScore,           // score (en temps réel)
            comboCount,             // comboCount (en temps réel)
            comboMultiplier,        // comboMultiplier (en temps réel)
            tourIndex,              // tour
            roundInTour,            // round
            rules.roundsPerTour,    // roundsPerTour
            money,                  // money
            tickets,                // tickets
            GetEntryCost(),         // roundCost
            debtRemaining,          // debtRemaining
            debtTotal,              // debtTotal
            depositedThisTour,      // depositedThisTour
            tourBonusPool,          // ticketReward
            timeRemaining,          // timeRemaining
            ""                      // message (vide pendant le round)
        );
    }

    // appelé par le bouton unique
    public void PressStandButton()
    {
        if (rules == null || board == null)
        {
            Debug.LogError("RunManager missing refs (rules/board).");
            return;
        }

        switch (state)
        {
            case RunState.Idle:
                StartRun();
                break;

            case RunState.WaitingToPay:
                PayAndStartRound();
                break;

            case RunState.InRound:
                // option: feedback
                hud?.SetMessage("Round en cours...");
                break;

            case RunState.GameOver:
                // option: restart
                hud?.SetMessage("Game Over. Relance la scène ou ajoute un bouton Restart.");
                break;
        }
    }

    void StartRun()
    {
        money = rules.startMoney;
        tickets = rules.startTickets;

        tourIndex = 1;
        roundInTour = 1;
        StartTour();

        state = RunState.WaitingToPay;
        RefreshHUD("Appuie sur le bouton du stand pour payer et lancer le round.");
    }

    void StartRunIfNeeded()
    {
        if (state != RunState.Idle)
            return;

        StartRun();
    }

    void StartTour()
    {
        tourBonusPool = rules.tourBonusTicketsStart;
        roundInTour = 1;

        debtTotal = rules.debtStart + (tourIndex - 1) * rules.debtAddPerTour;
        debtRemaining = debtTotal;
        depositedThisTour = 0;
        roundsPlayedThisTour = 0;
        debtGraceActive = false;
        debtGraceEndsAt = 0f;
    }

    int GetEntryCost()
    {
        if (rules.entryCostPerTourOnly)
        {
            int perTour = rules.entryCostFirstTour + (tourIndex - 1) * rules.entryCostAddPerTour;
            return Mathf.Max(0, perTour);
        }

        if (rules.useEntryCostScaling)
        {
            float[] multipliers = rules.entryCostRoundMultipliers;
            float roundMultiplier = 1f;
            if (multipliers != null && multipliers.Length > 0)
            {
                int roundIdx = Mathf.Clamp(roundInTour - 1, 0, multipliers.Length - 1);
                roundMultiplier = multipliers[roundIdx];
            }

            float scaled = rules.entryCostFirstTour
                           * Mathf.Pow(rules.entryCostScalingFactor, Mathf.Max(0, tourIndex - 1))
                           * roundMultiplier;

            return Mathf.Max(0, Mathf.RoundToInt(scaled));
        }

        int[] baseCosts = rules.entryCostBaseByRoundInTour;
        if (baseCosts == null || baseCosts.Length == 0)
            return 0;

        int idx = Mathf.Clamp(roundInTour - 1, 0, baseCosts.Length - 1);
        int baseCost = baseCosts[idx];

        // augmente par tour passé
        return Mathf.Max(0, baseCost + (tourIndex - 1) * rules.entryCostAddPerTour);
    }

    void PayAndStartRound()
    {
        if (debtGraceActive || roundsPlayedThisTour >= rules.roundsPerTour)
        {
            hud?.SetMessage("Tour terminé : rembourse la dette.");
            return;
        }

        int cost = GetEntryCost();

        if (money < cost)
        {
            state = RunState.GameOver;
            RefreshHUD($"GAME OVER : pas assez d'argent pour payer {cost}€.");
            return;
        }

        money -= cost;

        // chaque round "consommé" baisse le pool (joué uniquement, car pas de skip)
        tourBonusPool = Mathf.Max(0, tourBonusPool - rules.tourBonusTicketsDecayPerRound);

        state = RunState.InRound;
        RefreshHUD($"Round lancé (-{cost}€).");

        board.StartRound(tourIndex, roundInTour);
    }

    void HandleRoundEnded(int score)
    {
        lastRoundScore = score;

        // score -> money
        int moneyGain = Mathf.FloorToInt((float)score / Mathf.Max(1, rules.scoreToMoneyK));
        if (score > 0 && moneyGain == 0)
            moneyGain = 1;
        money += moneyGain;

        // +1 ticket par round terminé (toujours)
        tickets += rules.ticketsPerPlayedRound;

        roundsPlayedThisTour++;

        // avancer round
        roundInTour++;

        if (roundsPlayedThisTour >= rules.roundsPerTour && debtRemaining > 0)
        {
            debtGraceActive = true;
            debtGraceEndsAt = Time.time + debtGraceSeconds;
            state = RunState.WaitingToPay;
            RefreshHUD(
                $"Fin round: score {score} => +{moneyGain}€ +{rules.ticketsPerPlayedRound} ticket. Dette non remboursée : {Mathf.CeilToInt(debtGraceSeconds)}s."
            );
            return;
        }

        // fin de tour ?
        if (roundInTour > rules.roundsPerTour)
        {
            int bonus = tourBonusPool; // ce qui reste
            tickets += bonus;

            string endMsg = $"Fin TOUR {tourIndex}: +{bonus} tickets bonus.";

            tourIndex++;
            StartTour();

            state = RunState.WaitingToPay;
            RefreshHUD(
                $"Fin round: score {score} => +{moneyGain}€ +{rules.ticketsPerPlayedRound} ticket. {endMsg} Appuie pour lancer le prochain round."
            );
            return;
        }

        // continuer
        state = RunState.WaitingToPay;
        RefreshHUD(
            $"Fin round: score {score} => +{moneyGain}€ +{rules.ticketsPerPlayedRound} ticket. Appuie pour lancer le round suivant."
        );
    }

    void RefreshHUD(string msg)
    {
        if (hud != null)
        {
            hud.SetState(
                lastRoundScore,        // score
                0,                     // comboCount (pas encore implémenté)
                1.0f,                  // comboMultiplier (pas encore implémenté)
                tourIndex,             // tour
                roundInTour,           // round
                rules.roundsPerTour,   // roundsPerTour
                money,                 // money
                tickets,               // tickets
                GetEntryCost(),        // roundCost
                debtRemaining,         // debtRemaining
                debtTotal,             // debtTotal
                depositedThisTour,     // depositedThisTour
                tourBonusPool,         // ticketReward
                0f,                    // timeRemaining
                msg                    // message
            );
        }
        Debug.Log("[RUN] " + msg);
    }

    // Option debug si tu veux un "Add Money" bouton
    public int InsertMoney(int wantedAmount)
    {
        if (state != RunState.WaitingToPay)
            return 0;

        int cost = GetEntryCost();
        if (cost <= 0)
            return 0;

        int add;
        int missingBefore = Mathf.Max(0, cost - money);

        if (money < cost)
        {
            add = Mathf.Clamp(wantedAmount, 0, missingBefore); // jamais dépasser
        }
        else
        {
            add = afterEnoughOnlyAddOne ? 1 : 0;
        }

        if (add <= 0)
        {
            RefreshHUD($"Insert: {wantedAmount}€ | Déjà assez pour payer ({money}/{cost}).");
            return 0;
        }

        money += add;

        int missingAfter = Mathf.Max(0, cost - money);
        RefreshHUD($"Insert: {wantedAmount}€ | +{add}€ | manque {missingAfter}€ (cost {cost}€)");

        return add;
    }

    public int GetWantedInsertAmount()
    {
        // exemple demandé: 5 * nb tour
        return 5 * tourIndex;
    }

    public void PressInsertButton()
    {
        InsertMoney(GetWantedInsertAmount());
    }

    public void TryPayEntryAndStartRound()
    {
        if (state == RunState.Idle)
        {
            StartRun();
            return;
        }

        PressStandButton();
    }

    public void DepositToDebt()
    {
        StartRunIfNeeded();

        if (state == RunState.InRound)
        {
            hud?.SetMessage("Round en cours...");
            return;
        }

        if (state == RunState.GameOver)
        {
            hud?.SetMessage("Game Over. Relance la scène ou ajoute un bouton Restart.");
            return;
        }

        int step = rules.depositStepPerTour * tourIndex;
        int entryCost = GetEntryCost();
        int maxAllowed = Mathf.Max(0, money - entryCost);
        int deposit = Mathf.Min(step, Mathf.Min(maxAllowed, debtRemaining));

        if (deposit <= 0)
        {
            RefreshHUD("Aucun dépôt possible (garde assez pour jouer).");
            return;
        }

        money -= deposit;
        debtRemaining -= deposit;
        depositedThisTour = debtTotal - debtRemaining;

        if (debtRemaining == 0)
        {
            debtGraceActive = false;
            debtGraceEndsAt = 0f;

            int bonus = GetDebtTicketRewardPreview();
            tickets += bonus;

            tourIndex++;
            StartTour();
            state = RunState.WaitingToPay;
            RefreshHUD($"Dette remboursée: +{bonus} tickets bonus. Nouveau tour.");
            return;
        }

        RefreshHUD($"Dépôt dette: -{deposit}€ (reste {debtRemaining}€).");
    }

    int GetDebtTicketRewardPreview()
    {
        if (roundsPlayedThisTour <= 1)
            return 6;
        if (roundsPlayedThisTour == 2)
            return 4;
        if (roundsPlayedThisTour == 3)
            return 2;
        return 0;
    }
}
