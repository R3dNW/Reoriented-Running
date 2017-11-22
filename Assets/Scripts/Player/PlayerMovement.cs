using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private struct TimeRotation
    {
        public float time;
        public Vector2 rotation;
    }

    public Transform playerTransform;
    public Rigidbody playerRigidbody;
    public Transform cameraTransform;

    private Quaternion playerBaseRotation;
    private Quaternion cameraBaseRotation;

    private float speed = 25;

    private Vector2 rotationAngle;

    private Vector2 sensitivity = Vector2.one * 5;

    private List<TimeRotation> rollingRotations;

    private float minRotY = -80;
    private float maxRotY = 80;

    public float rollingAverageTime = 0.02f;

    private float gripStrength = 10.0f;

    public LayerMask metalSurfaceMask;

    public void Start()
    {
        this.rollingRotations = new List<TimeRotation>();

        this.playerBaseRotation = this.playerTransform.rotation;
        this.cameraBaseRotation = this.cameraTransform.rotation;

        Cursor.visible = false;
    }

    public void MouseRotation()
    {
        this.rotationAngle.x += Input.GetAxis("Mouse X") * this.sensitivity.x;
        this.rotationAngle.y += Input.GetAxis("Mouse Y") * this.sensitivity.y;

        this.rotationAngle.y = Mathf.Clamp(this.rotationAngle.y, this.minRotY, this.maxRotY);

        this.rollingRotations.Add(new TimeRotation() { time = Time.time, rotation = this.rotationAngle });

        while (this.rollingRotations.Count > 2 && this.rollingRotations[0].time < Time.time - this.rollingAverageTime)
        {
            this.rollingRotations.RemoveAt(0);
        }

        Vector2 avgRotation = Vector2.zero;

        foreach (TimeRotation tr in this.rollingRotations)
        {
            avgRotation += tr.rotation;
        }

        avgRotation /= this.rollingRotations.Count;

        Quaternion xQuaternion = Quaternion.AngleAxis(avgRotation.x, Vector3.up);
        Quaternion yQuaternion = Quaternion.AngleAxis(avgRotation.y, Vector3.left);

        this.playerTransform.rotation = this.playerBaseRotation * xQuaternion;
        this.cameraTransform.localRotation = this.cameraBaseRotation * yQuaternion;
    }

    public void KeyboardMovement()
    {
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * this.speed;


        /*Vector3 localVelocity = this.playerTransform.rotation * this.playerRigidBody.velocity;

        if (movement.x > 0 && localVelocity.x < 0 || movement.x < 0 && localVelocity.x > 0)
        {
            movement.x *= Mathf.Abs(localVelocity.x);
        }
        else if (movement.x > 0 && localVelocity.x > 0 || movement.x < 0 && localVelocity.x < 0)
        {
            movement.x /= localVelocity.x;
        }

        if (movement.y > 0 && localVelocity.y < 0 || movement.y < 0 && localVelocity.y > 0)
        {
            movement.y *= Mathf.Abs(localVelocity.y);
        }
        else if (movement.y > 0 && localVelocity.y > 0 || movement.y < 0 && localVelocity.y < 0)
        {
            movement.y /= localVelocity.y;
        }*/

        this.playerRigidbody.AddRelativeForce(movement);
    }

    public void GroundGrip()
    {
        this.playerRigidbody.angularVelocity = Vector3.zero;

        RaycastHit hitInfo;

        if (Physics.Raycast(this.playerTransform.position, -this.playerTransform.up, out hitInfo, 15, this.metalSurfaceMask))
        {
            Vector3 closestPoint = hitInfo.collider.ClosestPoint(this.playerTransform.position);
            Debug.DrawRay(closestPoint, Vector3.up * 0.1f, Color.red, 0.02f);

            Vector3 downDir = (closestPoint - this.transform.position).normalized;
            Debug.DrawRay(this.playerTransform.position, downDir * 5, Color.blue, 0.02f);

            this.playerRigidbody.AddForce(downDir * this.gripStrength);
            this.playerBaseRotation = Quaternion.Slerp(this.playerBaseRotation, Quaternion.LookRotation(downDir, Vector3.up) * Quaternion.Euler(90, -90, 90), 0.1f);
        }
    }

    public void Update()
    {
        this.MouseRotation();
    }

    public void FixedUpdate()
    {
        this.KeyboardMovement();

        this.GroundGrip();
    }
}
