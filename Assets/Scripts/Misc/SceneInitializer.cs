﻿using Catsland.Scripts.Sound;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Catsland.Scripts.Misc {
  public class SceneInitializer : MonoBehaviour {

    [System.Serializable]
    public class PortalInfo {
      public string portalName;
      public Transform portalPosition;
    }

    public List<PortalInfo> portals;

    private Transform checkpoint;

    public void initializeScene(string portalName) {
      bool foundPortal = false;
      // portal
      foreach (PortalInfo portal in portals) {
        if (portal.portalName == portalName) {
          setPlayerToPosition(portal.portalPosition.position);
          foundPortal = true;
          break;
        }
      }
      // save point
      if (!foundPortal) {
        Savepoint[] savepoints = FindObjectsOfType<Savepoint>();
        foreach (Savepoint savepoint in savepoints) {
          if (savepoint.portalName == portalName) {
            setPlayerToPosition(savepoint.transform.position);
            foundPortal = true;
            break;
          }
        }
      }
      Debug.Assert(foundPortal, "Unable to find portal: " + portalName);

      // play background music if set
      GetComponent<MusicPlayer>()?.Play();
    }

    private void setPlayerToPosition(Vector3 position) {
      Transform playerTransform = GameObject.FindGameObjectWithTag(Common.Tags.PLAYER).transform;
      playerTransform.position = Common.Utils.overrideXy(playerTransform.position, position);
    }

    public void registerCheckpoint(Transform checkpoint) {
      Debug.Log("Register checkpoint: " + checkpoint.gameObject.name);
      this.checkpoint = checkpoint;
    }

    public void loadCheckpoint() {
      if (checkpoint == null) {
        Debug.LogWarning("No checkpoint has been registered.");
      }
      GameObject player = GameObject.FindGameObjectWithTag(Common.Tags.PLAYER);

      player.transform.position = checkpoint.position;
      Rigidbody2D playerRb2d = player.GetComponent<Rigidbody2D>();
      playerRb2d.velocity = Vector2.zero;
    }
  }
}
