using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

public class InterceptionEnvironment : MonoBehaviour
{
    // Objects to manipulate
    private GameObject subject;
    private GameObject subjectRoad;
    private GameObject target;

    // Debug settings
    public bool RoadsEnabled = false;
    public bool ThirdPerson = false;

    // Public, trackable variables
    public float SubjectSpeed { get => subjectSpeed; }
    public float SubjectDistance { get => subjectDistance; }
    public bool HasChangedSpeed { get => hasChangedSpeed; }
    public float TargetSpeed { get => targetSpeed; }
    public float TargetDistance { get => targetDistance; }
    public bool WonPrevious { get => wonPrevious; }

    // Trial-specific variables
    private float approachAngle;
    private float heightAboveGround;
    private float subjectRadius;
    private float subjectDistance;
    private float subjectSpeedMin;
    private float subjectSpeedMax;
    private float subjectSpeed;
    private float targetRadius;
    private float targetDistance;
    private float targetSpeed;
    private float targetInitSpeed;
    private float targetFinalSpeed;
    private double targetSpeedChangeDuration;
    private bool hasChangedSpeed;
    private double timeToChangeSpeed;
    private double elapsedTime;
    private float lagCoefficient;
    private bool wonPrevious = false;

    private void Start() 
    {
        subject = GameObject.Find("Subject");
        target = GameObject.Find("Target");

        subjectRoad = GameObject.Find("SubjectRoad");
        var targetRoad = GameObject.Find("TargetRoad");
        subjectRoad.SetActive(RoadsEnabled);
        targetRoad.SetActive(RoadsEnabled);

        if (!ThirdPerson) {
            var camera = GameObject.Find("MainCamera");
            camera.transform.position = new Vector3(0, 0.5f, 0); //? Actual height of camera is decreased by 0.5 later for some reason?
            camera.transform.rotation = new Quaternion();
        }
    }

    public void Reset(Settings s) {
        approachAngle               = s.GetFloat("approachAngle");
        heightAboveGround           = s.GetFloat("heightAboveGround");
        subjectDistance             = s.GetFloat("subjectInitDistance");
        subjectSpeed                = s.GetFloat("subjectSpeedMin");
        subjectSpeedMin             = s.GetFloat("subjectSpeedMin");
        subjectSpeedMax             = s.GetFloat("subjectSpeedMax");
        lagCoefficient              = s.GetFloat("lagCoefficient");
        targetDistance              = s.GetFloat("targetInitDistance");
        targetSpeed                 = s.GetFloat("targetInitSpeed");
        targetInitSpeed             = s.GetFloat("targetInitSpeed");
        targetFinalSpeed            = s.GetFloat("targetFinalSpeed");
        targetSpeedChangeDuration   = s.GetDouble("targetSpeedChangeDuration");
        timeToChangeSpeed           = s.GetFloat("timeToChangeSpeed");

        subjectRadius = s.GetFloat("subjectRadius"); 
        targetRadius  = s.GetFloat("targetRadius");
        subject.transform.localScale = new Vector3(subjectRadius*2, subjectRadius*2, subjectRadius*2);
        target.transform.localScale = new Vector3(targetRadius*2, targetRadius*2, targetRadius*2);
        
        elapsedTime = 0d;
        hasChangedSpeed = false;
    }

    public bool Step(float deltaTime, float desiredNextSubjectSpeed)
    {
        elapsedTime += deltaTime;
        if (!hasChangedSpeed && elapsedTime >= timeToChangeSpeed) 
        {
            hasChangedSpeed = true;
            targetSpeed = targetFinalSpeed;
        }
        if (hasChangedSpeed) 
        {
            double speedProportion = (elapsedTime - timeToChangeSpeed) / targetSpeedChangeDuration;
            targetSpeed = Mathf.Min(targetFinalSpeed, (float)speedProportion * (targetFinalSpeed - targetInitSpeed) + targetInitSpeed);
        }

        subjectSpeed += lagCoefficient * (desiredNextSubjectSpeed - subjectSpeed) * deltaTime;
        subjectDistance -= subjectSpeed * deltaTime;
        targetDistance -= targetSpeed * deltaTime;
        
        Vector3 s = subject.transform.position;
        Vector3 t = target.transform.position;
        float targetSubjectDistance = Mathf.Sqrt(Mathf.Pow(s.x - t.x, 2) + Mathf.Pow(s.z - t.z, 2));
        bool won = targetSubjectDistance < (targetRadius + subjectRadius);
        if (subjectDistance < -subjectRadius*2 || targetDistance < -targetRadius*2 || won) {
            wonPrevious = won;
            return true;
        }
        return false;
    }

    public void Render()
    {
        target.transform.position = new Vector3(-targetDistance, heightAboveGround, 0);
        subject.transform.position = new Vector3(
            -(float)System.Math.Cos(approachAngle * Mathf.PI / 180) * subjectDistance, 
            heightAboveGround, 
            (float)System.Math.Sin(approachAngle * Mathf.PI / 180) * subjectDistance);

        var cameraRot = new Quaternion();
        cameraRot.eulerAngles = new Vector3(0, 90 + approachAngle, 0);
        subject.transform.rotation = cameraRot;
    
        subjectRoad.transform.rotation = cameraRot;
    }
}
