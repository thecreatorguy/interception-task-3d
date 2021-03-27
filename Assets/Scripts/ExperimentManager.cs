using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UXF;

public class ExperimentManager : MonoBehaviour
{
    public Camera BackgroundCamera;
    public Camera MainCamera;
    public TextMeshProUGUI Display;
    private TrialController tc;
    private InterceptionEnvironment env;
    private static float waitTime = 0.8f;

    // A no seed RNG
    //TODO- maybe add the ability to seed it later? Look with Gabe
    private System.Random _random = new System.Random();

    private void Start()
    {
        tc = GameObject.Find("TrialController").GetComponent<TrialController>();
        env = GameObject.Find("Environment").GetComponent<InterceptionEnvironment>();
        Display.text = "";
    }

    public void Generate()
    {
        Session sess = Session.instance;

        // Extract the settings
        Settings s = sess.settings;
        List<float> approachAngles   = s.GetFloatList("approachAngles");
        float subjectInitDistanceMin = s.GetFloat("subjectInitDistanceMin");
        float subjectInitDistanceMax = s.GetFloat("subjectInitDistanceMax");
        float targetInitDistance     = s.GetFloat("targetInitDistance");
        List<float> targetInitSpeeds = s.GetFloatList("targetInitSpeeds");
        float timeToChangeSpeedMin   = s.GetFloat("timeToChangeSpeedMin");
        float timeToChangeSpeedMax   = s.GetFloat("timeToChangeSpeedMax");
        float targetSpeedMin         = s.GetFloat("targetSpeedMin");
        float targetSpeedMax         = s.GetFloat("targetSpeedMax");
        float targetSpeedMean        = s.GetFloat("targetSpeedMean");
        float targetSpeedStdDev      = s.GetFloat("targetSpeedStdDev");

        int trialRepetitionsPerBlock = s.GetInt("trialRepetitionsPerBlock");
        int blocksPerSession         = s.GetInt("blocksPerSession");

        // Create practice block
        Block b = sess.CreateBlock();
        foreach (float approachAngle in approachAngles) 
        {
            foreach (float targetInitSpeed in targetInitSpeeds) 
            { 
                Trial t = b.CreateTrial();
                t.settings.SetValue("approachAngle", approachAngle);
                t.settings.SetValue("subjectInitDistance", Random.Range(subjectInitDistanceMin, subjectInitDistanceMax)); //? different random?
                t.settings.SetValue("targetInitSpeed", targetInitSpeed);
                t.settings.SetValue("timeToChangeSpeed", Random.Range(timeToChangeSpeedMin, timeToChangeSpeedMax)); //?
                t.settings.SetValue("targetFinalSpeed", 
                        Mathf.Max(targetSpeedMin, Mathf.Min(targetSpeedMax, (float)RandomNormal(targetSpeedMean, targetSpeedStdDev)))); //?
            }
        }
        b.trials.Shuffle(); 

        // Create randomized trials
        for (int blockNum = 0; blockNum < blocksPerSession; blockNum++) 
        {
            b = sess.CreateBlock();
            foreach (float approachAngle in approachAngles) 
            {
                foreach (float targetInitSpeed in targetInitSpeeds) 
                {
                    for (int repetition = 0; repetition < trialRepetitionsPerBlock; repetition++) 
                    {
                        Trial t = b.CreateTrial();
                        t.settings.SetValue("approachAngle", approachAngle);
                        t.settings.SetValue("subjectInitDistance", Random.Range(subjectInitDistanceMin, subjectInitDistanceMax)); //? different random?
                        t.settings.SetValue("targetInitSpeed", targetInitSpeed);
                        t.settings.SetValue("timeToChangeSpeed", Random.Range(timeToChangeSpeedMin, timeToChangeSpeedMax)); //?
                        t.settings.SetValue("targetFinalSpeed", 
                                Mathf.Max(targetSpeedMin, Mathf.Min(targetSpeedMax, (float)RandomNormal(targetSpeedMean, targetSpeedStdDev)))); //?
                    }
                }
            }
            b.trials.Shuffle();
        }
        
        // Cursor.visible = false;
        GameObject.Find("BackgroundCamera").GetComponent<Camera>().enabled = false;
        GameObject.Find("MainCamera").GetComponent<Camera>().enabled = true;
        tc.SetupNextTrial(Session.instance.NextTrial);
        StartCoroutine(CountdownToBegin(Session.instance.FirstTrial, "Practice"));
    }

    public void TrialEnded(Trial t)
    {
        if (t.Equals(Session.instance.LastTrial)) {
            Session.instance.End();
        } else {
            tc.SetupNextTrial(Session.instance.NextTrial);
            string blockLabel = null;
            if (Session.instance.NextTrial.block != t.block) {
                blockLabel = t.block.number.ToString();
            }
            StartCoroutine(CountdownToBegin(Session.instance.NextTrial, blockLabel));
        }
    }

    private IEnumerator CountdownToBegin(Trial t, string blockLabel = null)
    {
        Display.text = "";
        if (blockLabel != null) 
        {
            Display.text += $"Starting Block {blockLabel}\n";
        }
        if (!t.Equals(Session.instance.FirstTrial)) 
        {
            if (env.WonPrevious) 
            {
                Display.text += "Target Intercepted!\n";
            } 
            else 
            {
                Display.text += "Target Not Intercepted\n";
            }
        }
        Display.text += "Press 'A' To Start Trial";
        
        while (!Input.GetKeyDown(KeyCode.Joystick1Button0)) {
            yield return null;
        }
        Display.text = "3";
        yield return new WaitForSeconds(waitTime);
        Display.text = "2";
        yield return new WaitForSeconds(waitTime);
        Display.text = "1";
        yield return new WaitForSeconds(waitTime);
        Display.text = "";
        Session.instance.BeginNextTrial();
    }

    public void SessionOver()
    {
        Display.text = "";
        if (env.WonPrevious) 
        {
            Display.text += "Target Intercepted!\n";
        } 
        else 
        {
            Display.text += "Target Not Intercepted\n";
        }
        Display.text += "Final Trial Finished";
    }

    private double RandomNormal(double mean, double stdDev)
    {
        double u1 = 1.0-_random.NextDouble();
        double u2 = 1.0-_random.NextDouble();
        double randStdNormal = System.Math.Sqrt(-2.0f * System.Math.Log(u1)) * System.Math.Sin(2.0f * Mathf.PI * u2);
        return mean + stdDev * randStdNormal;
    }
}
