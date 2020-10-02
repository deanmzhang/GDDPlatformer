using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour {
    public float moveTime;

    Transform animParent;
    Transform curTarget;

    float curAngle; //better to keep this here instead of dealing with quaternions
    float targetAngle;

    float rotSpeed;
    float moveSpeed;

    const float MIN_SPEED = 10f;

    void Start() {
        animParent = transform.parent;
        transform.parent = null;

        Reparent(false, true);
    }

    public void Reparent(bool isClockwise, bool forcePos, Transform newTarget = null) {
        if (newTarget == null) {
            newTarget = animParent;
        }

        float dist = Vector3.Distance(newTarget.transform.position, transform.position);
        moveSpeed = Mathf.Max(dist / moveTime, MIN_SPEED);

        if (forcePos) {
            moveSpeed = float.MaxValue;
        }

        curTarget = newTarget;
        targetAngle = curTarget.eulerAngles.z;
        if (isClockwise) {
            if (targetAngle > curAngle) {
                targetAngle -= 360; //we would've rotated counter clockwise, so move the target the other way
            }
        } else {
            if (targetAngle < curAngle) {
                targetAngle += 360; //reverse
            }
        }

        rotSpeed = Mathf.Abs((targetAngle - curAngle) / moveTime);
    }

    void LateUpdate() {
        transform.position = Vector3.MoveTowards(transform.position, curTarget.position, moveSpeed * Time.deltaTime);
        curAngle = Mathf.MoveTowards(curAngle, targetAngle, rotSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, 0, curAngle);
    }
}
