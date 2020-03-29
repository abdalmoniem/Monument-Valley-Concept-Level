using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public PlayerController player;
    public List<PathCondition> pathConditions = new List<PathCondition>();
    public List<Transform> pivots;

    public Transform[] objectsToHide;

    public enum EaseOutPattern
    {
        Unset = Ease.Unset,
        Linear = Ease.Linear,
        InSine = Ease.InSine,
        OutSine = Ease.OutSine,
        InOutSine = Ease.InOutSine,
        InQuad = Ease.InQuad,
        OutQuad = Ease.OutQuad,
        InOutQuad = Ease.InOutQuad,
        InCubic = Ease.InCubic,
        OutCubic = Ease.OutCubic,
        InOutCubic = Ease.InOutCubic,
        InQuart = Ease.InQuart,
        OutQuart = Ease.OutQuart,
        InOutQuart = Ease.InOutQuart,
        InQuint = Ease.InQuint,
        OutQuint = Ease.OutQuint,
        InOutQuint = Ease.InOutQuint,
        InExpo = Ease.InExpo,
        OutExpo = Ease.OutExpo,
        InOutExpo = Ease.InOutExpo,
        InCirc = Ease.InCirc,
        OutCirc = Ease.OutCirc,
        InOutCirc = Ease.InOutCirc,
        InElastic = Ease.InElastic,
        OutElastic = Ease.OutElastic,
        InOutElastic = Ease.InOutElastic,
        InBack = Ease.InBack,
        OutBack = Ease.OutBack,
        InOutBack = Ease.InOutBack,
        InBounce = Ease.InBounce,
        OutBounce = Ease.OutBounce,
        InOutBounce = Ease.InOutBounce,
        Flash = Ease.Flash,
        InFlash = Ease.InFlash,
        OutFlash = Ease.OutFlash,
        InOutFlash = Ease.InOutFlash,
    };

    public EaseOutPattern easeOutPattern = EaseOutPattern.OutBack;

    [Space]

    [Header("Center Pivot Drag Threshold")]
    public float dragThreshold = 1;

    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit mouseHit;

        foreach (PathCondition pc in pathConditions)
        {
            int count = 0;
            for (int i = 0; i < pc.conditions.Count; i++)
            {
                if (pc.conditions[i].conditionObject.eulerAngles == pc.conditions[i].eulerAngle)
                {
                    count++;
                }
            }
            foreach (SinglePath sp in pc.paths)
            {
                sp.block.possiblePaths[sp.index].active = (count == pc.conditions.Count);
            }
        }

        //if (player.walking)
        //{
        //    return;
        //}

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            int multiplier = Input.GetKey(KeyCode.RightArrow) ? 1 : -1;
            pivots[0].DOComplete();
            pivots[0].DORotate(new Vector3(0, 90 * multiplier, 0), .6f, RotateMode.WorldAxisAdd).SetEase((Ease) easeOutPattern);
        }

        // DRAG CENTER PIVOT
        if (Input.GetMouseButton(0))
        {
            if (Physics.Raycast(mouseRay, out mouseHit))
            {
                if (mouseHit.transform.gameObject.tag == "Platform Rotator")
                {
                    /*TODO: Rotate platform with mouse movement.*/
                    Debug.Log("TODO: Rotate platform with mouse movement.");

                    if (Input.GetAxis("Mouse X") != 0)
                    {
                        int multiplier = 0;
                        if (Input.GetAxis("Mouse X") > dragThreshold)
                        {
                            multiplier = -1;
                        }
                        else if (Input.GetAxis("Mouse X") < -dragThreshold)
                        {
                            multiplier = 1;
                        }

                        pivots[0].DOComplete();
                        pivots[0].DORotate(new Vector3(0, 90 * multiplier, 0), .6f, RotateMode.WorldAxisAdd).SetEase((Ease) easeOutPattern);
                    }
                }
            }
        }

        foreach (Transform t in objectsToHide)
        {
            t.gameObject.SetActive((pivots[0].eulerAngles.y > 45) && (pivots[0].eulerAngles.y < (90 + 45)));
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
        }

    }

    public void RotateRightPivot()
    {
        pivots[1].DOComplete();
        pivots[1].DORotate(new Vector3(0, 0, 90), 1.5f).SetEase(Ease.InElastic);
    }
}

[System.Serializable]
public class PathCondition
{
    public string pathConditionName;
    public List<Condition> conditions;
    public List<SinglePath> paths;
}

[System.Serializable]
public class Condition
{
    public Transform conditionObject;
    public Vector3 eulerAngle;

}

[System.Serializable]
public class SinglePath
{
    public Walkable block;
    public int index;
}
