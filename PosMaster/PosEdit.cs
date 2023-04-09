using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Game;

namespace PosMaster
{
    public class PosEdit
    {
        private static Configuration configuration;
        public static bool Ready => DalamudApi.ClientState.LocalPlayer != null;
        public static IntPtr PlayerAddress => DalamudApi.ClientState.LocalPlayer.Address;
        public static IntPtr flyingAddress = IntPtr.Zero;

        private static float speed = 1f;
        private static bool posInitialized;
        private static Vector3 prevPos = Vector3.Zero;
        private static Vector3 posDelta = Vector3.Zero;
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
                ModelRotationX = (float)-Math.Cos(((double)value + Math.PI) / 2.0);
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
            posInitialized = false;
        }
        public static void NudgeUp(float amount)
        {
            if (!Ready)
                return;
            Y += amount;
            posInitialized = false;
        }

        public static void LoadAndSetPos(string name)
        {
            try
            {
                var ListItem = configuration.SavedPositions.Find(x => x.Name == name);
                if (ListItem == null || (!ListItem.IgnoreZone && DalamudApi.DataManager.GetExcelSheet<TerritoryType>()!.GetRow(DalamudApi.ClientState.TerritoryType)!.PlaceName.Row != ListItem.Zone)) return;
                X = ListItem.Position.X;
                Y = ListItem.Position.Y;
                Z = ListItem.Position.Z;
            }
            catch (Exception e)
            {
                PosMaster.PrintError($"Unable to find {name} ({e})");
            }
            posInitialized = false;
        }

        public static void Initialize()
        {
            try
            {
                configuration = PosMaster.Plugin.PosMasterConfiguration;
                flyingAddress = DalamudApi.SigScanner.GetStaticAddressFromSig("E8 ?? ?? ?? ?? F6 40 70 01", 39);
                DalamudApi.Framework.Update += Update;
            }
            catch
            {
                PosMaster.PrintError("Failed to get flying address");
            }
        }
        public static void SetPos(float x, float y, float z)
        {
            if (!Ready)
                return;
            X = x;
            Y = y;
            Z = z;
            posInitialized = false;
        }

        public static void SetSpeed(float s) => speed = s;

        public static void SetPos(Vector3 pos) => SetPos(pos.X, pos.Y, pos.Z);

        public static void MovePos(float x, float y, float z) => SetPos(X + x, Y + y, Z + z);

        public static void Update(Framework framework)
        {
            if (!Ready)
            {
                posInitialized = false;
            }
            else
            {
                if (posInitialized && (double)speed != 1.0)
                {
                    posDelta.X = X - prevPos.X;
                    posDelta.Y = Y - prevPos.Y;
                    posDelta.Z = Z - prevPos.Z;
                    if ((double)posDelta.Length() <= 5.0)
                    {
                        float num = speed - 1f;
                        MovePos(posDelta.X * num,  0.0f, posDelta.Z * num);
                        if (MovementState > 0)
                        {
                            PlayerX = FlyingX;
                            PlayerY = FlyingY;
                            PlayerZ = FlyingZ - 1E-05f;
                        }
                    }
                }
                prevPos.X = X;
                prevPos.Y = Y;
                prevPos.Z = Z;
                posInitialized = true;
            }
        }

        public static void Dispose()
        {
            DalamudApi.Framework.Update -= Update;
        }
    }
}
