using System.Collections;
using UnityEngine;
using TMPro;
using UXF;

public class TrialController : MonoBehaviour
{

    private InterceptionEnvironment env;
    private float speedMin, speedMax;
    private bool active = false;

    private void Start() {
        env = GameObject.Find("Environment").GetComponent<InterceptionEnvironment>();
    }

    public void SetupNextTrial(Trial t)
    {
        Settings s = t.settings;
        env.Reset(s);
        speedMin = s.GetFloat("subjectSpeedMin");
        speedMax = s.GetFloat("subjectSpeedMax");
    }

    public void BeginTrial(Trial t)
    {
        SetupNextTrial(t);
        active = true;
    }

    private void Update()
    {
        if (active) {
            float desiredSpeed = GetInput();
            bool done = env.Step(Time.deltaTime, desiredSpeed);
            if (done) {
                Session.instance.CurrentTrial.result["intercepted"] = env.WonPrevious;
                Session.instance.EndCurrentTrial();
                active = false;
            }
        }
        env.Render();
    }

    private float GetInput()
    {
        // TODO ask gabe about raw input?
        return Mathf.Max(0, Input.GetAxisRaw("Acceleration")) * (speedMax - speedMin) + speedMin;
    }
}
