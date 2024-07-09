using Dalamud.Configuration;
using System;
using System.ComponentModel;

namespace Sgtcatt.SkipCutscene;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    [DefaultValue(true)]
    public bool IsEnabled { get; set; }

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        SkipCutscene.Interface.SavePluginConfig(this);
    }
}
