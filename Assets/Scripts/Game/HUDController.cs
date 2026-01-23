using TMPro;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    [Header("World Texts (TMP 3D)")]
    public TextMeshPro scoreText; // TEXT_Score
    public TextMeshPro comboText; // TEXT_Combo
    public TextMeshPro tourRoundText; // TXT_TourRound
    public TextMeshPro tourCountText; // TXT_TourCount
    public TextMeshPro comboMultiplierText; // TXT_ComboMultiplier

    public TextMeshPro moneyText; // TXT_Money
    public TextMeshPro ticketsText; // TXT_Ticket

    public TextMeshPro roundCostText; // TXT_RoundCost

    public TextMeshPro debtText; // TXT_Debt
    public TextMeshPro depositedText; // TXT_Deposited
    public TextMeshPro ticketRewardText; // TXT_TicketReward

    public TextMeshPro messageText; // TXT_Message

    /// <summary>
    /// ticketReward = bonus tickets si la dette est remboursée maintenant (ex: 6/4/2/0)
    /// depositedThisTour = total déjà déposé au guichet sur ce tour
    /// comboMultiplier : ex 1.0, 1.1, 1.2 ...
    /// </summary>
    public void SetState(
        int score,
        int comboCount,
        float comboMultiplier,
        int tour,
        int round,
        int roundsPerTour,
        int money,
        int tickets,
        int roundCost,
        int debtRemaining,
        int debtTotal,
        int depositedThisTour,
        int ticketReward,
        string message
    )
    {
        if (scoreText)
            scoreText.text = $"{score}";
        if (comboText)
            comboText.text = $"{comboCount}";
        if (tourRoundText)
            tourRoundText.text = $"{tour}/{round}";
        if (tourCountText)
            tourCountText.text = $"{tour}";
        if (comboMultiplierText)
            comboMultiplierText.text = $"{comboMultiplier:0.0}";

        if (moneyText)
            moneyText.text = $"{money}";
        if (ticketsText)
            ticketsText.text = $"{tickets}";

        if (roundCostText)
            roundCostText.text = $"{roundCost}";

        if (debtText)
            debtText.text = $"{debtTotal}";
        if (depositedText)
            depositedText.text = $"{depositedThisTour}";
        if (ticketRewardText)
            ticketRewardText.text = $"{ticketReward}";

        if (messageText)
            messageText.text = message;
    }

    public void SetMessage(string message)
    {
        if (messageText)
            messageText.text = message;
    }
}
