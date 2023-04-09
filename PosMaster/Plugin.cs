using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using PosMaster.Windows;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Text.RegularExpressions;

namespace PosMaster
{
    public class PosMaster : IDalamudPlugin
    {
        public string Name => "Position Master";
        private const string MainCommand = "/posmaster";
        private const string NudgeFCommand = "/nudgef";
        private const string NudgeUCommand = "/nudgeu";
        private const string SetPosCommand = "/setpos";
        private const string LoadPosCommand = "/loadpos";
        private const string SetSpeedCommand = "/setspeed";

        public PosEdit position = new PosEdit();

        public static void PrintEcho(string message) => DalamudApi.ChatGui.Print($"[PosMaster] {message}");
        public static void PrintError(string message) => DalamudApi.ChatGui.PrintError($"[PosMaster] {message}");

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        public Configuration PosMasterConfiguration { get; init; }

        public static WindowSystem WindowSystem = new("PosMaster");

        public Configuration PosMasterConfig { get; init; }
        private MainWindow MainWindow { get; init; }
        public static PosMaster Plugin { get; private set; }

        public PosMaster(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {

            PluginInterface = pluginInterface;
            CommandManager = commandManager;

            PosMasterConfiguration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            PosMasterConfiguration.Initialize(PluginInterface);
            MainWindow = new MainWindow(this);
            Plugin = this;

            WindowSystem.AddWindow(MainWindow);

            CommandManager.AddHandler(MainCommand, new CommandInfo(OpenPosMaster)
            {
                HelpMessage = "Opens the main PosMaster GUI."
            });

            CommandManager.AddHandler(NudgeFCommand, new CommandInfo(NudgeForwardCommandHandler)
            {
                HelpMessage = "nudge forward"
            });

            CommandManager.AddHandler(NudgeUCommand, new CommandInfo(NudgeUpwardCommandHandler)
            {
                HelpMessage = "nudge upward"
            });

            CommandManager.AddHandler(SetPosCommand, new CommandInfo(SetPosCommandHandler)
            {
                HelpMessage = "set your position using values provided with command args \n example: /setpos 100 50 5"
            });

            CommandManager.AddHandler(LoadPosCommand, new CommandInfo(LoadPosCommandHandler)
            {
                HelpMessage = "Load a saved position by name"
            });

            CommandManager.AddHandler(SetSpeedCommand, new CommandInfo(SetSpeedCommandHandler)
            {
                HelpMessage = "Speed hack, multiplier as arg"
            });

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            DalamudApi.Initialize(this, pluginInterface);
            PosEdit.Initialize();
        }

        public void Dispose()
        {
            WindowSystem.RemoveAllWindows();
            
            MainWindow.Dispose();
            
            CommandManager.RemoveHandler(MainCommand);
            CommandManager.RemoveHandler(NudgeFCommand);
            CommandManager.RemoveHandler(NudgeUCommand);
            CommandManager.RemoveHandler(SetPosCommand);
            CommandManager.RemoveHandler(LoadPosCommand);
        }

        private void OpenPosMaster(string command, string args)
        {
            MainWindow.IsOpen = true;
        }

        private void LoadPosCommandHandler(string command, string args)
        {
            PosEdit.LoadAndSetPos(args);
        }

        private void SetPosCommandHandler(string command, string args)
        {
            Match match = Regex.Match(args, "^([-+]?[0-9]*\\.?[0-9]+) ([-+]?[0-9]*\\.?[0-9]+) ([-+]?[0-9]*\\.?[0-9]+)$");
            if (match.Success)
            {
                float result1;
                float.TryParse(match.Groups[1].Value, out result1);
                float result2;
                float.TryParse(match.Groups[2].Value, out result2);
                float result3;
                float.TryParse(match.Groups[3].Value, out result3);
                PosEdit.SetPos(result1, result2, result3);
            }
            else
            {
                PrintError($"error with arg '{args}'.");
            }
        }

        private void NudgeForwardCommandHandler(string command, string args)
        {
            try
            {
                PosEdit.NudgeForward(int.Parse(args));
            }
            catch
            {
                PrintError($"error with arg '{args}'. is it a number?");
            }
        }

        private void NudgeUpwardCommandHandler(string command, string args)
        {
            try
            {
                PosEdit.NudgeUp(int.Parse(args));
            }
            catch
            {
                PrintError($"error with arg '{args}'. is it a number?");
            }
        }

        private void SetSpeedCommandHandler(string command, string args)
        {
            try
            {
                PosEdit.SetSpeed(float.Parse(args));
            }
            catch
            {
                PrintError($"error with arg '{args}'. is it a number?");
            }
        }
        private void DrawUI()
        {
            WindowSystem.Draw();
        }
        public static string GetZoneName(uint RowID)
        {
            var Territory = DalamudApi.DataManager.GetExcelSheet<PlaceName>()?.GetRow(RowID);
            return Convert.ToString(Territory!.Name) ?? "Null";
        }

        public void DrawConfigUI()
        {
            MainWindow.IsOpen = true;
        }
    }
}
