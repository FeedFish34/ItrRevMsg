using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows.Forms;
using System.Text;
using System.IO;
using log4net;
using System.Threading;

namespace ItrRecMsg
{
    public class BiuSerialPort : BiuPort
    {
        public ILog log { get; set; }

        //取得当前字符编码
        public Encoding DefaultEncoding { get; set; }
        SerialPort comm = null;
        bool isDoing = false;
        object DoingLock = new object();

        /// <summary>
        /// 保存一次传送的完整信息
        /// </summary>
        string currStr = string.Empty;

        /// <summary>
        /// 保存一次传送的完整信息(STX-ETX)
        /// </summary>
        string SingleStr = string.Empty;

        public BiuSerialPort()
        {
            try
            {
                comm = new SerialPort();

                PortName = "COM2";
                Parity = (Parity)Enum.Parse(typeof(Parity), "0");
                BaudRate = Convert.ToInt32(9600);
                DataBits = Convert.ToInt32(8);
                StopBits = (StopBits)Enum.Parse(typeof(StopBits), "1");
                DefaultEncoding = Encoding.Default;
                comm.ReceivedBytesThreshold = 1;
                comm.ReadBufferSize = 1024 * 1024;
                comm.WriteBufferSize = 1024 * 20;
                comm.DataReceived += comm_DataReceived_Kim;
            }
            catch(Exception ex)
            {
                log.Error(ex.Message);
                MessageBox.Show(ex.Message);
            }
        }

        public List<string> GetPortNames()
        {
            string[] str = SerialPort.GetPortNames();
            if (str == null)
            {
                return new List<string>();
            }
            else
            {
                return new List<string>(str);
            }
        }

        /// <summary>
        /// 端口名称
        /// </summary>
        public string PortName
        {
            get
            {
                return this.comm.PortName;
            }
            set
            {
                this.comm.PortName = value;
            }
        }

        /// <summary>
        /// 波特率
        /// </summary>
        public int BaudRate
        {
            get
            {
                return this.comm.BaudRate;
            }
            set
            {
                this.comm.BaudRate = value;
            }
        }

        /// <summary>
        /// 停止位
        /// </summary>
        public StopBits StopBits
        {
            get
            {
                return this.comm.StopBits;
            }
            set
            {
                this.comm.StopBits = value;
            }
        }

        /// <summary>
        /// 数据位
        /// </summary>
        public int DataBits
        {
            get
            {
                return this.comm.DataBits;
            }
            set
            {
                this.comm.DataBits = value;
            }
        }

        /// <summary>
        /// 奇偶校验检查协议
        /// </summary>
        public Parity Parity
        {
            get
            {
                return this.comm.Parity;
            }
            set
            {
                this.comm.Parity = value;
            }
        }
        /// <summary>
        /// 结束符号
        /// </summary>
        public string ETXStr
        {
            get;
            set;
        }

        public bool IsOpen
        {
            get
            {
                return this.comm.IsOpen;
            }
        }

        /// <summary>
        /// 收到信息发回复
        /// </summary>
        public bool IsNeedAck { get; set; }

        /// <summary>
        /// 是否astm 应答模式
        /// </summary>
        public bool IsASTM { get; set; }

        public void Open()
        {
            try
            {
                Lib.LogManager.Logger.LogInfo("打开串口");
                this.comm.Open();
                lock (DoingLock)
                {
                    isDoing = true;
                    CreateTimer(1);
                }
            }
            catch(Exception ex)
            {
                Lib.LogManager.Logger.LogInfo("打开串口失败");
            }

        }

        public void Close()
        {
            lock (DoingLock)
            {
                isDoing = false;
                currStr = string.Empty;
                this.comm.Close();
                SohReceived = null;
            }




        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="buff"></param>
        public void Send(byte[] buff)
        {
            string recv = DefaultEncoding.GetString(buff);
            LogRevMsg.LogText("发送", recv);
            Lib.LogManager.Logger.LogInfo("发送:" + "\r" + recv);
            this.comm.Write(buff, 0, buff.Length);
            //MachineAdapter.Common.Manager.MonitorManager.Instance.SendLog(recv);
        }
        void comm_DataReceived_Kim(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                //log.Info("开始接收数据");
                byte[] RS232DataBuff = GetData(comm);
                SerialHandleObject handle = new SerialHandleObject();
                if (RS232Received != null)
                {
                    RS232Received(handle, RS232DataBuff);
                    if (handle.handled)
                    {
                        return;
                    }
                }
                //Encoding utf8 = Encoding.GetEncoding(65001);
                //Encoding gb2312 = Encoding.GetEncoding("gb2312");
                string recv = DefaultEncoding.GetString(RS232DataBuff);
                //LogRevMsg.LogText("单次接收", recv);
                //log.Info("单次接收" + "\r" + recv);

                if (!IsASTM)
                {
                    currStr += recv;

                    if (CheckEndStr(currStr, ETXStr))
                    {
                        foreach (byte b in RS232DataBuff)
                        {
                            handleComand(b);
                        }
                        LogRevMsg.LogText("接收", currStr);
                        Lib.LogManager.Logger.LogInfo("接收" + "\r" + currStr);
                        //MonitorManager.Instance.SendCommand(
                        //    RemoteCommand.CMD_LOG, new string[] { LocalConfig.Instance.ItrID, currStr }); //kim 发送至监控服务器
                        if (false)
                        {
                            LogRevMsg.GetFile(currStr);
                        }
                        else
                        {
                            LogRevMsg.GetBufferLog(currStr);
                        }
                        currStr = string.Empty;
                    }
                }
                else
                {
                    try
                    {
                        if (RS232DataBuff.Length == 2 && RS232DataBuff[0] == ASTMCommon.ACK_6 && RS232DataBuff[1] == ASTMCommon.EOT_4)
                        {
                            byte[] buff = new byte[1];
                            buff[0] = RS232DataBuff[0];
                            RS232DataBuff = buff;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex.Message);
                    }
                    foreach (byte b in RS232DataBuff)
                    {
                        if (IsASTM)
                        {
                            HandleAstmCommand(b);
                        }
                        char RecChar = Convert.ToChar(b);
                        currStr += RecChar;
                        if (CheckEndStr(currStr, ETXStr))
                        {
                            LogRevMsg.LogText("接收", currStr);
                            Lib.LogManager.Logger.LogInfo("接收" + "\r" + currStr);
                            //MonitorManager.Instance.SendCommand(
                            //    RemoteCommand.CMD_LOG, new string[] { LocalConfig.Instance.ItrID, currStr }); //kim 发送至监控服务器 
                            LogRevMsg.GetBufferLog(currStr);
                            currStr = string.Empty;
                        }
                        //if (CheckEndStr(SingleStr, ETXStr) || SingleStr.Contains(ASTMCommon.cENQ_5.ToString()))
                        //{
                        //    LogRevMsg.LogText("接收", SingleStr);
                        //    log.Info("接收" + "\r" + SingleStr);
                        //    //MonitorManager.Instance.SendCommand(
                        //    //    RemoteCommand.CMD_LOG, new string[] { LocalConfig.Instance.ItrID, currStr }); //kim 发送至监控服务器 
                        //    if (!SingleStr.Contains(ASTMCommon.cEOT_4.ToString()))
                        //    {
                        //        SingleStr = string.Empty;
                        //    }
                        //}
                        //if (SingleStr.Contains(ASTMCommon.cEOT_4.ToString()))
                        //{
                        //    if (ETXStr != ASTMCommon.cEOT_4.ToString())
                        //    {
                        //        LogRevMsg.LogText("接收", SingleStr);
                        //        log.Info("接收" + "\r" + SingleStr);
                        //    }
                        //    //MonitorManager.Instance.SendCommand(
                        //    //    RemoteCommand.CMD_LOG, new string[] { LocalConfig.Instance.ItrID, currStr }); //kim 发送至监控服务器
                        //    LogRevMsg.GetBufferLog(currStr);
                        //    currStr = string.Empty;
                        //    SingleStr = string.Empty;
                        //    break;
                        //}
                    }
                }
                if (IsNeedAck)
                {
                    if (RS232DataBuff.Length == 1)
                    {
                        if (RS232DataBuff[0] == ASTMCommon.NAK_21)
                        {
                            return;
                        }
                        if (RS232DataBuff[0] == ASTMCommon.ACK_6)
                        {
                            return;
                        }
                    }
                    SendACK();
                }
            }
            catch(Exception ex)
            {

            }
        }

        bool CheckEndStr(string currStr, string ETXStr)
        {
            if (string.IsNullOrEmpty(currStr) || string.IsNullOrEmpty(ETXStr))
            {
                return false;
            }

            string[] strArry = ETXStr.Split(';');

            if (ETXStr.Contains("or"))
            {
                string[] str = ETXStr.Replace("or", " ").Split(' ');
                return (currStr.Contains(str[0]) || currStr.Contains(str[1]));
            }
            if (ETXStr.Contains("and"))
            {
                string[] str = ETXStr.Replace("and", " ").Split(' ');
                bool EtxStr = currStr.Contains(str[0]);
                bool CrLFStr = false;
                if (currStr.Length > 2)
                {
                    var i = currStr.Substring(currStr.Length - 2);
                    CrLFStr = currStr.Substring(currStr.Length - 2).Contains(str[1]);
                }
                return (EtxStr && CrLFStr);
            }
            if (strArry.Length == 1)
            {
                return currStr.Contains(ETXStr);
            }


            if (strArry.Length == 2)
            {
                return currStr.Contains(strArry[0]) && currStr.Contains(strArry[1]);
            }

            return false;
        }

        bool handleComand(byte b)
        {
            switch (b)
            {
                case ASTMCommon.EOT_4:
                    if (EotReceived != null)
                        EotReceived(this, null);
                    return false;
                case ASTMCommon.ETX_3:
                    if (EtxReceived != null)
                        EtxReceived(this, null);
                    return false;
                default:
                    return false;
            }
        }

        bool HandleAstmCommand(byte b)
        {
            if (!IsASTM)
                return false;

            switch (b)
            {
                case ASTMCommon.ETX_3:
                    if (EtxReceived != null)
                        EtxReceived(this, null);
                    return false;

                case ASTMCommon.SOH_1:
                    if (SohReceived != null)
                        SohReceived(this, null);
                    return false;

                case ASTMCommon.NAK_21:
                    if (NakReceived != null)
                        NakReceived(this, null);
                    return false;
                case ASTMCommon.ACK_6:
                    if (AckReceived != null)
                        AckReceived(this, null);
                    return true;

                case ASTMCommon.ENQ_5:
                    currStr = string.Empty;
                    if (EnqReceived != null)
                        EnqReceived(this, null);
                    else
                    {
                        SendACK();
                    }
                    return false;
                case ASTMCommon.EOT_4:
                    if (EotReceived != null)
                    {
                        //Thread.Sleep(500);
                        EotReceived(this, null);
                    }
                    return false;
                case ASTMCommon.LF_10:
                    if (LfRecived != null)
                        LfRecived(LfRecived, null);
                    SendACK();
                    return false;
                default:
                    return false;
            }
        }

        private byte[] GetData(SerialPort comm)
        {
            byte[] RS232DataBuff;
            using (MemoryStream TempBuffer = new MemoryStream())
            {
                try
                {

                    string g_s_Data = string.Empty;
                    do
                    {
                        int count = comm.BytesToRead;
                        if (count <= 0)
                            break;
                        RS232DataBuff = new byte[count];
                        comm.Read(RS232DataBuff, 0, count);

                        foreach (byte b in RS232DataBuff)
                        {
                            g_s_Data += Convert.ToChar(b);
                            TempBuffer.WriteByte(b);
                        }
                        TempBuffer.Flush();
                    } while (comm.BytesToRead > 0);
                    return TempBuffer.ToArray();

                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                    return new byte[0];
                }

            }

        }

        private void SendACK()
        {
            if (OnAckSend != null)
            {
                OnAckSend(comm, null);
                return;
            }

            byte[] data = { ASTMCommon.ACK_6 };
            comm.Write(data, 0, data.Length);
            Lib.LogManager.Logger.LogInfo("发送:" + "\r" + ASTMCommon.cACK_6.ToString());
        }


        /// <summary>
        /// 创建计时器
        /// </summary>
        /// <param name="seconds">秒</param>
        public void CreateTimer(int seconds)
        {
            Thread JobThread = new Thread(HandleJob);
            JobThread.IsBackground = true;
            JobThread.Start(seconds * 1000);

            //timer = new Timer();
            //timer.Enabled = true;
            //timer.Interval = seconds * 1000;
            //timer.Tick += timer_Tick;
            //isDoing = false;
            //timer.Stop();
        }

        private void HandleJob(object parameter)
        {
            int SleepCount = Convert.ToInt32(parameter);
            while (true)
            {
                lock (DoingLock)
                {

                    Thread.Sleep(SleepCount);
                    if (isDoing)
                        ReadFileLog();
                    else
                        return;
                }
            }
        }




        private void ReadFileLog()
        {
            List<string> fileList = FileManager.GetFileItems(LogRevMsg.BasePath);

            foreach (var filepath in fileList)
            {
                try
                {
                    string ExtName = System.IO.Path.GetExtension(filepath);
                    if (ExtName.ToUpper() == ".BAK")
                        continue;

                    //Encoding fileEncoding = TxtFileEncoding.GetEncoding(filepath, Encoding.GetEncoding("GB2312"));//取得这txt文件的编码
                    StreamReader sr = new StreamReader(filepath, DefaultEncoding);//用该编码创建StreamReader 
                    string data = sr.ReadToEnd();
                    if (DataReceived != null)
                    {
                        DataReceived(this, new FileReadEventArgs(filepath, data));

                    }
                    sr.Close();


                }
                catch (Exception)
                {

                }

            }
        }

        public override void Dispose()
        {
            this.comm.Dispose();
        }

        public event FileReadEventHandler DataReceived;

        public event ByteReceivedEventHandler RS232Received;

        public event EventHandler OnAckSend;

        public event EventHandler SohReceived;
        public event EventHandler AckReceived;
        public event EventHandler NakReceived;
        public event EventHandler EnqReceived;
        public event EventHandler EotReceived;
        public event EventHandler LfRecived; //一个数据帧被接收，为校验和后续处理做准备
        public event EventHandler EtxReceived;

    }



}
