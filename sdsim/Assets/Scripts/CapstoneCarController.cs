using System;
using System.Collections;
using System.Collections.Generic;
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

    private float mElapsedTime = 0.0f;
    private CarMovementState mMoveState = CarMovementState.STEER_LEFT;

    private static readonly float BASE_TIME_TO_CHANGE_STATE = 0.5f;
    private float mTimeToChangeState = BASE_TIME_TO_CHANGE_STATE;
   
    void Awake()
    {
        if (mCarGameObject != null)
            mCar = mCarGameObject.GetComponent<ICar>();
    }

    private void OnEnable()
    {
        // Start off nice and steady with no angle.
        mCar.RequestThrottle(1.0f);
    }

    private void OnDisable()
    {
        mCar.RequestThrottle(0.0f);
        mCar.RequestSteering(0.0f);
        mCar.RequestFootBrake(1.0f);
    }

    private void FixedUpdate()
    {
        mElapsedTime += Time.fixedDeltaTime;

        CarMovementState prevState = mMoveState;
        System.Random random = new System.Random();


        while(mElapsedTime >= mTimeToChangeState)
        {
            // Advance to the next state.
            mMoveState = (CarMovementState)(((int)mMoveState + 1) % (int)CarMovementState.COUNT_OR_ERROR);

            mElapsedTime -= mTimeToChangeState;

            // Randomize the time to change to the next state to BASE_TIME_TO_CHANGE_STATE + [-0.5, 0.5[ seconds.
            mTimeToChangeState = BASE_TIME_TO_CHANGE_STATE + ((((float)random.NextDouble() * 2.0f) - 1.0f) * 0.5f);
        }

        if (mMoveState != prevState)
            UpdateMoveState();
    }

    private void UpdateMoveState()
    {
        switch(mMoveState)
        {
            case CarMovementState.STEER_LEFT:
                mCar.RequestSteering(-30.0f);
                break;

            case CarMovementState.STEER_RIGHT:
                mCar.RequestSteering(30.0f);
                break;

            default:
                break;
        }
    }
}
