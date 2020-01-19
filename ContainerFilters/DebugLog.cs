using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Sandbox.ModAPI;

namespace ContainerFilters
{
    public class DebugLog
    {
        private static DebugLog intstance = null;
        private TextWriter file = null;
        private string filename = "";

        private DebugLog()
        {

        }

        private static DebugLog GetInstance()
        {
            if (DebugLog.intstance == null) DebugLog.intstance = new DebugLog();
            return intstance;
        }

        public static bool Init(string name)
        {
            bool output = false;
            if (GetInstance().file == null)
            {
                try
                {
                    GetInstance().filename = name;
                    GetInstance().file = MyAPIGateway.Utilities.WriteFileInLocalStorage(name, typeof(DebugLog));
                    output = true;
                }
                catch (Exception e)
                {
                    MyAPIGateway.Utilities.ShowNotification(e.Message, 5000);
                }
            }
            else output = true;
            return output;
        }
        public static void Write(string text)
        {
            try
            {
                if(GetInstance().file != null)
                {
                    GetInstance().file.WriteLine($"{DateTime.Now:MM-dd-yy_HH-mm-ss-fff} - " + text);
                    GetInstance().file.Flush();
                }
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public static void Close()
        {
            if(GetInstance().file != null)
            {
                GetInstance().file.Flush();
                GetInstance().file.Close();
            }
        }
    }
}
