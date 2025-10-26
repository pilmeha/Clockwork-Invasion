using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BipodSlot : MonoBehaviour
{
    public Sprite bipodSprite;
    public GameObject bipodObject;
    public int price;

    public Image icon;
    public TextMeshProUGUI priceText;

    private Gamemanager gms;

    private void Start()
    {
        gms = GameObject.Find("GameManager").GetComponent<Gamemanager>();
        GetComponent<Button>().onClick.AddListener(BuyBipod);
    }

    private void BuyBipod()
    {
        gms.currentBipod = bipodObject;
        gms.currentBipodSprite = bipodSprite;
        gms.currentTurret = null;
    }

    private void OnValidate()
    {
        if (bipodSprite)
        {
            icon.enabled = true;
            icon.sprite = bipodSprite;
            priceText.text = price.ToString();

        
        }
        else
        {
            icon.enabled = false;
        }
    }
}
