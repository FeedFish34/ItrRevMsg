using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ItrRecMsg
{
    public class FileManager
    {
        /// <summary>
        /// 读取文件列表
        /// </summary>
        public static List<string> GetFileItems(string path)
        {
            List<string> list = new List<string>();
            //string FileExt = LocalConfig.Instance.FileExt;
            string[] files = Directory.GetFiles(path);
            //if (FileExt == "全部" || (FileExt.Trim().Length==0))
            //    files = Directory.GetFiles(path);
            //else
            //    files = Directory.GetFiles(path, FileExt);

            foreach (string s in files)
            {
                FileInfo fi = new FileInfo(s);
                list.Add(fi.FullName);
            }
            return list;
        }

        /// <summary>
        /// 读取文件列表
        /// </summary>
        public static List<FileInfo> GetFileInfos(string path)
        {
            List<FileInfo> list = new List<FileInfo>();
            string[] files = Directory.GetFiles(path);
            foreach (string s in files)
            {
                FileInfo fi = new FileInfo(s);
                list.Add(fi);
            }
            return list;
        }


        /// <summary>
        /// 创建文件夹
        /// </summary>
        public static void CreateFolder(string name, string parentName)
        {
            DirectoryInfo di = new DirectoryInfo(parentName);
            di.CreateSubdirectory(name);
        }

        /// <summary>
        /// 移动文件
        /// </summary>
        public static bool MoveFile(string oldPath, string newPath)
        {
            try
            {
                if (File.Exists(newPath))
                    File.Delete(newPath);
                File.Move(oldPath, newPath);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static void BackupFile(string FilePath)
        {
            try
            {
                File.Delete(FilePath);
            }
            catch (Exception)
            {
            }

        }

        //public static string GetFileContent(string filepath)
        //{
        //    if (File.Exists(filepath))
        //    {
        //        Encoding fileEncoding = TxtFileEncoding.GetEncoding(filepath, Encoding.GetEncoding("GB2312"));//取得这txt文件的编码
        //        StreamReader sr = new StreamReader(filepath, fileEncoding);//用该编码创建StreamReader 
        //        FileInfo fi = new FileInfo(filepath);
        //        //StreamReader sr = fi.OpenText();
        //        string data = sr.ReadToEnd();
        //        sr.Close();
        //        return data;
        //    }
        //    else
        //    {
        //        return string.Empty;
        //    }
        //}

        public static void DeleteFile(string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
            }

        }
    }
}
