using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FTD2XX_NET;

namespace _FTDI
{
    public class FTDI
    {
        public bool RTS
        {
            get { return true; }
            set
            {
                this.SetRTS(value);
            }
        }
        public int BytesToRead
        {
            get
            {
                uint rxbufcount=0;
                this.ftdiPort.GetRxBytesAvailable(ref rxbufcount);
                return (int)rxbufcount;
            }
        }
        FTD2XX_NET.FTDI ftdiPort;
        public FTD2XX_NET.FTDI.FT_STATUS status = FTD2XX_NET.FTDI.FT_STATUS.FT_OK;
        public void Close()
        {
            this.ftdiPort.Close();
        }
        public bool OpenAny(uint baudRate = 38400)
        {
            try
            {
                this.ftdiPort = new FTD2XX_NET.FTDI();
                uint count = 0;
                this.status = this.ftdiPort.GetNumberOfDevices(ref count);
                if (count == 1)
                {
                    status = this.ftdiPort.OpenByIndex(0);
                    status = this.ftdiPort.SetBaudRate(baudRate);
                    return true;
                }    

            }
            catch
            {
                return false;
            }
            return false;
        }

        public byte ReadByte()
        {
            byte[] buf = new byte[1];
            uint read=0;
            status = this.ftdiPort.Read(buf, 1, ref read);
            return buf[0];
        }

        public void SetRTS(bool state = true)
        {
            this.ftdiPort.SetRTS(state);
        }
        public void ClearRTS()
        {
            this.ftdiPort.SetRTS(false);
        }
        public int Write(byte[] bytes)
        {
            uint written = 0;
            status = this.ftdiPort.Write(bytes, (uint)bytes.Length, ref written);
            return (int)written;
        
        }
        public int WriteLine(String s)
        {
            return Write(s + "\n");
        }
        public int Write(String s)
        {
            return Write(System.Text.Encoding.ASCII.GetBytes(s));
        }
        public int Write(char c)
        {
            return Write((byte)c);
        }
        public int Write(byte b)
        {
            return Write(new byte[] {b});
        }
        public string GetPortName()
        {
            string name;
            this.ftdiPort.GetCOMPort(out name);
            return name;
        }

    }
}
