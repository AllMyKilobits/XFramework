using System;
using System.Collections.Generic;


namespace XF.Components
{
    public static class Cam2d // a camera with rotations
    {
        #region Private fields, states
        static crds4 restrictions_low;
        static crds4 restrictions_high;
        static crds4 current;
        static crds4 _target;
        static crds4 delta;
        static private float cos_phi;
        static private float inv_cos_phi;
        private const float ZOOM_FACTOR = 1f;
        private static float update_speed = 0.3f; 
        #endregion

        #region Constructor
        static Cam2d()
        {
            restrictions_low = new crds4(0f, 0f, 0f, 30f);
            restrictions_high = new crds4(200f, 200f, 0f, 300f);
            current = _target = new crds4(100f, 100f, 0f, 100f);
            set_angle(60);
        }    
        #endregion

        #region Game logic - update
        public static void on_tick()
        {
            current += delta;

            _target.x = _target.x.choke(restrictions_low.x, restrictions_high.x);
            _target.y = _target.y.choke(restrictions_low.y, restrictions_high.y);
            _target.z = _target.z.choke(restrictions_low.z, restrictions_high.z);
            _target.w = _target.w.choke(restrictions_low.w, restrictions_high.w);

            delta = _target - current;
            if (delta.lenght > update_speed) delta *= update_speed;
        } 
        #endregion

        #region Projections and unprojections
        public static crds3 project(crds2 world_coords)
        {
            var c = world_coords;
            var me = current + delta * Session.interpolation;
            return new crds3(
                Graphics.scrW2 + (c.x - me.x) * zoom,
                Graphics.scrH2 + ((c.y - me.y) * cos_phi) * zoom,
                zoom);
        }
        public static crds3 project(crds3 world_coords, crds3 world_delta = default(crds3))
        {
            var c = world_coords + world_delta * Session.interpolation;
            var me = current + delta * Session.interpolation;

            return new crds3(
                Graphics.scrW2 + (c.x - me.x) * zoom,
                Graphics.scrH2 +
                ( (c.y - me.y) * cos_phi
                 +(-c.z- me.z) * inv_cos_phi)
                     * zoom,
                zoom);
        }
        public static crds3 unproject(crds2 screen_coords)
        {
            var x = (screen_coords.x - Graphics.scrW2) / zoom + current.x;
            var y = (screen_coords.y - Graphics.scrH2) / (zoom * cos_phi) + current.y;
            //var A = (Graphics.scrH2 - screen_coords.y) / zoom;
            //var B = current.z * inv_cos_phi;
            //var y = (A + B) * inv_cos_phi + current.y;
            return new crds3(x, y, 0);
        } 
        #endregion

        #region Public properties
        public static float squash { get { return cos_phi; } }
        static public crds4 target { get { return _target; } set { _target = value; } }
        static public float zoom
        {
            get { return (current.w + delta.w * Session.interpolation) / ZOOM_FACTOR; }
            set { _target.w = value * ZOOM_FACTOR; }
        }
        #endregion

        #region Manipulations
        public static void set_update_speed(float value) { update_speed = value; }
        public static void set_angle(int angle)
        {
            cos_phi = XMath.cos(angle);
            inv_cos_phi = XMath.sin(angle);
        }
        public static void set_target(crds2 coords)
        {
            _target.x = coords.x;
            _target.y = coords.y;
        }
        public static void move_center(crds2 coords)
        {
            _target.x += coords.x;
            _target.y += coords.y;
        }
        public static void change_zoom(float how_much)
        {
            _target.w *= how_much;// *ZOOM_FACTOR;
        }
        public static void set_extremes(crds2 upper_left, crds2 lower_right, float min_zoom, float max_zoom)
        {
            restrictions_low.x = upper_left.x;
            restrictions_low.y = upper_left.y;

            restrictions_high.x = lower_right.x;
            restrictions_high.y = lower_right.y;

            restrictions_low.w = min_zoom * ZOOM_FACTOR;
            restrictions_high.w = max_zoom * ZOOM_FACTOR;

            set_target(upper_left.lerp(lower_right, 0.5f));
        }
        public static void encompass(crds2 hi_extr, crds2 lo_extr, float zoom_factor = 1f)
        {
            var c = (hi_extr + lo_extr) * 0.5f;
            var dhor = XMath.abs((hi_extr - lo_extr).x);
            var dver = XMath.abs((hi_extr - lo_extr).y);
            if (dhor < 1f) dhor = 1f;
            if (dver < 1f) dver = 1f;

            var aspect = dhor / dver;
            var screen_aspect = (float)Graphics.screen_w / Graphics.screen_h;

            var desired_zoom = 1f;

            if (aspect > screen_aspect) // width rules            
                desired_zoom = (float)Graphics.screen_w / dhor;
            else // height rules            
                desired_zoom = (float)Graphics.screen_h / dver;

            _target = new crds4(c.x, c.y, 0f, desired_zoom * zoom_factor);
            current = _target;
            delta = default(crds4);
            raise_restrictions();
        }
        static public void raise_restrictions()
        {
            restrictions_low = new crds4(float.MinValue, float.MinValue, float.MinValue, float.MinValue);
            restrictions_high = new crds4(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue);
        }
        public static void force(crds3 c, float desired_zoom)
        {
            _target.x = c.x;
            _target.y = c.y;
            _target.z = c.z;
            _target.w = desired_zoom;
            current = _target;
        } 
        #endregion
    }
}
