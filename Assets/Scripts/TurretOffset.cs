using UnityEngine;

public class TurretOffset : MonoBehaviour
{
    [Tooltip("Смещение относительно центра тайла")]
    public Vector3 placementOffset;

    // Вызывается при установке
    public void ApplyPlacementOffset()
    {
        transform.position += placementOffset;
    }
}
