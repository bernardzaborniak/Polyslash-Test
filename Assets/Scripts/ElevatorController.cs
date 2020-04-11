using System.Collections;
using System.Collections.Generic;
using UnityEngine;



// This class controlls the elevator using a state machine.

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
    [SerializeField]
    AudioClip elevatorMusic;


    ElevatorButton lastPressedButton;

    ElevatorStop targetStop;
    ElevatorStop lastTargetStop;

    // Elevator states "ES_" is used as abbreviation for ElevatorState.
    ElevatorState currentState;
    ES_WaitingClosedDoor es_WaitingClosedDoor;
    ES_WaitingOpenDoor es_WaitingOpenDoor;   
    ES_Moving es_Moving;
    ES_OpeningDoor es_OpeningDoor;
    ES_ClosingDoor es_ClosingDoor;

    #region Elevator States Implementation

    class ElevatorState
    {
        // An abbreviation to make code more readable.
        protected ElevatorController eC;

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

    // In this state the elevator is waiting for duty with doors closed.
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

    // In this state the elevator is waiting for duty with doors opened, they close after x seconds.
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

    // In this state the elevator moves smoothly between current position and target position.
    class ES_Moving : ElevatorState
    {
        float maxSpeed;
        float maxAcceleration ;
        float currentVelocity;
        bool goUp;
        float remainingDistance;
        //Represents the distance the elevator needs to deccelerate to 0 m/s at its current speed. Calculated without considering friction.
        float currentBreakDistance; 
        bool brake;

        public ES_Moving(ElevatorController elevatorController) : base(elevatorController)
        {
            maxAcceleration = eC.maxElevatorAcceleration;
            maxSpeed = eC.maxElevatorSpeed;
        }

        public override void OnStateEnter()
        {
            currentVelocity = 0;

            // audio
            eC.elevatorMechanicAudioSource.clip = eC.movingAudio;
            eC.elevatorMechanicAudioSource.loop = true;
            eC.elevatorMechanicAudioSource.Play();

            if (eC.IsPlayerInsideElevator())
            {
                eC.elevatorSpeaker.clip = eC.elevatorMusic;
                eC.elevatorSpeaker.loop = true;
                eC.elevatorSpeaker.Play();

                // To get rid of the player jumping around while drien by physics and moving down inside the elevator,
                // we set him as child to the elevator during the elevator movement.
                eC.SetPlayerParentToElevator();
            }
         
        }

        public override void OnStateExit()
        {
            eC.elevatorSpeaker.Stop();
            eC.elevatorSpeaker.loop = false;

            eC.elevatorMechanicAudioSource.Stop();
            eC.elevatorMechanicAudioSource.loop = false;

            eC.ResetPlayerParent();
        }

        public override void UpdateState()
        {
            // Determine whether the elevator should start to brake.
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

            // calculate deltaVelocity
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

            // calculate acceleration
            acceleration = deltaVelocity / Time.deltaTime;
           
            if (acceleration > maxAcceleration)
            {
                acceleration = maxAcceleration;
            }
            else if(acceleration < -maxAcceleration)
            {
                acceleration = -maxAcceleration;
            }

            // apply movement
            currentVelocity += acceleration * Time.deltaTime;
            eC.transform.position += new Vector3(0,  currentVelocity*Time.deltaTime, 0);

            // check if arrived
            if ((eC.transform.position - eC.targetStop.transform.position).magnitude<0.005f)
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

            // audio
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
            Debug.Log("new state: " + currentState);
            currentState.OnStateEnter();

        }
    }

    public void OnElevatorButtonPressed(ElevatorButton pressedButton)
    {
        // If the same button is pushed repeatedly or the button for the floor the elevator is currently on is pushed, nothing will happen.
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
        lastTargetStop.door.Close();
    }

    void OpenDoors()
    {
        elevatorDoor.Open();
        lastTargetStop.door.Open();
    }

    bool IsPlayerInsideElevator()
    {
        Collider[] colliders;
        colliders = Physics.OverlapBox(transform.position + transform.up, new Vector3(2, 2, 2));

        for (int i = 0; i < colliders.Length; i++)
        {
            if(colliders[i].tag == "Player")
            {
                return true;
            }
        }

        return false;
    }

    public void SetPlayerParentToElevator()
    {
        Collider[] colliders;
        colliders = Physics.OverlapBox(transform.position + transform.up, new Vector3(2, 2, 2));

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].tag == "Player")
            {
                colliders[i].transform.SetParent(transform);
            }
        }
    }

    public void ResetPlayerParent()
    {
        Collider[] colliders;
        colliders = Physics.OverlapBox(transform.position + transform.up, new Vector3(2, 2, 2));

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].tag == "Player")
            {
                colliders[i].transform.SetParent(null);
            }
        }
    }

}
