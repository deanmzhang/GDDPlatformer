using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollow : MonoBehaviour {
    public Transform target;
    public float lerpSpeed;
    public float minX;
    public float maxX;

    Vector3 offset;
    Vector3 targetPos;
    //Vector3 vel;

    bool done;

    void Start() {
        offset = transform.position - target.position;
    }

    void LateUpdate() {
        if (done) {
            return;
        }

        targetPos = target.position + offset;
        float xPos = Mathf.Clamp(Mathf.Lerp(transform.position.x, targetPos.x, lerpSpeed * Time.deltaTime), minX, maxX);
        transform.position = new Vector3(xPos, transform.position.y, transform.position.z);

        if (target.position.x > maxX + 15f) {//so jank
            done = true;
            SceneFader.instance.FadeToNext(0.3f, 0.5f);
        }

        //JANK AF ^^
        //float dist = Vector3.Distance(targetPos, transform.position);
        //transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref vel, dist / moveSpeed);
    }
}
