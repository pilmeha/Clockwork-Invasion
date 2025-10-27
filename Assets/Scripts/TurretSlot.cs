using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurretSlot : MonoBehaviour
{
    public Sprite turretSprite;
    public GameObject turretObject;
    public int price;

    public Image icon;
    public TextMeshProUGUI priceText;

    private Gamemanager gms;

    private void Start()
    {
        gms = GameObject.Find("GameManager").GetComponent<Gamemanager>();
        GetComponent<Button>().onClick.AddListener(BuyTurret);
    }

    private void BuyTurret()
    {
        gms.currentTurret = turretObject;
        gms.currentTurretSprite = turretSprite;
        gms.currentBipod = null; // сбрасываем выбор сошек
    }

    private void OnValidate()
    {
        if (turretSprite)
        {
            icon.enabled = true;
            icon.sprite = turretSprite;
            priceText.text = price.ToString();
        }
        else
        {
            icon.enabled = false;
        }
    }
}
