﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Catsland.Scripts.Common;

namespace Catsland.Scripts.Controller {
  public class BattleTrap : MonoBehaviour
  {
    public string riseParameterName = "Rise";
    public Animator[] battleWallAnimators;
    public bool enable = true;

    private void OnTriggerEnter2D(Collider2D collision) {
      if (collision.gameObject.tag == Tags.PLAYER && enable) {
        SetRise(true);
      }
    }

    public void SetRise(bool rise) {
      foreach (Animator animator in battleWallAnimators) {
        animator.SetBool(riseParameterName, rise);
      }
    }

    public void SetEnable(bool enable) {
      this.enable = enable;
    }
  }
}
