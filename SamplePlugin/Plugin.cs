using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Game;
using System.Diagnostics;
using System;
using System.Security.Cryptography;
using Dalamud;

namespace Sgtcatt.SkipCutscene;

public sealed class SkipCutscene : IDalamudPlugin
{
    public string Name => "SkipCutscene";

    [PluginService] internal static IDalamudPluginInterface Interface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] public static IPluginLog PluginLog { get; private set; }
    [PluginService] public static IChatGui ChatGui { get; private set; }
    [PluginService] public static ISigScanner SigScanner { get; private set; }

    private const string CommandName = "/sc";

    private readonly RandomNumberGenerator _csp;

    private readonly decimal _base = uint.MaxValue;

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("Skip-Cutscene");

    public CutsceneAddressResolver Address { get; }

    public void SetEnabled(bool isEnable)
    {
        if (!Address.Valid) return;
        if (isEnable)
        {
            SafeMemory.Write<short>(Address.Offset1, -28528);
            SafeMemory.Write<short>(Address.Offset2, -28528);
        }
        else
        {
            SafeMemory.Write<short>(Address.Offset1, 13173);
            SafeMemory.Write<short>(Address.Offset2, 6260);
        }
    }

    public SkipCutscene()
    {
        Configuration = Interface.GetPluginConfig() as Configuration ?? new Configuration();

        Address = new CutsceneAddressResolver();

        Address.Setup(SigScanner);

        if (Address.Valid)
        {
            PluginLog.Information("Cutscene Offset Found.");
            if (Configuration.IsEnabled)
                SetEnabled(true);
        }
        else
        {
            PluginLog.Error("Cutscene Offset Not Found.");
            PluginLog.Warning("Plugin Disabling...");
            Dispose();
            return;
        }

        _csp = RandomNumberGenerator.Create();

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Roll your sanity check dice."
        });
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        SetEnabled(false);
        GC.SuppressFinalize(this);

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        if (command.ToLower() != "/sc") return;
        byte[] rndSeries = new byte[4];
        _csp.GetBytes(rndSeries);
        int rnd = (int)Math.Abs(BitConverter.ToUInt32(rndSeries, 0) / _base * 50 + 1);
        ChatGui.Print(Configuration.IsEnabled
            ? $"sancheck: 1d100={rnd + 50}, Failed"
            : $"sancheck: 1d100={rnd}, Passed");
        Configuration.IsEnabled = !Configuration.IsEnabled;
        SetEnabled(Configuration.IsEnabled);
        Interface.SavePluginConfig(Configuration);
    }

    public class CutsceneAddressResolver : BaseAddressResolver
    {

        public bool Valid => Offset1 != IntPtr.Zero && Offset2 != IntPtr.Zero;

        public IntPtr Offset1 { get; private set; }
        public IntPtr Offset2 { get; private set; }

        protected override void Setup64Bit(ISigScanner sig)
        {
            Offset1 = sig.ScanText("75 33 48 8B 0D ?? ?? ?? ?? BA ?? 00 00 00 48 83 C1 10 E8 ?? ?? ?? ?? 83 78");
            Offset2 = sig.ScanText("74 18 8B D7 48 8D 0D");
            SkipCutscene.PluginLog.Information(
                "Offset1: [\"ffxiv_dx11.exe\"+{0}]",
                (Offset1.ToInt64() - Process.GetCurrentProcess().MainModule!.BaseAddress.ToInt64()).ToString("X")
                );
            SkipCutscene.PluginLog.Information(
                "Offset2: [\"ffxiv_dx11.exe\"+{0}]",
                (Offset2.ToInt64() - Process.GetCurrentProcess().MainModule!.BaseAddress.ToInt64()).ToString("X")
                );
        }

    }
}
