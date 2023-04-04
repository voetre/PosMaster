using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Dalamud.Logging;
using Lumina.Excel.GeneratedSheets;

namespace PosMaster
{
    public class PosEdit
    {
        private static Configuration configuration;
        public static bool Ready => DalamudApi.ClientState.LocalPlayer != null;
        public static IntPtr PlayerAddress => DalamudApi.ClientState.LocalPlayer.Address;
        public static Vector3 PositionVector = new Vector3();
        public static IntPtr flyingAddress = IntPtr.Zero;
        public static unsafe ref float PlayerX => ref ((GameObject*)PlayerAddress)->Position.X;
        public static unsafe ref float PlayerY => ref ((GameObject*)PlayerAddress)->Position.Y;
        public static unsafe ref float PlayerZ => ref ((GameObject*)PlayerAddress)->Position.Z;
        public static unsafe ref float PlayerRotation => ref ((GameObject*)PlayerAddress)->Rotation;
        public static unsafe ref float FlyingX => ref (*(float*)(void*)(flyingAddress + 16));
        public static unsafe ref float FlyingY => ref (*(float*)(void*)(flyingAddress + 20));
        public static unsafe ref float FlyingZ => ref (*(float*)(void*)(flyingAddress + 24));
        public static unsafe ref float FlyingHRotation => ref (*(float*)(void*)(flyingAddress + 64));
        public static unsafe ref float FlyingVRotation => ref (*(float*)(void*)(flyingAddress + 156));
        public static unsafe IntPtr ModelAddress => *(IntPtr*)(PlayerAddress + 256);
        public static unsafe ref float ModelX => ref (*(float*)(ModelAddress + 80));
        public static unsafe ref float ModelY => ref (*(float*)(ModelAddress + 84));
        public static unsafe ref float ModelZ => ref (*(float*)(ModelAddress + 88));
        private unsafe ref float ModelRotationX => ref (*(float*)(void*)(ModelAddress + 100));
        private unsafe ref float ModelRotationZ => ref (*(float*)(void*)(ModelAddress + 104));
        private unsafe ref float ModelRotationY => ref (*(float*)(void*)(ModelAddress + 108));
        public static unsafe ref int MovementState => ref (*(int*)(void*)(PlayerAddress + 1508));
        public static float X
        {
            get => PlayerX;
            set
            {
                if (MovementState == 0)
                {
                    PlayerX = value;
                    if (!(ModelAddress != IntPtr.Zero))
                        return;
                    ModelX = value;
                }
                else
                {
                    if (!(flyingAddress != IntPtr.Zero))
                        return;
                    FlyingX = value;
                }
            }
        }

        public static float Z
        {
            get => PlayerZ;
            set
            {
                if (MovementState == 0)
                {
                    PlayerZ = value;
                    if (!(ModelAddress != IntPtr.Zero))
                        return;
                    ModelZ = value;
                }
                else
                {
                    if (!(flyingAddress != IntPtr.Zero))
                        return;
                    FlyingZ = value;
                }
            }
        }

        public static float Y
        {
            get => PlayerY;
            set
            {
                if (MovementState == 0)
                {
                    PlayerY = value;
                    if (!(ModelAddress != IntPtr.Zero))
                        return;
                    ModelY = value;
                }
                else
                {
                    if (!(flyingAddress != IntPtr.Zero))
                        return;
                    FlyingY = value;
                }
            }
        }
        private float Rotation
        {
            get => PlayerRotation;
            set
            {
                PlayerRotation = value;
                if (!(ModelAddress != IntPtr.Zero))
                    return;
                this.ModelRotationX = (float)-Math.Cos(((double)value + Math.PI) / 2.0);
                ModelRotationY = (float)Math.Sin(((double)value + Math.PI) / 2.0);
            }
        }
        public static void NudgeForward(float amount)
        {
            if (!Ready)
                return;
            if (MovementState > 0)
            {
                float num = amount * (float)-((double)FlyingVRotation / 1.570796012878418);
                amount -= Math.Abs(num);
                Z += num;
            }
            double num1 = (double)PlayerRotation + Math.PI;
            X += amount * (float)-Math.Sin(num1);
            Z += amount * (float)-Math.Cos(num1);
        }
        public static void NudgeUp(float amount)
        {
            if (!Ready)
                return;
            Y += amount;
        }

        public static void LoadAndSetPos(string name)
        {
            try
            {
                var ListItem = configuration.SavedPositions.Find(x => x.Name == name);
                if (ListItem == null) return;
                if (ListItem.IgnoreZone)
                {
                    PosMaster.PrintError("ignored zone");
                    X = ListItem.Position.X;
                    Y = ListItem.Position.Y;
                    Z = ListItem.Position.Z;
                }
                if (DalamudApi.DataManager.GetExcelSheet<TerritoryType>()!.GetRow(DalamudApi.ClientState.TerritoryType)!.PlaceName.Row == ListItem.Zone)
                {
                    PosMaster.PrintError("Matched zone");
                    X = ListItem.Position.X;
                    Y = ListItem.Position.Y;
                    Z = ListItem.Position.Z;
                }

            }
            catch (Exception e)
            {
                PosMaster.PrintError($"Unable to find {name} ({e})");
            }
        }
        public static void Initialize()
        {
            try
            {
                configuration = PosMaster.Plugin.PosMasterConfiguration;
                flyingAddress = DalamudApi.SigScanner.GetStaticAddressFromSig("E8 ?? ?? ?? ?? F6 40 70 01", 39);
                PositionVector = new Vector3(X, Y, Z);
                PosMaster.PrintError($"{PlayerX}");
            }
            catch
            {
                PosMaster.PrintError("Failed to get flying address");
            }
        }
    }
}
