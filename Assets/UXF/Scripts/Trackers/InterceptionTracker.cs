﻿using UnityEngine;

namespace UXF
{
    /// <summary>
    /// Attach this component to any gameobject and assign it in the trackedObjects field in an ExperimentSession to automatically record the position of the mouse in screen coordinates. 
    /// Note (0, 0) is the bottom-left of the window displaying the game.
    /// </summary>
    public class InterceptionTracker : Tracker
    {
        /// <summary>
        /// Sets measurementDescriptor and customHeader to appropriate values
        /// </summary>
        protected override void SetupDescriptorAndHeader()
        {
            measurementDescriptor = "interception";
            
            customHeader = new string[]
            {
                "delta_time",
                "subject_speed",
                "subject_distance",
                "has_changed_speed",
                "target_speed",
                "target_distance"
            };
        }

        /// <summary>
        /// Returns the state required for machine learning
        /// TODO what data do we want to record?
        /// </summary>
        /// <returns></returns>
        protected override UXFDataRow GetCurrentValues()
        {
            TrialController tm = gameObject.GetComponent<TrialController>();

            string format = "0.####";
            var values = new UXFDataRow()
            {
                ("delta_time", Time.deltaTime.ToString(format)),
                ("subject_speed", tm.SubjectSpeed.ToString(format)),
                ("subject_distance", tm.SubjectDistance.ToString(format)),
                ("has_changed_speed", tm.HasChangedSpeed),
                ("target_speed", tm.TargetSpeed.ToString(format)),
                ("target_distance", tm.TargetDistance.ToString(format))
            };

            return values;
        }
    }
}