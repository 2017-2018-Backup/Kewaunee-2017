using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddIn
{
    public class WriteLoginLog
    {
        public static void CreateLog(string Username, string loggedDate, string login)
        {
            string logFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            logFile = Path.Combine(logFile, "LoginLog.txt");
            if (!File.Exists(logFile))
            {
                using (StreamWriter sw = File.CreateText(logFile))
                {
                    sw.WriteLine("--------Kewaunee Revit Application-User Login Information---------------");
                    sw.WriteLine("Logged User:          " + Username);
                    sw.WriteLine("Time:                 " + loggedDate);
                    sw.WriteLine("          " + login + "        ");
                    sw.WriteLine("----------------------");
                    sw.WriteLine(Environment.NewLine);

                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(logFile))
                {
                    sw.WriteLine("Logged User:" + Username);
                    sw.WriteLine("Time:" + loggedDate);
                    sw.WriteLine("          "+login+"        ");
                    sw.WriteLine("----------------------");
                    sw.WriteLine(Environment.NewLine);
                }
            }
        }
    }
}
