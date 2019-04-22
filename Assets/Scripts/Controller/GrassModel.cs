﻿using System.Collections.Generic;
using UnityEngine;
using Catsland.Scripts.Common;

namespace Catsland.Scripts.Controller {
  [RequireComponent(typeof(MeshRenderer))]
  [RequireComponent(typeof(MeshFilter))]
  [ExecuteInEditMode]
  public class GrassModel: MonoBehaviour {

    // The center of swing.
    public enum SwingCenter {
      BOTTOM,
      TOP,
    }

    // Whether and how the grass is pressed.
    public enum PressStatus {
      NONE = 0,
      LEFT = 1,
      RIGHT = 2,
    }

    // Mesh and sprite attributes.
    public Sprite sprite;
    public float width = 1.0f;
    public float height = 0.3f;

    private Vector3[] vertices;

    // Movement attributes.
    public SwingCenter swingCenter = SwingCenter.BOTTOM;

    // Value range of centerDegree.
    public float maxCenterDegree = 80.0f;

    // Center restoring speed degree per second.
    public float centerRestoringDegreeSpeed = 45.0f;

    // Range of frequency, ~swingCenter.
    public Vector2 frequencyRange = new Vector2(100.0f, 1000.0f);

    // Range of swing amp in degree, ~1/swingCenter.
    public Vector2 ampRangeDegree = new Vector2(5.0f, 20.0f);

    // Grass swings around center. 0.0 means middle. Value is within CenterRange.
    // for debugging
    public float centerDegree = 0.0f;

    // frequency = [minFrequency, maxFrequency] ~ centerDegree
    private float frequency = 10.0f;

    // Following attributes are used when pressStatus is NONE.
    // Degree of swing is calculate as:
    // 1. phase = frequency * t
    // 2. target_degree = centerDegree + Sin(phase) * amp
    // 3. degree = lerp(target_degree, degree)

    // phase is in range of [.0f, 360.0f]
    private float phase = 0.0f;
    private float degree = 0.0f;

    // The winds which affect this grass.
    // for debugging
    public HashSet<IWind> winds = new HashSet<IWind>();

    private PressStatus pressStatus = PressStatus.NONE;


    // Movement updating attributes.

    // Whether to assign a random init phase.
    public bool randomInitPhase = true;

    // Stop updating grass swinging if the camera is further than this distance.
    public float stopUpdateDistance = 10.0f;

    // Whether the mesh has been created.
    private bool hasInitialized = false;

    // How fast is the degree changes towards target degree.
    public float maxSwingSpeed = 45.0f;

    // Max degree when pressStatus is LEFT/RIGHT.
    public float maxPressDegree = 80.0f;

    private void Start() {
      if(randomInitPhase) {
        phase = Random.Range(0.0f, 360.0f);
      }
    }

    void Update() {
      if(!hasInitialized) {
        initializeMesh();
      }
      if(Application.isPlaying && isInMainCamera()) {
        updateWinds();
        updateDegree();
        updateVertices();
        updateTexture();
      }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
      IWind wind = collision.gameObject.GetComponent<IWind>();
      if (wind != null && !winds.Contains(wind)) {
        winds.Add(wind);
        return;
      }
      Rigidbody2D rigidbody = collision.GetComponent<Rigidbody2D>();
      if(rigidbody != null) {
        pressStatus = rigidbody.velocity.x > 0.0f ? PressStatus.RIGHT : PressStatus.LEFT;
      }
    }

    private void OnTriggerExit2D(Collider2D collision) {
      IWind wind = collision.gameObject.GetComponent<IWind>();
      if (wind != null && winds.Contains(wind)) {
        winds.Remove(wind);
        return;
      }
      pressStatus = PressStatus.NONE;
    }

    private void updateTexture() {
      MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
      meshRenderer.material.mainTexture = sprite.texture;
    }

    private void initializeMesh() {
      MeshFilter meshFilter = GetComponent<MeshFilter>();
      Mesh mesh = new Mesh();
      meshFilter.sharedMesh = mesh;

      vertices = new Vector3[4];

      if(swingCenter == SwingCenter.BOTTOM) {
        vertices[0] = new Vector3(-width / 2.0f, height, 0.0f);
        vertices[1] = new Vector3(width / 2.0f, height, 0.0f);
        vertices[2] = new Vector3(-width / 2.0f, 0.0f, 0.0f);
        vertices[3] = new Vector3(width / 2.0f, 0.0f, 0.0f);
      } else if(swingCenter == SwingCenter.TOP) {
        vertices[0] = new Vector3(-width / 2.0f, 0.0f, 0.0f);
        vertices[1] = new Vector3(width / 2.0f, 0.0f, 0.0f);
        vertices[2] = new Vector3(-width / 2.0f, -height, 0.0f);
        vertices[3] = new Vector3(width / 2.0f, -height, 0.0f);
      }

      mesh.vertices = vertices;
      int[] triangles = new int[6];

      triangles[0] = 0;
      triangles[1] = 2;
      triangles[2] = 1;

      triangles[3] = 2;
      triangles[4] = 3;
      triangles[5] = 1;

      mesh.triangles = triangles;

      Vector3[] normals = new Vector3[4];

      normals[0] = -Vector3.forward;
      normals[1] = -Vector3.forward;
      normals[2] = -Vector3.forward;
      normals[3] = -Vector3.forward;

      mesh.normals = normals;
      mesh.uv = sprite.uv;

      hasInitialized = true;
    }

    private void updateVertices() {

      Mesh mesh = GetComponent<MeshFilter>().sharedMesh;

      if(swingCenter == SwingCenter.BOTTOM) {
        Vector3 farendBias = new Vector3(
          height * Mathf.Sin(Mathf.Deg2Rad * degree),
          height * Mathf.Cos(Mathf.Deg2Rad * degree),
          0.0f);
        // Bottom-left -> Bottom-right -> Top-left -> Top-right
        vertices[2] = new Vector3(-width * 0.5f, 0.0f, 0.0f);
        vertices[3] = new Vector3(width * 0.5f, 0.0f, 0.0f);
        vertices[0] = vertices[2] + farendBias;
        vertices[1] = vertices[3] + farendBias;
      } else if(swingCenter == SwingCenter.TOP) {
        Vector3 farendBias = new Vector3(
          height * Mathf.Sin(Mathf.Deg2Rad * degree),
          -height * Mathf.Cos(Mathf.Deg2Rad * degree),
          0.0f);
        vertices[0] = new Vector3(-width * 0.5f, 0.0f, 0.0f);
        vertices[1] = new Vector3(width * 0.5f, 0.0f, 0.0f);
        vertices[2] = vertices[0] + farendBias;
        vertices[3] = vertices[1] + farendBias;
      }

      mesh.vertices = vertices;
      mesh.RecalculateBounds();
    }

    private void updateWinds() {
      // Calcuate total wind force.
      float power = 0.0f;
      winds.RemoveWhere(wind => wind == null);
      foreach (IWind wind in winds) {
        power += wind.GetWindPower();
      }

      if (power * power > 0.01f) {
        float absPower = Mathf.Abs(power);
        float absCenter = Mathf.Min(absPower, maxCenterDegree);
        centerDegree = Mathf.Sign(power) * absCenter;
        frequency = Mathf.Lerp(frequencyRange.x, frequencyRange.y, absCenter / maxCenterDegree);
      } else {
        // No wind, restoring centerDegree.
        centerDegree = Mathf.Lerp(centerDegree, 0.0f, centerRestoringDegreeSpeed * Time.deltaTime);
        frequency = Mathf.Lerp(frequencyRange.x, frequencyRange.y, Mathf.Abs(centerDegree) / maxCenterDegree);
      }
    }

    public void UpdateSize() {
      if(sprite != null) {
        width = sprite.rect.width / sprite.pixelsPerUnit;
        height = sprite.rect.height / sprite.pixelsPerUnit;
      }
    }

    private void updateDegree() {
      float targetDegree = degree;
      if(pressStatus == PressStatus.NONE) {
        phase += Time.deltaTime * frequency;
        phase -= 360.0f * Mathf.Floor(phase / 360.0f);

        float realAmp = Mathf.Lerp(ampRangeDegree.x, ampRangeDegree.y, 1.0f - Mathf.Abs(centerDegree) / maxCenterDegree);
        targetDegree = centerDegree + Mathf.Sin(Mathf.Deg2Rad * phase) * realAmp;
      } else {
        targetDegree = pressStatus == PressStatus.LEFT ? -maxPressDegree : maxPressDegree;
      }
      applyDegreeChange(targetDegree);
    }

    private void applyDegreeChange(float targetDegree) {
      float delta = targetDegree - degree;
      float confinedDelta = Mathf.Clamp(delta, -maxSwingSpeed * Time.deltaTime, maxSwingSpeed * Time.deltaTime);
      degree += confinedDelta;
    }

    public void OnAttributeUpdate() {
      updateVertices();
      updateTexture();
    }

    private bool isInMainCamera() {
      Vector3 cameraPosition = SceneConfig.getSceneConfig().MainCamera.transform.position;
      return Vector2.SqrMagnitude(cameraPosition - transform.position) < stopUpdateDistance * stopUpdateDistance;
    }
  }
}
