using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS1Lib
{
    public class GameShark
    {
        private class Common
        {
            public static TMAPI TmApi;
        }
        private string ProcessName
        {
            get
            {
                uint processID = Common.TmApi.SCE.ProcessID();
                PS3TMAPI.ProcessInfo current;
                PS3TMAPI.GetProcessInfo(0, processID, out current);
                return current.Hdr.ELFPath.Split('/').Last().Split('.')[0];
            }
        }
        /// <summary>This reads a pointer in memory and grabs the offset difference between Gameshark code and the PS3's memory region. Tested on a lot of games with great success, but if you find errors, please let me know!</summary>
        private uint magic
        {
            get
            {
                uint address = (uint)(ProcessName.Contains("net") ? 0x001BBF20 : 0x0017B710);
                byte[] buff = GetBytes(address, 4);
                Array.Reverse(buff, 0, 4);
                return BitConverter.ToUInt32(buff, 0);
            }
        }
        private void SetMem(uint Address, byte[] buffer)
        {
            Common.TmApi.SetMemory(Address, buffer);
        }
        private byte[] GetBytes(uint offset, uint length)
        {
            return Common.TmApi.GetBytes(offset, length); ;
        }
        private short ReadInt16(uint offset)
        {
            byte[] buffer = GetBytes(offset, 2);

            Array.Reverse(buffer, 0, 2);
            return BitConverter.ToInt16(buffer, 0);
        }
        private void WriteInt16(uint offset, short input)
        {
            byte[] buff = new byte[2];
            BitConverter.GetBytes(input).CopyTo(buff, 0);

            Array.Reverse(buff, 0, 2);
            SetMem(offset, buff);
        }
        private static byte[] STBA(string hex)
        {
            return (from x in Enumerable.Range(0, hex.Length)
                    where x % 2 == 0
                    select Convert.ToByte(hex.Substring(x, 2), 16)).ToArray();
        }
        /// <summary>Converts single line Gameshark code into PS3 offset</summary>
        /// <param name="GamesharkCode">input single line Gameshark code</param>
        /// <returns></returns>
        public uint Gameshark2PS3(string GamesharkCode)
        {
            if (GamesharkCode != "")
                return (Convert.ToUInt32(GamesharkCode.Split(' ')[0].Remove(0, 2), 16) + magic);
            return 0;
        }
        /// <summary>Converts PS3 offset to Gameshark offset</summary>
        /// <param name="PS3Offset"></param>
        /// <returns></returns>
        public string PS32Gameshark(uint PS3Offset)
        {
            if (PS3Offset > 0)
                return $"{(PS3Offset - magic):X}";
            return "";
        }
        /// <summary>
        /// It is Highly recommended to run this command through a timer of some sort.
        /// </summary>
        /// <param name="input">input single line Gameshark code</param>
        public void Write(string input)
        {
            if (input != "")
            {
                string identifier = input.Remove(2);
                uint address = (Convert.ToUInt32(input.Split(' ')[0].Remove(0, 2), 16) + magic);
                byte[] bytes = STBA(input.Split(' ')[1]);
                short data = (short)(bytes[0] + (bytes[1] << 8));
                if (identifier == "80" || identifier == "30") WriteInt16(address, data); //simple write
                if (identifier == "10" || identifier == "20")
                {
                    short toWrite = (short)(ReadInt16(address) + data);
                    WriteInt16(address, toWrite);
                } //increment value
                if (identifier == "11" || identifier == "21")
                {
                    short toWrite = (short)(ReadInt16(address) - data);
                    WriteInt16(address, toWrite);
                } //decrement value
            }
        }
        /// <summary>
        /// It is Highly recommended to run this command through a timer of some sort.
        /// </summary>
        /// <param name="input">input multi line Gameshark code</param>
        public void Write(string[] input)
        {
            if (input.Length > 0)
            {
                try
                {
                    for (int b = 0; b < input.Length; b++)
                    {
                        uint address = (Convert.ToUInt32(input[b].Split(' ')[0].Remove(0, 2), 16) + magic);
                        byte[] bytes = STBA(input[b].Split(' ')[1]);
                        short data = (short)(bytes[0] + (bytes[1] << 8));
                        string identifier = input[0].Remove(2);
                        if (identifier == "80" || identifier == "30") WriteInt16(address, data); //simple write
                        if (identifier == "10" || identifier == "20")
                        {
                            short toWrite = (short)(ReadInt16(address) + data);
                            WriteInt16(address, toWrite);
                        } //increment value
                        if (identifier == "11" || identifier == "21")
                        {
                            short toWrite = (short)(ReadInt16(address) - data);
                            WriteInt16(address, toWrite);
                        } //decrement value
                        if (identifier == "E0" || identifier == "D0")
                        {
                            if (ReadInt16(address) == data)
                            {
                                for (int i = 1; i < input.Length; i++)
                                    Write(input[i]);
                            }
                        } //Equal To
                        if (identifier == "E1" || identifier == "D1")
                        {
                            if (ReadInt16(address) != data)
                            {
                                for (int i = 1; i < input.Length; i++)
                                    Write(input[i]);
                            }
                        } //Different To
                        if (identifier == "E2" || identifier == "D2")
                        {
                            if (ReadInt16(address) < data)
                            {
                                for (int i = 1; i < input.Length; i++)
                                    Write(input[i]);
                            }
                        } //Less Than
                        if (identifier == "E3" || identifier == "D3")
                        {
                            if (ReadInt16(address) > data)
                            {
                                for (int i = 1; i < input.Length; i++)
                                    Write(input[i]);
                            }
                        } //Greater Than
                        if (identifier == "50")
                        {
                            int amnt = Convert.ToInt32(input[b].Split(' ')[0].Remove(0, 4).Remove(2), 16);
                            int jump = Convert.ToInt32(input[b].Split(' ')[0].Remove(0, 4).Remove(0, 2), 16);
                            uint _address = (Convert.ToUInt32(input[b + 1].Split(' ')[0].Remove(0, 2), 16) + magic);
                            byte[] _bytes = STBA(input[b + 1].Split(' ')[1]);
                            for (int a = 0; a < amnt; a++)
                            {
                                byte[] shit = BitConverter.GetBytes(_bytes[1] += bytes[1]);
                                short toWrite = (short)(_bytes[0] + (shit[0] << 8)); //line 2!!!
                                WriteInt16((uint)(_address + (a * jump)), toWrite);
                            }
                            b++;
                        } //Serial Repeater (For Loop)
                    }
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
            }
        }
        public GameShark()
        {
            if (Common.TmApi == null) Common.TmApi = new TMAPI();
        }

    }

}
