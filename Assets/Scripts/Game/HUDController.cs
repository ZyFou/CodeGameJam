using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
    public TextMeshPro moneyText;
    public TextMeshPro ticketsText;
    public TextMeshPro tourText;
    public TextMeshPro costText;
    public TextMeshPro poolText;
    public TextMeshPro lastScoreText;
    public TextMeshPro msgText;

    public void SetState(int money, int tickets, int tour, int round, int roundsPerTour, int entryCost, int bonusPool, int lastScore, string msg)
    {
        if (moneyText) moneyText.text = $"Money: {money}€";
        if (ticketsText) ticketsText.text = $"Tickets: {tickets}";
        if (tourText) tourText.text = $"Tour {tour} — Round {round}/{roundsPerTour}";
        if (costText) costText.text = $"Entry cost: {entryCost}€";
        if (poolText) poolText.text = $"Tour bonus pool: {bonusPool}";
        if (lastScoreText) lastScoreText.text = $"Last score: {lastScore}";
        if (msgText) msgText.text = msg;
    }

    public void SetMessage(string msg)
    {
        if (msgText) msgText.text = msg;
    }
}
