using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool isPause = false;
    public GameObject pauseMenuUI;
    public GameObject settingMenuUI;
    public Image settingImage;

    [Header("Audio (Optional)")]
    public AudioSource audioSource;
    public AudioClip pauseSound;
    public AudioClip clickSound;

    private Color originColor;
    private bool isInSettings = false;

    void Start()
    {
        if (settingImage != null)
            originColor = settingImage.color;

        // 确保游戏开始时暂停菜单和设置菜单都关闭
        if (pauseMenuUI != null && pauseMenuUI.activeInHierarchy)
            pauseMenuUI.SetActive(false);

        if (settingMenuUI != null && settingMenuUI.activeInHierarchy)
            settingMenuUI.SetActive(false);

        isPause = false;
        isInSettings = false;
        Time.timeScale = 1.0f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isInSettings || (settingMenuUI != null && settingMenuUI.activeInHierarchy))
            {
                BackToMainPause();
            }
            else if (isPause)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        if (!isPause) return;

        PlayClickSound();
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1.0f;
        isPause = false;
        isInSettings = false;
    }

    public void Restart()
    {
        PlayClickSound();
        isPause = false;
        isInSettings = false;
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Settings()
    {
        PlayClickSound();
        if (isPause)
        {
            pauseMenuUI.SetActive(false);
            settingMenuUI.SetActive(true);
            Time.timeScale = 0.0f;
            if (settingImage != null)
                settingImage.enabled = true;
            isInSettings = true;
        }
    }

    public void MainMenu()
    {
        PlayClickSound();
        isPause = false;
        isInSettings = false;
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("Start");
    }

    public void Pause()
    {
        if (isPause) return;

        PlayPauseSound();
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0.0f;
        isPause = true;
        isInSettings = false;
    }

    public void BackToMainPause()
    {
        PlayClickSound();
        settingMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
        isPause = true;
        isInSettings = false;
        Time.timeScale = 0.0f;
        if (settingImage != null)
            settingImage.enabled = false;
    }

    private void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    private void PlayPauseSound()
    {
        if (audioSource != null && pauseSound != null)
        {
            audioSource.PlayOneShot(pauseSound);
        }
    }
}
