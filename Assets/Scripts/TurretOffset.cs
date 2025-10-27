using UnityEngine;

public class TurretOffset : MonoBehaviour
{
    [Tooltip("�������� ������������ ������ �����")]
    public Vector3 placementOffset;

    // ���������� ��� ���������
    public void ApplyPlacementOffset()
    {
        transform.position += placementOffset;
    }
}
