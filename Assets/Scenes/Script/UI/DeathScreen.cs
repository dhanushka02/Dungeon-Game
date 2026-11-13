
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreen : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup deathCanvas;

    [Header("Fade Settings")]
    public float showDelay = 1f;
    public float fadeDuration = 1.5f;

    [Header("Scene Settings")]
    public string mainMenuScene = "MainMenu";

    private bool isShowing = false;

    void Start()
    {
        Time.timeScale = 1f;

        if (deathCanvas != null)
        {
            deathCanvas.alpha = 0f;
            deathCanvas.interactable = false;
            deathCanvas.blocksRaycasts = false;
        }
    }

    public void ShowDeathScreen()
    {
        if (isShowing)
            return;

        isShowing = true;
        StartCoroutine(ShowDeathRoutine());
    }

    IEnumerator ShowDeathRoutine()
    {
        // Wait for the Player death animation.
        yield return new WaitForSeconds(showDelay);

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            if (deathCanvas != null)
            {
                deathCanvas.alpha = Mathf.Clamp01(
                    elapsed / fadeDuration
                );
            }

            yield return null;
        }

        if (deathCanvas != null)
        {
            deathCanvas.alpha = 1f;
            deathCanvas.interactable = true;
            deathCanvas.blocksRaycasts = true;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    public void Retry()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }
}
