using UnityEngine;

public enum CastlePuzzleKind { Toy, Drawing, Moon, Sun, Star, Gate }

public sealed class CastlePuzzleTarget : MonoBehaviour
{
    [SerializeField] private CastlePuzzleKind kind;
    [SerializeField] private GameObject flame;
    [SerializeField] private Light torchLight;

    public CastlePuzzleKind Kind => kind;
    public bool IsLit { get; private set; }

    public void SetLit(bool lit)
    {
        IsLit = lit;
        if (flame != null) flame.SetActive(lit);
        if (torchLight != null) torchLight.enabled = lit;
    }
}
