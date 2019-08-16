using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ItrRecMsg
{
    public class LogRevMsg
    {
        /// <summary>
        /// 文件并发锁
        /// </summary>
        private static Dictionary<string, object> synLogFile = new Dictionary<string, object>();

        private static object BufferLock = new object();

        private static string _basePath;
        private static object synObject = new object();

        private static Dictionary<string, object> synLogFile2 = new Dictionary<string, object>();
        private static object synObject2 = new object();
        private static string _basePath2;

        /// <summary>
        /// 记录收到文件记录的路径
        /// </summary>
        public static string BasePath
        {
            get
            {
                string ItrID  = "";
                if (ItrID.Trim().Length == 0)
                    ItrID = "UnknowItrID";

                if (_basePath == null)
                {
                    lock (synObject)
                    {
                        string basePath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                        if (basePath == null)
                        {
                            basePath = AppDomain.CurrentDomain.BaseDirectory;
                        }
                        basePath = basePath + @"\" + ItrID + @"\";
                        basePath = basePath.Replace(@"\\", @"\");
                        if (!Directory.Exists(basePath))
                        {
                            Directory.CreateDirectory(basePath);
                        }
                        _basePath = basePath;
                    }
                }
                return _basePath;
            }
        }
        /// <summary>
        /// 记录收到文件记录的路径
        /// </summary>
        public static string LogBasePath
        {
            get
            {
                if (_basePath2 == null)
                {
                    lock (synObject2)
                    {
                        string basePath2 = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                        if (basePath2 == null)
                        {
                            basePath2 = AppDomain.CurrentDomain.BaseDirectory;
                        }
                        basePath2 = basePath2 + @"\log\";
                        basePath2 = basePath2.Replace(@"\\", @"\");
                        if (!Directory.Exists(basePath2))
                        {
                            Directory.CreateDirectory(basePath2);
                        }
                        _basePath2 = basePath2;
                    }
                }
                return _basePath2;
            }
        }

        public static void Log(string title, string details)
        {
            try
            {
                    //文件名
                    //每天每种类型的日志一个文件
                string fileName = string.Format("{0}_{1}.log", DateTime.Now.ToString("yyyyMMddHHmmssfff"), title);
                    string fullPath = BasePath + fileName;
                    if (!synLogFile.ContainsKey(fullPath))
                    {
                        lock (synLogFile)
                        {
                            synLogFile.Add(fullPath, new object());
                        }
                    }
                    lock (synLogFile[fullPath])
                    {
                        using (StreamWriter sw = new StreamWriter(fullPath, true, Encoding.Default))
                        {
                            sw.WriteLine(details);
                        }
                    }
                
            }
            catch (Exception ex)
            {
                Lib.LogManager.Logger.LogException(ex);
            }
        }

        public static void LogText(string title, string details)
        {
            try
            {
                //文件名
                //每天每种类型的日志一个文件
                string fileName = string.Format("{0}_{1}.log", DateTime.Now.ToString("yyyyMMdd"), "11022");
                string fullPath = LogBasePath + fileName;
                if (!synLogFile2.ContainsKey(fullPath))
                {
                    lock (synLogFile2)
                    {
                        synLogFile2.Add(fullPath, new object());
                    }
                }
                lock (synLogFile2[fullPath])
                {
                    using (StreamWriter sw = new StreamWriter(fullPath, true, Encoding.Default))
                    {
                        string format =
@"[{1}]  [{0}]
{2}";
                        string msg = string.Format(format, title, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), details);
                        sw.WriteLine(msg);
                    }
                }

            }
            catch (Exception ex)
            {
                Lib.LogManager.Logger.LogException(ex);
            }
        }

        public static void GetBufferLog(string details)
        {
            try
            {
                //文件名
                //每天每种类型的日志一个文件   
                lock (BufferLock)
                {
                    string fileName = string.Format("{0}_{1}.cmd", DateTime.Now.ToString("yyyyMMddHHmmssfff"), "11022");
                    string fullPath = BasePath + fileName;
                    using (StreamWriter sw = new StreamWriter(fullPath, true, Encoding.Default))
                    {
                        sw.WriteLine(details);
                    }

                }


            }
            catch (Exception ex)
            {
                Lib.LogManager.Logger.LogException(ex);
            }
        }

        public static void GetFile(string details)
        {
            try
            {
                //文件名
                //每天每种类型的日志一个文件   
                lock (BufferLock)
                {
                    string ItrID = "11022";
                    string fileName = string.Format("{0}_{1}.txt","N600", "11022");
                    string Path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                    if (Path == null)
                    {
                        Path = AppDomain.CurrentDomain.BaseDirectory;
                    }
                    Path = Path + @"\File\";
                    Path = Path.Replace(@"\\", @"\");
                    if (!Directory.Exists(Path))
                    {
                        Directory.CreateDirectory(Path);
                    }
                    string fullPath = Path + fileName;

                    File.AppendAllText(fullPath,details);

                }


            }
            catch (Exception ex)
            {
                Lib.LogManager.Logger.LogException(ex);
            }
        }


    }
}
