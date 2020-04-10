using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * This class controlls the elevator, it moves it, opens the doors etc...
 */
public class ElevatorController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    Door elevatorDoor;

    [Header("Elevator Movement")]
    [SerializeField]
    float maxElevatorSpeed;
    [SerializeField]
    float maxElevatorAcceleration;

    [Header("Doors")]
    //[Tooltip("if this is true, the elevator will hold a queue of all the floors it has to visit and will visit them in the pressed order - just like in real life")]
    //public bool useQueue; //Will not be implemented for this test
    [SerializeField]
    float timeBeforeDoorAutomaticallyCloses;
    [SerializeField]
    float doorOpenOrCloseTime;

    [Header("Audio Feedback")]
    [SerializeField]
    AudioSource elevatorMechanicAudioSource;
    [SerializeField]
    AudioSource elevatorSpeaker;
    [SerializeField]
    AudioClip openDoorsAudio;
    [SerializeField]
    AudioClip movingAudio;
    [SerializeField]
    AudioClip closingDoorsAudio;
    [SerializeField]
    AudioClip closingDoorsErrorAudio;

    //forr keeping track of target floors
    ElevatorStop targetStop;
    ElevatorStop lastTargetStop;

    //keep track of the button currently pressed
    ElevatorButton lastPressedButton;

    //elevator states ES_ used as abbreviation for ElevatorState
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

        public virtual void OnMoveToAnotherFloorOrderIssued()
        {

        }

        public virtual void OnPlayerEntersDangerousArea()
        {

        }

        public virtual void OnMoveToTheSameFloorTheElevatorIsOnButtonPressed()
        {

        }
    }

    //elevator is waiting for duty with doors closed
    class ES_WaitingClosedDoor : ElevatorState
    {
        public ES_WaitingClosedDoor(ElevatorController elevatorController) : base(elevatorController)
        {

        }

        public override void OnMoveToAnotherFloorOrderIssued()
        {
            eC.SetNewState(eC.es_Moving);
        }

        public override void OnMoveToTheSameFloorTheElevatorIsOnButtonPressed()
        {
            eC.SetNewState(eC.es_OpeningDoor);
        }
    }

    //elevator is waiting for duty with doors opened, they close after x seconds
    class ES_WaitingOpenDoor : ElevatorState
    {
        float closeDoorTime;

        public ES_WaitingOpenDoor(ElevatorController elevatorController) : base(elevatorController)
        {

        }

        public override void OnStateEnter()
        {
            closeDoorTime = Time.time + eC.timeBeforeDoorAutomaticallyCloses;
        }

        public override void OnStateExit()
        {

        }

        public override void UpdateState()
        {
            if (Time.time > closeDoorTime)
            {
                eC.SetNewState(eC.es_ClosingDoor);
            }
        }

        public override void OnMoveToAnotherFloorOrderIssued()
        {
            eC.SetNewState(eC.es_ClosingDoor);
        }
    }

    //elevator moves smoothly between current position and TargetFloor
    class ES_Moving : ElevatorState
    {
        float maxSpeed;
        float maxAcceleration ;
        float currentVelocity;
        bool goUp;
        float remainingDistance;
        float currentBreakDistance; //what distance does the elevator need to deccelerate to 0 at its current speed , calculated with no friction
        bool brake; //should the elevator brake?

        public ES_Moving(ElevatorController elevatorController) : base(elevatorController)
        {
            maxAcceleration = eC.maxElevatorAcceleration;
            maxSpeed = eC.maxElevatorSpeed;
        }

        public override void OnStateEnter()
        {
            currentVelocity = 0;

            //audio
            eC.elevatorSpeaker.clip = eC.movingAudio;
            eC.elevatorSpeaker.loop = true;
            eC.elevatorSpeaker.Play();
        }

        public override void OnStateExit()
        {
            eC.elevatorSpeaker.Stop();
            eC.elevatorSpeaker.loop = false;
        }

        public override void UpdateState()
        {
            //should the elevator start to brake?
            remainingDistance = (eC.targetStop.transform.position - eC.transform.position).magnitude;
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
                if (eC.targetStop.transform.position.y > eC.transform.position.y)
                {
                    deltaVelocity = maxSpeed - currentVelocity;
                }
                else
                {
                    deltaVelocity = -maxSpeed - currentVelocity;
                }
            }

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
            if (eC.transform.position == eC.targetStop.transform.position)
            {
                eC.lastPressedButton.SetReadyToBePressed();
                eC.lastTargetStop = eC.targetStop;
                eC.targetStop = null;
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

            //audio
            eC.elevatorMechanicAudioSource.clip = eC.closingDoorsAudio;
            eC.elevatorMechanicAudioSource.Play();
        }

        public override void UpdateState()
        {
            if(Time.time > endStateTime)
            {
                if (eC.targetStop)
                {
                    eC.SetNewState(eC.es_Moving);
                }
                else
                {
                    eC.SetNewState(eC.es_WaitingClosedDoor);
                }
               
            }
        }

        public override void OnPlayerEntersDangerousArea()
        {
            eC.elevatorSpeaker.clip = eC.closingDoorsErrorAudio;
            eC.elevatorSpeaker.Play();
            eC.SetNewState(eC.es_OpeningDoor);
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

            //audio
            eC.elevatorMechanicAudioSource.clip = eC.openDoorsAudio;
            eC.elevatorMechanicAudioSource.Play();
        }

        public override void UpdateState()
        {
            if (Time.time > endStateTime)
            {
                if (eC.targetStop)
                {
                    eC.SetNewState(eC.es_ClosingDoor);
                }
                else
                {
                    eC.SetNewState(eC.es_WaitingOpenDoor);
                }
            }
        }

        public override void OnMoveToAnotherFloorOrderIssued()
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

    public void OnElevatorButtonPressed(ElevatorButton pressedButton)
    {
        //if we push the same button repeatedly or the button for the floor we are currently on, nothing will happen
        if (lastPressedButton != null)
        {
            if (lastPressedButton != pressedButton)
            {
                lastPressedButton.SetReadyToBePressed();
            }
        }

        if (targetStop == null)
        {
            if (pressedButton.targetStop != lastTargetStop)
            {
                IssueMoveOrder(pressedButton);
            }
            else
            {
                currentState.OnMoveToTheSameFloorTheElevatorIsOnButtonPressed();
            }
        }
        else if (pressedButton.targetStop != targetStop)
        {
            IssueMoveOrder(pressedButton);
        }
        
    }

    void IssueMoveOrder(ElevatorButton pressedButton)
    {
        pressedButton.SetPressed();
        lastPressedButton = pressedButton;

        //lastTargetStop = targetStop;
        targetStop = pressedButton.targetStop;

        currentState.OnMoveToAnotherFloorOrderIssued();
    }

    public void OnPlayerEntersDangerousArea()
    {
        currentState.OnPlayerEntersDangerousArea();
    }

    void CloseDoors()
    {
        elevatorDoor.Close();
        Debug.Log("last Target Stop: " + lastTargetStop);
        lastTargetStop.door.Close();
    }

    void OpenDoors()
    {
        elevatorDoor.Open();
        Debug.Log("last Target Stop: " + lastTargetStop);

        lastTargetStop.door.Open();
    }

}
