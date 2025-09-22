using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;           // Player/Main
    public Vector2 offset = new Vector2(0f, 1.5f);
    public float smoothTime = 0.15f;   // mượt chuyển động
    public Vector2 minBounds = new Vector2(-9999, -9999);
    public Vector2 maxBounds = new Vector2(9999, 9999);

    [Header("Look Ahead")]
    public float lookAheadDist = 1.5f;
    public float lookAheadTime = 0.25f;
    public float lookAheadDamp = 5f;

    [Header("Pixel Art")]
    public bool snapToPixels = true;
    public int pixelsPerUnit = 32;     // đúng với sprite của bạn

    Vector3 _vel;
    Vector3 _lookAhead;
    Vector3 _lastTargetPos;

    void Start()
    {
        if (!target) enabled = false;
        _lastTargetPos = target.position;
        // đảm bảo camera đúng trục Z cho 2D
        var pos = transform.position; pos.z = -10f; transform.position = pos;
        // đảm bảo camera là Orthographic
        if (Camera.main) Camera.main.orthographic = true;
    }

    void LateUpdate()
    {
        if (!target) return;

        // tính look-ahead theo vận tốc mục tiêu
        Vector3 targetMove = (target.position - _lastTargetPos) / Mathf.Max(Time.deltaTime, 0.0001f);
        float dirX = Mathf.Sign(targetMove.x);
        Vector3 desiredLook = Vector3.right * dirX * lookAheadDist;
        _lookAhead = Vector3.Lerp(_lookAhead, desiredLook, lookAheadDamp * Time.deltaTime);
        _lastTargetPos = target.position;

        // điểm đích
        Vector3 desired = (Vector3)offset + target.position + _lookAhead;
        desired.z = -10f;

        // Clamp theo biên
        desired.x = Mathf.Clamp(desired.x, minBounds.x, maxBounds.x);
        desired.y = Mathf.Clamp(desired.y, minBounds.y, maxBounds.y);

        // Smooth
        Vector3 pos = Vector3.SmoothDamp(transform.position, desired, ref _vel, smoothTime);

        // Snap về lưới pixel (tránh “rung” pixel-art)
        if (snapToPixels && pixelsPerUnit > 0)
        {
            float unit = 1f / pixelsPerUnit;
            pos.x = Mathf.Round(pos.x / unit) * unit;
            pos.y = Mathf.Round(pos.y / unit) * unit;
        }

        transform.position = pos;
    }
}
