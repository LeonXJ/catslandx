﻿using UnityEngine;

namespace Catsland.Scripts.CharacterController {
  public interface ISensor {
    bool isStay();
    GameObject getTriggerGO();
  }
}