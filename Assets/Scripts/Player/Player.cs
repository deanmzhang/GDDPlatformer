using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : DamageableObject {
    #region PlayerAttributes
    [Header("Player")]
    public float movespeed;
    public float maxspeed;
    public float jumpforce;
    Rigidbody2D mechanic;

    public LayerMask floorLayers;
    public Transform groundCheck;

    public SpriteRenderer[] flippableSprites;
    public HandController offHand;
    public Transform wheel;
    public float wheelRotateSpeed;

    [Header("Death")]
    public Collider2D[] pieces;
    public float deathHorizVelocity;
    public float deathTorque;

    public bool isFlipped { get; private set; }
    public Vector3 mousePos { get; private set; }

    Animator anim;
    #endregion

    const float JUMP_CHECK_DIST = 0.25f;

    #region PlayerFunctionality
    void Awake() {
        mechanic = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        foreach (Collider2D piece in pieces) {
            piece.enabled = false;
        }
    }

    void FixedUpdate() {
        // Main Movement Function
        float horizontalMovement = Input.GetAxisRaw("Horizontal");

        mechanic.velocity = new Vector2(horizontalMovement * movespeed, mechanic.velocity.y);
        anim.SetFloat("Speed", horizontalMovement);
    }

    void Flip(bool newFlippedState) {
        isFlipped = newFlippedState;
        anim.SetBool("Flipped", newFlippedState);
        foreach (SpriteRenderer srend in flippableSprites) {
            srend.flipX = newFlippedState;
        }

        //Vector3 pos = attackTarget.transform.localPosition;
        //attackTarget.localPosition = new Vector3(sign * Mathf.Abs(pos.x), pos.y, 0);
        //attackTarget.rotation = Quaternion.Euler(0, 0, sign * 225); //can't be bothered to fix it with euler angles
    }

    protected override void OnUpdate() {
        if (Input.GetKeyDown(KeyCode.Space) && AbleToJump()) {
            mechanic.AddForce(new Vector2(0, jumpforce), ForceMode2D.Impulse);
        }

        float horizontalMovement = Input.GetAxisRaw("Horizontal");
        wheel.Rotate(Vector3.forward * wheelRotateSpeed * -horizontalMovement * Time.deltaTime);

        base.OnUpdate();
    }

    void LateUpdate() {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos = new Vector3(mousePos.x, mousePos.y, 0);
        float xDiff = mousePos.x - transform.position.x;

        if (xDiff < 0) {
            if (!isFlipped) {
                Flip(true);
            }
        } else if (xDiff > 0) {
            if (isFlipped) {
                Flip(false);
            }
        }
    }
    #endregion

    #region ConditionFunctions
    bool AbleToJump() {
        return Physics2D.Raycast(groundCheck.position, Vector2.down, JUMP_CHECK_DIST, floorLayers);
    }
    #endregion

    #region Health
    protected override void Die() {
        Destroy(anim);

        foreach (Collider2D piece in pieces) {
            piece.enabled = true;
            Rigidbody2D newRb = piece.gameObject.AddComponent<Rigidbody2D>();
            piece.transform.parent = null;
            newRb.velocity = mechanic.velocity;
            newRb.AddForce(new Vector2(Random.Range(-deathHorizVelocity, deathHorizVelocity), 0));
            newRb.angularVelocity = Random.Range(-deathTorque, deathTorque);
        }
        GetComponent<PlayerAttack>().OnDeath();
        GetComponent<GrabberScript>().OnDeath();

        gameObject.SetActive(false);
        SceneFader.instance.FadeCurrent(0.3f, 2f);
        //base.Die();
    }

    void OnCollisionEnter2D(Collision2D other) {
        if (other.collider.CompareTag("Enemy")) {
            Die();
        }
    }
    #endregion
}
