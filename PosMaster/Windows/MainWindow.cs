using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace PosMaster.Windows;

public class MainWindow : Window, IDisposable
{
    private PosMaster Plugin;
    private PosEdit pos = new PosEdit();
    private Configuration configuration;
    private float X;
    private float Y;
    private float Z;
    private string PosNameInput = string.Empty;
    private bool IgnoreZone = false;
    private float x = 0;
    private float y = 0;
    private float z = 0;

    float SliderX = 0.0f;
    float SliderY = 0.0f;
    float SliderZ = 0.0f;
    public MainWindow(PosMaster plugin) : base(
        "Position Master", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysAutoResize)
    {
        configuration = plugin.PosMasterConfiguration;
        this.Size = new Vector2(600, 375);
    }

    public void Dispose()
    {
    }

    public override void Draw()
    {
        if (!PosEdit.Ready) return;
        var territory = DalamudApi.DataManager.GetExcelSheet<TerritoryType>()!.GetRow(DalamudApi.ClientState.TerritoryType);
        x = PosEdit.X;
        y = PosEdit.Y;
        z = PosEdit.Z;

        if (ImGui.CollapsingHeader("Current Position"))
        {
            ImGui.PushItemWidth(185);
            if (ImGui.InputFloat("##InputX", ref x))
            {
                PosEdit.X = x;
            }

            ImGui.SameLine();
            ImGui.PushItemWidth(185);
            if (ImGui.InputFloat("##InputY", ref y))
            {
                PosEdit.Y = y;
            }

            ImGui.SameLine();
            ImGui.PushItemWidth(185);
            if (ImGui.InputFloat("##InputZ", ref z))
            {
                PosEdit.Z = z;
            }

            ImGui.PushItemWidth(185);
            if (ImGui.SliderFloat("##PlayerX", ref SliderX, -0.5f, 0.5f))
            {
                PosEdit.X = PosEdit.X + SliderX;
                SliderX = 0.0f;
            }

            ImGui.SameLine();
            ImGui.PushItemWidth(185);
            if (ImGui.SliderFloat("##PlayerY", ref SliderY, -0.5f, 0.5f))
            {
                PosEdit.Y = PosEdit.Y + SliderY;
                SliderY = 0.0f;
            }

            ImGui.SameLine();
            ImGui.PushItemWidth(185);
            if (ImGui.SliderFloat("##PlayerZ", ref SliderZ, -0.5f, 0.5f))
            {
                PosEdit.Z = PosEdit.Z + SliderZ;
                SliderZ = 0.0f;
            }
        }

        if (ImGui.CollapsingHeader("Position Saver"))
        {
            ImGui.PushID("##PositionSaverIDPush");

            ImGui.Text("Name: ");

            ImGui.SameLine();
            ImGui.PushItemWidth(150);
            ImGui.InputText("##PosName", ref PosNameInput, 50);

            ImGui.SameLine();
            ImGui.Text($"Zone: ({(int)territory.PlaceName.Row }) {PosMaster.GetZoneName(territory.PlaceName.Row)}");

            ImGui.SameLine();
            ImGui.Checkbox("Ignore", ref IgnoreZone);

            ImGui.PushItemWidth(125);
            ImGui.InputFloat("##InputX", ref X);
            ImGui.SameLine();
            ImGui.PushItemWidth(125);
            ImGui.InputFloat("##InputY", ref Y);
            ImGui.SameLine();
            ImGui.PushItemWidth(125);
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
                if (configuration.SavedPositions.Any(x => x.Name == PosNameInput)) return;
                configuration.SavedPositions.Add(new Configuration.PositionList
                {
                    Name = PosNameInput,
                    Position = new Vector3(X, Y, Z),
                    Zone = territory.PlaceName.Row,
                    IgnoreZone = IgnoreZone
                });
                configuration.Save();
            }
            ImGui.PopID();
        }
        if (ImGui.CollapsingHeader("Saved Positions"))
        {
            ImGui.BeginChild("##SavedPositionTableChild");
            if (ImGui.BeginTable("##SavedPositionTable", 5, ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.SizingStretchProp))
            {
                ImGui.PushItemWidth(150);
                ImGui.TableSetupColumn("Name");

                ImGui.PushItemWidth(150);
                ImGui.TableSetupColumn("Zone");

                ImGui.PushItemWidth(150);
                ImGui.TableSetupColumn("Position");

                ImGui.PushItemWidth(150);
                ImGui.TableSetupColumn("IgnoreZone");

                ImGui.PushItemWidth(50);
                ImGui.TableSetupColumn("##Controls");
                ImGui.TableHeadersRow();

                if (configuration.SavedPositions.Count > 0)
                {
                    for (int i = 0; i < configuration.SavedPositions.Count; i++)
                    {

                        PositionTableRow(configuration.SavedPositions[i].Name, (int)configuration.SavedPositions[i].Zone, configuration.SavedPositions[i].Position, configuration.SavedPositions[i].IgnoreZone);
                        ImGui.TableNextRow();
                    }
                }

                void PositionTableRow(
                string Name,
                int Zone,
                Vector3 Position,
                bool IgnoreZone)
                {

                    ImGui.PushID(Name);
                    ImGui.TableNextColumn();
                    ImGui.Text(Convert.ToString(Name));

                    ImGui.TableNextColumn();
                    ImGui.PushTextWrapPos();
                    ImGui.Text($"({Convert.ToString(Zone)}) {PosMaster.GetZoneName((uint)Zone)}");
                    ImGui.PopTextWrapPos();

                    ImGui.TableNextColumn();
                    ImGui.Text($"X: {Position.X}");
                    ImGui.Text($"Y: {Position.Y}");
                    ImGui.Text($"Z: {Position.Z}");

                    ImGui.TableNextColumn();
                    if (ImGui.Checkbox("##IgnoreZone", ref IgnoreZone))
                    {
                        configuration.SavedPositions.Find(x => x.Name == Name)!.IgnoreZone = IgnoreZone;
                    }

                    ImGui.TableNextColumn();
                    if (ImGui.Button($"Set"))
                    {
                        PosEdit.LoadAndSetPos(Name);
                    }
                    if (ImGui.Button($"Delete"))
                    {
                        configuration.SavedPositions.RemoveAll(x => x.Name == Name);
                        configuration.Save();
                    }
                    ImGui.PopID();

                }
                ImGui.EndTable();
            }
        }
        ImGui.EndChild();
    }
}
