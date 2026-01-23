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

    void StartTour()
    {
        tourBonusPool = rules.tourBonusTicketsStart;
        roundInTour = 1;
    }

    int GetEntryCost()
    {
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
        money += moneyGain;

        // +1 ticket par round terminé (toujours)
        tickets += rules.ticketsPerPlayedRound;

        // avancer round
        roundInTour++;

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
                0,                     // debtRemaining (pas encore implémenté)
                0,                     // debtTotal (pas encore implémenté)
                0,                     // depositedThisTour (pas encore implémenté)
                tourBonusPool,         // ticketReward
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
        // Si ton RunManager a déjà une méthode genre PressStandButton() ou PayAndStartRound()
        // appelle-la ici. Sinon, branche ta logique de paiement entrée + lancement round.
        PressStandButton();
    }

    public void DepositToDebt()
    {
        // TODO: Implémenter le système de dépôt de dette
        // Pour l'instant, cette fonctionnalité n'est pas encore implémentée
        Debug.Log("[RUN] DepositToDebt appelé mais pas encore implémenté");
    }
}
