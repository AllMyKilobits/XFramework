using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SM = System.Math;

using System.Reflection;

namespace XF
{
    public static class XMath
    {

        private const float pimod       = (float)System.Math.PI / 180.0f;
        private const float pimod_inv   = 180.0f / (float)System.Math.PI;

        public static float deg_to_rad(float degrees) { return degrees * pimod; }
        public static float rad_to_deg(float radians) { return radians * pimod_inv; }

        public static float sin(float a) {return (float)System.Math.Sin(deg_to_rad(a));}
        public static float cos(float a) {return (float)System.Math.Cos(deg_to_rad(a));}

        public static float sin(int a)   
        {
            if (a < 0) a = (a % 360) + 360; if (a > 360) a %= 360;
            return _sinii[a];
        }
        public static float cos(int a)
        {
            if (a < 0) a = (a % 360) + 360; if (a > 360) a %= 360;
            return _cosinii[a];
        }

        private static float[] _sinii;
        private static float[] _cosinii;
        
        public static void initialize()
        {
            _sinii = new float[360];
            _cosinii = new float[360];
            for (int i = 0; i < 360; i++)
            {
                _sinii[i] = (float)System.Math.Sin(deg_to_rad(i));
                _cosinii[i] = (float)System.Math.Cos(deg_to_rad(i));            
            }
            root2 = (float)System.Math.Sqrt(2);
            root3 = (float)System.Math.Sqrt(3);
        }

        public static int choke(this int a, int low, int high) // extension of integers
        {
            if (a < low)  a = low;
            if (a > high) a = high;
            return a;         
        }
        public static int smartchoke(this int a, int abslow, int abshigh)
        {
            if      (SM.Abs(a) < abslow)  a = (SM.Sign(a) * abslow) ;
            else if (SM.Abs(a) > abshigh) a = (SM.Sign(a) * abshigh);
            return a;
        }
        public static float choke(this float a, float low, float high) // extension of floats
        {
            if (a < low)  a = low;
            if (a > high) a = high;
            return a;
        }
        public static float choke01(this float a) { return a.choke(0f, 1f); }

        public static uint choke(this uint a, uint low, uint high)
        {
            if (a < low) a = low;
            if (a > high) a = high;
            return a;
        }

        public static bool odd(this uint a) { return a % 2 == 1; }
        public static bool odd(this  int a) { return a % 2 == 1; }

        public static float approach(this float a, float target, float max_approach)
        {
            float d = (target - a); float v = a;
            if (Math.Abs(d) < max_approach) v += d;
            else                            v += Math.Sign(d) * max_approach;
            return v;
        }

        public static bool approximately(this float a, float b)
        {
            return (Math.Abs(a - b) < 10f * float.Epsilon);
        }

        public static float get_angle(float dx, float dy)
        {
            if (Math.Abs(dx) < 0.0001f) return (dy > 0.0f) ? 90.0f : 270.0f;
            if (dx > 0.0f)  return (float)(Math.Atan(dy / dx) * pimod_inv);
                            return (float)(Math.Atan(dy / dx) * pimod_inv) + 180.0f;

        }

        public static float delta_angle(float angle, float target, float maxdelta = 360.0f)
        {			 
	        float psi = target - angle;
	        if (psi> 180.0f) psi -= 360.0f;
	        if (psi<-180.0f) psi += 360.0f;
	        if (Math.Abs(psi)>maxdelta)     return Math.Sign(psi) * maxdelta;
	        /* else */				        return psi;
        }

        public static float approach_angle(float source, float target, float maxdelta = 360f)
        {
            float psi = target - source;
            if (psi > 180f) psi -= 360f;
            if (psi <-180f) psi += 360f;
            if (Math.Abs(psi) > maxdelta)            
                return wrap_angle(source + Math.Sign(psi) * maxdelta);

            return wrap_angle(source + psi);
        }

        private static float fmod(float dividend, float divisor)
        {
            if (Math.Abs(divisor) < 0.000001f) return 0.0f;
            float d = dividend / divisor;
            float mod = dividend - (float)Math.Floor(d) * divisor;
            return mod;
        }

        public static float wrap_angle(float raw_angle)
        {
            if (raw_angle > 360.0f) return fmod(raw_angle, 360.0f);
            if (raw_angle < 0.0f)   return fmod(raw_angle, 360.0f);
            return raw_angle;
        }


        // wrap ceiling is never achieved
        public static uint wrap(uint a, uint wrap_ceiling) { return a % wrap_ceiling;}
        public static int wrap(int a, int wrap_ceiling) { if (a < 0) return a % wrap_ceiling + wrap_ceiling; else return a % wrap_ceiling; }

        private static float pi;
        private static float e;

        public static float lerp(float a, float b, float ratio){return a + (b - a) * ratio;}


        //public static float sin(float angle)
        //{
        //}
        [System.Obsolete("Use the Random.Range to generate values!")]
        public static int   rnd(int low, int high)      { return Random.range(low, high); }// .Next(low, high); }
        [System.Obsolete("Use the Random.Range to generate values!")]
        public static float rnd(float low, float high)  { return Random.range(low, high); }
        [System.Obsolete("Use the Random.Range to generate values!")]
        public static float rnd()                       { return Random.zero_to_one; }

        //public static 
                
        public static float saw_sequence(float min, float max, float period_in_seconds, float time_offset = 0f)
        {
            var q = (float)Math.IEEERemainder((Session.time + time_offset) / period_in_seconds, 1f) + 0.5f;
            return min + (max - min) * q;            
        }

        public static float ping_pong(float min, float max, float period_in_seconds, float time_offset = 0f)
        {
            var q = (float)Math.IEEERemainder(((Session.time + time_offset)*2f) / period_in_seconds, 2f);
            q += 1f;
            if (q > 1f) q = 2f - q;
            return min + (max - min) * q;
        }

        public static int floor(float number)
        {
            return (int)Math.Floor(number);
        }

        public static int myfloor(this float number)
        {
            return (int)Math.Floor(number);
        }

        public static float trunc(float number) { return number - myfloor(number); }

        public static float root2 {get; private set;}
        public static float root3 {get; private set;}

        public static float abs(float number)   { return Math.Abs(number); }
        public static int abs(int number)       { return Math.Abs(number); }

        public static uint HSV_to_RGB(float h, float s, float v) 
        {
            float m, n, f;
            var i = (h * 6f).myfloor();
            f = h * 6f - i;
            if (i % 2 == 0) f = 1f - f;
            m = v * (1 - s);
            n = v * (1 - s * f);
            
            switch (i)
            {                
                case 0: return makecolor(v, n, m);
                case 1: return makecolor(n, v, m);
                case 2: return makecolor(m, v, n);
                case 3: return makecolor(m, n, v);
                case 4: return makecolor(n, m, v);
                case 5: return makecolor(v, m, n);
            }
            throw new System.Exception("Invalid HSV to RGB!");
            return 0xffffffffu;
        }

        private static uint makecolor(float r, float g, float b)
        {

            return ((uint)((255f * b.choke01()).myfloor()))       |
                   ((uint)((255f * g.choke01()).myfloor()) << 8)  |
                   ((uint)((255f * r.choke01()).myfloor()) << 16) |
                   0xff000000u;

        }
    
    }

    public static class Random
    {
        //private static System.Random rndgen = new System.Random();
        private static XF.Components.MersenneTwister rndgen = new Components.MersenneTwister();

        public static int   range(int low,   int high)      { return rndgen.Next(low, high+1); }
        public static float range(float low, float high)    { return low + (float)rndgen.NextDouble() * (high-low); }
        public static float zero_to_one                     { get { return (float)rndgen.NextDouble(); } }
        public static int   plus_minus_one                  { get { return (rndgen.NextDouble() > 0.5) ? 1 : -1;} }
        public static bool  boolean                         { get { return (rndgen.NextDouble() > 0.5); } }

        /// <summary> Returns a random number between 1 and 100, inclusive</summary>
        public static int   d100                            { get { return range(1,100); } }

        static public bool percent_chance(this int source)
        {
            return d100 <= source;
        }

        static public T weighted_probability_pick<T>(IList<T> pickables) where T : class, IPickable
        {
            var sum = 0;
            for (int i = 0; i < pickables.Count; i++)
            {
                sum += pickables[i].weighted_probability.choke(0, int.MaxValue);
            }
            if (sum == 0) return null;

            var lsum = 0;
            var rnd = Random.range(0, sum-1);
            for (int i = 0; i < pickables.Count; i++)
            {
                var w = pickables[i].weighted_probability;                
                if (rnd >= lsum && rnd < lsum + w) return pickables[i];
                lsum += w;
            }

            //var temp = 0;
            //var rnd = Random.range(0,sum);
            //for (int i = 0; i < pickables.Count; i++)
            //{   
            //    temp += pickables[i].weighted_probability.choke(0, int.MaxValue);
            //    if (temp > rnd)
            //    {
            //        var index = i - 1;
            //        return pickables[(i - 1).choke(0, pickables.Count - 1)];
            //    }
            //}
            //return pickables[pickables.Count - 1];
            return null;
        }

    }

    public interface IPickable
    {
        int weighted_probability { get; }
    }

    public static class Math3D
    {
        public static void cartesian_to_spherical(crds3 cartesian, out float theta, out float phi)
        {
            crds3 norm = cartesian; norm.normalize();

            float sin_phi = cartesian.z / cartesian.lenght;
            phi = XMath.rad_to_deg((float)Math.Asin(sin_phi));
            float cos_phi = XMath.cos(phi);

            float d1 = 1.0f / (float)Math.Sqrt(cartesian.x * cartesian.x + cartesian.y * cartesian.y);
            float cos_theta = cartesian.x * d1;
            theta = XMath.rad_to_deg((float)SM.Acos(cos_theta));
            if (cartesian.y < 0) theta *= -1.0f;
        }

        public static crds3 find_intercept_vector(crds3 source, crds3 target, crds3 target_velocity, float seeker_speed)
        {
            const int RESOLUTION = 10;
            const int EXITVALUE = 1000;
            int tgt_iters = 0;
            int seek_iters = 0;
            bool loop = true;
            float difference = float.PositiveInfinity;

            var t = target;
            var d0 = new crds3();
            var d1 = new crds3();

            while (loop)
            {
                tgt_iters += RESOLUTION;
                if (tgt_iters >= EXITVALUE) loop = false;

                t = target + target_velocity * tgt_iters;
                d0 = d1;
                d1 = t - source;
                d1.normalize();
                float d = source.dist(t);
                seek_iters = (int)Math.Floor(d / seeker_speed);
                float new_diff = (float)Math.Abs(seek_iters - tgt_iters);
                if (new_diff >  difference) loop = false;
                else            difference = new_diff;
            }
            // at this moment, d1 is conveniently the seeker'state vector to catch up to the projected target'state position
            return (d0 + d1) * (seeker_speed * 0.5f);            

        }

    }


}
