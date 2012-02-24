using XF;
using System.Collections.Generic;

namespace XF.Components
{
    public class ScreenLog : State
    {
        public ScreenLog()
        {
            Debug.log_event += new Debug.call_on_log(intercepted_log); // subscribe to the EVENT by supplying the handler with a METHOD matching the DELEGATE

            register(Event.render, this.render);
            register(Event.tick, this.tick);            

            font = Graphics.default_font;
            active = true;
            if (Graphics.layers.Count > 0)
                draw_layer = Graphics.layers[Graphics.layers.Count - 1];

            fade_time = (int)(1f * Application.ticks_per_second);
            total_time = 2 * fade_time;
            
        }

        public Graphics.Layer draw_layer { private get; set; }

        public Graphics.BitmapFont font;

        public int fade_time;
        public int total_time;
        public int max_shown  = 32;

        public float alpha_multiplier = 0.6f;

        private int time_left;

        private class notification
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

        private bool active;

        private void render()
        {               
            if (!active) return;

            float y = Graphics.screen_h - 40f;
            if (font == null) font = Graphics.default_font;
            Graphics.current_font = font;
            Graphics.current_layer = draw_layer;            
            for (int i = notifications.Count - 1 ; i >= 0; i--)
            {
                y -= font.v_spacing;
                var spr = Graphics.add_text(20f, y, notifications[i].text);
                if (i == 0) spr.set_alpha((alpha_multiplier * time_left / fade_time).choke(0f, alpha_multiplier));
                else        spr.set_alpha(alpha_multiplier);                
            }
        }

        private void tick()
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
            if (Input.key(Input.Keys.D).pressed && Input.ctrl) active = !active;
        }

    }
}