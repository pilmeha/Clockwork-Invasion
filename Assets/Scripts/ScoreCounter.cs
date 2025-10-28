using DG.Tweening;
using TMPro;
using UnityEngine;

public sealed class ScoreCounter : MonoBehaviour
{
    public static ScoreCounter Instance { get; private set; }
    
    [SerializeField] private TextMeshProUGUI scoreTextEnergy;
    [SerializeField] private TextMeshProUGUI scoreTextGear;

    public int ScoreEnergy { get; private set; }
    public int ScoreGear { get; private set; }
    
    private void Awake() => Instance = this;

    public void AddToEnergy(int value)
    {
        ScoreEnergy += value;
        UpdateUI();
    }

    public void AddToGear(int value)
    {
        ScoreGear += value;
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        scoreTextEnergy.SetText($"{ScoreEnergy}");
        scoreTextGear.SetText($"{ScoreGear}");
    }
}
