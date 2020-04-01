using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[SelectionBase]
public class PlayerController : MonoBehaviour
{
    public bool walking = false;

    [Space]

    [Header("Transforms")]

    public Transform currentCube;
    public Transform clickedCube;
    public Transform indicator;

    [Space]

    public List<Transform> finalPath = new List<Transform>();

    //[Space]

    //[Header("End Game Effects")]
    //public ParticleSystem endGameParticleSystem;
    //public Text endGameText;

    private float blend;

    private int sceneIndex;

    float GetBlend()
    {
        return GetComponentInChildren<Animator>().GetFloat("Blend");
    }
    void SetBlend(float x)
    {
        GetComponentInChildren<Animator>().SetFloat("Blend", x);
    }

    void Start()
    {
        //endGameParticleSystem.gameObject.SetActive(false);
        //endGameParticleSystem.Stop();

        //endGameText.gameObject.SetActive(false);

        sceneIndex = SceneManager.GetActiveScene().buildIndex;

        RayCastDown();
    }

    void Update()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit mouseHit;

        //GET CURRENT CUBE (UNDER PLAYER)
        RayCastDown();

        if (currentCube.GetComponent<Walkable>().movingGround)
        {
            transform.parent = currentCube.parent;
        }
        else
        {
            transform.parent = null;
        }

        // CLICK ON CUBE
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(mouseRay, out mouseHit))
            {
                if (mouseHit.transform.GetComponent<Walkable>() != null)
                {
                    clickedCube = mouseHit.transform;

                    DOTween.Kill(gameObject.transform);
                    finalPath.Clear();
                    FindPath();

                    blend = transform.position.y - clickedCube.position.y > 0 ? -1 : 1;

                    indicator.position = mouseHit.transform.GetComponent<Walkable>().GetWalkPoint();
                    Sequence s = DOTween.Sequence();
                    s.AppendCallback(() => indicator.GetComponentInChildren<ParticleSystem>().Play());
                    s.Append(indicator.GetComponent<Renderer>().material.DOColor(Color.white, 0.1f));
                    s.Append(indicator.GetComponent<Renderer>().material.DOColor(Color.red, 0.3f).SetDelay(0.2f));
                    s.Append(indicator.GetComponent<Renderer>().material.DOColor(Color.clear, 0.3f));
                }
            }
        }

        //if (Input.GetMouseButton(0))
        //{
        //    if (Physics.Raycast(mouseRay, out mouseHit))
        //    {
        //        if (mouseHit.transform.gameObject.tag == "Simple Rotator")
        //        {
        //            /*TODO: Rotate platform with mouse movement.*/
        //            Debug.Log("TODO: Rotate platform with mouse movement.");

        //            if (Input.GetAxis("Mouse X") != 0)
        //            {
        //                int multiplier = 0;
        //                if (Input.GetAxis("Mouse X") > 0.1)
        //                {
        //                    multiplier = -1;
        //                }
        //                else if (Input.GetAxis("Mouse X") < -0.1)
        //                {
        //                    multiplier = 1;
        //                }

        //                foreach (Transform pv in pivots)
        //                {
        //                    pv.DOComplete();
        //                    pv.DORotate(new Vector3(90 * multiplier, 0, 0), 0.6f, RotateMode.WorldAxisAdd).SetEase(Ease.OutBack);
        //                }
        //            }

        //        }
        //    }
        //}
    }

    void FindPath()
    {
        List<Transform> nextCubes = new List<Transform>();
        List<Transform> pastCubes = new List<Transform>();

        foreach (WalkPath path in currentCube.GetComponent<Walkable>().possiblePaths)
        {
            if (path.active)
            {
                nextCubes.Add(path.target);
                path.target.GetComponent<Walkable>().previousBlock = currentCube;
            }
        }

        pastCubes.Add(currentCube);

        ExplorePaths(nextCubes, pastCubes);
        BuildPath();
    }

    void ExplorePaths(List<Transform> nextCubes, List<Transform> visitedCubes)
    {
        Transform current = nextCubes.First();
        nextCubes.Remove(current);

        if (current == clickedCube)
        {
            return;
        }

        foreach (WalkPath path in current.GetComponent<Walkable>().possiblePaths)
        {
            if (!visitedCubes.Contains(path.target) && path.active)
            {
                nextCubes.Add(path.target);
                path.target.GetComponent<Walkable>().previousBlock = current;
            }
        }

        visitedCubes.Add(current);

        if (nextCubes.Any())
        {
            ExplorePaths(nextCubes, visitedCubes);
        }
    }

    void BuildPath()
    {
        Transform cube = clickedCube;
        while (cube != currentCube)
        {
            finalPath.Add(cube);
            if (cube.GetComponent<Walkable>().previousBlock != null)
            {
                cube = cube.GetComponent<Walkable>().previousBlock;
            }
            else
            {
                return;
            }
        }

        finalPath.Insert(0, clickedCube);

        FollowPath();
    }

    void FollowPath()
    {
        Sequence s = DOTween.Sequence();

        walking = true;

        for (int i = finalPath.Count - 1; i > 0; i--)
        {
            float time = finalPath[i].GetComponent<Walkable>().isStair ? 1.5f : 1;

            s.Append(transform.DOMove(finalPath[i].GetComponent<Walkable>().GetWalkPoint(), 0.2f * time).SetEase(Ease.Linear));

            if (!finalPath[i].GetComponent<Walkable>().dontRotate)
            {
                s.Join(transform.DOLookAt(finalPath[i].position, 0.1f, AxisConstraint.Y, Vector3.up));
            }
        }

        if (clickedCube.GetComponent<Walkable>().isButton)
        {
            s.AppendCallback(() => GameManager.instance.RotateRightPivot());
        }

        s.AppendCallback(() => ClearPath());
    }

    void ClearPath()
    {
        foreach (Transform t in finalPath)
        {
            t.GetComponent<Walkable>().previousBlock = null;
        }
        finalPath.Clear();
        walking = false;
    }

    public void RayCastDown()
    {

        Ray playerRay = new Ray(transform.GetChild(0).position, -transform.up);
        RaycastHit playerHit;

        if (Physics.Raycast(playerRay, out playerHit))
        {
            if (playerHit.transform.GetComponent<Walkable>() != null)
            {
                //Debug.Log("found walkable: " + playerHit.transform.gameObject.name);

                currentCube = playerHit.transform;

                if (playerHit.transform.GetComponent<Walkable>().isStair)
                {
                    DOVirtual.Float(GetBlend(), blend, 0.1f, SetBlend);
                }
                else if (playerHit.transform.GetComponent<Walkable>().isButton)
                {
                    currentCube.GetChild(0).transform.position = currentCube.GetChild(0).transform.parent.transform.position + new Vector3(0, 0.2f, 0);
                }
                else
                {
                    DOVirtual.Float(GetBlend(), 0, 0.1f, SetBlend);
                }
            }
            else
            {
                //Debug.Log("found transform: " + playerHit.transform.gameObject.name);
            }
        }
        else
        {
            //Debug.Log("Didn't Hit");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Ray ray = new Ray(transform.GetChild(0).position, -transform.up);
        Gizmos.DrawRay(ray);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Finish Cube"))
        {
            if (sceneIndex == 0)
            {
                Transform cameraTransform = Camera.main.transform;
                Camera.main.transform.DOMove(new Vector3(cameraTransform.position.x, 45, Camera.main.transform.position.z), 2.5f).SetEase(Ease.Linear);
            }
            else if (sceneIndex == 1)
            {
                //endGameParticleSystem.gameObject.SetActive(true);
                //endGameParticleSystem.Play();

                //endGameText.gameObject.SetActive(true);

                Debug.Log("Hurraaaaay!!!");
            }
        }
    }
}
