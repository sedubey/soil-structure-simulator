using UnityEngine;
using System.Collections;

public class PitUnderpinningController : MonoBehaviour
{
    [Header("Structural Elements")]
    public Transform steelBeams;
    public Transform concretePit1;
    public Transform concretePit2;
    public Transform concretePit3;
    public Transform concretePit4;
    public Transform concretePit5;

    [Header("Movement")]
    public float beamDropDist = 4f;
    public float pitDropDist = 2f;
    public float moveSpeed = 2f;

    private int curr = 0;

    private Vector3 beamsTarget;
    private Vector3 pit1Target;
    private Vector3 pit2Target;
    private Vector3 pit3Target;
    private Vector3 pit4Target;
    private Vector3 pit5Target;

    private Vector3 beamsStart;
    private Vector3 pit1Start;
    private Vector3 pit2Start;
    private Vector3 pit3Start;
    private Vector3 pit4Start;
    private Vector3 pit5Start;
    private bool hasCachedStart;

    void Start()
    {
        CacheStartState();

        beamsTarget = steelBeams.position + Vector3.down * beamDropDist;
        pit1Target = concretePit1.position + Vector3.down * pitDropDist;
        pit2Target = concretePit2.position + Vector3.down * pitDropDist;
        pit3Target = concretePit3.position + Vector3.down * pitDropDist;
        pit4Target = concretePit4.position + Vector3.down * pitDropDist;
        pit5Target = concretePit5.position + Vector3.down * pitDropDist;

        ResetScenario();
    }

    public void AdvanceStep()
    {
        if (curr >= 11)
        {
            return;
        }
        curr++;
        Step();
    }

    void Step()
    {
        switch (curr)
        {
            case 1:
                RemoveByTag("PitDig1");
                StartCoroutine(Move(steelBeams, beamsTarget));
                break;

            case 2:
                RemoveByTag("PitDig2");
                break;

            case 3:
                StartCoroutine(Move(concretePit1, pit1Target));
                break;

            case 4:
                RemoveByTag("PitDig3");
                break;

            case 5:
                StartCoroutine(Move(concretePit2, pit2Target));
                break;

            case 6:
                RemoveByTag("PitDig4");
                break;

            case 7:
                StartCoroutine(Move(concretePit3, pit3Target));
                break;

            case 8:
                RemoveByTag("PitDig5");
                break;

            case 9:
                StartCoroutine(Move(concretePit4, pit4Target));
                break;

            case 10:
                RemoveByTag("PitDig6");
                break;

            case 11:
                StartCoroutine(Move(concretePit5, pit5Target));
                break;

            default:
                break;
        }
    }

    private void CacheStartState()
    {
        if (hasCachedStart) return;

        if (steelBeams != null) beamsStart = steelBeams.position;
        if (concretePit1 != null) pit1Start = concretePit1.position;
        if (concretePit2 != null) pit2Start = concretePit2.position;
        if (concretePit3 != null) pit3Start = concretePit3.position;
        if (concretePit4 != null) pit4Start = concretePit4.position;
        if (concretePit5 != null) pit5Start = concretePit5.position;

        hasCachedStart = true;
    }

    public void ResetScenario()
    {
        CacheStartState();
        StopAllCoroutines();
        curr = 0;

        if (steelBeams != null)
        {
            steelBeams.position = beamsStart;
            SetVisible(steelBeams, false);
        }

        if (concretePit1 != null)
        {
            concretePit1.position = pit1Start;
            SetVisible(concretePit1, false);
        }

        if (concretePit2 != null)
        {
            concretePit2.position = pit2Start;
            SetVisible(concretePit2, false);
        }

        if (concretePit3 != null)
        {
            concretePit3.position = pit3Start;
            SetVisible(concretePit3, false);
        }

        if (concretePit4 != null)
        {
            concretePit4.position = pit4Start;
            SetVisible(concretePit4, false);
        }

        if (concretePit5 != null)
        {
            concretePit5.position = pit5Start;
            SetVisible(concretePit5, false);
        }

        SetByTag("PitDig1", true);
        SetByTag("PitDig2", true);
        SetByTag("PitDig3", true);
        SetByTag("PitDig4", true);
        SetByTag("PitDig5", true);
        SetByTag("PitDig6", true);
    }

    void RemoveByTag(string tagName)
    {
        SetByTag(tagName, false);
    }

    private void SetByTag(string tagName, bool visible)
    {
        GameObject[] blocks = GameObject.FindGameObjectsWithTag(tagName);
        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i].SetActive(visible);
        }
    }

    IEnumerator Move(Transform obj, Vector3 target)
    {
        SetVisible(obj, true);

        while (Vector3.Distance(obj.position, target) > 0.01f)
        {
            obj.position = Vector3.MoveTowards(obj.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    void SetVisible(Transform obj, bool visible)
    {
        Renderer[] rends = obj.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < rends.Length; i++)
        {
            rends[i].enabled = visible;
        }
    }
}