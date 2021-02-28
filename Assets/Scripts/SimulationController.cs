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
    public float[] SubjectSpeeds = {2.0f, 4.0f, 8.0f, 10.0f, 12.0f, 14.0f};
    public float SubjectRadius = 0.35f;
    public float TargetRadius = 0.35f;

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
    private void BeginTrial(Trial t)
    {
        Settings s = t.settings;
        _approachAngle = s.GetFloat("approachAngle");
        _subjectDistance = s.GetFloat("subjectDistance");
        _targetDistance = s.GetFloat("targetDistance");
        _targetSpeed = s.GetFloat("targetInitSpeed");
        _targetFinalSpeed = s.GetFloat("targetFinalSpeed");
        _timeToChangeSpeed = s.GetFloat("timeToChangeSpeed");

        _subjectSpeed = SubjectSpeeds.Min();
        _elapsedTime = 0d;
        _hasChangedSpeed = false;
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
}
