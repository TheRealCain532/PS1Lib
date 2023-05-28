using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
/*
 * PS1 is Big Endian
 * 
 */
namespace PS1Lib
{
    public class PS1API
    {
        private class Common
        {
            public static TMAPI TmApi;
        }

        private static string targetIp = String.Empty;
        uint processID = 0;
        private void SetMem(uint Address, byte[] buffer)
        {
            new TMAPI().SetMemory(Address, buffer);
        }

        private void GetMem(uint offset, byte[] buffer)
        {
            new TMAPI().GetMemory(offset, buffer);
        }

        private byte[] GetBytes(uint offset, uint length)
        {
            byte[] buffer = new byte[length];
                buffer = new TMAPI().GetBytes(offset, length);
            return buffer;
        }
        public PS1API(string IP, uint ProcessID = 0) { Common.TmApi = new TMAPI(); this.IP = IP; this.processID = ProcessID; }

        string IP { get { return targetIp; } set { targetIp = value; } }

        bool Debug
        {
            get
            {
                if (IP == "") { MessageBox.Show("You need to set an IP!"); return false; }
                else
                    return new WebClient().DownloadString($"http://{IP}/peek.lv2?0x00003B38").Contains("38 60 00 01 4E 80 00 20");
            }
            set
            {
                if (IP != "")
                {
                    string data = value ? "386000014E800020" : "E92280087C0802A6";
                    string magic = $"http://{IP}/poke.lv2?0x00003B38={data}";
                    new WebClient().DownloadString(magic);
                }
            }
        }
        TMAPI.SCECMD stats = new TMAPI.SCECMD();
        public Boolean Connect { get => stats.GetStatus() == "Connected"; 
        set
            {
                if (!Debug) Debug = true;
                if (value)
                {
                    if (Common.TmApi.ConnectTarget())
                    {
                        if (this.processID > 0)
                            Common.TmApi.AttachProcess(processID);
                        new ProcessList().AttachProcess("ps1");
                    }
                }
                else
                    Common.TmApi.DisconnectTarget();
            }
        }
        /// <summary>Read a signed byte.</summary>
        public sbyte ReadSByte(uint offset, bool Gameshark)
        {
            uint address = 0;
            byte[] buffer = new byte[1];
            GetMem(address, buffer);
            return (sbyte)buffer[0];
        }

        /// <summary>Read a byte a check if his value. This return a bool according the byte detected.</summary>
        public bool ReadBool(uint offset)
        {
            byte[] buffer = new byte[1];
            GetMem(offset, buffer);
            return buffer[0] != 0;
        }

        /// <summary>Read and return an integer 16 bits.</summary>
        public short ReadInt16(uint offset)
        {
            byte[] buffer = GetBytes(offset, 2);

            Array.Reverse(buffer, 0, 2);
            return BitConverter.ToInt16(buffer, 0);
        }
        /// <summary>Read and return an integer 32 bits.</summary>
        public int ReadInt32(uint offset)
        {
            byte[] buffer = GetBytes(offset, 4);

            Array.Reverse(buffer, 0, 4);
            return BitConverter.ToInt32(buffer, 0);
        }
        /// <summary>Read and return a Float.</summary>
        public float ReadFloat(uint offset)
        {
            byte[] buffer = GetBytes(offset, 4);

            Array.Reverse(buffer, 0, 4);
            return BitConverter.ToSingle(buffer, 0);
        }
        /// <summary>Read a string with a length to the first byte equal to an value null (0x00).</summary>
        public byte[] ReadBytes(uint offset, int length)
        {
            return GetBytes(offset, (uint)length);
        }
        /// <summary>Read a string very fast by buffer and stop only when a byte null is detected (0x00).</summary>
        public string ReadString(uint offset)
        {
            int blocksize = 40;
            int scalesize = 0;
            string str = string.Empty;

            while (!str.Contains('\0'))
            {
                byte[] buffer = ReadBytes(offset + (uint)scalesize, blocksize);
                str += Encoding.UTF8.GetString(buffer);
                scalesize += blocksize;
            }
            return str.Substring(0, str.IndexOf('\0'));
        }

        /// <summary>Write a signed byte.</summary>
        public void WriteSByte(uint offset, sbyte input)
        {
            byte[] buff = new byte[1];
            buff[0] = (byte)input;
            SetMem(offset, buff);
        }

        /// <summary>Write a boolean.</summary>
        public void WriteBool(uint offset, bool input)
        {
            byte[] buff = new byte[1];
            buff[0] = input ? (byte)1 : (byte)0;
            SetMem(offset, buff);
        }

        /// <summary>Write an interger 16 bits.</summary>
        public void WriteInt16(uint offset, short input)
        {
            byte[] buff = new byte[2];
            BitConverter.GetBytes(input).CopyTo(buff, 0);

            Array.Reverse(buff, 0, 2);
            SetMem(offset, buff);
        }
        /// <summary>Write a byte array.</summary>
        public void WriteBytes(uint offset, byte[] input)
        {
            byte[] buff = input;
            SetMem(offset, buff);
        }

        /// <summary>Write a string.</summary>
        public void WriteString(uint offset, string input)
        {
            byte[] buff = Encoding.UTF8.GetBytes(input);
            Array.Resize(ref buff, buff.Length + 1);
            SetMem(offset, buff);
        }
        /// <summary>Write a Float.</summary>
        public void WriteFloat(uint offset, float input)
        {
            byte[] buff = new byte[4];
            BitConverter.GetBytes(input).CopyTo(buff, 0);

            Array.Reverse(buff, 0, 4);
            SetMem(offset, buff);
        }

        public GameShark GameShark { get { return new GameShark(); } }



    }
}
