using UnityEngine;

namespace VaguelyRealistic.Resources
{
    public class IgniterResource : IConfigNode
    {
        [SerializeField]
        public int Ignitions;

        public IgniterResource()
        {
        }

        public void Load(ConfigNode node)
        {
            string value = node.GetValue(nameof(Ignitions));

            Ignitions = int.Parse(value);
        }

        public void Save(ConfigNode node)
        {
            node.AddValue(nameof(Ignitions), Ignitions);
        }
    }
}