﻿using System.Collections;
using UnityEngine;
using DG.Tweening;
using Catsland.Scripts.Bullets;
using Catsland.Scripts.Misc;

namespace Catsland.Scripts.Controller {

  [RequireComponent(typeof(Rigidbody2D)), RequireComponent(typeof(Animator)), RequireComponent(typeof(SlimeInput))]
  public class SlimeController : MonoBehaviour {

    public interface SlimeInput {
      float getHorizontal();
    }

    public Vector2 jumpForce = new Vector2(1.0f, 1.0f);
    public float jumpInternalInS = 1.0f;

    public TriggerBasedSensor groundSensor;
    public float frezeTimeInS = .2f;

    public GameObject dustPrefab;
    public Transform dustPosition;

    public int maxHp = 3;
    private int currentHp;

    private float wantJumpHorizon = 0.0f;
    private SlimeInput input;
    private Animator animator;
    private Rigidbody2D rb2d;
    private DiamondGenerator diamondGenerator;

    private AnimatorStateInfo currentState;
    private float lastJumpTime = -10000.0f;

    private static readonly string STATUS_IDEAL = "Ideal";

    private static readonly string IS_ON_GROUND = "IsOnGround";
    private static readonly string VSpeed = "VSpeed";
    private static readonly string JUMP = "Jump";

    private bool isLastOnGround = false;

    private void Awake() {
      input = GetComponent<SlimeInput>();
      animator = GetComponent<Animator>();
      rb2d = GetComponent<Rigidbody2D>();
      diamondGenerator = GetComponent<DiamondGenerator>();
      currentHp = maxHp;
    }

    // Update is called once per frame
    void Update() {
      currentState = animator.GetCurrentAnimatorStateInfo(0);

      if (CanMove()) {
        float wantHorizontal = input.getHorizontal();
        if (Mathf.Abs(wantHorizontal) > 0.1f) {
          // jump
          Jump(wantHorizontal);
        }
      }

      // Dust effect
      bool isOnGround = groundSensor.isStay();
      if (isOnGround && !isLastOnGround) {
        if (dustPrefab != null) {
          GameObject dust = Instantiate(dustPrefab);
          dust.transform.position = new Vector3(
            dustPosition.position.x, dustPosition.position.y, dust.transform.position.z);
          dust.GetComponent<ParticleSystem>()?.Play();
          Destroy(dust, 5f);
        }
      }
      isLastOnGround = isOnGround;

      animator.SetBool(IS_ON_GROUND, isOnGround);
      animator.SetFloat(VSpeed, rb2d.velocity.y);
    }

    public bool CanMove() {
      return currentState.IsName(STATUS_IDEAL) && (Time.time - lastJumpTime > jumpInternalInS); 
    }

    private void Jump(float horizontalSpeed) {
      wantJumpHorizon = horizontalSpeed;
      animator.SetTrigger(JUMP);
    }

    public void DoJump() {
      rb2d.AddForce(new Vector2(Mathf.Sign(wantJumpHorizon) * jumpForce.x, jumpForce.y));
      animator.ResetTrigger(JUMP);
      lastJumpTime = Time.time;
    }

    public void damage(DamageInfo damageInfo) {
      currentHp -= damageInfo.damage;
      if (currentHp < 0) {
        enterDie();
        return;
      }
      StartCoroutine(freezeThen(frezeTimeInS, damageInfo));
    }

    private void enterDie() {
      if (diamondGenerator != null) {
        diamondGenerator.Generate(2, 1);
      }
      Destroy(gameObject);
    }

    private IEnumerator freezeThen(float time, DamageInfo damageInfo) {

      rb2d.velocity = Vector2.zero;
      rb2d.bodyType = RigidbodyType2D.Kinematic;
      animator.speed = 0f;

      transform.DOShakePosition(time, .15f, 30, 120);

      yield return new WaitForSeconds(time);

      animator.speed = 1f;
      rb2d.bodyType = RigidbodyType2D.Dynamic;
      Utils.ApplyRepel(damageInfo, rb2d);
    }
  }
}
