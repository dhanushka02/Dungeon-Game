using UnityEngine;

public sealed class CastleRescueTarget : MonoBehaviour
{
    public enum Kind { CellDoor, Son }
    [SerializeField] private Kind targetKind;
    public Kind TargetKind => targetKind;
}
