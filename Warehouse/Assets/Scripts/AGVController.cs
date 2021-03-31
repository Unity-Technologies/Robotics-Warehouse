using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.Control
{
    public enum ControlMode { Keyboard, PID };

    public class AGVController : MonoBehaviour
    {
        public GameObject wheel1;
        public GameObject wheel2;
        public GameObject centerPoint;
        public Goal goalFunc; //Automate this
        public ControlMode mode = ControlMode.Keyboard;

        private ArticulationBody wA1;
        private ArticulationBody wA2;

        public float maxLinearSpeed = 2; //  m/s
        public float maxRotationalSpeed = 1;//
        public float wheelRadius = 0.033f; //meters
        public float trackWidth = 0.288f; // meters Distance between tyres
        public float forceLimit = 10;
        public float damping = 10;

        public float kp = 0;
        public float ki = 0;
        public float kd = 0;

        public float kpRotational = 0;
        public float kiRotational = 0;
        public float kdRotational = 0;

        private static float deltaX = .0002f; //m 
        private static float deltaZ = .0002f; //m
        private static float deltaTheta = .0002f; //radians
        private robotState goal;
        private robotState currentPosition;
        private int robotSleep = 0;
        private int robotAwake = 1;

        public float feedbackLinearSpeed;
        public float feedbackRotSpeed;


        private RotationDirection direction;

        void Start()
        {
            wA1 = wheel1.GetComponent<ArticulationBody>();
            wA2 = wheel2.GetComponent<ArticulationBody>();
            currentPosition = new robotState(0, 0, 0);
            GetGoal();
            SetParameters(wA1);
            SetParameters(wA2);
        }

        void FixedUpdate()
        {
            UpdateState();
            if (mode == ControlMode.Keyboard)
                KeyBoardUpdate();
            else
                PIDControl();
        }

        private void SetParameters(ArticulationBody joint)
        {
            ArticulationDrive drive = joint.xDrive;
            drive.forceLimit = forceLimit;
            drive.damping = damping;
            joint.xDrive = drive;
        }

        private void SetSpeed(ArticulationBody joint, float wheelSpeed = float.NaN)
        {
            ArticulationDrive drive = joint.xDrive;
            if (float.IsNaN(wheelSpeed))
                drive.targetVelocity = ((2 * maxLinearSpeed) / wheelRadius) * Mathf.Rad2Deg * (int)direction;
            else
                drive.targetVelocity = wheelSpeed;
            joint.xDrive = drive;
        }

        private void KeyBoardUpdate()
        {
            float moveDirection = Input.GetAxis("Vertical");
            float inputSpeed;
            float inputRotationSpeed;
            if (moveDirection > 0)
            {
                inputSpeed = maxLinearSpeed;
            }
            else if (moveDirection < 0)
            {
                inputSpeed = maxLinearSpeed * -1;
            }
            else
            {
                inputSpeed = 0;
            }

            float turnDirction = Input.GetAxis("Horizontal");
            if (turnDirction > 0)
            {
                inputRotationSpeed = maxRotationalSpeed;
            }
            else if (turnDirction < 0)
            {
                inputRotationSpeed = maxRotationalSpeed * -1;
            }
            else
            {
                inputRotationSpeed = 0;
            }
            robotInput(inputSpeed, inputRotationSpeed);
        }


        private void PIDControl()
        {
            // Debug.Log(PropErrorDistance() + " Angle Error : " + PropErrorAngle());
            if (!NearGoal())
            {
                robotjointState(robotAwake);
                if (!NearHeading())
                {
                    feedbackRotSpeed = NearHeading() ? 0 : kpRotational * PropErrorAngle();
                    robotInput(0, feedbackRotSpeed);
                }
                else
                {
                    feedbackLinearSpeed = NearGoal() ? 0 : kp * PropErrorDistance();
                    robotInput(feedbackLinearSpeed, 0);
                }
            }
            else
            {
                robotjointState(robotSleep);
            }
        }

        private void robotInput(float speed, float rotSpeed) // m/s and rad/s
        {
            if (speed > maxLinearSpeed)
                speed = maxLinearSpeed;
            if (rotSpeed > maxRotationalSpeed)
                rotSpeed = maxRotationalSpeed;
            // Debug.Log("Input: " + speed + " " + rotSpeed);
            float wheel1Rotation = (speed / wheelRadius);
            float wheel2Rotation = wheel1Rotation;
            float wheelSpeedDiff = ((rotSpeed * trackWidth) / wheelRadius);
            if (rotSpeed != 0)
            {
                wheel1Rotation = (wheel1Rotation + (wheelSpeedDiff / 1)) * Mathf.Rad2Deg;
                wheel2Rotation = (wheel2Rotation - (wheelSpeedDiff / 1)) * Mathf.Rad2Deg;
            }
            else
            {
                wheel1Rotation *= Mathf.Rad2Deg;
                wheel2Rotation *= Mathf.Rad2Deg;
            }
            SetSpeed(wA1, wheel1Rotation);
            SetSpeed(wA2, wheel2Rotation);
        }

        private void robotjointState(int status)
        {
            if (status == robotSleep)
            {
                wA1.Sleep();
                wA2.Sleep();
            }
            if (status == robotAwake)
            {
                wA1.WakeUp();
                wA2.WakeUp();
            }

        }

        private bool NearGoal()
        {
            if (PropErrorDistance() > .01)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool NearHeading()
        {
            if (Mathf.Abs(PropErrorAngle()) > .1)
                return false;
            else
                return true;
        }

        private void GetGoal()
        {
            goal = goalFunc.GetGoal();
            goal.SetAngle(-1 * Mathf.Atan2(goal.Z() - currentPosition.Z(), goal.X() - currentPosition.X()));
        }

        private float PropErrorDistance()
        {
            return robotState.DistanceError(goal, currentPosition);
        }

        private float PropErrorAngle()
        {
            return robotState.AngleError(goal, currentPosition);
        }

        private void UpdateState()
        {
            float robotYawAngle = (centerPoint.transform.rotation.eulerAngles.y * Mathf.Deg2Rad) + (-.5f * Mathf.PI);
            currentPosition.SetValues(centerPoint.transform.position.x, centerPoint.transform.position.z, robotYawAngle);//(robotYawAngle < 2 * Mathf.PI) ? robotYawAngle : robotYawAngle - 2 * Mathf.PI);
            GetGoal();
        }
    }

    public class robotState
    {
        private float x;
        private float z;
        private float theta;

        public robotState(float xVal, float zVal, float thetaVal)
        {
            x = xVal;
            z = zVal;
            theta = thetaVal;
        }

        public void SetValues(float xVal = 0, float zVal = 0, float thetaVal = 0)
        {
            x = xVal;
            z = zVal;
            theta = thetaVal;
        }

        public float X()
        {
            return x;
        }

        public float Z()
        {
            return z;
        }

        public float Angle()
        {
            return theta;
        }

        public void SetAngle(float angle)
        {
            theta = angle;
        }

        public static robotState operator -(robotState a, robotState b)
        {
            float delx = a.X() - b.X();
            float delz = a.Z() - b.Z();
            float delTheta = a.Angle() - b.Angle();
            return new robotState(delx, delz, delTheta);
        }

        public robotState Abs()
        {
            return new robotState(Mathf.Abs(x), Mathf.Abs(z), Mathf.Abs(theta));
        }

        public static float DistanceError(robotState a, robotState b)
        {
            float zerror = Mathf.Pow(a.z - b.z, 2);
            float xerror = Mathf.Pow(a.x - b.x, 2);
            return Mathf.Sqrt(zerror + xerror);
        }

        public static float AngleError(robotState a, robotState b)
        {
            float angleError = a.theta - b.theta;
            if (Mathf.Abs(angleError) <= Mathf.PI)
                return angleError;
            else
                return (Mathf.PI * 2 * (angleError / Mathf.Abs(angleError)) - angleError);
        }
    }

}
