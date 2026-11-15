using UnityEngine;

public enum GuardsRoomClue { Orders = 0, PrisonerCorner = 1, Drawer = 2, Map = 3, Keepsake = 6 }

public sealed class GuardsRoomTarget : MonoBehaviour
{
    [SerializeField] private GuardsRoomClue clue;
    public GuardsRoomClue Clue => clue;
}
