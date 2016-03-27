﻿using UnityEngine;
using System.Collections;
using System;

namespace Catslandx {
  public class CharacterVulnerable :MonoBehaviour, IVulnerable {

    public int life = 1;
    public bool isCanGetHurt = true;
    public bool isCanGetRepel = true;

    private Rigidbody2D rigidbody2D;
    private CharacterController2D characterController;

    public bool canGetHurt() {
      return isCanGetHurt;
    }

    public int getHurt(int hurtPoint, Vector2 repelForce) {
      if (isCanGetHurt) {
        life -= hurtPoint;
        // get repel
        if (isCanGetRepel && rigidbody2D != null) {
          characterController.getHurt(hurtPoint);
          //rigidbody2D.AddForce(repelForce);
          rigidbody2D.velocity = repelForce;
        }
      }
      return 0;
    }

    // Use this for initialization
    void Start() {
      rigidbody2D = GetComponent<Rigidbody2D>();
      characterController = GetComponent<CharacterController2D>();
    }

    // Update is called once per frame
    void Update() {

    }
  }
}

