using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolEnemy : EnemyController {
    public float speed;

    Rigidbody2D enemyRb;

    public Transform groundCheck;
    public Transform frontCheck;

    public LayerMask boundLayers;

    public Transform customLeftBound;
    public Transform customRightBound;
    public bool boundToPlatform;

    public float stateCheckDelay; //how often to check for obstacles

    float nextStateCheck;
    bool isGrounded;
    bool isFlipped;

    const float GROUND_CHECK_STEP_SIZE = 0.5f; //how frequently we check for ground
    const int MAX_CHECK_ATTEMPTS = 300;

    protected override void OnStart() {
        base.OnStart();
        enemyRb = GetComponent<Rigidbody2D>();

        if (boundToPlatform) {
            SetupBounds();
        }
    }

    void SetupBounds() {
        if (customLeftBound == null) {
            GameObject newLeftBound = new GameObject(gameObject.name + " - left bound");
            customLeftBound = newLeftBound.transform;
        }

        if (customRightBound == null) {
            GameObject newRightBound = new GameObject(gameObject.name + " - right bound");
            customRightBound = newRightBound.transform;
        }

        //first, find ground
        customLeftBound.transform.position = transform.position;
        RaycastHit2D hitInfo;
        int maxAttempts = MAX_CHECK_ATTEMPTS;
        do {
            customLeftBound.transform.position += Vector3.down * GROUND_CHECK_STEP_SIZE;
            hitInfo = Physics2D.Raycast(customLeftBound.position, Vector2.zero, 1f, boundLayers);
            maxAttempts--;
        } while ((hitInfo.collider == null || hitInfo.rigidbody == enemyRb) && maxAttempts > 0);

        if (hitInfo.collider == null) {
            Debug.LogError("No ground");
            return;
        }

        customRightBound.transform.position = customLeftBound.transform.position;

        //position left bound
        Vector3 nextPos = customLeftBound.transform.position;
        maxAttempts = MAX_CHECK_ATTEMPTS;
        do {
            customLeftBound.transform.position = nextPos;
            nextPos = customLeftBound.transform.position - Vector3.right * GROUND_CHECK_STEP_SIZE;
            hitInfo = Physics2D.Raycast(nextPos, Vector2.zero, 1f, boundLayers);
            maxAttempts--;
        } while(hitInfo.collider != null && maxAttempts > 0);

        nextPos = customRightBound.transform.position;
        maxAttempts = MAX_CHECK_ATTEMPTS;
        do {
            customRightBound.transform.position = nextPos;
            nextPos = customRightBound.transform.position + Vector3.right * GROUND_CHECK_STEP_SIZE;
            hitInfo = Physics2D.Raycast(nextPos, Vector2.zero, 1f, boundLayers);
            maxAttempts--;
        } while (hitInfo.collider != null && maxAttempts > 0);
    }

    protected override void OnUpdate() {
        base.OnUpdate();
        if (Time.time > nextStateCheck) {
            UpdateState();
        }
    }

    void FixedUpdate() {
        if (isGrounded) {
            enemyRb.velocity = transform.right * speed;
        }
    }

    void UpdateState() {
        nextStateCheck = Time.time + stateCheckDelay;
        isGrounded = LinecastHit(groundCheck.position);

        if (isGrounded && IsAtBounds()) {
            isFlipped = !isFlipped;
            transform.rotation = Quaternion.Euler(0, isFlipped ? 180 : 0, 0); //jank
        }
    }

    bool IsAtBounds() {
        if (LinecastHit(frontCheck.position)) {
            return true;
        }

        if (!isFlipped) {
            return transform.position.x >= GetMaxX();
        } else {
            return transform.position.x <= GetMinX();
        }
    }

    float GetMinX() {
        return customLeftBound ? customLeftBound.position.x : float.MinValue;
    }

    float GetMaxX() {
        return customRightBound ? customRightBound.position.x : float.MaxValue;
    }

    bool LinecastHit(Vector3 target) {
        RaycastHit2D[] hits = Physics2D.LinecastAll(transform.position, target, boundLayers);
        foreach (RaycastHit2D hit in hits) {
            if (hit.rigidbody != enemyRb) {
                return true;
            }
        }

        return false;
    }
}
