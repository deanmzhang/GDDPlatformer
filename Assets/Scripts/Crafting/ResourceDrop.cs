using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceDrop : MonoBehaviour {
    public float bobSpeed;
    public float bobHeight;
    public float ungrabbableTime;
    public Collider2D physicalCollider;

    public float verticalForce;
    public float maxHorizontalForce;

    SpriteRenderer srend;

    float lifetime;
    int id;
    bool isGrabbed;
    bool isFloating;

    const float MAX_RANDOM_OFFSET = 0.5f;
    float floatSpeed;

    Transform player;

    public void Init(int resourceId) {
        id = resourceId;

        srend = GetComponentInChildren<SpriteRenderer>();
        srend.sprite = CraftingManager.instance.GetResourceSprite(id);

        lifetime = Random.Range(0, MAX_RANDOM_OFFSET);

        GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(-maxHorizontalForce, maxHorizontalForce), verticalForce), ForceMode2D.Impulse);
    }

    void Update() {
        UpdateBob();

        if (!isFloating && transform.position.y < -7f) {
            Float();
        }

        if (isFloating) {
            if (player == null) {
                Destroy(gameObject);
                return;
            }
            transform.position = Vector3.MoveTowards(transform.position, player.position, floatSpeed * Time.deltaTime);
        }
    }

    void Float() {
        isFloating = true;
        physicalCollider.enabled = false;
        Player pScript = FindObjectOfType<Player>();

        if (pScript == null) {
            Destroy(gameObject);
            return;
        }

        player = pScript.transform;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true; //so bad
        rb.velocity = Vector2.zero;

        floatSpeed = Random.Range(10f, 20f);
    }

    void UpdateBob() {
        lifetime += Time.deltaTime;
        srend.transform.localPosition = new Vector3(0, bobHeight * Mathf.Sin(lifetime * bobSpeed), 0);
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (isGrabbed) {
            return;
        }

        if (other.CompareTag("Player")) {
            StartCoroutine(GrabSequence());
        }
    }

    IEnumerator GrabSequence() {
        isGrabbed = true;
        float wait = ungrabbableTime - lifetime;

        if (wait > 0) {
            yield return new WaitForSeconds(wait);
        }

        CraftingManager.instance.PickupResource(id);
        Destroy(gameObject);
    }
}
