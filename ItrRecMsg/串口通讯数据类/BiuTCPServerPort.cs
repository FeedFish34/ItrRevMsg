using System;
using System.Collections.Generic;

using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace ItrRecMsg
{
    public class BiuTCPServerPort : BiuPort
    {
        TcpListener tcpListener = null;
        bool isStart = false;
        // 网络流列表
        private List<NetworkStream> streams = new List<NetworkStream>();

        public BiuTCPServerPort(string NetIP, int NetMidPort)
        {

            try
            {
                HostName = NetIP;
                Port = NetMidPort;
                tcpListener = new TcpListener(IPAddress.Parse(HostName), Port);
            }
            catch (Exception)
            {
            }

        }

        public string HostName { get; set; }
        public int Port { get; set; }

        public void Open()
        {
            try
            {
                tcpListener.Start();
                isStart = true;
                //然后?
                Receive();
            }
            catch (Exception ex)
            {
                Lib.LogManager.Logger.LogException(ex);
            }
        }

        public void Close()
        {
            tcpListener.Stop();
            streams.Clear();
            isStart = false;
        }

        public void Send(byte[] buff)
        {
            SendEventAndTarget(buff);
            //服务发东西？

        }

        public void Receive()
        {

            try
            {
                // 后台线程1：用于接收tcp连接请求，并将网络流加入列表。随主线程的退出而退出。
                new Thread(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(100);// 可以根据需要设置时间
                        if (!isStart) return;
                        try
                        {
                            if (!tcpListener.Pending())
                            {
                                continue;
                            }
                            var client = tcpListener.AcceptTcpClient();
                            // 下面属性根据需要进行设置
                            client.ReceiveBufferSize = 1024 * 1024;
                            // client.ReceiveTimeout
                            client.SendBufferSize = 1024 * 1024;
                            // client.SendTimeout
                            if (!client.Connected)
                            {
                                continue;
                            }
                            streams.Add(client.GetStream());


                        }
                        catch (Exception ex)
                        {
                            new WorkerThreadExceptionHandlerDelegate(
                           WorkerThreadExceptionHandler).BeginInvoke(ex
                           , null
                           , null);
                            Reset();
                        }
                    }
                })
                { IsBackground = true }.Start();

                // 后台线程2：用于接收请求，并作出响应。随主线程的退出而退出。
                new Thread(() =>
                {
                    while (true)
                    {
                        if (!isStart) return;
                        Thread.Sleep(100);// 可以根据需要设置时间
                        if (streams == null || streams.Count == 0 || !isStart)
                        {
                            continue;
                        }
                        try
                        {
                            //streams = streams.Where(s => s.CanRead && s.CanWrite).ToList();
                            foreach (var stream in streams)
                            {
                                if (!stream.CanRead || !stream.CanWrite || !isStart) continue;
                                AsyncReceiveBytes(stream);
                            }

                            if (streams.Count > 50)
                            {
                                streams.RemoveRange(0, 30);
                            }
                        }
                        catch (Exception ex)
                        {
                            new WorkerThreadExceptionHandlerDelegate(
                           WorkerThreadExceptionHandler).BeginInvoke(ex
                           , null
                           , null);
                            Reset();
                        }
                    }
                })
                { IsBackground = true }.Start();



            }
            catch (Exception ex)
            {
                Lib.LogManager.Logger.LogException(ex);
            }
        }

        private void Reset()
        {
            tcpListener.Stop();
            streams.Clear();
            isStart = false;
            tcpListener.Start();
            isStart = true;
        }

        // 发送事件和目标的入口
        public void SendEventAndTarget(byte[] senddata)
        {
            if (streams == null || streams.Count == 0)
            {
                return;
            }
            //streams = streams.Where(s => s.CanRead && s.CanWrite).ToList();

            foreach (var stream in streams)
            {
                if (!stream.CanRead || !stream.CanWrite) continue;
                AsyncSendBytes(stream, senddata);// todo:这里将待发送的C#对象转换的字节数组替换new byte[0]。
            }
        }

        /// <summary>
        /// 工作线程的异常处理
        /// </summary>
        /// <param name="e"></param>
        public void WorkerThreadExceptionHandler(Exception e)
        {
            ///添加其他的处理代码

            ///通知全局异常处理程序
            MainUIThreadExceptionHandler(
                this, new System.Threading.
                ThreadExceptionEventArgs(e));
        }

        public void MainUIThreadExceptionHandler(object
            sender, ThreadExceptionEventArgs e)
        {
            Lib.LogManager.Logger.LogException(e.Exception);
        }

        string curMsg = string.Empty;
        void AsyncReceiveBytes(NetworkStream stream)
        {
            try
            {
                // 短时后台线程：用于处理网络流的读操作，处理完成后即归还线程池。
                // 每个网络流都会分配一个线程。
                //ThreadPool.SetMaxThreads();根据需要设置。
                ThreadPool.QueueUserWorkItem(delegate
                {
                    try
                    {
                        var buffer = new byte[1024 * 1024];// 1024：根据需要进行设置。
                        if (!stream.DataAvailable) return;
                        var a = stream.Read(buffer, 0, buffer.Length);
                        var data = System.Text.Encoding.Default.GetString(buffer, 0, a);
                        if (SpcialItrID == "ASTM")
                        {
                            if (data.Contains(((char)6).ToString()))
                            {
                                if (AckReceived != null)
                                    AckReceived(stream, null);
                                return;
                            }
                            if (data.Contains(((char)21).ToString()))
                            {
                                if (NakReceived != null)
                                    NakReceived(stream, null);
                                return;
                            }
                            if (!data.Contains(((char)04).ToString()))
                            {
                                curMsg = curMsg + data;
                                byte[] msg1 = System.Text.Encoding.Default.GetBytes(data);
                                foreach (byte b in msg1)
                                {
                                    if (b == ASTMCommon.cETX_3 || b == ASTMCommon.cSTX_2 || b == ASTMCommon.cENQ_5)
                                    {
                                        LogRevMsg.LogText("发送", ((char)6).ToString());
                                        AsyncSendBytes(stream, System.Text.UTF8Encoding.ASCII.GetBytes(((char)6).ToString()));
                                    }
                                }
                                return;
                            }
                            else
                            {

                                LogRevMsg.LogText("接收:", data);
                                byte[] msg1 = System.Text.Encoding.Default.GetBytes(data);
                                foreach (byte b in msg1)
                                {
                                    if (b == ASTMCommon.cETX_3 || b == ASTMCommon.cSTX_2 || b == ASTMCommon.cENQ_5)
                                    {
                                        LogRevMsg.LogText("发送", ((char)6).ToString());
                                        AsyncSendBytes(stream, System.Text.UTF8Encoding.ASCII.GetBytes(((char)6).ToString()));
                                    }
                                }
                                curMsg += data;
                            }
                        }
                        else if (SpcialItrID == "HL7")
                        {
                            LogRevMsg.LogText("接收:", data);
                            var fs = ((char)28).ToString();
                            var rpstring = data.Replace(fs, "").Trim();
                            if (rpstring.Length > 500)
                            {
                                curMsg = curMsg + data;
                                if (rpstring.Substring(rpstring.Length - 3) != "||F")
                                {
                                    return;
                                }
                            }
                            else
                            {
                                curMsg = data;
                            }
                        }
                        else if (SpcialItrID == "FS")
                        {
                            LogRevMsg.LogText("接收:", data);
                            if (!data.Contains(((char)28).ToString()) && !data.Contains(((char)0).ToString()))
                            {

                                curMsg = curMsg + data;

                                return;
                            }
                            else
                            {
                                curMsg += data;
                            }
                        }
                        else if (SpcialItrID == "EOT")
                        {
                            LogRevMsg.LogText("接收:", data);
                            if (!data.Contains(((char)04).ToString()) && !data.Contains(((char)0).ToString()))
                            {
                                curMsg = curMsg + data;

                                return;
                            }
                            else
                            {
                                curMsg += data;
                            }
                        }
                        else if (SpcialItrID == "EE" || SpcialItrID == "</TRANSMIT>" || SpcialItrID == "$")
                        {
                            if (!data.Contains(SpcialItrID))
                            {
                                curMsg = curMsg + data;

                                return;
                            }
                            else
                            {
                                curMsg += data;
                            }
                        }
                        else if (SpcialItrID == "}")
                        {
                            LogRevMsg.LogText("接收:", data);
                            if (!data.Contains(((char)28).ToString()) && !data.Contains("}"))
                            {

                                curMsg = curMsg + data;

                                return;
                            }
                            else
                            {
                                curMsg += data;
                            }
                        }
                        else
                        {
                            curMsg = data;
                        }
                        // 本地存储
                        LogRevMsg.Log("11022", curMsg);

                        byte[] msg = System.Text.Encoding.Default.GetBytes(curMsg);
                        string TureMessage = curMsg.Replace(ASTMCommon.cSTX_2.ToString(), "").
                    Replace(ASTMCommon.cETX_3.ToString(), "").
                    Replace(ASTMCommon.cEOT_4.ToString(), "").
                    Replace(ASTMCommon.cENQ_5.ToString(), "");
                        string[] str =
                            TureMessage.Split(new char[] { ASTMCommon.cLF_10, ASTMCommon.cCR_13 }, StringSplitOptions.RemoveEmptyEntries);
                        string sampleId = str[0].Split('-')[0];
                        string result = "";
                        Random random = new Random();
                        result = random.Next().ToString();
                        ASTMDAO dao = new ASTMDAO();
                        dao.UpdateOrInsertASTM(sampleId, result);
                        string ResMsg = ASTMCommon.cENQ_5.ToString() + ASTMCommon.cSTX_2.ToString() + sampleId + "-" + result + ASTMCommon.cETX_3.ToString() + ASTMCommon.cEOT_4.ToString();
                        LogRevMsg.LogText("发送:", ResMsg);
                        AsyncSendBytes(stream, UTF8Encoding.ASCII.GetBytes(ResMsg));
                        curMsg = string.Empty;

                        //callback(stream);
                    }
                    catch (Exception ex)
                    {
                        new WorkerThreadExceptionHandlerDelegate(
                       WorkerThreadExceptionHandler).BeginInvoke(ex
                       , null
                       , null);
                        Reset();
                    }
                });
            }
            catch (Exception ex)
            {
                Lib.LogManager.Logger.LogException(ex);
            }
        }
        public void AsyncSendBytes(NetworkStream stream, byte[] bytes)
        {
            // 短时后台线程：用于处理网络流的写操作，处理完成后即归还线程池。
            // 每个网络流都会分配一个线程。
            //ThreadPool.SetMaxThreads();根据需要设置。
            try
            {
                ThreadPool.QueueUserWorkItem(delegate
                {

                    try
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }
                    catch (Exception ex)
                    {
                        new WorkerThreadExceptionHandlerDelegate(
                     WorkerThreadExceptionHandler).BeginInvoke(ex
                    , null
                    , null);
                        Reset();
                    }

                });
            }
            catch (Exception ex)
            {
                Lib.LogManager.Logger.LogException(ex);
                //MessageBox.Show("远程主机主动断开此连接！");// 也可以做其它处理。
            }
        }

        public override void Dispose()
        {
            Close();
            streams = null;
            tcpListener = null;
        }


        /// <summary>
        /// 特殊仪器处理
        /// </summary>
        public string SpcialItrID
        {
            get;
            set;
        }
        public event ByteReceivedEventHandler DataReceived;

        public event EventHandler AckReceived;

        public event EventHandler NakReceived;
    }
}
