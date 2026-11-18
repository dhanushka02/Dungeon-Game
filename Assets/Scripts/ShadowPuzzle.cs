using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class ShadowPuzzle : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform figure;
    [SerializeField] private Transform lantern;
    [SerializeField] private Transform[] lanternSpots;
    [SerializeField] private Transform sheet;
    [SerializeField] private Transform wallBoard;
    [SerializeField] private GameObject mapPiece;
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private AudioSource puzzleAudio;
    [SerializeField] private AudioClip turnSound;
    [SerializeField] private AudioClip placeSound;
    [SerializeField] private AudioClip openSound;
    [SerializeField, Min(1)] private int turnCount = 8;
    [SerializeField] private int solutionTurn = 6;
    [SerializeField] private int solutionSpot = 1;
    [SerializeField, Min(0.5f)] private float interactionDistance = 2.4f;
    [SerializeField, Min(0.05f)] private float moveDuration = 0.4f;
    [SerializeField, Min(0.1f)] private float openingDuration = 2f;
    [SerializeField] private Vector3 boardOffset = new Vector3(0f, 0f, -0.6f);

    private Quaternion figureRotation;
    private Vector3 boardClosed;
    private int turn;
    private int spot;
    private bool moving;
    private string message;
    private float messageUntil;

    public bool OutlineRead { get; private set; }
    public bool IsSolved { get; private set; }
    public bool IsOpen { get; private set; }

    private void Awake()
    {
        figureRotation = figure.localRotation;
        boardClosed = wallBoard.localPosition;
        lantern.position = lanternSpots[spot].position;
        mapPiece.SetActive(false);
        promptPanel.SetActive(false);
    }

    private void Update()
    {
        if (playerCamera == null) return;

        Transform target = LookTarget();
        if (target != null && !moving && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            Interact(target);

        string text = Time.time < messageUntil ? message : Hint(target);
        promptPanel.SetActive(!string.IsNullOrEmpty(text));
        promptText.text = text ?? "";
        if (objectiveText != null && !IsOpen)
            objectiveText.text = IsSolved ? "FIND YOUR SON\nSomething moved in the wall..." :
                OutlineRead ? "THE CHALK OUTLINE\nMake the knight's shadow fit the outline." :
                "FIND YOUR SON\nSearch the house for anything he left behind.";
    }

    private Transform LookTarget()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (!Physics.Raycast(ray, out RaycastHit hit, interactionDistance,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) return null;
        if (hit.transform.IsChildOf(figure)) return figure;
        if (hit.transform.IsChildOf(lantern)) return lantern;
        if (hit.transform.IsChildOf(sheet)) return sheet;
        return null;
    }

    private string Hint(Transform target)
    {
        if (target == null || IsSolved) return null;
        if (target == figure) return "[E] Turn the wooden knight";
        if (target == lantern) return "[E] Move the lantern";
        return "[E] Look at the chalk marks on the sheet";
    }

    private void Interact(Transform target)
    {
        if (target == sheet)
        {
            OutlineRead = true;
            Say("His old sheet... we made shadow puppets here every night before bed.\nHe traced around his little knight. Make the shadow fit.", 7f);
            return;
        }
        if (IsSolved) return;

        if (target == figure)
        {
            turn = (turn + 1) % turnCount;
            Play(turnSound);
            StartCoroutine(Turn(figure.localRotation, figureRotation * Quaternion.Euler(0f, turn * 360f / turnCount, 0f)));
        }
        else
        {
            Vector3 from = lantern.position;
            spot = (spot + 1) % lanternSpots.Length;
            Play(placeSound);
            StartCoroutine(Hang(from, lanternSpots[spot].position));
        }
    }

    private IEnumerator Turn(Quaternion from, Quaternion to)
    {
        moving = true;
        for (float elapsed = 0f; elapsed < moveDuration; elapsed += Time.deltaTime)
        {
            figure.localRotation = Quaternion.Slerp(from, to, Mathf.SmoothStep(0f, 1f, elapsed / moveDuration));
            yield return null;
        }
        figure.localRotation = to;
        moving = false;
        Check();
    }

    private IEnumerator Hang(Vector3 from, Vector3 to)
    {
        moving = true;
        for (float elapsed = 0f; elapsed < moveDuration; elapsed += Time.deltaTime)
        {
            lantern.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, elapsed / moveDuration));
            yield return null;
        }
        lantern.position = to;
        moving = false;
        Check();
    }

    private void Check()
    {
        if (turn != solutionTurn || spot != solutionSpot) return;
        IsSolved = true;
        Say("The shadow fits his tracing exactly...\nA board in the wall just slid open.", openingDuration + 1f);
        StartCoroutine(OpenBoard());
    }

    private IEnumerator OpenBoard()
    {
        if (openSound != null)
            CastleAudioEffects.Play(openSound, wallBoard.position, 0.7f, openSound.length / Mathf.Max(0.1f, openingDuration));
        Vector3 opened = boardClosed + boardOffset;
        for (float elapsed = 0f; elapsed < openingDuration; elapsed += Time.deltaTime)
        {
            wallBoard.localPosition = Vector3.Lerp(boardClosed, opened, Mathf.SmoothStep(0f, 1f, elapsed / openingDuration));
            yield return null;
        }
        wallBoard.localPosition = opened;
        mapPiece.SetActive(true);
        IsOpen = true;
    }

    private void Play(AudioClip clip)
    {
        if (clip != null && puzzleAudio != null) puzzleAudio.PlayOneShot(clip);
    }

    private void Say(string text, float seconds)
    {
        message = text;
        messageUntil = Time.time + seconds;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        moving = false;
        if (IsSolved && !IsOpen && wallBoard != null)
        {
            wallBoard.localPosition = boardClosed + boardOffset;
            mapPiece.SetActive(true);
            IsOpen = true;
        }
        if (promptPanel != null) promptPanel.SetActive(false);
    }
}
