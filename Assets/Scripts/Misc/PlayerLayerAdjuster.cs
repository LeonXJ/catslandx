﻿using UnityEngine;

namespace Catsland.Scripts.Misc {
  [RequireComponent(typeof(Collider2D))]
  public class PlayerLayerAdjuster: MonoBehaviour {

    public GameObject playerGameObject;
    public int targetLayer;

    public void OnTriggerStay2D(Collider2D collision) {
      if(collision.gameObject == playerGameObject) {
        SpriteRenderer render = playerGameObject.GetComponent<SpriteRenderer>();
        if(render != null) {
          render.sortingOrder = targetLayer;
        }
        // other SpriteRenderer
        SpriteRenderer[] renderers = playerGameObject.GetComponentsInChildren<SpriteRenderer>();
        foreach(SpriteRenderer renderer in renderers) {
          if(renderer == render) {
            continue;
          }
          renderer.sortingOrder = targetLayer + 1;
        }
        LineRenderer lineRenderer = playerGameObject.GetComponentInChildren<LineRenderer>();
        if(lineRenderer != null) {
          lineRenderer.sortingOrder = targetLayer;
        }
      }
    }
  }
}
