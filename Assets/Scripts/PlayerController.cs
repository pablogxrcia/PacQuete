using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    // --- PROPIEDADES PÚBLICAS ---
    public float playerSpeed;

    [Header("Invencibilidad (Pastilla de Poder)")]
    public float invincibilityDuration = 10f; // Usamos float para mayor precisión
    public bool isInvincible = false;

    [Header("Referencias de Escena")]
    public int defeatSceneIndex = 5; // Índice de la escena de Derrota (Global)

    [Header("Sonido de Recoger Punto")]
    // [1] Clip de sonido para asignar en el Inspector
    public AudioClip pointCollectSound;

    // Índices de las escenas de Victoria específicas por nivel
    public int victoryLevel1Index = 4; // Victoria del Nivel 1
    public int victoryLevel2Index = 6; // Victoria del Nivel 2
    public int victoryLevel3Index = 9; // Victoria del Nivel 3

    // --- NUEVA REFERENCIA DE TRANSICIÓN ---
    private GameMenuManager gameMenuManager; // Referencia al script que maneja el Fade Out

    // --- PROPIEDADES PRIVADAS ---
    private Rigidbody playerRb;
    private AudioSource audioSource;
    private float invincibilityTimer;
    private int totalPointsInScene;
    private bool gameOver = false;

    // Start se llama una vez al inicio
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();

        // 1. Encontrar el GameMenuManager en la escena
        gameMenuManager = FindAnyObjectByType<GameMenuManager>();
        if (gameMenuManager == null)
        {
            Debug.LogError("FATAL: No se encontró el GameMenuManager en la escena. Las transiciones no funcionarán.");
        }

        audioSource = GetComponent<AudioSource>();

        // Comprobación de seguridad
        if (audioSource == null)
        {
            Debug.LogError("El Player necesita un componente AudioSource para reproducir sonidos.");
        }

        // 2. Contar todos los puntos con el tag "Point" al inicio del nivel
        // Usamos FindGameObjectsWithTag en lugar de GetComponentsInChildren para asegurarnos de contar TODOS los puntos
        GameObject[] points = GameObject.FindGameObjectsWithTag("Point");
        totalPointsInScene = points.Length;

        // Comprobación de integridad: si no hay puntos, evitamos un error de división por cero o victoria instantánea
        if (totalPointsInScene == 0)
        {
            Debug.LogWarning("No se encontraron objetos con el Tag 'Point'. La condición de victoria no se cumplirá.");
        }
    }

    // Update se llama una vez por frame
    void Update()
    {
        // Si el juego ha terminado (victoria o derrota), no permitimos mover al jugador
        if (gameOver) return;

        PlayerMove();
        CheckInvincibilityTime();
    }

    // Controla el movimiento del jugador
    void PlayerMove()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        // Aseguramos que solo haya movimiento en X y Z (plano)
        Vector3 movement = new Vector3(moveX, 0f, moveY);

        // Usar velocity para un mejor control en Rigidbody
        playerRb.linearVelocity = movement.normalized * playerSpeed;
    }

    // Temporizador para la invencibilidad
    void CheckInvincibilityTime()
    {
        if (isInvincible)
        {
            invincibilityTimer += Time.deltaTime;

            // Lógica de parpadeo (opcional, para feedback visual)
            if (Mathf.RoundToInt(invincibilityTimer * 5) % 2 == 0)
            {
                // Ejemplo: hace parpadear la malla para indicar invencibilidad
                // GetComponent<Renderer>().enabled = true; 
            }
            else
            {
                // GetComponent<Renderer>().enabled = false;
            }

            if (invincibilityTimer >= invincibilityDuration)
            {
                EndInvincibility();
            }
        }
    }

    public void StartInvincibility()
    {
        isInvincible = true;
        invincibilityTimer = 0f;
        // Aquí puedes añadir código para cambiar el color del jugador o activar un efecto visual
    }

    public void EndInvincibility()
    {
        isInvincible = false;
        // Restaura la apariencia normal del jugador
        // GetComponent<Renderer>().enabled = true; 
    }

    // Colisiones con otros objetos
    private void OnTriggerEnter(Collider other)
    {
        if (gameOver) return;

        // --- COLISIÓN CON PUNTOS ---
        if (other.CompareTag("Point"))
        {

            if (audioSource != null && pointCollectSound != null)
            {
                // PlayOneShot reproduce el clip de una vez y permite que otros sonidos sigan reproduciéndose
                audioSource.PlayOneShot(pointCollectSound);
            }
            // Destruye el punto y actualiza el contador
            Destroy(other.gameObject);
            totalPointsInScene--;

            // Lógica para puntos de poder (si tienen un tag diferente, ej. "PowerPellet")
            // Tendrías que etiquetar los puntos grandes como "PowerPellet" si quieres que esta lógica funcione bien
            if (other.name.Contains("PowerPellet") || other.name.Contains("power"))
            {
                StartInvincibility();
            }

            // Verifica si el jugador ganó
            CheckVictoryCondition();
        }

        // --- COLISIÓN CON ENEMIGO ---
        if (other.CompareTag("Enemy") && !isInvincible)
        {
            // Derrota: Si no eres invencible y tocas un enemigo.
            HandleDefeat();
        }
        else if (other.CompareTag("Enemy") && isInvincible)
        {
            // Fantasma comido: Envía al fantasma a su base.
            Debug.Log("Fantasma comido!");
            // Nota: La lógica para que el fantasma reaparezca/vuelva a su base está en EnemiyController.cs,
            // que detecta el estado "isInvincible" del jugador.
        }
    }

    // Verifica si todos los puntos fueron comidos.
    void CheckVictoryCondition()
    {
        if (totalPointsInScene <= 0)
        {
            Debug.Log("¡Nivel Completado! Determinando escena de Victoria...");
            gameOver = true;

            // Obtener el índice de la escena actual
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextVictorySceneIndex = -1; // Valor predeterminado para errores

            // Lógica para determinar la escena de victoria basada en el nivel actual
            // NOTA: Los índices de Nivel 1, 2, y 3 deben coincidir con SCENE_LEVEL_1, SCENE_LEVEL_2, etc. en GameMenuManager.cs
            switch (currentSceneIndex)
            {
                case 1: // SCENE_LEVEL_1 (Asumido)
                    nextVictorySceneIndex = victoryLevel1Index; // 4
                    break;
                case 2: // SCENE_LEVEL_2 (Asumido)
                    nextVictorySceneIndex = victoryLevel2Index; // 6
                    break;
                case 4: // SCENE_LEVEL_3 (Asumido)
                    nextVictorySceneIndex = victoryLevel3Index; // 8
                    break;
                default:
                    Debug.LogWarning("Escena actual (" + currentSceneIndex + ") no mapeada. Cargando Victoria de Nivel 1 por defecto.");
                    nextVictorySceneIndex = victoryLevel1Index;
                    break;
            }

            // Cargar la escena de victoria
            if (gameMenuManager != null)
            {
                gameMenuManager.LoadScene(nextVictorySceneIndex);
            }
            else
            {
                SceneManager.LoadScene(nextVictorySceneIndex);
            }
        }
    }

    // Maneja la condición de derrota (Game Over).
    void HandleDefeat()
    {
        if (!gameOver)
        {
            Debug.Log("¡Derrota! Game Over.");
            gameOver = true;
            // Opcional: Desactiva el jugador para evitar más colisiones
            gameObject.SetActive(false);

            // *** AHORA LLAMAMOS AL MANAGER PARA LA TRANSICIÓN ***
            if (gameMenuManager != null)
            {
                gameMenuManager.LoadScene(defeatSceneIndex);
            }
            else
            {
                SceneManager.LoadScene(defeatSceneIndex);
            }
        }
    }
}