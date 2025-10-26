using UnityEngine;

public class TurretTile : MonoBehaviour
{
    public bool hasBipod;
    public bool hasTurret;

    [HideInInspector] public GameObject bipodObject;
    [HideInInspector] public GameObject turretObject;
}