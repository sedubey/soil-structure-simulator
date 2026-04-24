using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class SimulationResetController : MonoBehaviour
{
    private Transform player;
    private CharacterController playerCharacterController;
    private Rigidbody playerRigidbody;
    private Vector3 playerSpawnPosition;
    private Quaternion playerSpawnRotation;
    private bool hasPlayerSpawn;

    private Transform playerCamera;
    private Quaternion playerCameraSpawnLocalRotation;
    private bool hasCameraSpawn;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindObjectOfType<SimulationResetController>() != null)
        {
            return;
        }

        GameObject host = new GameObject("Simulation Reset Controller");
        DontDestroyOnLoad(host);
        host.AddComponent<SimulationResetController>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void Start()
    {
        TryCachePlayerSpawn();
    }

    private void Update()
    {
        if (WasResetPressed())
        {
            ResetSimulation();
        }

        if (player == null || !hasPlayerSpawn)
        {
            TryCachePlayerSpawn();
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ClearCachedPlayer();
        TryCachePlayerSpawn();
    }

    public void ResetSimulation()
    {
        FoundationController[] foundations = FindObjectsOfType<FoundationController>();
        for (int i = 0; i < foundations.Length; i++)
        {
            foundations[i].ResetVisuals();
        }

        RetainingController[] retainingModules = FindObjectsOfType<RetainingController>();
        for (int i = 0; i < retainingModules.Length; i++)
        {
            retainingModules[i].ResetScenario();
        }

        RespawnPlayer();
    }

    private bool WasResetPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.R);
#else
        return false;
#endif
    }

    private void TryCachePlayerSpawn()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            playerObject = GameObject.Find("Player");
        }

        if (playerObject == null)
        {
            return;
        }

        if (player != playerObject.transform)
        {
            player = playerObject.transform;
            playerCharacterController = playerObject.GetComponent<CharacterController>();
            playerRigidbody = playerObject.GetComponent<Rigidbody>();
            hasPlayerSpawn = false;
            hasCameraSpawn = false;
        }

        if (!hasPlayerSpawn)
        {
            playerSpawnPosition = player.position;
            playerSpawnRotation = player.rotation;
            hasPlayerSpawn = true;
        }

        if (!hasCameraSpawn)
        {
            Camera childCamera = playerObject.GetComponentInChildren<Camera>(true);
            if (childCamera != null && childCamera.transform != null && childCamera.transform.IsChildOf(player))
            {
                playerCamera = childCamera.transform;
                playerCameraSpawnLocalRotation = playerCamera.localRotation;
                hasCameraSpawn = true;
            }
        }
    }

    private void RespawnPlayer()
    {
        if (player == null || !hasPlayerSpawn)
        {
            TryCachePlayerSpawn();
            if (player == null || !hasPlayerSpawn)
            {
                return;
            }
        }

        bool hadCharacterController = playerCharacterController != null;
        if (hadCharacterController)
        {
            playerCharacterController.enabled = false;
        }

        player.position = playerSpawnPosition;
        player.rotation = playerSpawnRotation;

        if (hasCameraSpawn && playerCamera != null && playerCamera.IsChildOf(player))
        {
            playerCamera.localRotation = playerCameraSpawnLocalRotation;
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }

        if (hadCharacterController)
        {
            playerCharacterController.enabled = true;
        }
    }

    private void ClearCachedPlayer()
    {
        player = null;
        playerCharacterController = null;
        playerRigidbody = null;
        hasPlayerSpawn = false;
        playerCamera = null;
        hasCameraSpawn = false;
    }
}
