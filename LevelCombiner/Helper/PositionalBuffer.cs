using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace LevelCombiner
{
    public class PositionalBuffer
    {
        public byte[] rom { get; private set; }
        public int offset;

        public void TransferFrom(PositionalBuffer buf)
        {
            this.rom = buf.rom;
        }

        public PositionalBuffer(byte[] buf)
        {
            this.rom = buf;
        }

        public void AddOffset(int delta)
        {
            this.offset += delta;
        }

        public void Write8(byte data, int extraOffset = 0)
        {
            rom[offset + extraOffset] = data;
        }

        public byte Read8(int extraOffset = 0)
        {
            return rom[offset + extraOffset];
        }

        public Int16 Read16(int extraOffset = 0)
        {
            Int16 ret = BitConverter.ToInt16(rom, offset + extraOffset);
            return (Int16)IPAddress.NetworkToHostOrder(ret);
        }

        public void Write16(int data, int extraOffset = 0)
        {
            int endianData = IPAddress.HostToNetworkOrder(data);
            byte[] convertedData = BitConverter.GetBytes(endianData);
            Array.Copy(convertedData, 2, rom, offset + extraOffset, 2);
        }

        public Int32 Read32(int extraOffset = 0)
        {
            Int32 ret = BitConverter.ToInt32(rom, offset + extraOffset);
            return (Int32)IPAddress.NetworkToHostOrder(ret);
        }

        public void Write32(int data, int extraOffset = 0)
        {
            int endianData = IPAddress.HostToNetworkOrder(data);
            byte[] convertedData = BitConverter.GetBytes(endianData);
            Array.Copy(convertedData, 0, rom, offset + extraOffset, 4);
        }

        public Int64 Read64(int extraOffset = 0)
        {
            Int64 ret = BitConverter.ToInt64(rom, offset + extraOffset);
            return IPAddress.NetworkToHostOrder(ret);
        }
        public void Write64(UInt64 data, int extraOffset = 0)
        {
            Int64 endianData = IPAddress.HostToNetworkOrder((Int64)data);
            byte[] convertedData = BitConverter.GetBytes(endianData);
            Array.Copy(convertedData, 0, rom, offset + extraOffset, 8);
        }


        public byte PRead8(int offset)
        {
            return rom[offset];
        }

        public Int16 PRead16(int offset)
        {
            Int16 ret = BitConverter.ToInt16(rom, offset);
            return IPAddress.NetworkToHostOrder(ret);
        }

        public Int32 PRead32(int offset)
        {
            Int32 ret = BitConverter.ToInt32(rom, offset);
            return IPAddress.NetworkToHostOrder(ret);
        }

        public Int64 PRead64(int offset)
        {
            Int64 ret = BitConverter.ToInt64(rom, offset);
            return IPAddress.NetworkToHostOrder(ret);
        }
    }
}
