using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logic.Constant;
using static Logic.Constant.Constant;
using Communication.Proto;
using Google.Protobuf;
using UnityEngine.UI;
using System.Net;
using static THUnity2D.Tools;
using Debug = UnityEngine.Debug;
using static Tools;

public class PackScript : MonoBehaviour
{
    private Vector3 omega;
    private Vector3 amplitude;
    private float minSpeed;
    private float maxSpeed;
    private float minScale;
    private float maxScale;
    private float speed;
    private float scale;
    private float time;
    private bool isActive;
    private Vector3 originPosition;

    // Start is called before the first frame update
    void Start()
    {
        minSpeed = 0.2f;
        maxSpeed = 0.5f;
        minScale = 0.1f;
        maxScale = 0.3f;
        originPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (isActive)
        {
            time += Time.deltaTime * speed;
            this.transform.localPosition = originPosition + DotSin(amplitude, omega, time, scale);
            this.transform.rotation = Quaternion.Euler(DotSin(amplitude, omega, time, 10));
        }
    }

    Vector3 DotSin(Vector3 v1, Vector3 v2, float t, float k)
    {
        return new Vector3(v1.x * Mathf.Sin(v2.x * t), v1.y * Mathf.Sin(v2.y * t), v1.z * Mathf.Sin(v2.z * t)) * k;
    }

    public void SetActive(bool isSetActive)
    {
        if (isSetActive)
        {
            this.isActive = true;
            omega = Random.onUnitSphere;
            amplitude = Random.onUnitSphere;
            speed = Random.Range(minSpeed, maxSpeed);
            scale = Random.Range(minScale, maxScale);
            time = 0;
        }
        else
        {
            this.isActive = false;
        }
    }
}