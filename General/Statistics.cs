using System;
using System.Collections.Generic;
using XF;

namespace XF
{
    public static class Statistics
    {        
        public static uint frames_last_second;
        private static uint frame_counter;
        public static uint sprites_drawn    ;
        public static uint sprites_discarded;
        public static long total_ticks { get { return XF.Session.tick_index; } }
        public static long total_cycles;

        public static float actual_time { get { return (float)time_elapsed.Elapsed.TotalSeconds; } }

        private static System.Diagnostics.Stopwatch fps_watch    = new System.Diagnostics.Stopwatch();
        private static System.Diagnostics.Stopwatch time_elapsed = new System.Diagnostics.Stopwatch();

        static Statistics()
        {
            frame_counter = 0;
            fps_watch.Start();
            time_elapsed.Start();
        }
                
        public static void on_tick()
        {
            if (fps_watch.ElapsedMilliseconds > 1000)
            {
                frames_last_second = frame_counter;
                frame_counter = 0;
                fps_watch.Restart();

                total_sleep_last_second = sleep_counter;
                sleep_counter = 0;
            }

        }

        public static void on_render()
        {
            frame_counter++;
        }

        static public int total_sleep_last_second { get; private set; }
        static private int sleep_counter = 0;
        internal static void report_about_to_sleep(int ms)
        {
            sleep_counter += ms;
        }
    }
}
