using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageableObject : WeightedObject {
    public float maxHealth;
    float curHealth;

    public bool canBeCrushed;
    public float maxPressure;

    public GameObject explosion;
    public float explosionScale;
    public Color mainColor;

    public void TakeDamage(float damage) {
        curHealth -= damage;
        if (curHealth <= 0) {
            Die();
        }
    }

    protected virtual void Die() {
        if (explosion != null) {
            GameObject newSmoke = Instantiate(explosion, transform.position, Quaternion.identity);
            newSmoke.transform.localScale = Vector3.one * explosionScale;
            var mainSystem = newSmoke.GetComponent<ParticleSystem>().main;
            mainSystem.startColor = mainColor;
        }

        Destroy(gameObject);
    }

    protected override void OnUpdate() {
        base.OnUpdate();

        if (transform.position.y < -7f) {
            Die(); //bad
        }
    }

    protected override void OnWeightUpdate(float newPressure) {
        if (newPressure >= maxPressure && canBeCrushed) {
            Die();
        }
    }
}
