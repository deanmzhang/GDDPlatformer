using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabberScript : MonoBehaviour {
    public HandController grabHand;

    public Transform grabTarget;
    Transform grabCenter;
    Vector3 hitOffset;

    public float reachDistance;
    public float holdDistance;
    public LayerMask grabbableLayers;

    public float grabForce;
    public float grabDrag;
    public float grabBreakDist;

    Collider2D playerCol;

    Rigidbody2D grabbedRb;
    Collider2D grabbedCol;
    Player player;

    float oldDrag;
    float oldGravity;
    Vector2 oldCenterOfMass;

    bool isGrabbing;

    void Start() {
        playerCol = GetComponentInChildren<Collider2D>();
        player = GetComponent<Player>();
        grabCenter = grabTarget.parent;
    }

    void LateUpdate() {
        //super trass
        bool isTopScreen = Input.mousePosition.y > (Screen.height * 0.85f);

        if (Input.GetMouseButtonDown(0) && !isTopScreen && grabbedRb == null && !CraftingManager.instance.isCrafting) {
            StartCoroutine(GrabSequence());
        }

        if (Input.GetMouseButtonUp(0)) {
            StopAllCoroutines();
            Drop();
        }

        if (grabbedRb != null) {
            grabTarget.transform.position = grabbedRb.transform.TransformPoint(hitOffset);
        } else if (isGrabbing) {
            Drop();
        }
    }

    void Grab(Collider2D col) {
        isGrabbing = true;
        grabbedRb = col.GetComponent<Rigidbody2D>();

        oldDrag = grabbedRb.drag;
        oldGravity = grabbedRb.gravityScale;
        oldCenterOfMass = grabbedRb.centerOfMass;

        grabbedRb.drag = grabDrag;
        grabbedRb.centerOfMass = hitOffset;
        grabbedRb.gravityScale = 0;

        grabbedRb.interpolation = RigidbodyInterpolation2D.Extrapolate;

        grabbedCol = col;

        if (!col.CompareTag("Enemy")) {
            Physics2D.IgnoreCollision(playerCol, grabbedCol, true);
        }
    }

    void Drop() {
        isGrabbing = false;
        if (grabbedRb != null) {
            grabbedRb.drag = oldDrag;
            grabbedRb.gravityScale = oldGravity;
            grabbedRb.centerOfMass = oldCenterOfMass;
            grabbedRb.interpolation = RigidbodyInterpolation2D.None;
            Physics2D.IgnoreCollision(playerCol, grabbedCol, false);
        }
        grabbedRb = null;
        grabHand.Reparent(false, false);
        
        StopAllCoroutines();
    }

    void FixedUpdate() {
        if (grabbedRb == null) {
            return;
        }

        Vector3 mousePos = player.mousePos;
        Vector3 dir = mousePos - grabCenter.position;
        if (dir.magnitude > holdDistance) {
            dir = dir.normalized * holdDistance;
        }

        Vector3 target = grabCenter.transform.position + dir;
        Vector3 diff = target - grabTarget.transform.position;
        float dist = diff.magnitude;

        if (dist > grabBreakDist) {
            Drop();
            return;
        }

        float powerRatio = Mathf.Clamp01(dist / reachDistance);
        grabbedRb.AddForce(diff * powerRatio * grabForce * grabbedRb.mass);
    }

    IEnumerator GrabSequence() {
        Vector3 mousePos = player.mousePos;
        Vector3 dir = mousePos - grabCenter.position;

        grabHand.Reparent(false, false, grabTarget);

        RaycastHit2D hitInfo = Physics2D.Raycast(grabCenter.position, dir, reachDistance, grabbableLayers);
        Collider2D hitCol = null;

        if (hitInfo.collider != null) {
            WeightedObject obj = hitInfo.collider.GetComponent<WeightedObject>();
            if (obj != null && obj.isGrabbable) {
                hitCol = hitInfo.collider;
                grabTarget.transform.position = hitInfo.point;
                hitOffset = hitInfo.transform.InverseTransformPoint(grabTarget.transform.position);
            }
        }

        if (hitCol == null) {
            grabTarget.parent = grabCenter;
            grabTarget.transform.localPosition = dir.normalized * reachDistance;
        }

        float timer = 0.16f; //can't be bothered lol
        while (timer > 0f) {
            if (hitCol != null) {
                grabTarget.transform.position = hitCol.transform.TransformPoint(hitOffset);
            }
            yield return null;
            timer -= Time.deltaTime;
        }

        bool tookTwoTries = false;

        if (hitCol == null) {
            //try again
            hitInfo = Physics2D.Raycast(grabCenter.position, dir, reachDistance, grabbableLayers);
            if (hitInfo.collider != null) {
                WeightedObject obj = hitInfo.collider.GetComponent<WeightedObject>();
                if (obj != null && obj.isGrabbable) {
                    hitCol = hitInfo.collider;
                    grabTarget.transform.position = hitInfo.point;
                    hitOffset = hitInfo.transform.InverseTransformPoint(hitInfo.point);
                    tookTwoTries = true;
                }
            }
        }

        if (hitCol != null) {
            Grab(hitInfo.collider);
            if (tookTwoTries) {
                yield return new WaitForSeconds(0.15f); //super hacky but just prevents some jitteriness
            }
            grabHand.Reparent(false, false, grabTarget);
        } else {
            Drop();
        }
    }

    public void OnDeath() {
        Drop();
        Destroy(grabHand);
    }
}
