using System;
using System.Collections.Generic;
using System.Text;

namespace ItrRecMsg
{

        /// <summary>
        /// 对象返回，可以返回字符串，Datatable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        public delegate void ObjectReceivedEventHandler(object sender, object data);

        public delegate void BufferReceivedEventHandler(SerialHandleObject Handled, byte[] buff);
        public delegate void DataFrameEventHandler(SerialHandleObject Handled, string buff);

        public delegate void ByteReceivedEventHandler(object sender, byte[] buff);

        public delegate void ByteProcessEventHandler(SerialHandleObject Handled, ByteEventArgs ByteInfo);

        public delegate void DataNotifyEventHandler(object sender, object e);


        public delegate void FileReadEventHandler(object sender, FileReadEventArgs data);
        public class  SerialHandleObject {
            public bool handled =false;
        }
     /// <summary>
        /// 声明工作线程的delegate
        /// </summary>
        public delegate void WorkerThreadExceptionHandlerDelegate(Exception e);



    public class ByteEventArgs : EventArgs {
        public byte[] Buffer; //接收到的缓存
        public int Index; //该字节所在位置
        public byte CurrByte; //当前处理的字节
    }

        public class FileReadEventArgs : EventArgs
        {

            private string message;
            private string filepath;

            public FileReadEventArgs(string filePath,string msg)
            {
                message = msg;
                filepath = filePath;
            }

            public string Message
            {
                get { return message; }

            }
            public string FilePath
            {
                get { return filepath; }
            }
        }

        


}
