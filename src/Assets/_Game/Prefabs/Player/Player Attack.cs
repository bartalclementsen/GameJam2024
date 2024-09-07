using System.Collections;
using System.Collections.Generic;
using Core.Loggers;
using UnityEngine;
using ILogger = Core.Loggers.ILogger;

public class PlayerAttack : MonoBehaviour
{
    public GameObject spear;  // Assign the spear GameObject in the inspector
    public float attackDistance = 1.5f; // How far the spear moves forward
    public float attackSpeed = 10f; // Speed of spear thrust
    private Vector3 initialSpearPosition;
    private bool isAttacking = false;
    private bool spearForward = true;

    public float attackDuration = 0.2f;
    private float attackTime;
    private ILogger _logger;

    // Start is called before the first frame update
    void Start()
    {
        initialSpearPosition = spear.transform.localPosition;
    }
    
    void Update()
    {
        // Trigger attack on button press (left mouse button)
        if (Input.GetButtonDown("Fire1") && !isAttacking)
        {
            isAttacking = true;
        }

        // If attacking, count down the attack duration
        if (isAttacking)
        {
            MoveSpear();
        }
    }

    void MoveSpear()
    {
        // Move spear forward if spearForward is true, otherwise move back
        if (spearForward)
        {
            // Move the spear forward in local space
            spear.transform.localPosition = Vector3.MoveTowards(
                spear.transform.localPosition, 
                initialSpearPosition + transform.up * attackDistance, // or forward in 3D
                attackSpeed * Time.deltaTime
            );

            // Check if the spear has reached the attack distance
            if (Vector3.Distance(spear.transform.localPosition, initialSpearPosition + transform.up * attackDistance) < 0.1f)
            {
                spearForward = false; // Start retracting
            }
        }
        else
        {
            // Move the spear back to its original position
            spear.transform.localPosition = Vector3.MoveTowards(
                spear.transform.localPosition, 
                initialSpearPosition, 
                attackSpeed * Time.deltaTime
            );

            // Check if the spear has returned to its initial position
            if (Vector3.Distance(spear.transform.localPosition, initialSpearPosition) < 0.1f)
            {
                spearForward = true; // Ready for the next attack
                isAttacking = false; // Attack sequence ends
            }
        }
    }
}
