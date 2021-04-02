using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.SideChannels;
using UXF;

public class ActiveInferenceAgent : Agent
{
    public int FPS = 60;

    private InterceptionEnvironment env;
    private Settings settings;
    private System.Random _random = new System.Random();

    // Start is called before the first frame update
    void Start()
    {
        env = GameObject.Find("Environment").GetComponent<InterceptionEnvironment>();

        string settingsPath = Path.Combine(Application.streamingAssetsPath, "interception_task.json");
        string settingsText;
        try
        {
            settingsText = File.ReadAllText(settingsPath);
        }
        catch (FileNotFoundException e)
        {
            Debug.LogException(e);
            return;
        }
        Dictionary<string, object> deserializedJson = (Dictionary<string, object>)MiniJSON.Json.Deserialize(settingsText);
        settings = new Settings(deserializedJson);
    }
    public override void OnEpisodeBegin()
    {
        List<float> approachAngles   = settings.GetFloatList("approachAngles");
        float subjectInitDistanceMin = settings.GetFloat("subjectInitDistanceMin");
        float subjectInitDistanceMax = settings.GetFloat("subjectInitDistanceMax");
        List<float> targetInitSpeeds = settings.GetFloatList("targetInitSpeeds");
        float timeToChangeSpeedMin   = settings.GetFloat("timeToChangeSpeedMin");
        float timeToChangeSpeedMax   = settings.GetFloat("timeToChangeSpeedMax");
        float targetSpeedMin         = settings.GetFloat("targetSpeedMin");
        float targetSpeedMax         = settings.GetFloat("targetSpeedMax");
        float targetSpeedMean        = settings.GetFloat("targetSpeedMean");
        float targetSpeedStdDev      = settings.GetFloat("targetSpeedStdDev");

        EnvironmentParameters ep = Academy.Instance.EnvironmentParameters;
        float aa = ep.GetWithDefault("approachAngle", approachAngles[Random.Range(0, approachAngles.Count)]);
        settings.SetValue("approachAngle", aa);

        float sid = ep.GetWithDefault("subjectInitDistance", Random.Range(subjectInitDistanceMin, subjectInitDistanceMax));
        settings.SetValue("subjectInitDistance", sid);

        float tis = ep.GetWithDefault("targetInitSpeed", targetInitSpeeds[Random.Range(0, targetInitSpeeds.Count)]);
        settings.SetValue("targetInitSpeed", tis);

        float ttcs = ep.GetWithDefault("timeToChangeSpeed", Random.Range(timeToChangeSpeedMin, timeToChangeSpeedMax));
        settings.SetValue("timeToChangeSpeed", ttcs);

        float tfs = ep.GetWithDefault("targetFinalSpeed", 
            Mathf.Max(targetSpeedMin, Mathf.Min(targetSpeedMax, (float)RandomNormal(targetSpeedMean, targetSpeedStdDev))));
        settings.SetValue("targetFinalSpeed", tfs);
        
        env.Reset(settings);
        env.Render();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(env.TargetDistance);
        sensor.AddObservation(env.TargetSpeed);
        sensor.AddObservation(env.HasChangedSpeed ? 1f : 0f);
        sensor.AddObservation(env.SubjectDistance);
        sensor.AddObservation(env.SubjectSpeed);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        float desiredSpeed = vectorAction[0];

        if (env.Step(1f/FPS, desiredSpeed)) {
            if (env.WonPrevious) {
                SetReward(100);
            }
            EndEpisode();
        }
        env.Render();
    }

    private double RandomNormal(double mean, double stdDev)
    {
        double u1 = 1.0-_random.NextDouble();
        double u2 = 1.0-_random.NextDouble();
        double randStdNormal = System.Math.Sqrt(-2.0f * System.Math.Log(u1)) * System.Math.Sin(2.0f * Mathf.PI * u2);
        return mean + stdDev * randStdNormal;
    }
}
