using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace XF
{
    public static class Debug
    {
        public  enum   priorities
        {
            low,       // These are usually reported in development mode only.
            normal,    // things with a high priority. These are reported in release mode, unless overriden for some reason.
            critical   // these are always reported, no matter what.
        }
        private static priorities _min_priority;
        public  static priorities minimum_priority 
        { 
            get {return _min_priority; }
            set { Debug.Log("Setting log priority to : " + value, priorities.critical); _min_priority = value; }
        }

        public static void on_init()
        {
            File.Delete(Application.root_path + "log.txt"); // "an exception is NOT thrown if the file doesn'tex exist".            
        }   

        private static string tableader = "";
        public static void StartLogGroup()
        {   
            tableader += "  ";         
        }

        public static void EndLogGroup()
        {
            if (tableader.Length < 2) return;
            tableader = tableader.Substring(0, tableader.Length - 2);
        }

        public static void on_terminate()
        {
            
        }

        // this is my VERY FIRST EVENT EVER!

        public delegate void call_on_log (string logged_text);   // you first declare a delegate that will be used for the event
        static public event  call_on_log log_event;              // then you declare the EVENT object, supplying a delegate

        public static void Log(Object obj, priorities priority = priorities.low) // this is a normal method.
        {
            if (priority < minimum_priority) return;

            using (StreamWriter logstream = File.AppendText(Application.root_path + "log.txt"))
            {
                logstream.WriteLine(tableader + obj.ToString());
                logstream.Close();
                if (log_event != null) log_event(tableader + obj);   // this is how you TRIGGER the event.             
            }
        }
        public static void Assert(bool condition) { if (!condition) throw new Exception("Assertion failed !"); }

    }
}
