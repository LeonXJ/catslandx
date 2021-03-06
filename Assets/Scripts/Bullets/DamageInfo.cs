﻿using UnityEngine;

namespace Catsland.Scripts.Bullets {
  public class DamageInfo {

    public readonly int damage;
    public readonly float repelIntense;
    public readonly Vector2 repelDirection;
    public readonly Vector2 damagePosition;
    public readonly bool isShellBreaking;
    public readonly bool isDash;
    public readonly bool isKick;
    public readonly GameObject owner;
    public readonly bool damageDashStatus;
    public readonly OnDamageFeedback onDamageFeedback;
    // Whether to set the plyaer to the last checkpoint when the damage is applied.
    // This is used for river.
    public readonly bool setPositionToLastCheckpoint;

    public Collider2D hitCollider;

    public class DamageFeedback {
      public bool damageSuccess;

      public DamageFeedback(bool damageSuccess) {
        this.damageSuccess = damageSuccess;
      }
    }

    public delegate void OnDamageFeedback(DamageFeedback damageFeedback);

    public bool isKnockback() {
      return damage == 0;
    }

    public readonly bool isSmashAttack = false;

    public DamageInfo(
      int damage, Vector2 damagePosition, Vector2 repelDirection, float repelIntense,
      bool isSmashAttack = false,
      bool isDash = false,
      bool isKick = false,
      GameObject owner = null,
      bool damageDashStatus = true,
      OnDamageFeedback onDamageFeedback = null,
      bool isShellBreaking = false,
      bool setPositionToLastCheckpoint = false) {

      this.damage = damage;
      this.repelDirection = repelDirection;
      this.repelIntense = repelIntense;
      this.damagePosition = damagePosition;
      this.isSmashAttack = isSmashAttack;
      this.isDash = isDash;
      this.isKick = isKick;
      this.owner = owner;
      this.damageDashStatus = damageDashStatus;
      this.onDamageFeedback = onDamageFeedback;
      this.isShellBreaking = isShellBreaking;
      this.setPositionToLastCheckpoint = setPositionToLastCheckpoint;
    }

    public DamageInfo setHitCollider(Collider2D collider) {
      this.hitCollider = collider;
      return this;
    }

    public Vector2 GetRepelVelocity() {
      return repelDirection * repelIntense;
    }
  }
}
