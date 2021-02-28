using UnityEngine;
using System.Linq;
using TMPro;
using UXF;

public class TrialController : MonoBehaviour
{
    // Objects to manipulate
    public GameObject Target;
    public GameObject Subject;
    public GameObject Path;
    public TextMeshProUGUI StateDisplay;

    // Experiment settings
    public int FPS = 30;
    // public float[] ApproachAngleList = {135f, 140f, 145f};
    public float[] SubjectSpeeds = {2.0f, 4.0f, 8.0f, 10.0f, 12.0f, 14.0f};
    // public float SubjectMinInitDistance = 20f;
    // public float SubjectMaxInitDistance = 30f;
    public float SubjectRadius = 0.35f;
    // public float TargetInitDistance = 45f;
    // public float[] TargetInitSpeedList = {11.25f, 9.47f, 8.18f};
    // public float MinTimeToChangeSpeed = 2.5f;
    // public float MaxTimeToChangeSpeed = 3.25f;
    // public float TargetMinSpeed = 10f;
    // public float TargetMaxSpeed = 20f;
    // public float TargetSpeedMean = 15f;
    // public float TargetSpeedStd = 5f;
    public float TargetRadius = 0.35f;

    // A no seed RNG- maybe add the ability to seed it later?
    private System.Random _random = new System.Random();

    // Trial-specific variables
    private float _approachAngle;
    private float _targetDistance;
    private float _targetSpeed;
    private float _targetFinalSpeed;
    private float _subjectDistance;
    private float _subjectSpeed;
    private bool _hasChangedSpeed;
    private double _timeToChangeSpeed;
    private double _elapsedTime;
    private bool _done;
    

    protected void Awake()
    {
        // Set the framerate so that the time between each frame is the same
        Time.fixedDeltaTime = 1f / FPS;
    }

    private void Start() 
    {
        Target.transform.localScale = new Vector3(TargetRadius, TargetRadius, TargetRadius);
        Subject.transform.localScale = new Vector3(SubjectRadius, SubjectRadius, SubjectRadius);

        // Reset(_random.Next(0, TargetInitSpeedList.Length), _random.Next(0, ApproachAngleList.Length));
    }

    private void FixedUpdate() 
    {
        if (_done) return;
        Step(_subjectSpeed);
    }

    private void Update()
    {
        GetInput();
        SetPositions();
        RenderUI();
    }
    private void Reset(float approachAngle, float subjectDistance, float targetDistance,
                        float targetInitSpeed, float targetFinalSpeed, float timeToChangeSpeed)
    {
        _approachAngle = approachAngle;
        _subjectDistance = subjectDistance;
        _targetDistance = targetDistance;
        _targetSpeed = targetInitSpeed;
        _targetFinalSpeed = targetFinalSpeed;
        _timeToChangeSpeed = timeToChangeSpeed;
        // _timeToChangeSpeed = Random.Range(MinTimeToChangeSpeed, MaxTimeToChangeSpeed);
        // _hasChangedSpeed = false;
        // _targetDistance = TargetInitDistance;
        // _targetSpeed = TargetInitSpeedList[targetSpeedIndex];
        // _approachAngle = ApproachAngleList[approachAngleIndex];
        // _subjectDistance = Random.Range(SubjectMinInitDistance, SubjectMaxInitDistance);
        _subjectSpeed = SubjectSpeeds.Min();
        _elapsedTime = 0d;
        _done = false;
    }

    private void Step(float subjectSpeed)
    {
        _subjectDistance -= subjectSpeed / FPS;
        _targetDistance -= _targetSpeed / FPS;

        _elapsedTime += 1d / FPS;
        if (!_hasChangedSpeed && _elapsedTime >= _timeToChangeSpeed) 
        {
            _hasChangedSpeed = true;
            _targetSpeed = _targetFinalSpeed;
            // _targetSpeed = Mathf.Max(TargetMinSpeed, Mathf.Min(TargetMaxSpeed, (float)RandomNormal(TargetSpeedMean, TargetSpeedStd)));
        }
        
        float targetSubjectDistance = Mathf.Sqrt(Mathf.Pow(_targetDistance, 2) + Mathf.Pow(_subjectDistance, 2) - 
                                                    2 * _targetDistance * _subjectDistance * Mathf.Cos(_approachAngle * Mathf.PI / 180));
        _done = _subjectDistance < -SubjectRadius || _targetDistance < -TargetRadius || targetSubjectDistance < (TargetRadius + SubjectRadius);
    }

    private void SetPositions()
    {
        Target.transform.position = new Vector3(-_targetDistance, TargetRadius/2, 0);
        Subject.transform.position = new Vector3(
            -(float)System.Math.Cos(_approachAngle * Mathf.PI / 180) * _subjectDistance, 
            SubjectRadius/2, 
            (float)System.Math.Sin(_approachAngle * Mathf.PI / 180) * _subjectDistance);
        var cameraRot = new Quaternion();
        cameraRot.eulerAngles = new Vector3(0, 90 + _approachAngle, 0);
        Subject.transform.rotation = cameraRot;
        
        Path.transform.rotation = cameraRot;
    }

    private void GetInput()
    {
        for (int i = 1; i <= SubjectSpeeds.Length && i < 10; i++)
        {
            if (Input.GetKey(i.ToString()))
            {
                _subjectSpeed = SubjectSpeeds[i-1];
            }
        }
    }

    private void RenderUI()
    {
        StateDisplay.text = $"Target Distance: {_targetDistance}\nTarget Speed: {_targetSpeed}\nHas Changed Speed: {_hasChangedSpeed}\nSubject Distance: {_subjectDistance}\nSubject Speed: {_subjectSpeed}";
    }

    private double RandomNormal(double mean, double stdDev)
    {
        double u1 = 1.0-_random.NextDouble();
        double u2 = 1.0-_random.NextDouble();
        double randStdNormal = System.Math.Sqrt(-2.0f * System.Math.Log(u1)) * System.Math.Sin(2.0f * Mathf.PI * u2);
        return mean + stdDev * randStdNormal;
    }
}
