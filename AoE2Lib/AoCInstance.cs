using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AoE2Lib
{
    public class AoCInstance : AoE2Instance
    {
        public AoCInstance(Process process) : base(process) { }

        public override int[] GetGoals(int player)
        {
            lock (this)
            {
                var goals1 = GetBaseGoals(player);
                var goals2 = GetExtendedGoals(player);

                if (goals1 == null || goals2 == null)
                {
                    return null;
                }

                var goals = new int[goals1.Length + goals2.Length];
                Array.Copy(goals1, goals, goals1.Length);
                Array.Copy(goals2, 0, goals, goals1.Length, goals2.Length);

                return goals;
            }
        }

        public override bool SetGoal(int player, int index, int value)
        {
            if (index < 0 || index > 511)
            {
                throw new ArgumentOutOfRangeException("index", "index must be between 0 and 511");
            }

            lock (this)
            {
                var ai = GetAI(player);

                if (ai == IntPtr.Zero)
                {
                    return false;
                }

                if (index < 40)
                {
                    var offset = 0x1764 + (index * 4);

                    WriteInt32(ai + offset, value);
                }
                else
                {
                    index -= 40;

                    var egoals = (IntPtr)ReadInt32(ai + 0x29F8);
                    var offset = 0x10 + (index * 4);

                    WriteInt32(egoals + offset, value);
                }

                return true;
            }
        }

        public override bool SetGoals(int player, int start_index, params int[] values)
        {
            var success = true;

            for (int i = 0; i < values.Length; i++)
            {
                var good = SetGoal(player, start_index + i, values[i]);

                if (!good)
                {
                    success = false;
                }
            }

            return success;
        }

        public override int[] GetStrategicNumbers(int player)
        {
            lock (this)
            {
                var sn1 = GetBaseStrategicNumbers(player);
                var sn2 = GetExtendedStrategicNumbers(player);

                if (sn1 == null || sn2 == null)
                {
                    return null;
                }

                var sn = new int[sn1.Length + sn2.Length];
                Array.Copy(sn1, sn, sn1.Length);
                Array.Copy(sn2, 0, sn, sn1.Length, sn2.Length);

                return sn;
            }
        }

        public override bool SetStrategicNumber(int player, int index, int value)
        {
            if (index < 0 || index > 511)
            {
                throw new ArgumentOutOfRangeException("id", "id must be between 0 and 511");
            }

            lock (this)
            {
                var ai = GetAI(player);

                if (ai == IntPtr.Zero)
                {
                    return false;
                }

                if (index < 242)
                {
                    var offset = 0x4734 + (index * 4);

                    WriteInt32(ai + offset, value);
                }
                else
                {
                    index -= 242;

                    var esn = (IntPtr)ReadInt32(ai + 0x29FC);
                    var offset = 0xB0 + (index * 4);

                    WriteInt32(esn + offset, value);
                }

                return true;
            }
        }

        public override bool SetStrategicNumbers(int player, int start_index, params int[] values)
        {
            var success = true;

            for (int i = 0; i < values.Length; i++)
            {
                var good = SetStrategicNumber(player, start_index + i, values[i]);

                if (!good)
                {
                    success = false;
                }
            }

            return success;
        }

        private IntPtr GetGame()
        {
            return (IntPtr)ReadInt32((IntPtr)0x7912A0);
        }

        private IntPtr GetWorld()
        {
            var game = GetGame();

            if (game != IntPtr.Zero)
            {
                return (IntPtr)ReadInt32(game + 0x424);
            }
            else
            {
                return IntPtr.Zero;
            }
        }

        private IntPtr GetPlayer(int player)
        {
            var world = GetWorld();

            if (world != IntPtr.Zero)
            {
                var players = (IntPtr)ReadInt32(world + 0x4C);

                return (IntPtr)ReadInt32(players + (player * 4));
            }
            else
            {
                return IntPtr.Zero;
            }
        }

        private IntPtr GetAI(int player)
        {
            var p = GetPlayer(player);

            if (p != IntPtr.Zero)
            {
                return (IntPtr)ReadInt32(p + 0x12DC);
            }
            else
            {
                return IntPtr.Zero;
            }
        }

        private int[] GetBaseGoals(int player)
        {
            var ai = GetAI(player);

            if (ai == IntPtr.Zero)
            {
                return null;
            }

            var bytes = ReadByteArray(ai + 0x1764, 40 * 4);

            var ints = new int[bytes.Length / 4];
            Buffer.BlockCopy(bytes, 0, ints, 0, bytes.Length);

            return ints;
        }

        private int[] GetExtendedGoals(int player)
        {
            var ai = GetAI(player);

            if (ai == IntPtr.Zero)
            {
                return null;
            }

            var egoals = (IntPtr)ReadInt32(ai + 0x29F8);
            var bytes = ReadByteArray(egoals + 0x10, 472 * 4);

            var ints = new int[bytes.Length / 4];
            Buffer.BlockCopy(bytes, 0, ints, 0, bytes.Length);

            return ints;
        }

        private int[] GetBaseStrategicNumbers(int player)
        {
            var ai = GetAI(player);

            if (ai == IntPtr.Zero)
            {
                return null;
            }

            var bytes = ReadByteArray(ai + 0x4734, 242 * 4);

            var ints = new int[bytes.Length / 4];
            Buffer.BlockCopy(bytes, 0, ints, 0, bytes.Length);

            return ints;
        }

        private int[] GetExtendedStrategicNumbers(int player)
        {
            var ai = GetAI(player);

            if (ai == IntPtr.Zero)
            {
                return null;
            }

            var egoals = (IntPtr)ReadInt32(ai + 0x29FC);
            var bytes = ReadByteArray(egoals + 0xB0, 270 * 4);

            var ints = new int[bytes.Length / 4];
            Buffer.BlockCopy(bytes, 0, ints, 0, bytes.Length);

            return ints;
        }

        
    }
}
