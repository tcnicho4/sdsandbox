using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CapstoneCarController : MonoBehaviour
{
    private enum CarMovementState
    {
        STEER_LEFT,
        STEER_RIGHT,

        COUNT_OR_ERROR
    }

    public GameObject mCarGameObject;
    private ICar mCar = null;

    private static Capstone.IPCServer mServer;
   
    // This function gets called once: Just before it needs to be used.
    void Awake()
    {
        if (mCarGameObject != null)
            mCar = mCarGameObject.GetComponent<ICar>();

        bool serverCreated = false;

        // Start a new thread to manage the IPCServer.
        Thread serverThread = new Thread(() =>
        {
            mServer = new Capstone.IPCServer();
            serverCreated = true;

            mServer.RunIPCServerLoop();
        });
        serverThread.Start();

        // Wait for the other thread to create the IPCServer instance. We place a memory barrier
        // here so that this thread does not incorrectly cache serverCreated, thus never exiting
        // the loop.
        while (!serverCreated)
            Interlocked.MemoryBarrier();
    }

    private void OnEnable()
    {
        // Add stuff here if/when needed. This is called when the car is spawned in.
    }

    private void OnDisable()
    {
        mCar.RequestThrottle(0.0f);
        mCar.RequestSteering(0.0f);
        mCar.RequestFootBrake(1.0f);
    }

    private void FixedUpdate()
    {
        mServer.ExecutePendingActions(this);
    }

    public ICar GetCar()
    {
        return mCar;
    }
}
