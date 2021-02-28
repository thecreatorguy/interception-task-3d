using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UXF;

public class TrialController : MonoBehaviour
{
    // self.state = (target_dis, target_speed, has_changed_speed, subject_dis, subject_speed)
    class State {
        public float targetDistance;
        public float targetSpeed;
        public bool hasChangedSpeed;
        public float subjectDistance;
        public float subjectSpeed;

        // public State(float targetDistance, float targetSpeed, bool hasChangedSpeed, float subjectDistance, float subjectSpeed) {
        //     this.targetDistance = targetDistance;
        //     this.targetSpeed = targetSpeed;
        //     this.hasChangedSpeed = hasChangedSpeed;
        //     this.subjectDistance = subjectDistance;
        //     this.subjectSpeed = subjectSpeed;
        // }
    }

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
    private bool _active = false;
    private List<object> _recordedState;

    protected void Awake()
    {
        Time.fixedDeltaTime = 1f / FPS;
    }

    private void Start() 
    {
        Target.transform.localScale = new Vector3(TargetRadius, TargetRadius, TargetRadius);
        Subject.transform.localScale = new Vector3(SubjectRadius, SubjectRadius, SubjectRadius);
    }

    private void FixedUpdate() 
    {
        if (!_active) return;
        Step(_subjectSpeed);
    }

    private void Update()
    {
        GetInput();
        SetPositions();
        RenderUI();
    }
    public void BeginTrial(Trial t)
    {
        Settings s = t.settings;
        _approachAngle = s.GetFloat("approachAngle");
        _subjectDistance = s.GetFloat("subjectInitDistance");
        _targetDistance = s.GetFloat("targetInitDistance");
        _targetSpeed = s.GetFloat("targetInitSpeed");
        _targetFinalSpeed = s.GetFloat("targetFinalSpeed");
        _timeToChangeSpeed = s.GetFloat("timeToChangeSpeed");

        _subjectSpeed = SubjectSpeeds.Min();
        _elapsedTime = 0d;
        _hasChangedSpeed = false;
        _active = true;
        _recordedState = new List<object>();
        RecordCurrentState();
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
        bool won = targetSubjectDistance < (TargetRadius + SubjectRadius);
        _active = !(_subjectDistance < -SubjectRadius*2 || _targetDistance < -TargetRadius*2 || won);

        RecordCurrentState();
        
        if (!_active) {
            Session.instance.CurrentTrial.SaveJSONSerializableObject(_recordedState, won ? "state_data_won" : "state_data_lost");
            Session.instance.EndCurrentTrial();
        }
    }

    private void RecordCurrentState() {
        // Dictionary form
        _recordedState.Add(new Dictionary<string, object>(){
            ["target_distance"]   = _targetDistance, 
            ["target_speed"]      = _targetSpeed,
            ["has_changed_speed"] = _hasChangedSpeed,
            ["subject_distance"]  = _subjectDistance,
            ["subject_speed"]     = _subjectSpeed
        });

        // List form
        // _recordedState.Add(new List<object>(){_targetDistance, _targetSpeed, _hasChangedSpeed, _subjectDistance, _subjectSpeed});
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
