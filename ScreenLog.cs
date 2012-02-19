using XF;
using System.Collections.Generic;

namespace XF
{
    class ScreenLog : State
    {
        public int draw_layer { private get; set; }
        public Graphics.BitmapFont font;

        public int fade_time  = 60 ;
        public int total_time = 120;
        public int max_shown  = 32;

        public float alpha_multiplier = 0.6f;

        private int time_left;

        public override void on_state_init()
        {
            base.on_state_init();
            Debug.log_event += new Debug.call_on_log( intercepted_log ); // subscribe to the EVENT by supplying the handler with a METHOD matching the DELEGATE            
        }

        class notification
        {
            private string _txt;
            internal int    repeats;
            public notification (string text)
	        {
                _txt = text;
                repeats = 1;
	        }
            internal string text
            {
                get
                {
                    return (repeats == 1) ? 
                        _txt :
                        "" + repeats + "x : " + _txt;
                }
            }
        }
        List<notification> notifications = new List<notification>();

        private string last_intercepted_log;

        private void intercepted_log(string text)
        {
            if (text == last_intercepted_log)
            {
                if (notifications.Count > 0)
                {
                    notifications[notifications.Count - 1].repeats ++;
                    return;
                }
            }

            notifications.Add(new notification(text));
            if (notifications.Count > max_shown + 1 ) notifications.RemoveAt(0);
            last_intercepted_log = text;
        }

        public override void on_render(float interpolation)
        {   
            base.on_render(interpolation);

            float y = Graphics.screen_h - 40f; ;

            for (int i = notifications.Count - 1 ; i >= 0; i--)
            {
                y -= 10f;
                var spr = Graphics.add_text(draw_layer, font, 20f, y, notifications[i].text);
                if (i == 0) spr.set_alpha((alpha_multiplier * time_left / fade_time).choke(0f, alpha_multiplier));
                else        spr.set_alpha(alpha_multiplier);
                //spr.set_scale(0.5f, 0.5f);
                
            }
        }

        public override void on_tick()
        {
            if (notifications.Count == 0)
            {
                time_left = total_time;
                return;
            }

            time_left--;
            if (time_left <= 0)
            {
                notifications.RemoveAt(0);
                time_left = total_time;
            }            
        }

    }
}