using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;


namespace PMGSearch
{
    class CommonFunction
    {
        private CommonFunction()
        {

        }

        public static void LogInfo(string LogMessage,bool IsPMGLogging =false,string PMGLogFileLocation = "")
        {
            try
            {
                if (ConfigurationManager.AppSettings["IsPMGLogging"] != null && Convert.ToBoolean(ConfigurationManager.AppSettings["IsPMGLogging"], System.Globalization.CultureInfo.CurrentCulture) == true)
                 {
                     string path = ConfigurationManager.AppSettings["PMGLogFileLocation"] + "LOG_" + DateTime.Today.ToString("MMddyyyy",System.Globalization.CultureInfo.CurrentCulture) + ".csv";

                     if (!File.Exists(path))
                     {
                         File.Create(path).Close();
                     }
                     using (StreamWriter w = File.AppendText(path))
                     {
                         w.WriteLine(DateTime.Now.ToString() + " , [INFO] ,\"" + LogMessage + "\"");
                     }
                 }
                 else if (ConfigurationManager.AppSettings["IsPMGLogging"] == null && IsPMGLogging == true && !string.IsNullOrEmpty(PMGLogFileLocation))
                 {
                     string path = PMGLogFileLocation + "LOG_" + DateTime.Today.ToString("MMddyyyy",System.Globalization.CultureInfo.CurrentCulture) + ".csv";

                     if (!File.Exists(path))
                     {
                         File.Create(path).Close();
                     }
                     using (StreamWriter w = File.AppendText(path))
                     {
                         w.WriteLine(DateTime.Now.ToString() + " , [INFO] ,\"" + LogMessage + "\"");
                     }
                 }
            }
            catch (Exception)
            {
            }
        }
    }
}
