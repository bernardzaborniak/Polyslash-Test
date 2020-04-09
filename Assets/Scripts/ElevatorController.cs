using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * This class controlls the elevator, it moves it, opens the doors etc...
 */
public class ElevatorController : MonoBehaviour
{
    [Header("References")]
    public ElevatorStopsManager elevatorStopsManager;

    [Header("Movement")]
    public float maxElevatorSpeed;
    public float elevatorAcceleration;

    [Header("Logic")]
    [Tooltip("if this is true, the elevator will hold a queue of all the floors it has to visit and will visit them in the pressed order - just like in real life")]
    public bool useQueue;

    //forr keeping track of floors
    Vector3 currentTargetFloorPosition;
 
    //states ES_ used as abbreviation for ElevatorState
    ElevatorState currentState;
    ES_Waiting es_Waiting;   
    ES_Moving es_Moving;
    ES_OpeningDoor es_OpeningDoor;
    ES_ClosingDoor es_ClosingDoor;

    #region Elevator States Implementation

    class ElevatorState
    {
        protected ElevatorController eC;//abbreviation to make code more readable

        public ElevatorState(ElevatorController elevatorController)
        {
            this.eC = elevatorController;
        }

        public virtual void OnStateEnter()
        {

        }

        public virtual void OnStateExit()
        {

        }

        public virtual void UpdateState()
        {

        }
    }

    class ES_Waiting: ElevatorState
    {
        public ES_Waiting(ElevatorController elevatorController) : base(elevatorController)
        {

        }

        public override void OnStateEnter()
        {

        }

        public override void OnStateExit()
        {

        }

        public override void UpdateState()
        {

        }
    }

    class ES_Moving : ElevatorState
    {
        float maxSpeed = 1f;
        float maxAcceleration = 0.5f;
        float currentVelocity;
        bool goUp;
        float remainingDistance;
        float currentBreakDistance; //what distance does the elevator need to deccelerate to 0 at its current speed , calculated with no friction
        bool brake; //should the elevator brake?

        public ES_Moving(ElevatorController elevatorController) : base(elevatorController)
        {

        }

        public override void OnStateEnter()
        {
            currentVelocity = 0;
        }

        public override void OnStateExit()
        {

        }

        public override void UpdateState()
        {
            //should the elevator start to brake?
            remainingDistance = (eC.currentTargetFloorPosition - eC.transform.position).magnitude;
            currentBreakDistance = currentVelocity * currentVelocity / (2 * maxAcceleration);

            if (remainingDistance < currentBreakDistance)
            {
                brake = true;
            }
            else
            {
                brake = false;
            }

            float deltaVelocity;
            float acceleration;

            //calculate deltaVelocity
            if (brake)
            {
                deltaVelocity = 0 - currentVelocity;
            }
            else
            {
                if (eC.currentTargetFloorPosition.y > eC.transform.position.y)
                {
                    deltaVelocity = maxSpeed - currentVelocity;
                }
                else
                {
                    deltaVelocity = -maxSpeed - currentVelocity;
                }
            }

            /*if (eC.currentTargetFloorPosition.y > eC.transform.position.y)
            {
                goUp = true;

                if (brake)
                {
                    deltaVelocity = 0 - currentVelocity;
                }
                else
                {
                    deltaVelocity = maxSpeed - currentVelocity;
                }
                
            }
            else
            {
                goUp = false;

                if (brake)
                {
                    deltaVelocity = 0 - currentVelocity;
                }
                else
                {
                    deltaVelocity = -maxSpeed - currentVelocity;
                }
            }*/

            //calculate acceleration
            acceleration = deltaVelocity / Time.deltaTime;
           
            if (acceleration > maxAcceleration)
            {
                acceleration = maxAcceleration;
            }
            else if(acceleration < -maxAcceleration)
            {
                acceleration = -maxAcceleration;
            }

            //apply movement
            currentVelocity += acceleration * Time.deltaTime;
            eC.transform.position += new Vector3(0,  currentVelocity*Time.deltaTime, 0);

            //check if arrived
            if (eC.transform.position == eC.currentTargetFloorPosition)
            {
                Debug.Log("arrived");
                eC.SetNewState(eC.es_Waiting);
            }
        }
    }

    class ES_ClosingDoor : ElevatorState
    {
        public ES_ClosingDoor(ElevatorController elevatorController) : base(elevatorController)
        {

        }

        public override void OnStateEnter()
        {

        }

        public override void OnStateExit()
        {

        }

        public override void UpdateState()
        {

        }
    }

    class ES_OpeningDoor : ElevatorState
    {
        public ES_OpeningDoor(ElevatorController elevatorController) : base(elevatorController)
        {

        }

        public override void OnStateEnter()
        {

        }

        public override void OnStateExit()
        {

        }

        public override void UpdateState()
        {

        }
    }

    #endregion

    void Start()
    { 
        es_Waiting = new ES_Waiting(this);
        es_Moving = new ES_Moving(this);
        es_OpeningDoor = new ES_OpeningDoor(this);
        es_ClosingDoor = new ES_ClosingDoor(this);

        currentState = es_Waiting;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            MoveToFloor(0);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            MoveToFloor(1);
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            MoveToFloor(2);
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            MoveToFloor(3);
        }

        currentState.UpdateState();
    }

    void SetNewState(ElevatorState newState)
    {
        if (newState != currentState)
        {
            currentState.OnStateExit();
            currentState = newState;
            currentState.OnStateEnter();
        }
    }

    void MoveToFloor(int floorID)
    {
        currentTargetFloorPosition = elevatorStopsManager.GetStopPostion(floorID);
        SetNewState(es_Moving);
    }

    public void OnElevatorButtonPressed(ElevatorButton pressedButton)
    {
        pressedButton.SetPressed();
        MoveToFloor(pressedButton.targetFloorID);
    }
}
