using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagneticObject : MonoBehaviour
{
    public float gripStrength = 9.81f;
    public LayerMask metalSurfaceMask;

    private new Rigidbody rigidbody;

    [HideInInspector]
    public bool magnetised = true;

    public void Start()
    {
        this.rigidbody = this.GetComponent<Rigidbody>();
    }

    public void FixedUpdate()
    {
        if (this.magnetised)
        {
            Collider[] colliders = Physics.OverlapSphere(this.transform.position, 10, this.metalSurfaceMask);
            Collider bestCollider = null;
            float bestSqrDist = Mathf.Infinity;

            for (int i = 0; i < colliders.Length; i++)
            {
                float sqrDist = (colliders[i].ClosestPoint(this.transform.position) - this.transform.position).sqrMagnitude;

                if (sqrDist < bestSqrDist)
                {
                    bestCollider = colliders[i];
                    bestSqrDist = sqrDist;
                }
            }

            Vector3 closestPoint = bestCollider.ClosestPoint(this.transform.position);
            Debug.DrawRay(closestPoint, Vector3.up * 0.1f, Color.red, 0.02f);

            Vector3 downDirection = (closestPoint - this.transform.position).normalized;
            Debug.DrawRay(this.transform.position, downDirection * 5, Color.blue, 0.02f);

            this.rigidbody.AddForce(downDirection * this.gripStrength * this.rigidbody.mass);
        }
    }
}
