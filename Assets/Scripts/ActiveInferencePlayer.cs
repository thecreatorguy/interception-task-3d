using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ActiveInferencePlayer
{
    [System.Serializable]
    public class ModelState 
    {
        public float subjectSpeed;
        public float subjectDistance;
        public bool hasChangedSpeed;
        public float targetSpeed;
        public float targetDistance;

        public ModelState(float ss, float sd, bool hcs, float ts, float td)
        {
            subjectSpeed = ss;
            subjectDistance = sd;
            hasChangedSpeed = hcs;
            targetSpeed = ts;
            targetDistance = td;
        }
    }

    [System.Serializable]
    public class ModelResult
    {
        public float desiredSpeed;
    }

    private System.Diagnostics.Process model;
    private StreamWriter modelIn;
    private StreamReader modelOut;

    public ActiveInferencePlayer(string shell, string command)
    {
        model = new System.Diagnostics.Process();
        model.StartInfo.UseShellExecute = false;
        model.StartInfo.CreateNoWindow = true;
        model.StartInfo.RedirectStandardInput = true;
        model.StartInfo.RedirectStandardOutput = true;
        model.StartInfo.FileName = shell;
        model.StartInfo.Arguments = command;
        model.Start();

        modelIn = model.StandardInput;
        modelOut = model.StandardOutput;
    }

    public float Step(float ss, float sd, bool hcs, float ts, float td)
    {
        modelIn.WriteLine(JsonUtility.ToJson(new ModelState(ss, sd, hcs, ts, td)));
        return JsonUtility.FromJson<ModelResult>(modelOut.ReadLine()).desiredSpeed;
    }

    public void Cleanup()
    {
        modelIn.Close();
        modelOut.Close();
        model.Close();
    }
}
