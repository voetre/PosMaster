using Dalamud.Configuration;
using Dalamud.Plugin;
using System.Numerics;
using System;
using System.Collections.Generic;

namespace PosMaster
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public class PositionList
        {
            public string Name { get; set; }
            public uint Zone { get; set; }
            public Vector3 Position { get; set; }
            public bool IgnoreZone { get; set; }
        }

        public int Version { get; set; } = 0;

        public List<PositionList> SavedPositions { get; set; } = new List<PositionList>();

        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private DalamudPluginInterface? PluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.PluginInterface!.SavePluginConfig(this);
        }
    }
}
