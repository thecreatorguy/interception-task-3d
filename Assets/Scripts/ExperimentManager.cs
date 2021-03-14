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
    private static float waitTime = 0.8f;

    private TrialController tc;
    private bool aiPlayerEnabled = false;
    

    // A no seed RNG
    //TODO- maybe add the ability to seed it later? Look with Gabe
    private System.Random _random = new System.Random();

    private void Start()
    {
        Display.text = "";
        tc = GameObject.Find("TrialController").GetComponent<TrialController>();
    }

    public void Generate()
    {
        // Generate the session
        Session sess = Session.instance;

        // Add participant detail settings
        Settings s = sess.settings;
        object temp;
        Session.instance.participantDetails.TryGetValue("aiPlayerEnabled", out temp);
        aiPlayerEnabled = (bool)temp;
        s.SetValue("aiPlayerEnabled", aiPlayerEnabled);
        Session.instance.participantDetails.TryGetValue("shell", out temp);
        s.SetValue("shell", (string)temp);
        Session.instance.participantDetails.TryGetValue("aiCommand", out temp);
        s.SetValue("aiCommand", (string)temp);

        // Extract the settings used for generating trials
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
        
        if (!aiPlayerEnabled) Cursor.visible = false;
        BackgroundCamera.enabled = false;
        MainCamera.enabled = true;
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
        if (!aiPlayerEnabled) 
        {
            Display.text = "";
            if (blockLabel != null) 
            {
                Display.text += $"Starting Block {blockLabel}\n";
            }
            if (!t.Equals(Session.instance.FirstTrial)) 
            {
                if (tc.WonPrevious) 
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
        }
        Session.instance.BeginNextTrial();
    }

    public void SessionOver()
    {
        StartCoroutine(WaitToExit());
    }
    private IEnumerator WaitToExit()
    {   
        if (!aiPlayerEnabled) 
        {
            Display.text = "";
            if (tc.WonPrevious) 
            {
                Display.text += "Target Intercepted!\n";
            } 
            else 
            {
                Display.text += "Target Not Intercepted\n";
            }
            Display.text += "Final Trial Finished\nPress 'A' To Exit";
            while (!Input.GetKeyDown(KeyCode.Joystick1Button0)) {
                yield return null;
            }
        }
        Display.text = "Exiting...";
        Application.Quit();
    }

    private double RandomNormal(double mean, double stdDev)
    {
        double u1 = 1.0-_random.NextDouble();
        double u2 = 1.0-_random.NextDouble();
        double randStdNormal = System.Math.Sqrt(-2.0f * System.Math.Log(u1)) * System.Math.Sin(2.0f * Mathf.PI * u2);
        return mean + stdDev * randStdNormal;
    }
}
