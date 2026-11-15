using UnityEngine;

public interface IAxeBreakable
{
    bool CanBeHit { get; }
    bool Hit(Vector3 point, Vector3 normal, Vector3 direction);
}
