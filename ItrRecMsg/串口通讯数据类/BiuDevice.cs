using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ItrRecMsg
{
//    [Export(typeof(IBiuDevice))]
    public  class BiuDevice:  IBiuDevice
    {
        static System.Threading.Timer RegTimer = null;
        const int RegTimer_inv = 5000; //向监控服务器报告在线的频率 

        /// <summary>
        /// 生产厂商
        /// </summary>
        public virtual string Manufacturer
        {
            get
            {
                return string.Empty;
            }
        }
        public virtual string LoggerName {
            get {
                return Model;
            }
        }

        /// <summary>
        /// 系列
        /// </summary>
        public virtual string Series
        {
            get
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// 交互方式：单向、双向
        /// </summary>
        public virtual string InteractiveMode
        {
            get
            {
                return "单向";
            }
        }
        
        /// <summary>
        /// 联机帮助
        /// </summary>
        public virtual string Desc
        {
            get
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// 型号
        /// </summary>
        public virtual string Model
        {
            get
            {
                return "未定义型号";
            }
        }

        /// <summary>
        /// 类名，反射用
        /// </summary>
        public virtual string FullName
        {
            get
            {
               return Model;
            }
        }

        public virtual string PortType
        {
            get
            {
                throw new Exception("端口未定义，请定义端口");
            }
        }

        public virtual string Author
        {
            get
            {
                throw new Exception("作者未定义，请定义作者");
            }
        }

        public virtual string Date
        {
            get
            {
                throw new Exception("制作日期未定义，请定制作日期");
            }
        }

        ILog pLogger = null;
        public virtual ILog Logger
        {
            get
            {
                return pLogger;

            }

            set
            {
                pLogger = value;
            }
        }

        /// <summary>
        /// 读取配置、初始化等等
        /// </summary>
        public virtual void Init(IAdapterContainer AdapterContainer)
        {
            //Logger = CustomRollingFileLogger.GetCustomLogger(this.LoggerName, this.LoggerName);
            //RegTimer = new System.Threading.Timer(
            //  new TimerCallback(TimerReg), null, 0, RegTimer_inv);

            //string OutPutStr = string.Format(
            //    @"-------设备被载入-------    
            //     设备名称:{0}  
            //     设备端口类型:{1}
            //     适配程序制作者:{2}
            //     制作日期:{3}",this.FullName,this.PortType,this.Author,this.Date);
            //Logger.Info(OutPutStr);
        }
        private void TimerReg(object state)
        {
            //try
            //{
            //    Thread.Sleep(1000);
            //    if (MonitorManager.Instance.MonitorPort == null)
            //        return;
            //    if (MonitorManager.Instance.MonitorPort.IsConnect)
            //    {
            //        //MonitorManager.Instance.SendCommand(
            //        //    RemoteCommand.CMD_REG, new string[] { LocalConfig.Instance.ItrID }); //kim 发送至监控服务器
            //    }
            //}
            //catch (Exception ex)
            //{
            //    SystemLogManager.SystemErrorLog(ex);
            //}
        }
        /// <summary>
        /// 开始通讯
        /// </summary>
        public virtual void StartWorking()
        {
 
        }

        /// <summary>
        /// 停止通讯
        /// </summary>
        public virtual void StopWork()
        {

        }

        /// <summary>
        /// 数据重传
        /// </summary>
        public virtual void RetraData()
        {

        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="obj"></param>
        public virtual void SendData(object obj)
        {

        }

        /// <summary>
        /// 收到信息事件
        /// </summary>
        public event DataNotifyEventHandler DataRecved;

        public void OnDataRecved(object data)
        {
            if (DataRecved!=null)
            {
                DataRecved(this, data);
            }
        }

        /// <summary>
        /// 收到信息事件
        /// </summary>
        public event DataNotifyEventHandler DataSend;

        public void OnDataSend(object data)
        {
            if (DataSend != null)
            {
                DataSend(this, data);
            }
        }

        //public virtual UserControl GetConfigUI()
        //{
            
        //    return new ucFileConfig();
        //}

      
    }

  
}