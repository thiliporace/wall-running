using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("Wall running")] 
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;

    public float wallrunForce;
    //private float maxwallrunTime = 1000f;
    private float wallrunTimer;
    public float wallClimbSpeed;

    [Header("Input")] 
    private float horizontalInput;
    private float verticalInput;

    public KeyCode upwardsKey = KeyCode.LeftShift;
    public KeyCode downwardsKey = KeyCode.LeftControl;
    private bool upwardsRunning;
    private bool downwardsRunning;

    [Header("Detection")] public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;

    [Header("References")] 
    public Transform orientation;
    private NewPlayerMovement pm;
    private Rigidbody rb;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<NewPlayerMovement>();
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if(pm.wallrunning)
            WallrunningMovement();
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance,
            whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance,
            whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        //Getting inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        upwardsRunning = Input.GetKey(upwardsKey);
        downwardsRunning = Input.GetKey(downwardsKey);
        
        //State 1 - Wallrunning
        if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround())
        {
            //start wallrun here
            if (!pm.wallrunning)
                StartWallrun();
            
        }
        else
        {
            if(pm.wallrunning)
                StopWallrun();
        }
    }

    private void StartWallrun()
    {
        pm.wallrunning = true;
    }

    private void StopWallrun()
    {
        pm.wallrunning = false;
        
    }

    private void WallrunningMovement()
    {
        rb.useGravity = false;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;
        
        // forward force
        rb.AddForce(wallForward * wallrunForce, ForceMode.Force);
        
        // upwards/downwards force
        if (upwardsRunning)
            rb.velocity = new Vector3(rb.velocity.x, wallClimbSpeed, rb.velocity.z);
        if (downwardsRunning)
            rb.velocity = new Vector3(rb.velocity.x, -wallClimbSpeed, rb.velocity.z);
        
        // push to wall force
        if(!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
            rb.AddForce(-wallNormal * 100f, ForceMode.Force);
        
        
    }
}
