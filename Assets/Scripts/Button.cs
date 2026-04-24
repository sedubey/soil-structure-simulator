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
        Debug.Log($"Button '{gameObject.name}' type: {buttonType}, controller: {(foundationController != null ? foundationController.gameObject.name : "NULL")}");
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"E pressed near {buttonType}");

            if (buttonType == ButtonType.RetainingStep)
            {
                if (retainingController != null)
                    retainingController.AdvanceStep();
                else
                    Debug.LogError("retainingController is NULL!");
            }

            if (buttonType == ButtonType.ShallowPass)
            {
                Debug.Log("Running ShallowPass");
                if (foundationController != null)
                    foundationController.RunScenario(0);
                else
                    Debug.LogError("foundationController is NULL!");
            }

            if (buttonType == ButtonType.ShallowFail)
            {
                Debug.Log("Running ShallowFail");
                if (foundationController != null)
                    foundationController.RunScenario(1);
                else
                    Debug.LogError("foundationController is NULL!");
            }

            if (buttonType == ButtonType.ShallowInteractive)
            {
                Debug.Log("Opening interactive popup");
                // Just open the popup, don't run anything else
                InteractiveShallowFoundationPopup.TryOpenPopup();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}