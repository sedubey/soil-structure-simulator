using UnityEngine;

public class Button : MonoBehaviour
{
    public enum ButtonType
    {
        RetainingStep,
        ShallowPass,
        ShallowFail,
        ShallowInteractive
    }

    public ButtonType buttonType;

    public RetainingController retainingController;
    public FoundationController foundationController;

    private bool playerInRange = false;

    void Start()
    {
        Debug.Log($"Button '{gameObject.name}' initialized with type: {buttonType}, foundationController: {(foundationController != null ? foundationController.gameObject.name : "NULL")}");
    }

    void Update()
    {
        // Keyboard shortcuts for testing
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("KEYBOARD: Running ShallowPass");
            if (foundationController != null)
                foundationController.RunScenario(0);
            else
                Debug.LogError("foundationController is NULL!");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("KEYBOARD: Running ShallowFail");
            if (foundationController != null)
                foundationController.RunScenario(1);
            else
                Debug.LogError("foundationController is NULL!");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("KEYBOARD: Running ShallowInteractive");
            if (foundationController != null)
                foundationController.RunScenario(2);
            else
                Debug.LogError("foundationController is NULL!");
        }

        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"Player pressed E near button type: {buttonType}");

            if (buttonType == ButtonType.RetainingStep)
            {
                if (retainingController != null)
                {
                    retainingController.AdvanceStep();
                }
                else
                {
                    Debug.LogError("retainingController is NULL!");
                }
            }

            if (buttonType == ButtonType.ShallowPass)
            {
                Debug.Log("Running shallow pass");
                if (foundationController != null)
                {
                    foundationController.RunScenario(0);
                }
                else
                {
                    Debug.LogError("foundationController is NULL! Cannot run pass scenario.");
                }
            }

            if (buttonType == ButtonType.ShallowFail)
            {
                Debug.Log("Running shallow fail");
                if (foundationController != null)
                {
                    foundationController.RunScenario(1);
                }
                else
                {
                    Debug.LogError("foundationController is NULL! Cannot run fail scenario.");
                }
            }

            if (buttonType == ButtonType.ShallowInteractive)
            {
                Debug.Log("Opening shallow interactive popup");
                
                bool popupOpened = InteractiveShallowFoundationPopup.TryOpenPopup();
                Debug.Log($"TryOpenPopup returned: {popupOpened}");
                
                if (!popupOpened && foundationController != null)
                {
                    Debug.Log("Popup failed to open, falling back to direct RunScenario(2)");
                    foundationController.RunScenario(2);
                }
                else if (!popupOpened && foundationController == null)
                {
                    Debug.LogError("Popup failed AND foundationController is NULL!");
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Something entered trigger on '{gameObject.name}': {other.name}");

        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log($"Player entered range of '{gameObject.name}' ({buttonType})");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log($"Player left range of '{gameObject.name}'");
        }
    }
}