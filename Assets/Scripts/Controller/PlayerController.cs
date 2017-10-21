﻿using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

using Catsland.Scripts.Bullets;
using Catsland.Scripts.Common;

namespace Catsland.Scripts.Controller {

  [RequireComponent(typeof(IInput))]
  [RequireComponent(typeof(Animator))]
  public class PlayerController :MonoBehaviour {

    // Locomoation
    public float maxHorizontalSpeed = 1.0f;
    public float acceleration = 1.0f;
    public float jumpForce = 5.0f;

    // Attack
    public float arrowSpeed = 5.0f;
    public float arrowLifetime = 3.0f;
    private bool isDrawing = false;
    private float shootingCd = 0.5f;
    private bool isShooting = false;

    // Health
    public int maxHealth = 3;
    public int currentHealth;
    public float dizzyTime = 1.0f;
    private bool isDizzy = false;

    // References
    public GameObject groundSensorGO;
    public GameObject arrowPrefab;
    public Transform shootPoint;
    private ISensor groundSensor;
    private IInput input;
    private Rigidbody2D rb2d;
    private Animator animator;

    // Animation
    private const string H_SPEED = "HSpeed";
    private const string V_SPEED = "VSpeed";
    private const string GROUNDED = "Grounded";
    private const string DRAWING = "Drawing";
    private const string DIZZY = "Dizzy";

    public void Awake() {
      input = GetComponent<IInput>();
      rb2d = GetComponent<Rigidbody2D>();
      groundSensor = groundSensorGO.GetComponent<ISensor>();
      animator = GetComponent<Animator>();
    }

    public void Start() {
      currentHealth = maxHealth;
    }

    public void Update() {
      float desiredSpeed = input.getHorizontal();
      float currentVerticleVolocity = rb2d.velocity.y;
      bool verticleStable = Mathf.Abs(currentVerticleVolocity) < 0.1f;

      // Draw and shoot 
      bool currentIsDrawing =
        groundSensor.isStay()
        && verticleStable
        && input.attack()
        && !isShooting
        && !isDizzy;
      // Shoot if string is released
      if(isDrawing && !currentIsDrawing && !isDizzy) {
        StartCoroutine(shoot());
      }
      isDrawing = currentIsDrawing;

      // Movement
      if(groundSensor.isStay() && verticleStable && !isDizzy) {
        if(input.jump()) {
          // jump down
          if(input.getVertical() < -0.1f) {
            if(groundSensor.getTriggerGO().CompareTag(Tags.ONESIDE)) {
              StartCoroutine(jumpDown(groundSensor.getTriggerGO()));
            }
          } else {
            // jump up
            rb2d.AddForce(new Vector2(0.0f, jumpForce));
          }
        }
      }
      gameObject.transform.parent =
        groundSensor.isStay() ? groundSensor.getTriggerGO().transform : null;
      if(!isDizzy) {
        if(!isDrawing && Mathf.Abs(desiredSpeed) > Mathf.Epsilon) {
          rb2d.AddForce(new Vector2(acceleration * desiredSpeed, 0.0f));
          rb2d.velocity = new Vector2(
            Mathf.Clamp(rb2d.velocity.x, -maxHorizontalSpeed, maxHorizontalSpeed),
            rb2d.velocity.y);
        } else {
          rb2d.velocity = new Vector2(0.0f, rb2d.velocity.y);
        }
      } else {
        // dizzy, damp on horizontal speed
        rb2d.velocity =
          new Vector2(rb2d.velocity.x * 0.8f, rb2d.velocity.y);
      }

      // Update facing
      if(Mathf.Abs(desiredSpeed) > Mathf.Epsilon && !isDizzy) {
        float parentLossyScale = gameObject.transform.parent != null
            ? gameObject.transform.parent.lossyScale.x : 1.0f;
        if(desiredSpeed * parentLossyScale > 0.0f) {
          transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        if(desiredSpeed * parentLossyScale < 0.0f) {
          transform.localScale = new Vector3(
            -Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
      }

      // Update animation
      animator.SetBool(GROUNDED, groundSensor.isStay());
      animator.SetFloat(H_SPEED, Mathf.Abs(rb2d.velocity.x));
      animator.SetFloat(V_SPEED, rb2d.velocity.y);
      animator.SetBool(DRAWING, isDrawing);
      animator.SetBool(DIZZY, isDizzy);
    }

    public void damage(DamageInfo damageInfo) {
      rb2d.AddForce(damageInfo.repelDirection * damageInfo.repelIntense);
      currentHealth -= damageInfo.damage;
      if(currentHealth <= 0) {
        // Die
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
      } else {
        // Dizzy
        StartCoroutine(dizzy());
      }
    }

    private IEnumerator shoot() {
      Debug.Assert(arrowPrefab != null, "Arrow prefab is not set");
      Debug.Assert(shootPoint != null, "Shoot point is not set");

      GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, shootPoint.rotation);
      ArrowCarrier arrowCarrier = arrow.GetComponent<ArrowCarrier>();
      Debug.Log("Lossy Scale X: " + transform.lossyScale.x);
      StartCoroutine(arrowCarrier.fire(
        new Vector2(transform.lossyScale.x > 0.0f ? arrowSpeed : -arrowSpeed, 0.0f),
        arrowLifetime,
        gameObject.tag));
      isShooting = true;
      yield return new WaitForSeconds(shootingCd);
      isShooting = false;
    }

    private IEnumerator jumpDown(GameObject onesideGO) {
      Collider2D collider = onesideGO.GetComponent<Collider2D>();
      collider.enabled = false;
      yield return new WaitForSeconds(1.0f);
      collider.enabled = true;
    }

    private IEnumerator dizzy() {
      isDizzy = true;
      yield return new WaitForSeconds(dizzyTime);
      isDizzy = false;
    }
  }
}
