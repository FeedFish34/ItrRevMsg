using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace ItrRecMsg
{
    /// <summary>
    /// ASTM协议通用变量与方法
    /// </summary>
    public class ASTMCommon
    {
        #region 相关常量

        #region ASTM协议相关常量
        // <summary>
        /// 正文结束
        /// </summary>
        public const byte ETX_3 = 3;
        public static char cETX_3 = Convert.ToChar(ETX_3); //ASTM 正文结束

        /// <summary>
        /// 查询
        /// </summary>
        public const byte ENQ_5 = 5;
        public static char cENQ_5 = Convert.ToChar(ENQ_5); //ASTM 查询

        /// <summary>
        /// 肯定应答
        /// </summary>
        public const byte ACK_6 = 6;
        public static char cACK_6 = Convert.ToChar(ACK_6); //ASTM 肯定应答

        /// <summary>
        /// 否定应答
        /// </summary>
        public const byte NAK_21 = 21;
        public static char cNAK_21 = Convert.ToChar(NAK_21); //ASTM 否定应答

        /// <summary>
        /// 传输块结束
        /// </summary>
        public const byte ETB_23 = 23;
        public static char cETB_23 = Convert.ToChar(ETB_23); //ASTM 传输块结束

        /// <summary>
        /// 替代
        /// </summary>
        public const byte SUB_26 = 26;
        public static char cSUB_26 = Convert.ToChar(SUB_26); //ASTM 替代

        // <summary>
        /// 空字符
        /// </summary>
        public const byte NUL_0 = 0;
        public static char cNUL_0 = Convert.ToChar(NUL_0); //ASTM 空字符
        /// <summary>
        /// 标题开始
        /// </summary>
        public const byte SOH_1 = 1;
        public static char cSOH_1 = Convert.ToChar(SOH_1); //ASTM 标题开始

        /// <summary>
        /// 换行
        /// </summary>
        public const byte LF_10 = 10;
        public static char cLF_10 = Convert.ToChar(LF_10); //ASTM 换行

        /// <summary>
        /// 回车
        /// </summary>
        public const byte CR_13 = 13;
        public static char cCR_13 = Convert.ToChar(CR_13); //ASTM 回车

        /// <summary>
        /// 正文开始
        /// </summary>
        public const byte STX_2 = 2;
        public static char cSTX_2 = Convert.ToChar(STX_2); //ASTM 正文开始

        /// <summary>
        /// 传输结束
        /// </summary>
        public const byte EOT_4 = 4;
        public static char cEOT_4 = Convert.ToChar(EOT_4); //ASTM 传输结束

        /// <summary>
        /// FS文件分隔符
        /// </summary>
        public const byte FS_28 = 28;
        public static char cFS_28 = Convert.ToChar(FS_28); //ASTM 文件分隔符

        /// <summary>
        /// VT纵向制表
        /// </summary>
        public const byte VT_11 = 11;
        public static char cVT_11 = Convert.ToChar(VT_11); //ASTM 纵向制表

        /// <summary>
        /// RS记录分隔符
        /// </summary>
        public const byte RS_30 = 30;
        public static char cRS_30 = Convert.ToChar(RS_30); //ASTM 记录分隔符

        /// <summary>
        /// US单元分隔符
        /// </summary>
        public const byte US_31 = 31;
        public static char cUS_31 = Convert.ToChar(US_31); //ASTM 单元分隔符

        /// <summary>
        /// 超时
        /// </summary>
        public const byte OVER_TIME_7 = 7;
        public static char cOVER_TIME_7 = Convert.ToChar(OVER_TIME_7); //ASTM 超时

        /// <summary>
        /// 允许发送的消息的最大长度
        /// </summary>
        public const int MsgMaxLen = 240;


        #endregion

        #endregion

        #region 消息队列
        /// <summary>
        /// 服务器消息队列，保存收到的消息
        /// </summary>
        //public static System.Collections.Concurrent.ConcurrentQueue<Frame> ServerQueue =
        //    new System.Collections.Concurrent.ConcurrentQueue<Frame>();

        #endregion

        #region 套接字相关操作函数
        /// <summary>
        /// 发送信息函数
        /// </summary>
        /// <param name="socket">发送的套接字</param>
        /// <param name="buffer">发送的数据</param>
        public static void SendTo(Socket socket, byte[] buffer)
        {
            try
            {
                socket.Send(buffer);
            }  //try
            catch(Exception)
            {
                //SystemManager.RecordError("LisMachine.ASTMNetHelper",
                //    "ASTMCommon", "SendTo", ex.GetType().ToString(), ex.Message, "合法");
            }  //end try - catch
        }  //end SendTo

        /// <summary>
        /// 接收信息函数
        /// </summary>
        /// <param name="socket">接收的套接字</param>
        /// <param name="buffer">读取的消息保存的数组</param>
        /// <returns>正确收取到的数据长度</returns>
        public static int ReceiveFrom(Socket socket, out byte[] buffer)
        {
            try
            {
                buffer = new byte[256];
                bool isContinue = true;
                byte[] currentByte = new byte[1];
                int index = 0;
                while (isContinue)
                {
                    int len = socket.Receive(currentByte, 1, SocketFlags.None);
                    if(0 == len)
                    {
                        return 0;
                    }  //end if 
                    switch (currentByte[0])
                    {
                        case ASTMCommon.EOT_4: case ASTMCommon.ENQ_5: case ASTMCommon.ACK_6:
                        case ASTMCommon.LF_10:  case ASTMCommon.NAK_21:
                            isContinue = false;
                            break;
                    }  //end switch
                    buffer[index] = currentByte[0]; 
                    index++;
                    if (250 == index)
                    {
                        return index;
                    }  //end if
                }  //end while
                return index;
            }  //end try
            catch(Exception ex)
            {
                if (null != socket)
                {            
                    string msg = socket.RemoteEndPoint.ToString() + ex.Message;
                    //SystemManager.RecordError("LisMachine.ASTMNetHelper",
                    //"ASTMCommon", "ReceiveFrom", ex.GetType().ToString(), msg, "合法");
                }  //end if 
                buffer = null;
                return 0;
            }  //end try - catch
        }  //ebd ReceiveFrom

        /// <summary>
        /// 接收指定发送者的数据
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="buffer">接收到的数据</param>
        /// <param name="sender">指定发送者的IP</param>
        /// <param name="sec">接收时限</param>
        /// <returns></returns>
        public static int ReceiveWithTimeLimit(int second, Socket socket, out byte[] buffer)
        {
            socket.ReceiveTimeout = second * 1000;
            int len = ReceiveFrom(socket, out buffer);
            socket.ReceiveTimeout = -1;
            return len;
        }  //end ReceiveWithTimeLimit
        #endregion

        #region 数据帧相关函数
        /// <summary>
        /// 检查帧是否正确并从帧中解析出数据
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="frameLen"></param>
        /// <param name="result">帧中包含的数据</param>
        /// <returns>返回-1表明帧格式错误,-2表示检验和不对，否则返回帧序号</returns>
        public static int AnalyzeFrame(byte[] frame, int frameLen, out string result, out byte frameType)
        {
            result = "";
            frameType = 0;
            string pattern1 = string.Format(@"{0}[0-7][\s\S]*[{1}|{2}][0-9a-fA-F][0-9a-fA-F]\r\n",
                (char)ASTMCommon.STX_2, (char)ASTMCommon.ETB_23, (char)ASTMCommon.ETX_3);
            if (!Regex.IsMatch(Encoding.ASCII.GetString(frame, 0, frameLen), pattern1))
            {
                return -1;
            }  //end if 
            int index = 0, textStartIndex, textEndIndex;
            while (frame[index] != ASTMCommon.STX_2) index++;
            textStartIndex = index + 2;
            while (frame[index] != ASTMCommon.ETB_23 && frame[index] != ASTMCommon.ETX_3) index++;
            frameType = frame[index];
            textEndIndex = index;
            string checkSum = GetCheckSum(frame, textStartIndex - 1, textEndIndex);
            if (!checkSum.Equals(Encoding.ASCII.GetString(frame, textEndIndex + 1, 2)))
                return -2;
            result = Encoding.ASCII.GetString(frame, textStartIndex, textEndIndex - textStartIndex);
            int fn = frame[textStartIndex - 1] - '0';
            return fn;
        }  //end AnalyzeFrame

        /// <summary>
        /// 生成帧数据
        /// </summary>
        /// <param name="text">帧文本,长度<=240</param>
        /// <param name="fn">帧序号,0<=fn<8</param>
        /// <param name="frameType"></param>
        /// <returns></returns>
        public static byte[] GenFrame(string text, int fn, byte frameType)
        {
            if (frameType != ASTMCommon.ETB_23 && frameType != ASTMCommon.ETX_3) return null;
            if (fn < 0 || fn > 7) return null;
            byte[] frame = new byte[text.Length + 7];
            int index = 0;
            frame[index++] = ASTMCommon.STX_2;
            frame[index++] = (byte)(fn + '0');
            foreach(char ch in text)
                frame[index++] = (byte)ch;
            frame[index++] = frameType;
            string checkSum = GetCheckSum(frame, 1, text.Length + 2);
            frame[index++] = (byte)checkSum[0];
            frame[index++] = (byte)checkSum[1];
            frame[index++] = ASTMCommon.CR_13;
            frame[index++] = ASTMCommon.LF_10;
            return frame;
        }  //end GenFrame
        #endregion

        #region 相关检验函数
        /// <summary>
        /// 检验发送的数据是否合法
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool CheckMsg(string msg)
        {
            if (string.IsNullOrEmpty(msg)) return false;
            byte[] d = Encoding.ASCII.GetBytes(msg);
            foreach(byte b in d)
                if (b == ASTMCommon.LF_10) return false;
            return true;
        }  //end CheckMsg

        /// <summary>
        /// 计算校验和
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        private static string GetCheckSum(byte[] buf, int startIndex, int endIndex)
        {
            int sum = 0;
            for (int i = startIndex; i < endIndex; i++) sum += buf[i];
            sum &= 0xff;
            int c1 = sum >> 4;
            int c2 = sum & 0xf;
            return string.Format("{0:X}{1:X}", c1, c2);
        }  //end GetCheckSum

        /// <summary>
        /// 根据长度拆分字符串
        /// </summary>
        /// <param name="str">待拆分的字符串</param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static List<string> SplitByLen(string str, int len)
        {
            List<string> listString = new List<string>();

            int index = 0;
            while(str.Length - index > len)
            {
                listString.Add(str.Substring(index, len));
                index += len;
            }  //end while
            if (str.Length != index) listString.Add(str.Substring(index));
            return listString;
        }  //end SplitByLen

        /// <summary>
        /// 检测是否是同一个EndPoint
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public static bool CheckSameEndPoint(IPEndPoint point1 , IPEndPoint point2)
        {
            if (null == point1 || null == point2)
            {
                return false;
            }  //end if 
            if (point1.Address.ToString().Equals(point2.Address.ToString()) && point1.Port == point2.Port)
            {
                return true;
            }  //if - 端口和IP地址相同
            else
            {
                return false;
            }  //end if - else
        }  //end CheckSameSocket

        #endregion

    }  //end class ASTMCommon
}  //end namespace LisMachine.ASTMNetHelper
