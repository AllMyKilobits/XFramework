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
        public static string    title = "[no name]";
        public static int       ticks_per_second = 50;        
        
        private static List<string> asset_dirs = new List<string>();

        public static bool asset_tracking = false;
        public static bool audio_disabled = false;
        public static bool screen_logger  = false;

        public static void load_assets(string dir)
        {
            asset_dirs.Add(dir);
        }

        public static void startup()
        {         
            Session.set_tick_interval(1f / ticks_per_second);
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
