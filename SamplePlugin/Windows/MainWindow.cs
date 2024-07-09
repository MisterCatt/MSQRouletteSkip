using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;

namespace Sgtcatt.SkipCutscene;

[Serializable]
public class MainWindow : Window, IDisposable
{
    private SkipCutscene plugin;
    private Configuration Configuration;
    public MainWindow(SkipCutscene plugin, Configuration config)
        : base("Data window##With a hidden ID", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.plugin = plugin;
        Configuration = config;
    }

    public void Dispose()
    {
    }

    public override void Draw()
    {
        ImGui.Text($"Toggle skip cutscenes");
        var configValue = Configuration.IsEnabled;
        if (ImGui.Checkbox("Random Config Bool", ref configValue))
        {
            Configuration.IsEnabled = configValue;
            // can save immediately on change, if you don't want to provide a "Save and Close" button
            Configuration.Save();
        }
    }
}
