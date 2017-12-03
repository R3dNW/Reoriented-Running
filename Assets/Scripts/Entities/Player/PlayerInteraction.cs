using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public Camera FPSCamera;

    public LayerMask physicsObjectsMask;

    public enum InteractionState
    {
        Idle,       // The player may not be Idle, but they're not doing anything with their hands.
        Carrying,
        Grappling
    }

    private InteractionState state;

    private GameObject carrying;
    private MagneticObject carryingMO;
    private Rigidbody carryingRb;
    private float holdDistance = 2.25f;

    public void Start()
    {
        this.state = InteractionState.Idle;
    }

    void Update()
    {
        if (this.state == InteractionState.Idle)
        {
            this.Update_Idle();
        }
        else if (this.state == InteractionState.Carrying)
        {
            this.Update_Carrying();
        }
        else if (this.state == InteractionState.Grappling)
        {
            // Do some rad maths physics!!!!
        }
    }

    void Update_Idle()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            RaycastHit hitInfo;
            
            if (Physics.Raycast(this.FPSCamera.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                GameObject hitGO = hitInfo.collider.gameObject;

                if ((this.physicsObjectsMask & (1 << hitGO.layer)) != 0)
                {
                    this.carrying = hitGO;
                    this.carryingMO = hitGO.GetComponent<MagneticObject>();
                    this.carryingMO.magnetised = false;
                    this.carryingRb = hitGO.GetComponent<Rigidbody>();
                    this.state = InteractionState.Carrying;
                }
                // Else, did it hit a button etc.?
            }
        }
    }

    void Update_Carrying()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            this.state = InteractionState.Idle;
            this.carryingMO.magnetised = true;
            return;
        }

        this.carryingRb.velocity = Vector3.zero;

        Vector3 lookingDirection = this.FPSCamera.ScreenPointToRay(Input.mousePosition).direction;

        Vector3 targetPosition =
            this.FPSCamera.transform.position
            + lookingDirection * this.holdDistance;  // Hold the object out in front of you
            //+ Vector3.Cross(this.FPSCamera.transform.right, lookingDirection).normalized * 0.65f; // Hold the object slightly lower

        this.carrying.transform.position = Vector3.Lerp(this.carrying.transform.position, targetPosition, 0.25f);

        // Lock to your horizontal rotation not vertical?
        this.carrying.transform.rotation = Quaternion.Lerp(this.carrying.transform.rotation, this.FPSCamera.transform.parent.rotation, 0.25f);
    }
}
