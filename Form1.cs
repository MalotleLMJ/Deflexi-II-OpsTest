using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using _FTDI;

namespace Deflexi_II_OpsTest
{
    public enum wave
    {
        _NOSIGNAL = 0,
        _100MHZ = 1,
        _500MHZ = 2,
        _1HZ = 3,
        _REEDSWITCH = 4,
        _LEDSTATUS = 5,
        _TESTCOMMS = 6,
        _SLEEP = 5000,
        _BYTESAMPLES = 800,
        _CHANNELSAMPLES = 100,
        _SAMPLES = 400,
    };

    public partial class Form1 : Form
    {

        FTDI serialPort1 = new FTDI();
        Deflexi DUT;
        Serialnum serial = new Serialnum();
        //Channels[] chn = new Channels[4];
        int[] info;
        int[] data;
        bool opencomms = false;
        bool res = false;
        int ananlogCounter = 0;

        Label[] labelStatus = new Label[7];
        Label[,] labelAnalog = new Label[4, 4];

        public Form1()
        {
            InitializeComponent();
            OpenComms();
            Init();

        }

        public void Init()
        {
            initStatusLabels();
            initResultLabels();
        }

        public void initStatusLabels()
        {
            labelStatus[0] = _labelSN;
            labelStatus[1] = _labelComms;
            labelStatus[6] = _labelSwitch;
            labelStatus[2] = _labelVolt;
            labelStatus[3] = _label100m;
            labelStatus[4] = _label500m;
            labelStatus[5] = _label1Hz;
            for (int i = 0; i < 7; i++)
                labelStatus[i].BackColor = DefaultBackColor;
        }
        public void initResultLabels()
        {
            labelAnalog[0, 0] = labelVolt_1;
            labelAnalog[0, 1] = labelVolt_2;
            labelAnalog[0, 2] = labelVolt_3;
            labelAnalog[0, 3] = labelVolt_4;
            labelAnalog[1, 0] = label100m_1;
            labelAnalog[1, 1] = label100m_2;
            labelAnalog[1, 2] = label100m_3;
            labelAnalog[1, 3] = label100m_4;
            labelAnalog[2, 0] = label500m_1;
            labelAnalog[2, 1] = label500m_2;
            labelAnalog[2, 2] = label500m_3;
            labelAnalog[2, 3] = label500m_4;
            labelAnalog[3, 0] = label1Hz_1;
            labelAnalog[3, 1] = label1Hz_2;
            labelAnalog[3, 2] = label1Hz_3;
            labelAnalog[3, 3] = label1Hz_4;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                    labelAnalog[i, j].BackColor = DefaultBackColor;
            }
        }
        public void clearAll()
        {
            this.BackColor = DefaultBackColor;
            for (int i = 0; i < 7; i++)
                labelStatus[i].BackColor = DefaultBackColor;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                    labelAnalog[i, j].BackColor = DefaultBackColor;
            }
            labelSwitch.BackColor = DefaultBackColor;
            labelComms.BackColor = DefaultBackColor;
        }
        void OpenComms()
        {
            int retries = 2;
            button1.Enabled = false;
            while (true)
            {
                if (serialPort1.OpenAny() == true)
                {
                    this.BackColor = DefaultBackColor;
                    button1.Enabled = true;
                    opencomms = true;
                    break;
                }
                else
                {
                    this.BackColor = System.Drawing.Color.Red;
                    MessageBox.Show("Connection Failed.\r\nConnect jig usb and click OK.\r\n" + Convert.ToString(retries) + " retry left");
                    Thread.Sleep(2000);
                }
                if (retries == 0)
                {
                    this.BackColor = System.Drawing.Color.Red;
                    break;
                }
                retries--;
            }
        }
        public void sendCommand(wave cmd)
        {
            flush();
            Thread.Sleep(200);
            switch (cmd)
            {
                case wave._NOSIGNAL: serialPort1.Write("N"); break;
                case wave._100MHZ: serialPort1.Write("X"); break;
                case wave._500MHZ: serialPort1.Write("Y"); break;
                case wave._1HZ: serialPort1.Write("Z"); break;
                case wave._TESTCOMMS: serialPort1.Write("A"); break;
                case wave._LEDSTATUS: serialPort1.Write("C"); break;
            }
        }
        public void flush()
        {
            while (serialPort1.BytesToRead != 0) serialPort1.ReadByte();
        }
        public void waveAcquisition(wave cmd)
        {
            int[] temp;
            int i = 0;
            int[,] signals;
            int sleeper = 20000;
            this.Refresh();
            flush();
            Thread.Sleep(500);
            sendCommand(cmd);

            Thread.Sleep(sleeper);
            int totalBytes = (int)wave._BYTESAMPLES;
            int jointByte = totalBytes / 2;
            if (serialPort1.BytesToRead == totalBytes)
            {
                temp = new int[totalBytes];
                while (serialPort1.BytesToRead > 0)
                {
                    temp[i] = serialPort1.ReadByte();
                    i++;
                }

                data = new int[jointByte];
                signals = new int[4, (int)wave._CHANNELSAMPLES];
                int s = 0;
                int _s = 0;
                for (i = 0; i < jointByte; i++)
                {
                    data[i] = ((temp[i * 2] * 256) + temp[i * 2 + 1]);
                    _s = i / (int)wave._CHANNELSAMPLES;
                    s = i % (int)wave._CHANNELSAMPLES;
                    signals[_s, s] = data[i];
                }
                for (i = 0; i < 4; i++)
                {
                    _dataProcessing(signals, i, cmd);
                    __dataCaputure(signals, i, cmd);
                }
                //ResultsPrint(cmd);
            }
            else flush();
        }
        public void waveAcqState(wave cmd)
        {
            int i = 0;
            switch (cmd)
            {
                case wave._NOSIGNAL:
                    for (i = 0; i < 4; i++)
                    {
                        ananlogCounter = ananlogCounter + checkAnalog(cmd, i);
                    }
                    break;
                case wave._100MHZ:
                    for (i = 0; i < 4; i++)
                    {
                        ananlogCounter = ananlogCounter + checkAnalog(cmd, i);
                    }
                    break;
                case wave._500MHZ:
                    for (i = 0; i < 4; i++)
                    {
                        ananlogCounter = ananlogCounter + checkAnalog(cmd, i);
                    }
                    break;
                case wave._1HZ:
                    for (i = 0; i < 4; i++)
                    {
                        ananlogCounter = ananlogCounter + checkAnalog(cmd, i);
                    }
                    break;
            }
        }
        public int checkAnalog(wave cmd, int i)
        {
            Criterias criteria = new Criterias();
            criteria.init();

            int stat = 0;

            switch (cmd)
            {
                case wave._NOSIGNAL:
                    if (DUT.channels[i].avgOffset > criteria.NoSignalMin && DUT.channels[i].avgOffset < criteria.NoSignalMax) stat = stat + 1;
                    else stat = stat + 0;

                    if (DUT.channels[i].rmsOffset > criteria.NoSignalMin && DUT.channels[i].rmsOffset < criteria.NoSignalMax) stat = stat + 1;
                    else stat = stat + 0;

                    if (DUT.channels[i].pkOffset > criteria.pkNoSignalMin && DUT.channels[i].pkOffset < criteria.pkNoSignalMax) stat = stat + 1;
                    else stat = stat + 0;
                    if (stat == 3)
                    {
                        DUT.channels[i].resultOffset = true;
                        labelAnalog[0, i].BackColor = Color.LightGreen;
                    }
                    else
                    {
                        DUT.channels[i].resultOffset = false;
                        labelAnalog[0, i].BackColor = Color.Red;
                    }
                        return stat;
     
                case wave._100MHZ:
                    if (DUT.channels[i].avg100mHz > criteria._100SignalMin && DUT.channels[i].avg100mHz < criteria._100SignalMax) stat = stat + 1;
                    else stat = stat + 0;

                    if (DUT.channels[i].rms100mHz > criteria._100SignalMin && DUT.channels[i].rms100mHz < criteria._100SignalMax) stat = stat + 1;
                    else stat = stat + 0;

                    if (DUT.channels[i].pk100mHz > criteria.pk100SignalMin && DUT.channels[i].pk100mHz < criteria.pk100SignalMax) stat = stat + 1;
                    else stat = stat + 0;
                    if (stat == 3)
                    {
                        DUT.channels[i].result100mHz = true;
                        labelAnalog[1, i].BackColor = Color.LightGreen;
                    }
                    else
                    {
                        DUT.channels[i].result100mHz = false;
                        labelAnalog[1, i].BackColor = Color.Red;
                    }
                    return stat;
               
                case wave._500MHZ:
                    if (DUT.channels[i].avg500mHz > criteria._500SignalMin && DUT.channels[i].avg500mHz < criteria._500SignalMax) stat = stat + 1;
                    else stat = stat + 0;

                    if (DUT.channels[i].rms500mHz > criteria._500SignalMin && DUT.channels[i].rms500mHz < criteria._500SignalMax) stat = stat + 1;
                    else stat = stat + 0;

                    if (DUT.channels[i].pk500mHz > criteria.pk500SignalMin && DUT.channels[i].pk500mHz < criteria.pk500SignalMax) stat = stat + 1;
                    else stat = stat + 0;
                    if (stat == 3)
                    {
                        DUT.channels[i].result500mHz = true;
                        labelAnalog[2, i].BackColor = Color.LightGreen;
                    }
                    else
                    {
                        DUT.channels[i].result500mHz = false;
                        labelAnalog[2, i].BackColor = Color.Red;
                    }
                    return stat;
                   
                case wave._1HZ:
                    if (DUT.channels[i].avg1Hz > criteria._1SignalMin && DUT.channels[i].avg1Hz < criteria._1SignalMax) stat = stat + 1;
                    else stat = stat + 0;

                    if (DUT.channels[i].rms1Hz > criteria._1SignalMin && DUT.channels[i].rms1Hz < criteria._1SignalMax) stat = stat + 1;
                    else stat = stat + 0;

                    if (DUT.channels[i].pk1Hz > criteria.pk1SignalMin && DUT.channels[i].pk1Hz < criteria.pk1SignalMax) stat = stat + 1;
                    else stat = stat + 0;
                    if (stat == 3)
                    {
                        DUT.channels[i].result1Hz = true;
                        labelAnalog[3, i].BackColor = Color.LightGreen;
                    }
                    else
                    {
                        DUT.channels[i].result1Hz = false;
                        labelAnalog[3, i].BackColor = Color.Red;
                    }
                    return stat;
            }
            return 0;
        }
        public void _dataProcessing(int[,] dat, int chan, wave cmd)
        {
            long info = 0;
            int _info = 0;

            int min = 512;
            int max = 512;

            double avg;
            double rms;
            double vpk;


            for (int i = 0; i < (int)wave._CHANNELSAMPLES; i++)
            {
                info = info + (long)(Math.Pow(dat[chan, i], 2));
                _info = _info + dat[chan, i];
                if (min > dat[chan, i]) min = dat[chan, i];
                if (max < dat[chan, i]) max = dat[chan, i];
            }
            info = info / (int)wave._CHANNELSAMPLES;



            avg = Math.Round(((_info / (double)wave._CHANNELSAMPLES)/1024)*2.2,3);
            rms = Math.Round(((Math.Sqrt(info)) / 1024) * 2.2, 3);
            vpk = Math.Round((((double)max - (double)min)/ 1024)*2.2,3);

            switch(cmd)
            {
                case wave._NOSIGNAL:
                    DUT.channels[chan].avgOffset = avg;
                    DUT.channels[chan].rmsOffset = rms;
                    DUT.channels[chan].pkOffset = vpk;
                    break;
                case wave._100MHZ:
                    DUT.channels[chan].avg100mHz = avg;
                    DUT.channels[chan].rms100mHz = rms;
                    DUT.channels[chan].pk100mHz = vpk;
                    break;
                case wave._500MHZ:
                    DUT.channels[chan].avg500mHz = avg;
                    DUT.channels[chan].rms500mHz = rms;
                    DUT.channels[chan].pk500mHz = vpk;
                    break;
                case wave._1HZ:
                    DUT.channels[chan].avg1Hz = avg;
                    DUT.channels[chan].rms1Hz = rms;
                    DUT.channels[chan].pk1Hz = vpk;
                    break;

            }
        }
        public void __dataCaputure(int[,] dat, int num, wave cmd)
        {

            string path = Directory.GetCurrentDirectory();
            string filename = path + "\\" +DUT.sn+ "_data.txt";
            string filename2 = path + "\\";
            string s = dat[num, 0].ToString();
            string _s = "";

            for (int i = 1; i < (int)wave._CHANNELSAMPLES; i++)
            {
                s = s + "," + dat[num, i].ToString();
            }

            switch (cmd)
            {
                case wave._NOSIGNAL: _s = "_NOSIGNAL"; break;
                case wave._100MHZ: _s = "_100mHz"; break;
                case wave._500MHZ: _s = "_500mHz"; break;
                case wave._1HZ: _s = "_1Hz"; break;
            }
            filename2 = filename2 + _s + ".txt";
            _s = num.ToString() + _s;


            StreamWriter writer = new StreamWriter(File.Open(filename, FileMode.Append));
            writer.WriteLine("{0},{1},{2}", DateTime.Now.ToString(), _s, s);
            writer.Close();

            //StreamWriter writer2 = new StreamWriter(File.Open(filename2, FileMode.Append));
            //writer2.WriteLine("{0},{1}", DateTime.Now.ToString(), s);
            //writer2.Close();
        }
        /*public void ResultsPrint(wave cmd)
        {
            string path = Directory.GetCurrentDirectory();
            string filename = path + "\\Results.txt";
            string _s = "";
            string s = AVG[0].ToString() + "," + AVG[1].ToString() + "," + AVG[2].ToString() + "," + AVG[3].ToString() + ",";
            s = s + RMS[0].ToString() + "," + RMS[1].ToString() + "," + RMS[2].ToString() + "," + RMS[3].ToString() + ",";
            s = s + VPK[0].ToString() + "," + VPK[1].ToString() + "," + VPK[2].ToString() + "," + VPK[3].ToString();

            switch (cmd)
            {
                case wave._NOSIGNAL: _s = "_NOSIGNAL"; break;
                case wave._100MHZ: _s = "_100mHz"; break;
                case wave._500MHZ: _s = "_500mHz"; break;
                case wave._1HZ: _s = "_1Hz"; break;
            }


            StreamWriter writer = new StreamWriter(File.Open(filename, FileMode.Append));
            writer.WriteLine("{0},{1},{2}", DateTime.Now.ToString(), _s, s);
            writer.Close();
        }*/
        public void test()
        {
            clearAll();
            DUT = new Deflexi();
            DUT.init();
            DUT.dt = DateTime.Now;
            int testCounter = 0;
            res = false;

            while(true)
            {
                labelStatus[testCounter].BackColor = Color.PowderBlue;
                switch (testCounter)
                {
                    case 0:
                        setSN();
                        break;
                    case 1:
                        testDUTComms();
                        break;
                    case 2:
                        testAnalogs();
                        break;
                    case 3:
                        testReed();
                        break;
                }
                if (res == false)
                {
                    this.BackColor = System.Drawing.Color.Red;
                    DUT.result = false;
                    break;
                }
                if (testCounter == 3 && res == true)
                {
                    this.BackColor = System.Drawing.Color.LightGreen;
                    DUT.result = true;
                    break;
                }
                testCounter++;
            }
            DUT.tofile();
        }
        public void setSN()
        {
            
            DialogResult r = serial.ShowDialog();
            
            if (r == System.Windows.Forms.DialogResult.OK)
            {
                DUT.sn = serial.sernum;
                labelSN.Text = DUT.sn;
                res = true;
            }
        }
        public void testDUTComms()
        {
            int temp = -1;
            int tries = 5; 
            sendCommand(wave._TESTCOMMS);
            do
            {
                Thread.Sleep(100);
                if (serialPort1.BytesToRead == 1)
                {
                    temp = serialPort1.ReadByte();
                    tries = 0;
                }
                if (tries == 0) break;
                else tries--;
            } while (true);

            if (temp == 65)
            {
                DUT.comms = true;
                res = true;
                labelComms.BackColor = Color.LightGreen;
            }
            else
            {
                DUT.comms = false;
                res = false;
                labelComms.BackColor = Color.Red;
            }
        }
        public void testAnalogs()
        {
            int i = 0;
            ananlogCounter = 0;
            while (true)
            {
                switch (i)
                {
                    case 0 :
                        MessageBox.Show("Turn the signal off. \nPress OK to continue");
                        Thread.Sleep(20000);
                        waveAcquisition(wave._NOSIGNAL);
                        waveAcqState(wave._NOSIGNAL);
                        break;
                    case 1 :
                        MessageBox.Show("Set the signal to 100mHz @ 100mV with a 0V Offset. \nPress OK to continue");
                        Thread.Sleep(15000);
                        waveAcquisition(wave._100MHZ);
                        waveAcqState(wave._100MHZ);
                        break;
                    case 2 :
                        MessageBox.Show("Set the signal to 500mHz @ 100mV with a 0V Offset. \nPress OK to continue");
                        Thread.Sleep(15000);
                        waveAcquisition(wave._500MHZ);
                        waveAcqState(wave._500MHZ);
                        break;
                    case 3 :
                        MessageBox.Show("Set the signal to 1Hz @ 100mV with a 0V Offset. \nPress OK to continue");
                        Thread.Sleep(15000);
                        waveAcquisition(wave._1HZ);
                        waveAcqState(wave._1HZ);
                        break;
                }
                i++;
                this.Refresh();
                if (ananlogCounter != 12)
                {
                    res = false;
                    break;
                }
                else ananlogCounter = 0;

                if (i == 4 && res == true)
                {
                    res = true;
                    break;
                }
            }

        }
        public void testReed()
        {
            int counter = 0;
            _labelSwitch.BackColor = Color.PowderBlue;
            MessageBox.Show("Place a magnet near the reed switch.\n\rPress OK to continue");
            while (true)
            {
                flush();
                labelSwitch.Text = "Place magnet";
                sendCommand(wave._LEDSTATUS);
                Thread.Sleep(2000);
                if (serialPort1.BytesToRead == 1)
                {
                    byte temp = serialPort1.ReadByte();
                    int temp2 = (int)temp & 0x04;
                    if (temp2 == 4)
                    {
                        labelSwitch.BackColor = Color.LightGreen;
                        DUT.reed = true;
                        res = true;
                        break;
                    }
                    else
                    {
                        labelSwitch.Text = "Fail-" + counter.ToString();
                        labelSwitch.BackColor = Color.Red;
                        DUT.reed = false;
                        res = false;
                    }
                    if (counter == 5)
                        break;
                    else counter++;
                }
                if (counter == 5)
                {
                    labelSwitch.BackColor = Color.Red;
                    DUT.reed = false;
                    res = false;
                    break;
                }
                else counter++;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            test();
        }
    }

    public class Deflexi
    {
        public string sn;
        public DateTime dt;
        public bool result;
        public bool reed;
        public bool comms;
        public Channel[] channels = new Channel[4];
        public void init ()
        {
            for (int i = 0; i < 4; i++)
                channels[i] = new Channel();
        }
        public string fileprint()
        {
            string s = "";
            s = "Date-Time of Test: " + dt.ToString() +"\r"+ 
                "Serial Number: " + sn +"\r"+ 
                "Test Results: " + result.ToString() +"\r"+
                "Communication Test: " + comms.ToString() +"\r"+
                "Reed Switch: " + reed.ToString() +"\r"+
                "Offset results\r";
            string[] ch = new string[4];
            for (int i = 0; i < 4; i++)
            {
                ch[i] = "CH" + (i+1).ToString() + ": " + channels[i].avgOffset.ToString() + "," + channels[i].rmsOffset.ToString() + "," + channels[i].pkOffset.ToString() + "," + channels[i].resultOffset.ToString() + "," +
                                                     channels[i].avg100mHz.ToString() + "," + channels[i].rms100mHz.ToString() + "," + channels[i].pk100mHz.ToString() + "," + channels[i].result100mHz.ToString() + "," +
                                                     channels[i].avg500mHz.ToString() + "," + channels[i].rms500mHz.ToString() + "," + channels[i].pk500mHz.ToString() + "," + channels[i].result500mHz.ToString() + "," +
                                                     channels[i].avg1Hz.ToString() + "," + channels[i].rms1Hz.ToString() + "," + channels[i].pk1Hz.ToString() + "," + channels[i].result1Hz.ToString();
            }

            return s + ch[0] + "\r" + ch[1] + "\r" + ch[2] + "\r" + ch[3];
        }

        public void tofile()
        {
            string path = Directory.GetCurrentDirectory();
            string filename = path + "\\"+ sn +".txt";
            string s = fileprint();
            StreamWriter writer = new StreamWriter(File.Open(filename, FileMode.Append));
            writer.WriteLine("{0}", s);
            writer.Close();

        }
         
    }

    public class Channel
    {
        public double rmsOffset = 0.0;
        public double avgOffset = 0.0;
        public double pkOffset = 0.0;
        public bool resultOffset = false;
        public double rms100mHz = 0.0;
        public double avg100mHz = 0.0;
        public double pk100mHz = 0.0;
        public bool result100mHz = false;
        public double rms500mHz = 0.0;
        public double avg500mHz = 0.0;
        public double pk500mHz = 0.0;
        public bool result500mHz = false;
        public double rms1Hz = 0.0;
        public double avg1Hz = 0.0;
        public double pk1Hz = 0.0;
        public bool result1Hz = false;
    }

    public class Criterias
    {
        double tolences = 0.07;
        // 10% on  pk voltages
        // approx 40mV on Avg at steady state. _NoSignal

        double Signal = 1.05;
        double noSignalTol = 0.07;
        double pkNoSignal = 0.03;
        double pkNoSignalTol = 0.7;
        double _100SignalTol = 0.13;
        double pk100Signal = 0.45;
        double pk100SignalTol = 0.1;
        double _500SignalTol = 0.13;
        double pk500Signal = 0.9;
        double pk500SignalTol = 0.1;
        double _1SignalTol = 0.13;
        double pk1Signal = 0.9;
        double pk1SignalTol = 0.13;

        public double NoSignalMin;
        public double NoSignalMax;
        public double pkNoSignalMin;
        public double pkNoSignalMax;
        public double _100SignalMin;
        public double _100SignalMax;
        public double pk100SignalMin;
        public double pk100SignalMax;
        public double _500SignalMin;
        public double _500SignalMax;
        public double pk500SignalMin;
        public double pk500SignalMax;
        public double _1SignalMin;
        public double _1SignalMax;
        public double pk1SignalMin;
        public double pk1SignalMax;

        public void init()
        {
            
             NoSignalMin = Signal * (1 - noSignalTol);
             NoSignalMax = Signal * (1 + noSignalTol);

            
             pkNoSignalMin = pkNoSignal * (1 - pkNoSignalTol);
             pkNoSignalMax = pkNoSignal * (1 + pkNoSignalTol);

           
             _100SignalMin = Signal * (1 - _100SignalTol);
             _100SignalMax = Signal * (1 + _100SignalTol);

            
             pk100SignalMin = pk100Signal * (1 - pk100SignalTol);
             pk100SignalMax = pk100Signal * (1 + pk100SignalTol);

            
             _500SignalMin = Signal * (1 - _500SignalTol);
             _500SignalMax = Signal * (1 + _500SignalTol);

            
             pk500SignalMin = pk500Signal * (1 - pk500SignalTol);
             pk500SignalMax = pk500Signal * (1 + pk500SignalTol);

           
             _1SignalMin = Signal * (1 - _1SignalTol);
             _1SignalMax = Signal * (1 + _1SignalTol);

             pk1SignalMin = pk1Signal * (1 - pk1SignalTol);
             pk1SignalMax = pk1Signal * (1 + pk1SignalTol);
        }

    }
}
