using System.Collections;
using System.Collections.Generic;
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
