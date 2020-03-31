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

    [Space]
    public Transform[] objectsToHide;

    public Ease easeOutPattern = Ease.OutBack;

    [Space]

    [Header("Center Pivot Drag Threshold")]
    public float dragThreshold = 1;

    private int sceneIndex;

    private void Awake()
    {
        instance = this;

        sceneIndex = SceneManager.GetActiveScene().buildIndex;
    }

    void Update()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit mouseHit;

        //if (player.walking)
        //{
        //    return;
        //}

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

        foreach (Transform t in objectsToHide)
        {
            if (sceneIndex == 1)
            {
                t.gameObject.SetActive((pivots[0].eulerAngles.y > 45) && (pivots[0].eulerAngles.y < (90 + 45)));
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (sceneIndex == 0)
            {
                int multiplier = Input.GetKey(KeyCode.RightArrow) ? -1 : 1;
                foreach (Transform pv in pivots)
                {
                    pv.DOComplete();
                    pv.DORotate(new Vector3(90 * multiplier, 0, 0), .6f, RotateMode.WorldAxisAdd).SetEase(Ease.OutBack);
                }
            }
            else if (sceneIndex == 1)
            {
                int multiplier = Input.GetKey(KeyCode.RightArrow) ? 1 : -1;
                pivots[0].DOComplete();
                pivots[0].DORotate(new Vector3(0, 90 * multiplier, 0), .6f, RotateMode.WorldAxisAdd).SetEase(easeOutPattern);
            }
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
                        pivots[0].DORotate(new Vector3(0, 90 * multiplier, 0), .6f, RotateMode.WorldAxisAdd).SetEase(easeOutPattern);
                    }
                }
            }
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
