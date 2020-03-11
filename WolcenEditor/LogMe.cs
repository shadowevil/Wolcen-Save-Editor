using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WolcenEditor
{
    public static class LogMe
    {
        private static string logOutputFile = "LogMe.log";

        public static void InitLog()
        {
            if (File.Exists(".\\Logs\\" + logOutputFile))
            {
                string newFile = Directory.GetCurrentDirectory() + "\\Logs\\" + logOutputFile.Substring(0, logOutputFile.Length - 4) + "_" + DateTime.Now.ToLongTimeString().Replace(':', '-') + ".log";
                string oldFile = Directory.GetCurrentDirectory() + "\\Logs\\" + logOutputFile;
                File.Move(oldFile, newFile);
            }
        }

        public static void WriteLog(string text)
        {
            if (!Directory.Exists(".\\Logs\\")) Directory.CreateDirectory(".\\Logs\\");
            using (StreamWriter sw = File.AppendText(".\\Logs\\" + logOutputFile))
            {
                sw.WriteLine(text);
            }
        }
    }
}
