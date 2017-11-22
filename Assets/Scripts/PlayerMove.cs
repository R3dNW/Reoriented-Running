using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerMove : MonoBehaviour
{
    float speed = 10;
    float jumpAmount = 2;

    float gravity = 1f;

    Rigidbody rb;

    public LayerMask layerMask;

    public CameraController controller;

    //Quaternion _facing;

    void Start()
    {
        this.rb = this.GetComponent<Rigidbody>();

       // this._facing = this.transform.rotation;
    }

    void FixedUpdate()
    {
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        this.rb.AddForce(this.transform.localRotation * movement * this.speed);

        this.rb.angularVelocity = Vector3.zero;

        // Should we get the closest collider
        /*
        RaycastHit[] hitDatas = Physics.SphereCastAll(this.transform.position, 10, Vector3.up, 0, this.layerMask);
        
        if (hitDatas.Length > 0)
        {
            hitDatas = (from RaycastHit hitData in hitDatas
                        orderby hitData.distance ascending
                        select hitData).ToArray();

            RaycastHit hitInfo = hitDatas[0];
        */

        // or the collider beneath the player

        RaycastHit hitInfo;

        if (Physics.Raycast(this.transform.position, -this.transform.up, out hitInfo, 10, this.layerMask))
        {
            Vector3 closestPoint = hitInfo.collider.ClosestPoint(this.transform.position);

            Debug.DrawRay(closestPoint, Vector3.up * 0.1f, Color.green);

            Vector3 downDir = (closestPoint - this.transform.position).normalized;

            Debug.DrawRay(this.transform.position, downDir * 5, Color.blue, 0.02f);

            this.rb.AddForce(downDir * gravity);

            controller.originalPlayerRot = Quaternion.LookRotation(downDir, Vector3.up) * Quaternion.Euler(90, -90, 90);
        }
    }
}