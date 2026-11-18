using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class CastleGuardsRoom : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Bounds roomBounds = new Bounds(new Vector3(-6f, 1f, -10f), new Vector3(8f, 4f, 8f));
    [SerializeField, Min(0.5f)] private float interactionDistance = 2.4f;
    [SerializeField] private Transform drawer;
    [SerializeField] private Vector3 drawerOffset = new Vector3(0f, 0f, -0.42f);
    [SerializeField] private GameObject map;
    [SerializeField] private GameObject keepsake;
    [SerializeField] private GameObject hintPanel;
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private GameObject readerPanel;
    [SerializeField] private TMP_Text readerTitle;
    [SerializeField] private TMP_Text readerBody;
    [SerializeField] private GameObject mapDiagram;
    [SerializeField] private GameObject objectivePanel;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private AudioClip drawerSound;
    [SerializeField] private AudioClip paperSound;

    private Vector3 drawerClosed;
    private Transform readingSource;
    private string temporaryHint;
    private float hintUntil;
    private bool moving;

    public bool OrdersRead { get; private set; }
    public bool PrisonerCornerInspected { get; private set; }
    public bool DrawerOpen { get; private set; }
    public bool MapRead { get; private set; }
    public bool KeepsakeCollected { get; private set; }
    public bool IsReading => readerPanel != null && readerPanel.activeSelf;

    private void Awake()
    {
        drawerClosed = drawer.localPosition;
        map.SetActive(false);
        hintPanel.SetActive(false);
        readerPanel.SetActive(false);
        objectivePanel.SetActive(false);
    }

    private void Update()
    {
        if (player == null || playerCamera == null) return;
        bool inside = roomBounds.Contains(player.position);
        objectivePanel.SetActive(inside);
        if (!inside) { CloseReader(); hintPanel.SetActive(false); return; }
        UpdateObjective();

        bool use = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        bool close = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        if (IsReading)
        {
            hintPanel.SetActive(false);
            if (use || close || readingSource == null ||
                Vector3.Distance(player.position, readingSource.position) > 3.2f) CloseReader();
            return;
        }

        GuardsRoomTarget target = LookTarget();
        if (use && !moving && target != null) Interact(target);
        string hint = Time.time < hintUntil ? temporaryHint : moving ? "Moving..." : Hint(target);
        hintPanel.SetActive(!IsReading && !string.IsNullOrEmpty(hint));
        hintText.text = hint ?? "";
    }

    private GuardsRoomTarget LookTarget()
    {
        if (!Physics.Raycast(playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f)),
                out RaycastHit hit, interactionDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) return null;
        var target = hit.collider.GetComponentInParent<GuardsRoomTarget>();
        return target != null && target.transform.IsChildOf(transform) ? target : null;
    }

    private string Hint(GuardsRoomTarget target)
    {
        if (target == null) return null;
        switch (target.Clue)
        {
            case GuardsRoomClue.Orders: return "[E] Read the guard's orders";
            case GuardsRoomClue.PrisonerCorner: return "[E] Inspect the prisoner's bedding";
            case GuardsRoomClue.Drawer: return DrawerOpen ? "The drawer is open. Look inside for the map." : "[E] Search the desk drawer";
            case GuardsRoomClue.Map: return DrawerOpen ? "[E] Examine the marked castle map" : null;
            case GuardsRoomClue.Keepsake: return KeepsakeCollected ? null : "[E] Keep the family drawing";
            default: return null;
        }
    }

    private void Interact(GuardsRoomTarget target)
    {
        switch (target.Clue)
        {
            case GuardsRoomClue.Orders:
                OrdersRead = true;
                Read(target, "THE GUARD'S ORDERS", "Move the boy to the far storeroom.\nBoard up the passage before we leave.\nThe marked route is in the desk drawer.\n\nThe storeroom... Those wooden barriers must lead to him.");
                break;
            case GuardsRoomClue.PrisonerCorner:
                PrisonerCornerInspected = true;
                Read(target, "SOMEONE WAS KEPT HERE", "A thin mattress. Loose rope. An empty bowl.\nSomeone was held here recently.\n\nA small drawing peeks out from beneath the bedding...\nWas my son trying to leave something for me?");
                break;
            case GuardsRoomClue.Drawer:
                if (!DrawerOpen) StartCoroutine(OpenDrawer());
                break;
            case GuardsRoomClue.Map:
                if (!DrawerOpen) return;
                MapRead = true;
                Read(target, "THE MARKED STOREROOM", "The guards marked a route from this room to the far storeroom.\n\nGo around the central stone enclosure to the upper passage.\nTwo wooden barricades block the way.\nI can break through them with my axe.", true);
                break;
            case GuardsRoomClue.Keepsake:
                if (KeepsakeCollected) return;
                KeepsakeCollected = true;
                Read(target, "A DRAWING WORTH KEEPING", "Two figures holding hands outside a little house.\nDad and me, together again.\n\nHe kept this with him. I'll keep it safe until I find him.");
                keepsake.SetActive(false);
                break;
        }
    }

    private IEnumerator OpenDrawer()
    {
        moving = true;
        CastleAudioEffects.Play(drawerSound, drawer.position, 0.35f);
        yield return Slide(drawer, drawerClosed, drawerClosed + drawerOffset, 0.65f);
        DrawerOpen = true;
        map.SetActive(true);
        moving = false;
        Say("There is a marked map inside the drawer. Look down to inspect it.");
    }

    private IEnumerator Slide(Transform item, Vector3 start, Vector3 end, float duration)
    {
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            item.localPosition = Vector3.Lerp(start, end, Mathf.SmoothStep(0, 1, t / duration));
            yield return null;
        }
        item.localPosition = end;
    }

    private void Read(GuardsRoomTarget source, string title, string body, bool showMap = false)
    {
        readingSource = source.transform;
        readerTitle.text = title;
        readerBody.text = body;
        mapDiagram.SetActive(showMap);
        readerPanel.SetActive(true);
        CastleAudioEffects.Play(paperSound, source.transform.position, 0.25f);
    }

    private void CloseReader() => readerPanel.SetActive(false);

    private void Say(string text)
    {
        temporaryHint = text;
        hintUntil = Time.time + 4f;
    }

    private void UpdateObjective()
    {
        objectiveText.text = "THE ABANDONED GUARDS' ROOM\n" +
            (MapRead ? "Follow the boarded passage to the storeroom." :
                OrdersRead ? "Search the desk and look for your son's trail." : "Search the room for signs of your son.") +
            (KeepsakeCollected ? "\nFamily drawing safely kept." : "");
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (moving)
        {
            // Complete an interrupted move when the room is temporarily disabled.
            if (!DrawerOpen && drawer.localPosition != drawerClosed)
            { drawer.localPosition = drawerClosed + drawerOffset; DrawerOpen = true; map.SetActive(true); }
            moving = false;
        }
        if (hintPanel != null) hintPanel.SetActive(false);
        if (readerPanel != null) readerPanel.SetActive(false);
        if (objectivePanel != null) objectivePanel.SetActive(false);
    }
}
