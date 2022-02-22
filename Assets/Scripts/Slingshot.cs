using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slingshot : MonoBehaviour
{
    static private Slingshot S;

    [Header("Set in Inspector")]
    public GameObject prefabProjectile;
    public float velocityMult = 8f;
    public GameObject leftBoundary;
    bool canFire = true;
    public Button splitButton;
    public Button smallButton;
    public Button normalButton;
    public Button largeButton;

    [Header("Set Dynamically")]
    public GameObject launchPoint;
    public Vector3 launchPos;
    public GameObject projectile;
    public bool aimingMode;
    private Rigidbody projectileRigidbody;
    private Rigidbody prefabProjectileRigidBody;
    private BallSize size;

    public enum BallSize
    {
        small,
        normal,
        big
    }

    static public Vector3 LAUNCH_POS
    {
        get
        {
            if (S == null) return Vector3.zero;
            return S.launchPos;
        }
    }

    void Awake()
    {
        S = this;

        Transform lauchPointTrans = transform.Find("LaunchPoint");
        launchPoint = lauchPointTrans.gameObject;
        launchPoint.SetActive(false);
        launchPos = lauchPointTrans.position;

        prefabProjectile.transform.localScale = new Vector3(1f, 1f, 1f);

        prefabProjectileRigidBody = prefabProjectile.GetComponent<Rigidbody>();
        prefabProjectileRigidBody.mass = 5f;
        velocityMult = 9f;

        size = BallSize.normal;
    }

    void OnMouseEnter()
    {
        //Debug.Log("Slingshot OnMouseEnter");
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Projectile");
        bool canFire = true;

        foreach(GameObject p in gos)
        {
            if (!p.GetComponent<Rigidbody>().IsSleeping())
            {
                canFire = false;
            }
        }

        if (canFire)
        {
            launchPoint.SetActive(true);
        }
    }

    void OnMouseExit()
    {
        //Debug.Log("Slingshot OnMouseExit");
        launchPoint.SetActive(false);
    }

    void OnMouseDown()
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Projectile");
        bool canFire = true;

        foreach (GameObject p in gos)
        {
            if (!p.GetComponent<Rigidbody>().IsSleeping())
            {
                canFire = false;
            }
        }

        if (canFire)
        {
            aimingMode = true;
            projectile = Instantiate(prefabProjectile);
            projectile.transform.position = launchPos;
            //projectile.GetComponent<Rigidbody>().isKinematic = true;
            projectileRigidbody = projectile.GetComponent<Rigidbody>();
            projectileRigidbody.isKinematic = true;

            if (size == BallSize.small)
            {
                splitButton.interactable = true;
            }
        }
    }

    void Update()
    {
        bool canFire = true;
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Projectile");

        foreach(GameObject p in gos)
        {
            if (!p.GetComponent<Rigidbody>().IsSleeping())
            {
                canFire = false;
            }
        }

        if (canFire)
        {
            smallButton.interactable = true;
            normalButton.interactable = true;
            largeButton.interactable = true;
        }
        else
        {
            smallButton.interactable = false;
            normalButton.interactable = false;
            largeButton.interactable = false;
        }

        if (aimingMode)
        {
            Vector3 mousePos2D = Input.mousePosition;
            mousePos2D.z = -Camera.main.transform.position.z;
            Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(mousePos2D);

            Vector3 mouseDelta = mousePos3D - launchPos;

            float maxMagnitude = GetComponent<SphereCollider>().radius;
            if (mouseDelta.magnitude > maxMagnitude)
            {
                mouseDelta.Normalize();
                mouseDelta *= maxMagnitude;
            }

            Vector3 projectilePos = launchPos + mouseDelta;
            projectile.transform.position = projectilePos;

            if (Input.GetMouseButtonUp(0))
            {
                aimingMode = false;
                projectileRigidbody.isKinematic = false;
                projectileRigidbody.velocity = -mouseDelta * velocityMult;
                FollowCam.POI = projectile;
                projectile = null;

                MissionDemolition.ShotFired();
            }
        }

        else
        {
            foreach (GameObject p in gos)
            {
                if (p.transform.position.x < -100f || p.transform.position.x > 200f)
                {
                    Destroy(p);
                }
            }
        }
    }

    public void SmallProjectile()
    {
        size = BallSize.small;
        prefabProjectile.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        prefabProjectileRigidBody = prefabProjectile.GetComponent<Rigidbody>();
        prefabProjectileRigidBody.mass = 2.5f;
        velocityMult = 12f;

        splitButton.interactable = true;
    }

    public void NormalProjectile()
    {
        size = BallSize.normal;
        prefabProjectile.transform.localScale = new Vector3(1f, 1f, 1f);

        prefabProjectileRigidBody = prefabProjectile.GetComponent<Rigidbody>();
        prefabProjectileRigidBody.mass = 5f;
        velocityMult = 9f;

        splitButton.interactable = false;
    }

    public void LargeProjectile()
    {
        size = BallSize.big;
        prefabProjectile.transform.localScale = new Vector3(2f, 2f, 2f);

        prefabProjectileRigidBody = prefabProjectile.GetComponent<Rigidbody>();
        prefabProjectileRigidBody.mass = 150f;
        velocityMult = 6f;

        splitButton.interactable = false;
    }

    public void SplitProjectile()
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Projectile");

        foreach (GameObject p in gos)
        {
            if (!p.GetComponent<Rigidbody>().IsSleeping())
            {
                GameObject p1, p2;

                Vector3 v1 = p.GetComponent<Rigidbody>().velocity;
                Vector3 v2 = p.GetComponent<Rigidbody>().velocity;
                v1.y += 2f;
                v2.y -= 2f;

                p1 = Instantiate(prefabProjectile);
                p2 = Instantiate(prefabProjectile);

                p1.GetComponent<Rigidbody>().mass /= 3;
                p2.GetComponent<Rigidbody>().mass /= 3;
                p.GetComponent<Rigidbody>().mass /= 3;

                p1.transform.position = p.transform.position;
                p2.transform.position = p.transform.position;

                p1.GetComponent<Rigidbody>().velocity = v1;
                p2.GetComponent<Rigidbody>().velocity = v2;
            }
        }

        splitButton.interactable = false;
    }
}
