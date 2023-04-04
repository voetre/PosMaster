using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using PosMaster.Windows;
using Lumina.Excel.GeneratedSheets;
using System;

namespace PosMaster
{
    public class PosMaster : IDalamudPlugin
    {
        public string Name => "Position Master";
        private const string MainCommand = "/posmaster";
        private const string NudgeFCommand = "/nudgef";
        private const string NudgeUCommand = "/nudgeu";
        private const string SetPosCommand = "/setpos";

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
                HelpMessage = "Load a saved position by name"
            });

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            DalamudApi.Initialize(this, pluginInterface);
            PosEdit.Initialize();

            MainWindow.IsOpen = true;
        }

        public void Dispose()
        {
            WindowSystem.RemoveAllWindows();
            
            MainWindow.Dispose();
            
            CommandManager.RemoveHandler(MainCommand);
            CommandManager.RemoveHandler(NudgeFCommand);
            CommandManager.RemoveHandler(NudgeUCommand);
            CommandManager.RemoveHandler(SetPosCommand);
        }

        private void OpenPosMaster(string command, string args)
        {
            MainWindow.IsOpen = true;
        }

        private void SetPosCommandHandler(string command, string args)
        {
            PosEdit.LoadAndSetPos(args);
        }

        private void NudgeForwardCommandHandler(string command, string args)
        {
            try
            {
                PosEdit.NudgeForward(Convert.ToInt32(args));
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
                PosEdit.NudgeUp(Convert.ToInt32(args));
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
