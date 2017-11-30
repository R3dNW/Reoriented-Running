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

    public Transform playerContainerTransform;
    public Transform playerBodyTransform;
    public Rigidbody playerRigidbody;
    public Transform cameraTransform;

    //private Quaternion playerBaseRotation;
    //private Quaternion cameraBaseRotation;

    private float speed = 25;

    private Vector2 rotationAngle;

    private Vector2 sensitivity = Vector2.one * 5;

    private List<TimeRotation> rollingRotations;

    private float minRotY = -80;
    private float maxRotY = 80;

    public float rollingAverageTime = 0.02f;

    private float gripStrength = 10.0f;

    public LayerMask metalSurfaceMask;

    private Collider attachedToColl;
    private float attachedRotationT;
    private Quaternion attachedStartRot;
    private Vector3 downDirection;

    public void Start()
    {
        this.rollingRotations = new List<TimeRotation>();

        //this.playerBaseRotation = this.playerTransform.rotation;
        //this.cameraBaseRotation = this.cameraTransform.rotation;

        Cursor.visible = false;

        this.playerRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
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

        this.playerBodyTransform.localRotation = xQuaternion;
        this.cameraTransform.localRotation = yQuaternion;
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

        this.playerRigidbody.AddForce(this.playerBodyTransform.rotation * movement);
    }

    public void GroundGrip()
    {
        this.playerRigidbody.angularVelocity = Vector3.zero;

        RaycastHit hitInfo;

        if (Physics.Raycast(this.playerContainerTransform.position, -this.playerContainerTransform.up, out hitInfo, 4, this.metalSurfaceMask))
        {
            AttractToCollider(hitInfo.collider, 2.25f);
        }
        else
        {
            Collider[] colliders = Physics.OverlapSphere(this.playerContainerTransform.position, 50, this.metalSurfaceMask);
            Collider bestCollider = null;
            float bestSqrDist = Mathf.Infinity;

            for (int i = 0; i < colliders.Length; i++)
            {
                float sqrDist = (colliders[i].ClosestPointOnBounds(this.playerContainerTransform.position) - this.playerContainerTransform.position).sqrMagnitude;

                if (sqrDist < bestSqrDist)
                {
                    bestCollider = colliders[i];
                    bestSqrDist = sqrDist;
                }
            }

            AttractToCollider(bestCollider, (1f/(bestSqrDist+1)) * 2.25f);
        }
    }

    public void AttractToCollider(Collider collider, float rotationRate)
    {
        if (collider != this.attachedToColl)
        {
            this.attachedToColl = collider;
            this.attachedRotationT = 0;
            this.attachedStartRot = this.playerContainerTransform.rotation;
        }

        this.attachedRotationT = Mathf.Clamp01(this.attachedRotationT + (rotationRate * Time.deltaTime));

        Vector3 closestPoint = collider.ClosestPoint(this.playerContainerTransform.position);
        Debug.DrawRay(closestPoint, Vector3.up * 0.1f, Color.red, 0.02f);

        this.downDirection = (closestPoint - this.playerContainerTransform.position).normalized;
        Debug.DrawRay(this.playerContainerTransform.position, this.downDirection * 5, Color.blue, 0.02f);

        this.playerContainerTransform.rotation = Quaternion.Slerp(
            this.attachedStartRot,
            Quaternion.LookRotation(Vector3.Cross(this.playerContainerTransform.right, -this.downDirection), -this.downDirection),
            this.attachedRotationT);
    }

    public void Update()
    {
        this.MouseRotation();

        this.GroundGrip();
    }

    public void FixedUpdate()
    {
        this.KeyboardMovement();
        
        this.playerRigidbody.AddForce(this.downDirection * this.gripStrength);
    }
}
