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
    public Door elevatorDoor;

    [Header("Movement")]
    public float maxElevatorSpeed;
    public float elevatorAcceleration;
    public float doorOpenOrCloseTime;

    [Header("Logic")]
    [Tooltip("if this is true, the elevator will hold a queue of all the floors it has to visit and will visit them in the pressed order - just like in real life")]
    public bool useQueue;

    //forr keeping track of floors
    Vector3 currentTargetFloorPosition;
    int currentTartgetFloorID;
    int previousTartgetFloorID;

    //keep track of the buttons currently pressed
    //List<ElevatorButton> currentlyPressedButtons = new List<ElevatorButton>();
    ElevatorButton currentlyPressedButton;

    //states ES_ used as abbreviation for ElevatorState
    ElevatorState currentState;
    ES_WaitingClosedDoor es_WaitingClosedDoor;
    ES_WaitingOpenDoor es_WaitingOpenDoor;   
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

        public virtual void OnMoveToFloorOrderIssued()
        {

        }
    }

    class ES_WaitingClosedDoor : ElevatorState
    {
        public ES_WaitingClosedDoor(ElevatorController elevatorController) : base(elevatorController)
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

        public override void OnMoveToFloorOrderIssued()
        {
            eC.SetNewState(eC.es_Moving);
        }
    }

    class ES_WaitingOpenDoor: ElevatorState
    {
        public ES_WaitingOpenDoor(ElevatorController elevatorController) : base(elevatorController)
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

        public override void OnMoveToFloorOrderIssued()
        {
            eC.SetNewState(eC.es_ClosingDoor);
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
                eC.SetNewState(eC.es_OpeningDoor);
            }
        }
    }

    class ES_ClosingDoor : ElevatorState
    {
        float endStateTime;

        public ES_ClosingDoor(ElevatorController elevatorController) : base(elevatorController)
        {

        }

        public override void OnStateEnter()
        {
            eC.CloseDoors();
            endStateTime = Time.time + eC.doorOpenOrCloseTime;
        }

        public override void OnStateExit()
        {

        }

        public override void UpdateState()
        {
            if(Time.time > endStateTime)
            {
                eC.SetNewState(eC.es_Moving);
            }
        }
    }

    class ES_OpeningDoor : ElevatorState
    {
        float endStateTime;

        public ES_OpeningDoor(ElevatorController elevatorController) : base(elevatorController)
        {

        }

        public override void OnStateEnter()
        {
            eC.OpenDoors();
            endStateTime = Time.time + eC.doorOpenOrCloseTime;
        }

        public override void OnStateExit()
        {

        }

        public override void UpdateState()
        {
            //Debug.Log("opening update");
            if (Time.time > endStateTime)
            {
                //Debug.Log("Time.time: " + Time.time + " > " + endStateTime);
                eC.SetNewState(eC.es_WaitingOpenDoor);
            }
        }

        public override void OnMoveToFloorOrderIssued()
        {
            eC.SetNewState(eC.es_ClosingDoor);
        }
    }

    #endregion

    void Start()
    {
        es_WaitingClosedDoor = new ES_WaitingClosedDoor(this);
        es_WaitingOpenDoor = new ES_WaitingOpenDoor(this);
        es_Moving = new ES_Moving(this);
        es_OpeningDoor = new ES_OpeningDoor(this);
        es_ClosingDoor = new ES_ClosingDoor(this);

        currentState = es_WaitingClosedDoor;
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
            Debug.Log("----------Change State-----------");
            currentState.OnStateExit();
            Debug.Log("old state: " + currentState);
            currentState = newState;
            Debug.Log("new: " + currentState);
            currentState.OnStateEnter();

        }
    }

    void MoveToFloor(int floorID)
    {
        previousTartgetFloorID = currentTartgetFloorID;
        currentTartgetFloorID = floorID;
        currentTargetFloorPosition = elevatorStopsManager.GetStopPostion(currentTartgetFloorID);
        currentState.OnMoveToFloorOrderIssued();
    }

    public void OnElevatorButtonPressed(ElevatorButton pressedButton)
    {
        pressedButton.SetPressed();

        //adjust the currently pressedButtons
        if (currentlyPressedButton)
        {
            if(currentlyPressedButton != pressedButton)
            {
                currentlyPressedButton.SetReadyToBePressed();
            }
        }
        currentlyPressedButton = pressedButton;

        //MoveToFloor(pressedButton.targetFloorID);
        previousTartgetFloorID = currentTartgetFloorID;
        currentTartgetFloorID = pressedButton.targetFloorID;
        currentTargetFloorPosition = elevatorStopsManager.GetStopPostion(currentTartgetFloorID);
        currentState.OnMoveToFloorOrderIssued();
            

    }

    void CloseDoors()
    {
        Debug.Log("Close Doors");
        elevatorDoor.Close();
        elevatorStopsManager.GetOuterElevatorDoor(previousTartgetFloorID).Close();
    }

    void OpenDoors()
    {
        Debug.Log("open elevator Doors");
        elevatorDoor.Open();
        elevatorStopsManager.GetOuterElevatorDoor(currentTartgetFloorID).Open();
    }
}
