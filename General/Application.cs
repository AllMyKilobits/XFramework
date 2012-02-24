using System;
using System.Collections.Generic;

namespace XF
{
    /// <summary>
    /// A static class created explicitly to ease startup.
    /// </summary>
    public static class Application
    {
        public static string    root_path;

        /// <summary>itle of the window </summary>
        public static string    title = "[no name]";

        /// <summary>Set the master Session tick frequency</summary>
        public static float     ticks_per_second
        {
            get { return (1f / (float)Session.Threading.tick_duration);}
            set { Session.Threading.set_tick_interval(1f / value); }
        }
        
        private static List<string> asset_dirs = new List<string>();

        public static bool asset_tracking = false;
        public static bool audio_disabled = false;
        public static bool screen_logger  = false;

        /// <summary>Mark a directory for loading all found graphics, shader, and audio assets </summary>        
        public static void load_assets(string dir)
        {
            asset_dirs.Add(dir);
        }

        public static void startup()
        {   
            Graphics.back_color = 0xff404040;
                        
            foreach (var path in asset_dirs)
            {
                Debug.Log("Scanning folder " + path + " for assets");                
                Graphics.load_batch_textures(root_path + path);
                Audio.load_batch_samples    (root_path + path);
                Graphics.load_batch_shaders (root_path + path);
            }

            //if (screen_logger) XF.Session.add_state("Logger", new XF.Components.ScreenLog());

        }

        public static void set_window(uint w, uint h, bool full_screen)
        {
            Graphics.set_window(w, h, full_screen);
        }
    }
}
