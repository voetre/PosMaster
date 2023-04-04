using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace PosMaster.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;
    private float X;
    private float Y;
    private float Z;
    private string PosNameInput = string.Empty;
    private bool IgnoreZone = false;
    public ConfigWindow(PosMaster plugin) : base(
        "PosMaster Configurator",
        ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.Size = new Vector2(500, 100);
        this.SizeCondition = ImGuiCond.Always;
        this.Configuration = plugin.PosMasterConfiguration;
    }

    public void Dispose() { }

    public override void Draw()
    {

        var territory = DalamudApi.DataManager.GetExcelSheet<TerritoryType>()!.GetRow(DalamudApi.ClientState.TerritoryType);

        ImGui.Text("Name: ");
        ImGui.SameLine();
        ImGui.PushItemWidth(150);
        ImGui.InputText("##PosName", ref PosNameInput, 50);

        ImGui.SameLine();
        ImGui.Text($"Zone: ({(int)territory.PlaceName.Row}) {PosMaster.GetZoneName(territory.PlaceName.Row)}");

        ImGui.SameLine();
        ImGui.Checkbox("Ignore", ref IgnoreZone);

        ImGui.PushItemWidth(100);
        ImGui.InputFloat("##InputX", ref X);
        ImGui.SameLine();
        ImGui.PushItemWidth(100);
        ImGui.InputFloat("##InputY", ref Y);
        ImGui.SameLine();
        ImGui.PushItemWidth(100);
        ImGui.InputFloat("##InputZ", ref Z);

        ImGui.SameLine();
        if (ImGui.Button("Get Pos"))
        {
            X = PosEdit.X;
            Y = PosEdit.Y;
            Z = PosEdit.Z;
        }

        ImGui.SameLine();
        if (ImGui.Button("Save"))
        {
            if (PosNameInput == string.Empty) return;
            if (Configuration.SavedPositions.Any(x => x.Name == PosNameInput)) return;
            Configuration.SavedPositions.Add(new Configuration.PositionList
            {
                Name = PosNameInput,
                Position = new Vector3(X, Y, Z),
                Zone = territory.PlaceName.Row,
                IgnoreZone = IgnoreZone
            });
            Configuration.Save();
        }

    }
}
