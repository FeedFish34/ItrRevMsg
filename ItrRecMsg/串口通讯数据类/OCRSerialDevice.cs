using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ItrRecMsg
{
    /// <summary>
    /// 
    /// </summary>
    public class OCRSerialDevice : SerialDeviceBase
    {
        public override string EndChar
        {
            get { return ASTMCommon.cEOT_4.ToString(); }
        }
        public override bool IsASTM
        {
            get { return true; }
        }
        public override bool IsNeedAck
        {
            get
            {
                return true;
            }
        }
        string SendBuffer = string.Empty;//串口发送缓冲
        public  List<string> AnswerList = new List<string>();
        int NakIndex = 0;
        string CurrAnswer = string.Empty;
        public override void SendData()
        {
            try
            {
                if (NakIndex > 0 && CurrAnswer != string.Empty)
                { //最后出现nak的数据帧数
                    SendData(CurrAnswer);//向仪器发送请求
                    NakIndex = 0;
                    if (AnswerList.Count > 0 && NakIndex == 0)
                    {
                        SendEnqCommand(1000);
                    }
                    return;
                }

                if (AnswerList.Count == 0)
                    return;

                CurrAnswer = AnswerList[0];
                SendData(CurrAnswer);//向仪器发送请求
                AnswerList.Remove(CurrAnswer);
                if (AnswerList.Count > 0 && CurrAnswer == ASTMCommon.cEOT_4.ToString())
                {
                    SendEnqCommand(3000);
                }
            }

            catch (Exception ex)
            {
                Lib.LogManager.Logger.LogException(ex);
            }
        }
        public override void port_NakReceived(object sender, EventArgs e)
        {
            NakIndex = 1;

            Thread threadStart = new Thread(delegate ()
            {
                SendEnqCommand(5000);
            });
        }
        public override void port_EnqReceived(object sender, EventArgs e)
        {
            SendData(ASTMCommon.cACK_6.ToString());
        }
        public override void port_EotReceived(object sender, EventArgs e)
        {
            Thread threadStart = new Thread(delegate ()
            {
                SendEnqCommand(5000);
            });
        }

        public override void port_AckReceived(object sender, EventArgs e)
        {
            SendData();
        }
        public override void HandleData(FileReadEventArgs eventarg)
        {
            string TureMessage = PrepreaMessage(eventarg.Message).Replace(ASTMCommon.cSTX_2.ToString(), "").
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
            dao.UpdateOrInsertASTM(sampleId,result);

            AnswerList.Add(ASTMCommon.cSTX_2.ToString() + sampleId+"-"+ result + ASTMCommon.cETX_3.ToString());
            AnswerList.Add(ASTMCommon.cEOT_4.ToString());
            if (AnswerList.Count > 0 || NakIndex > 0)
            {
                SendEnqCommand(0);
                return;
            }

        }

        private string PrepreaMessage(string message)
        {
            int strLen = 0;
            string Result = string.Empty;
            do
            {
                if (message[strLen] == ASTMCommon.cETB_23)
                    strLen += 7;
                Result += message[strLen];
                strLen++;
            } while (strLen < message.Length);
            return Result;
        }

        public void SendEnqCommand(int sleep)
        {
            if (sleep > 0)
                Thread.Sleep(sleep);
            SendData(ASTMCommon.cENQ_5.ToString());//向对方发送请求
        }

        /// <summary>
        /// 开始
        /// </summary>
        public override void StartWorking()
        {
            base.StartWorking();
        }
        public override string Model
        {
            get { return "OCR"; }
        }
        public override string PortType
        {
            get { return "串口"; }
        }

        public override int Speed
        {
            get { return 5; }
        }
        public override string Author
        {
            get { return "ZWY"; }
        }
        public override string Desc
        {
            get
            {
                return this.Model;
            }
        }
        public override string Date
        {
            get { return "2018-01-12"; }
        }
        public override string LoggerName
        {
            get
            {
                return this.Model;
            }
        }
        public override string FullName
        {
            get { return this.Model; }
        }

    }
}
