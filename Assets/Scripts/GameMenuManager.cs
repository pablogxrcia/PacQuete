using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Necesario para Coroutines (transiciones)
using UnityEngine.UI; // Necesario para CanvasGroup (fundido)

public class GameMenuManager : MonoBehaviour
{
    [Header("Configuración de Transición")]
    // **NOTA IMPORTANTE:** Este campo debe ser enlazado manualmente 
    // en el Inspector con un objeto Panel UI que tenga un componente CanvasGroup.
    public CanvasGroup fadePanel;

    // Tiempo que dura el fundido (fade out) en segundos.
    public float transitionDuration = 0.5f;

    [Header("Configuración de Pausa")]
    // NUEVO: Arrastra aquí el CanvasGroup del menú de pausa
    public CanvasGroup pausePanel;
    private bool isPaused = false;

    // --- Índices de Escena (Basado en tu código anterior) ---
    private const int SCENE_MENU = 0;
    private const int SCENE_LEVEL_1 = 1;
    private const int SCENE_LEVEL_2 = 2;
    private const int SCENE_HOW_TO_PLAY = 3; // Scene 3: Cómo Jugar
    private const int SCENE_LEVEL_3 = 8; // Scene 4: Nivel 3

    // 1. Solución del Problema: Se ejecuta al inicio de la escena.
    void Start()
    {
        // Inicializar el tiempo de juego a 1 (normal)
        Time.timeScale = 1f;

        // Inicializar el panel de fundido (transparente, no bloquea clics)
        if (fadePanel != null)
        {
            SetPanelState(fadePanel, false);
            fadePanel.interactable = false;
        }

        // Inicializar el panel de pausa (oculto)
        if (pausePanel != null)
        {
            SetPanelState(pausePanel, false);
        }
    }

    // Detecta la tecla de pausa (Escape o P)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            // Solo pausar si NO estamos en medio de una transición de escena
            if (fadePanel == null || !fadePanel.blocksRaycasts)
            {
                TogglePause();
            }
        }
    }

    // ------------------------------------------
    // --- LÓGICA DE PAUSA (TogglePause) ---
    // ------------------------------------------

    /// <summary>
    /// Activa o desactiva el estado de pausa. Enlazado al botón "Reanudar".
    /// </summary>
    public void TogglePause()
    {
        if (pausePanel == null)
        {
            Debug.LogWarning("No se asignó el Panel de Pausa en el Inspector.");
            return;
        }

        isPaused = !isPaused;

        if (isPaused)
        {
            // Entrar en Pausa: Detener el tiempo
            Time.timeScale = 0f;
            SetPanelState(pausePanel, true); // Mostrar el panel
        }
        else
        {
            // Reanudar: Devolver el tiempo a la normalidad
            Time.timeScale = 1f;
            SetPanelState(pausePanel, false); // Ocultar el panel
        }
    }

    /// <summary>
    /// Función de utilidad para controlar la visibilidad e interacción de los paneles.
    /// </summary>
    private void SetPanelState(CanvasGroup panel, bool isVisible)
    {
        panel.alpha = isVisible ? 1f : 0f;
        panel.interactable = isVisible;
        panel.blocksRaycasts = isVisible;
    }


    // ------------------------------------------
    // --- LÓGICA DE TRANSICIÓN DE ESCENA ---
    // ------------------------------------------

    public void LoadScene(int sceneIndex)
    {
        // Asegurarse de que el tiempo se reanude antes de cargar la escena
        Time.timeScale = 1f;

        // Ocultar el menú de pausa si estaba activo antes de la carga
        if (pausePanel != null) SetPanelState(pausePanel, false);

        if (fadePanel != null)
        {
            StartCoroutine(TransitionAndLoadScene(sceneIndex));
        }
        else
        {
            Debug.LogWarning("¡ADVERTENCIA! No se asignó un Fade Panel. Cargando escena sin transición visual.");
            SceneManager.LoadScene(sceneIndex);
        }
    }

    /// <summary>
    /// Rutina que realiza el efecto visual de fundido (Fade Out) y luego carga la nueva escena.
    /// </summary>
    private IEnumerator TransitionAndLoadScene(int sceneIndex)
    {
        float timer = 0f;

        // Bloquear Raycasts: Activamos el bloqueo ANTES de la transición
        SetPanelState(fadePanel, true);
        fadePanel.interactable = false;

        // Animación de Fundido (Alpha: 0f -> 1f)
        while (timer < transitionDuration)
        {
            // IMPORTANTE: USAR UNscaledDeltaTime para que el fundido funcione aunque Time.timeScale sea 0
            timer += Time.unscaledDeltaTime;
            fadePanel.alpha = Mathf.Lerp(0f, 1f, timer / transitionDuration);
            yield return null;
        }

        // Asegura que el panel esté completamente opaco.
        fadePanel.alpha = 1f;

        Debug.Log("Transición completa. Cargando Escena con índice: " + sceneIndex);

        // Carga de la nueva escena.
        SceneManager.LoadScene(sceneIndex);
    }

    // ------------------------------------------
    // --- Funciones Públicas Enlazables (Botones) ---
    // ------------------------------------------

    /// <summary>
    /// Función para el botón "Volver al Menú". Enlazado al botón "Volver al Menú" del Pause Panel.
    /// </summary>
    public void LoadMenu()
    {
        LoadScene(SCENE_MENU);
    }

    // Tenías dos funciones para volver al menú, mantenemos LoadMenu como la principal
    public void GoBackToMainMenu()
    {
        LoadMenu();
    }

    public void LoadLevel1()
    {
        LoadScene(SCENE_LEVEL_1);
    }

    public void LoadLevel2()
    {
        LoadScene(SCENE_LEVEL_2);
    }

    public void LoadLevel3()
    {
        LoadScene(SCENE_LEVEL_3);
    }

    public void LoadHowToPlay()
    {
        LoadScene(SCENE_HOW_TO_PLAY);
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del Juego...");
#if UNITY_STANDALONE
        Application.Quit();
#endif
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}