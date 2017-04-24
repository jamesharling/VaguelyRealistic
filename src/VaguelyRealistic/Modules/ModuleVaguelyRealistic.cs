using System;

namespace VaguelyRealistic.Modules
{
    [KSPModule("Vaguely Realistic")]
    public class ModuleVaguelyRealistic : PartModule
    {
        public override string GetInfo() => "Now with enhanced difficulty!";

        public override void OnLoad(ConfigNode node) => base.OnLoad(node);

        public override void OnStart(StartState state) => base.OnStart(state);
    }
}