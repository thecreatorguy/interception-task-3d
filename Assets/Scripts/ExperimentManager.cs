using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

public class ExperimentManager : MonoBehaviour
{
    // A no seed RNG
    //TODO- maybe add the ability to seed it later? Look with Gabe
    private System.Random _random = new System.Random();

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

        // Create randomized trials
        // TODO: look at this with Gabe
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
        Session.instance.BeginNextTrial();
    }

    public void TrialEnded(Trial t)
    {
        if (t.Equals(t.block.lastTrial)) {
            Session.instance.End();
            // Application.Quit();
        } else {
            StartCoroutine(NextTrialAfterWait());
        }
    }

    private IEnumerator NextTrialAfterWait()
    {
        yield return new WaitForSeconds(1f);
        Session.instance.BeginNextTrial();
    }

    private double RandomNormal(double mean, double stdDev)
    {
        double u1 = 1.0-_random.NextDouble();
        double u2 = 1.0-_random.NextDouble();
        double randStdNormal = System.Math.Sqrt(-2.0f * System.Math.Log(u1)) * System.Math.Sin(2.0f * Mathf.PI * u2);
        return mean + stdDev * randStdNormal;
    }
}
