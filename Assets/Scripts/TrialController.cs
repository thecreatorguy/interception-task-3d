using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UXF;

public class TrialController : MonoBehaviour
{
    // Objects to manipulate
    public GameObject Subject;
    public GameObject SubjectRoad;
    public GameObject Target;
    public GameObject TargetRoad;
    public TextMeshProUGUI StateDisplay;

    // Experiment settings, should be constant across all trials
    public int FPS = 30;
    public float[] SubjectSpeeds = {2.0f, 4.0f, 8.0f, 10.0f, 12.0f, 14.0f};
    public float SubjectRadius = 0.35f;
    public float TargetRadius = 0.35f;
    public bool RoadsEnabled = true;
    public bool DisplayStats = false;

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

    private void Start() 
    {
        // Write all experiment wide settings
        Time.fixedDeltaTime = 1f / FPS;
        Subject.transform.localScale = new Vector3(SubjectRadius, SubjectRadius, SubjectRadius);
        SubjectRoad.SetActive(RoadsEnabled);
        Target.transform.localScale = new Vector3(TargetRadius, TargetRadius, TargetRadius);
        TargetRoad.SetActive(RoadsEnabled);
        StateDisplay.alpha = DisplayStats ? 1 : 0;
    }

    private void FixedUpdate() // constant distance between calls
    {
        if (!_active) return;
        Step(_subjectSpeed);
    }

    private void Update() // Tied to frame renders
    {
        GetInput();
        SetPositions();
        RenderUI();
    }
    public void BeginTrial(Trial t)
    {
        Settings s = t.settings;
        Reset(
            s.GetFloat("approachAngle"),
            s.GetFloat("subjectInitDistance"),
            s.GetFloat("targetInitDistance"),
            s.GetFloat("targetInitSpeed"),
            s.GetFloat("targetFinalSpeed"),
            s.GetFloat("timeToChangeSpeed")
        );
        StartCoroutine(CountdownToBegin());
    }

    private IEnumerator CountdownToBegin()
    {
        yield return new WaitForSeconds(1f);
        _active = true;
    }

    public void Reset(float approachAngle, float subjectInitDistance, float targetInitDistance, float targetInitSpeed,
                        float targetFinalSpeed, float timeToChangeSpeed)
    {
        _approachAngle = approachAngle;
        _subjectDistance = subjectInitDistance;
        _targetDistance = targetInitDistance;
        _targetSpeed = targetInitSpeed;
        _targetFinalSpeed = targetFinalSpeed;
        _timeToChangeSpeed = timeToChangeSpeed;
        _subjectSpeed = SubjectSpeeds.Min();
        _elapsedTime = 0d;
        _hasChangedSpeed = false;
        _recordedState = new List<object>();
        RecordCurrentState();
    }

    private void Step(float subjectSpeed)
    {
        _subjectDistance -= subjectSpeed * (1 / FPS); // Time.timeDelta
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
        // self.state = (target_dis, target_speed, has_changed_speed, subject_dis, subject_speed)

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
        
        SubjectRoad.transform.rotation = cameraRot;
    }

    private void GetInput()
    {
        if (!_active) return;
        
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
