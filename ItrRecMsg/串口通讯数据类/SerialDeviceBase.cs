using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ItrRecMsg
{
    /// <summary>
    /// 串口仪器基类
    /// </summary>
    public abstract class SerialDeviceBase : BiuDevice
    {
        public BiuSerialPort port = null;
        Hashtable InProcessList = new Hashtable(); //创建一个Hashtable实例
        public SerialDeviceBase()
        {

        }
        public virtual Encoding DefaultEncoding
        {
            get
            {
                return Encoding.Default;
            }
        }
        /// <summary>
        /// 数据处理频率
        /// </summary>
        public virtual int Speed
        {
            get
            {
                return 4;
            }
        }


        /// <summary>
        /// 结束符
        /// </summary>
        public virtual string EndChar
        {
            get
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// 是否双向
        /// </summary>
        public virtual bool IsASTM
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsNeedAck
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// 读取配置、初始化等等
        /// </summary>
        public override void Init(IAdapterContainer AdapterContainer)
        {

            base.Init(AdapterContainer);
        }


        public virtual void port_EotReceived(object sender, EventArgs e)
        {
        }

        public virtual void port_EnqReceived(object sender, EventArgs e)
        {

        }

        public virtual void port_NakReceived(object sender, EventArgs e)
        {

        }

        public virtual void port_AckReceived(object sender, EventArgs e)
        {
            SendData();
        }

        public virtual void port_RS232Received(object sender, byte[] buff)
        {
        }

        /// <summary>
        /// 开始
        /// </summary>
        public override void StartWorking()
        {
            if (port == null)
            {
                port = new BiuSerialPort();
                port.ETXStr = EndChar;
                port.IsASTM = IsASTM;
                port.IsNeedAck = IsNeedAck;
                port.DataReceived += port_DataReceived;
                port.AckReceived += port_AckReceived;
                port.NakReceived += port_NakReceived;
                port.EnqReceived += port_EnqReceived;
                port.EotReceived += port_EotReceived;
                port.SohReceived += port_SohReceived;
                port.EtxReceived += port_EtxReceived;
                port.RS232Received += port_RS232Received;
                //port.CreateTimer(Speed);
                port.log = Logger;
                port.DefaultEncoding = this.DefaultEncoding;
            }


            if (!port.IsOpen)
                port.Open();

        }



        public virtual void port_EtxReceived(object sender, EventArgs e)
        {

        }

        public virtual void port_SohReceived(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// 停止
        /// </summary>
        public override void StopWork()
        {
            if (port == null)
                return;

            if (port.IsOpen)
                port.Close();


            port.Dispose();
            port = null;
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="obj"></param>
        public override void SendData(object obj)
        {
            if (!port.IsOpen)
                port.Open();

            byte[] data = Encoding.Default.GetBytes(obj.ToString());
            port.Send(data);
        }

        void port_DataReceived(object sender, FileReadEventArgs buff)
        {
            if (InProcessList.ContainsKey(buff.FilePath))
                return;

            Thread t = new Thread(ThreadHandleReceivedData);
            t.IsBackground = true;
            t.Start(buff);
        }
        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="parameter"></param>
        void ThreadHandleReceivedData(object parameter)
        {
            FileReadEventArgs eventarg = parameter as FileReadEventArgs;
            try
            {
                InProcessList.Add(eventarg.FilePath, eventarg);
                //数据处理
                if (eventarg == null || string.IsNullOrEmpty(eventarg.Message))
                {
                    return;
                }

                //通知主界面
                OnDataRecved(eventarg.Message);

                //数据处理
                HandleData(eventarg);

                //清空文件
                FileManager.DeleteFile(eventarg.FilePath);
            }
            catch (SqlException ex)
            {
                Lib.LogManager.Logger.LogException(ex);
            }
            catch (Exception ex)
            {
                FileManager.DeleteFile(eventarg.FilePath);
                Lib.LogManager.Logger.LogException(ex);
            }
            finally
            {
                InProcessList.Remove(eventarg.FilePath);
            }
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        /// <param name="eventarg"></param>
        public virtual void HandleData(FileReadEventArgs eventarg)
        {
        }
        public virtual void SendData()
        {
        }
        public override string Model
        {
            get { return ""; }
        }

        public override string PortType
        {
            get { return "串口"; }
        }

        public override string Author
        {
            get { return "hb"; }
        }

        public override string Desc
        {
            get
            {
                return "";
            }
        }

        public override string Date
        {
            get { return "2016-08-30"; }
        }

        public override string FullName
        {
            get { return GetType().FullName; }
        }
    }
}
