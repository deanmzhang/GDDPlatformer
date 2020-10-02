using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeightedObject : MonoBehaviour {
    public float weight;
    public float radius;
    public bool isGrabbable = true;

    float curPressure;
    List<WeightedObject> nearbyObjects = new List<WeightedObject>();
    float nextCheck;

    //how often we check the pressure
    const float PRESSURE_CHECK_DELAY = 0.1f;
    //how different two pressures have to be to be considered different
    const float PRESSURE_DIFF_BUFFER = 0.05f;

    void Awake() {
        OnAwake();
    }

    protected virtual void OnAwake() { }

    void Start() {
        OnStart();
    }

    protected virtual void OnStart() { }

    //may redo if this proves to be inconsistent
    void OnTriggerEnter2D(Collider2D other) {
        WeightedObject otherWeight = other.GetComponent<WeightedObject>();
        if (otherWeight != null && otherWeight != this) {
            if (nearbyObjects.Contains(otherWeight)) {
                Debug.LogWarning("Double add");
                return;
            }

            nearbyObjects.Add(otherWeight);
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        WeightedObject otherWeight = other.GetComponent<WeightedObject>();
        if (otherWeight && otherWeight != this) {
            if (!nearbyObjects.Contains(otherWeight)) {
                Debug.LogWarning("Object not in list");
                return;
            }

            nearbyObjects.Remove(otherWeight);
        }
    }

    void Update() {
        OnUpdate();
    }

    protected virtual void OnUpdate() {
        if (Time.time > nextCheck) {
            CheckPressure();
        }
    }

    void CheckPressure() {
        //NOTE: each stack is considered in isolation, so adjacent stacks to not compound pressure

        nextCheck = Time.time + PRESSURE_CHECK_DELAY;
        float maxPressure = 0;
        for (int i = 0; i < nearbyObjects.Count; i++) {
            WeightedObject obj = nearbyObjects[i];

            if (obj == null) {
                nearbyObjects.RemoveAt(i);
                i--;
                continue;
            }

            //only check the objects above us
            if (obj.transform.position.y > transform.position.y + radius + obj.radius) {
                maxPressure = Mathf.Max(maxPressure, obj.weight + obj.curPressure);
            }
        }

        //if there's been a change in pressure
        if (Mathf.Abs(maxPressure - curPressure) > PRESSURE_DIFF_BUFFER) {
            curPressure = maxPressure;
            OnWeightUpdate(curPressure);
        }
    }

    protected abstract void OnWeightUpdate(float newPressure);
}
