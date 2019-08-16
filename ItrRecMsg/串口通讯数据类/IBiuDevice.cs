using log4net;
using System.Windows.Forms;

namespace ItrRecMsg
{

    public interface IBiuDevice
    {
        //UserControl  GetConfigUI();


        /// <summary>
        /// 生产厂商
        /// </summary>
        string Manufacturer { get; }

        ILog Logger { get; set; }
        /// <summary>
        /// 系列
        /// </summary>
        string Series { get; }


        /// <summary>
        /// 型号
        /// </summary>
        string Model { get; }

        /// <summary>
        /// 类名，反射用
        /// </summary>
        string FullName { get; }

        ///// <summary>
        ///// 接收到内容
        ///// </summary>
        //public abstract object RecvData { get; }

        /// <summary>
        /// 接口方式：串口、网口、文本等等
        /// </summary>
        string PortType { get; }


        /// <summary>
        /// 交互方式：单向、双向
        /// </summary>
        string InteractiveMode { get; }

        /// <summary>
        /// 作者
        /// </summary>
        string Author { get; }

        /// <summary>
        /// 最后版本日期
        /// </summary>
        string Date { get; }

        /// <summary>
        /// 联机帮助
        /// </summary>
        string Desc{ get; }

        /// <summary>
        /// 读取配置、初始化等等
        /// </summary>
        void Init(IAdapterContainer AdapterContainer);

        /// <summary>
        /// 开始通讯
        /// </summary>
        void StartWorking();

        /// <summary>
        /// 数据重传
        /// </summary>
        void RetraData();
        /// <summary>
        /// 停止通讯
        /// </summary>
        void StopWork();

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="obj"></param>
        void SendData(object obj);



        /// <summary>
        /// 收到信息事件
        /// </summary>
        event DataNotifyEventHandler DataRecved;

        void OnDataRecved(object data);

        /// <summary>
        /// 收到信息事件
        /// </summary>
        event DataNotifyEventHandler DataSend;

        void OnDataSend(object data);

        
    }

  
}