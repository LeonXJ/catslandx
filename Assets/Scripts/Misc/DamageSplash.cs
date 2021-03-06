﻿using UnityEngine;
using Catsland.Scripts.Bullets;
using Catsland.Scripts.Common;

namespace Catsland.Scripts.Misc {
  public class DamageSplash: MonoBehaviour {

    public GameObject splashPrefab;
    public bool enable = true;
    public IDamageInterceptor damageInterceptor;

    void Awake() {
      damageInterceptor = GetComponent<IDamageInterceptor>();
    }

    public void damage(DamageInfo damageInfo) {
      if (!enable) {
        return;
      }
      if(damageInterceptor != null && !damageInterceptor.shouldSplashOnDamage(damageInfo)) {
        return;
      }
      if (damageInfo.damage <= 0) {
        return;
      }

      GameObject splash = Instantiate(splashPrefab);
      splash.transform.position = new Vector3(damageInfo.damagePosition.x, damageInfo.damagePosition.y, AxisZ.SPLASH);
      splash.transform.localScale =
        new Vector3(Mathf.Sign(damageInfo.repelDirection.x), 1.0f, 1.0f);
    }
  }
}
