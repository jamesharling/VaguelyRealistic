using System;
using UnityEngine;

namespace VaguelyRealistic.Modules
{
    public class ModuleEngineIgniter : PartModule
    {
        [KSPField(isPersistant = false)]
        public int ignitionsPermitted;

        [KSPField(isPersistant = true)]
        public int ignitionsUsed;

        private int ignitionsAvailable => this.ignitionsPermitted - this.ignitionsUsed < 0 ? 0 : this.ignitionsPermitted - this.ignitionsUsed;

        public override string GetInfo()
        {
            string info = "This engine can be started ";

            if (this.requiresIgnition)
            {
                info += $"{this.ignitionsPermitted} {this.Pluralise("time", this.ignitionsPermitted)}";
            }
            else
            {
                info += "an unlimited number of times.";
            }

            return info;
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            string info = $"Loaded on {this.part.name}; ";

            if (this.requiresIgnition)
            {
                info += $"{this.ignitionsPermitted} ignitions permitted, {this.ignitionsAvailable} ignitions remaining.";
            }
            else
            {
                info += "infinite ignitions available.";
            }

            this.Log(info);
        }

        public void OnMouseEnter()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                this.isMouseFocus = true;
            }
        }

        public void OnMouseExit()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                this.isMouseFocus = false;
            }
        }

        public override void OnStart(StartState state)
        {
            this.startState = state;

            this.engine = this.part.Modules.GetModule<ModuleEngines>();

            // If we are in the editor, set the available ignitions to max
            if (this.startState == StartState.Editor)
            {
                this.ignitionsUsed = 0;
            }
        }

        public override void OnUpdate()
        {
            // Don't do anything if we are in the editor
            if (startState == StartState.None || startState == StartState.Editor)
            {
                return;
            }

            // If the engine was previously not ignited, is in a start-able state AND the throttle is
            // being operated, we can count this as an attempt at ignition of the engine
            if (this.engineState == EngineIgnitionState.NotIgnited && this.engine.requestedThrottle > 0.0f)
            {
                // If the engine is in a start-able state, use up one ignition count and switch the
                // engine's state
                if (this.isStartable)
                {
                    this.ignitionsUsed++;

                    this.Log($"Igniting {this.engine.name}: {this.ignitionsAvailable} ignitions remaining");

                    this.engineState = EngineIgnitionState.Ignited;
                }

                // If not, we forcibly shutdown the engine
                else
                {
                    string info = $"{this.engine.name} has no ignitions remaining";

                    this.Log(info);

                    // Let's be nice and tell the pilot what's going on...
                    ScreenMessages.PostScreenMessage(info);

                    this.ShutdownEngine();
                }
            }

            // Conversely, if the engine previously was ignited and the throttle has since dropped to
            // 0, the engine is considered to be shutdown
            else if (this.engineState == EngineIgnitionState.Ignited && this.engine.requestedThrottle <= 0.0f)
            {
                this.engineState = EngineIgnitionState.NotIgnited;
            }
        }

        private ModuleEngines engine;

        private EngineIgnitionState engineState = EngineIgnitionState.NotIgnited;

        private bool isMouseFocus;

        private StartState startState = StartState.None;

        private bool isStartable => !this.requiresIgnition || this.ignitionsAvailable > 0;

        private bool requiresIgnition => this.ignitionsPermitted != -1;

        private void Log(string message)
        {
            Debug.Log($"[ModuleEngineIgnitor]: {message}");
        }

        private void OnGUI()
        {
            if (this.isMouseFocus == false)
            {
                return;
            }

            string info = "Igniter: ";

            if (this.requiresIgnition)
            {
                info += $"{this.ignitionsAvailable} ignitions remaining.";
            }
            else
            {
                info += $"Infinite ignitions available.";
            }

            var coords = Camera.main.WorldToScreenPoint(part.transform.position);
            var rect = new Rect(coords.x - 100.0f, Screen.height - coords.y - 30.0f, 200.0f, 20.0f);

            var style = new GUIStyle();
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.red;

            GUI.Label(rect, info, style);
        }

        private string Pluralise(string noun, int count) => noun + (count != 1 ? "s" : String.Empty);

        private void ShutdownEngine()
        {
            //this.Log("ShutdownEngine()");

            this.engine.SetRunningGroupsActive(false);

            foreach (var e in engine.Events)
            {
                if (e.name.IndexOf("shutdown", StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    this.Log($"Shutting down engine {this.engine.name}");

                    e.Invoke();
                }
            }

            this.engine.SetRunningGroupsActive(false);
        }
    }
}