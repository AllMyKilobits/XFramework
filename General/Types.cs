using System;

namespace XF
{

        /// <summary>
        /// D2 = Something two-dimensional of any type
        /// </summary>
        /// <typeparam name="T"> Variable type</typeparam>
        [System.Diagnostics.DebuggerDisplay("{x}:{y}")]
        public struct D2<T>
        {
            public D2(T x, T y)
            {
                this.x = x;
                this.y = y;
            }

            public void set(T x, T y)
            {
                this.x = x;
                this.y = y;
            }

            public void set(T value) { x = value; y = value; }

            public T x;
            public T y;

        }

        public struct MinMaxF
        {
            private float min;
            private float max;

            public MinMaxF(float min, float max)
            {
                this.min = min;
                this.max = max;
            }
            public void  set(float min, float max) { this.min = min; this.max = max; }
            public float get() { return Random.range(min, max);}
        }

        public struct MinMaxI
        {
            private int min;
            private int max;

            public MinMaxI(int min, int max)
            {
                this.min = min;
                this.max = max;
            }
            public void set(int min, int max) {this.min = min; this.max = max;}
            public int get() { return Random.range(min, max);}
            
        }


        struct angle
        {
            //private float value {key; set;}
            private float value;
            public angle(float val)
            {
                value = 0.0f;
                value = validate(val);
            }
            
            public float get() { return value; }
            public void  set(float new_value) { value = validate(new_value); }

            private float validate(float raw)  {
                if (raw >= 0.0f && raw <= 360.0f) return raw;
                if (raw < 0.0f) raw += (int)((-raw / 360.0f) + 1) * 360.0f;
                else if (raw > 360.0f) raw -= (int)(raw / 360.0f) * 360.0f;
                return raw;
            }

            public static angle operator +(angle a, angle b)    { return new angle(a.value + b.value);}
            public static angle operator -(angle a, angle b)    { return new angle(a.value - b.value);}            
        }
        [System.Diagnostics.DebuggerDisplay("[{x} : {y}]")]
        public struct crds2
        {
            public float x;
            public float y;
            public crds2(float x, float y)
            {
                this.x = x;
                this.y = y;
            }

            public static crds2 zero { get {return new crds2(0f,0f);}}

            public float len2   { get { return (x * x + y * y); } }
            public float lenght { get { return (float)System.Math.Sqrt(x * x + y * y); } }
            public float dist        (crds2 other) { return (float)System.Math.Sqrt((x - other.x) * (x - other.x) + (y - other.y) * (y - other.y)); }
            public float dist_squared(crds2 other) { return                        ((x - other.x) * (x - other.x) + (y - other.y) * (y - other.y)); }

            public void  set(float x, float y) { this.x = x; this.y = y; }
            public crds2 trunc { get { return new crds2((float)Math.Floor(this.x), (float)Math.Floor(this.y)); } }

            static public crds2 operator +(crds2 a, crds2 b) { return new crds2(a.x + b.x, a.y + b.y); }
            static public crds2 operator -(crds2 a, crds2 b) { return new crds2(a.x - b.x, a.y - b.y); }
            static public crds2 operator *(crds2 c, float a) { return new crds2(c.x * a, c.y * a); }

            static public float dot(crds2 a, crds2 b)   { return a.x * b.x + a.y * b.y; }

            static public crds2 deflect(crds2 anchor, crds2 to_deflect, float amount)
            {
                var delta = to_deflect - anchor;
                delta.normalize();
                return to_deflect + delta * amount;
            }

            static public crds2 approach(crds2 source, crds2 target, float max_distance)
            {
                var delta = target - source;
                if (delta.len2 < max_distance * max_distance) 
                    return target;

                delta.normalize();                
                return source + delta * max_distance;                   
            }

            static public crds2 scatter(crds2 source, float amount)
            {
                var c = crds2.zero;
                do
                {
                    c.x = Random.range(-1f, 1f);
                    c.y = Random.range(-1f, 1f);                    

                } while (c.len2 > 1f);
                return source + c * amount;
            }

            public void normalize()
            {
                float l = lenght;
                if (l < 0.00001f) x = 1.0f;
                else this *= 1.0f / l;
            }

            static public float angle(crds2 a, crds2 b)
            {
                var d = (b - a);
                return XMath.get_angle(d.x, d.y);
            }

            public crds2 lerp(crds2 other, float value)
            {
                return this + (other - this) * value;
            }

            public string print { get { return "[ " + x + " ; " + y +" ]"; } }
            
        }

        [System.Diagnostics.DebuggerDisplay("[{x} : {y} : {z}]")]
        public struct crds3
        {  
            public float x;
            public float y;
            public float z;

            public crds3(float x, float y, float z)     { this.x = x; this.y = y; this.z = z; }
            public void set(float x, float y, float z)  { this.x = x; this.y = y; this.z = z; }
            public float lenght { get { return (float)System.Math.Sqrt(len2); } }
            public float len2   { get { return (x * x + y * y + z * z); } }
            public float dist (crds3 other) {return (this - other).lenght;}
            public float dist_squared(crds3 other) { return (this - other).len2; }

            public void normalize()
            {
                float l = lenght;
                if (l < 0.00001f)   x = 1.0f;                
                else                this *= 1.0f / l;                
            }

            public crds2 flatten  { get { return new crds2(x, y); } }            

            // DOT AND CROSS PRODUCTS!

            public float dot(crds3 other)
            {
                return x * other.x + y * other.y + z * other.z;
            }
            public crds3 cross(crds3 b)
            { 
                return new crds3(   y * b.z - z * b.y,
                                    z * b.x - x * b.z,
                                    x * b.y - y * b.x   );
            }

            public crds3 lerp(crds3 other, float value)
            {
                return this + (other - this) * value;
            }

            static public crds3 zero { get { return new crds3(0f, 0f, 0f); } }

            static public crds3 operator + (crds3 a, crds3 b) { return new crds3(a.x + b.x, a.y + b.y, a.z + b.z);}
            static public crds3 operator - (crds3 a, crds3 b) { return new crds3(a.x - b.x, a.y - b.y, a.z - b.z);}
            static public crds3 operator * (crds3 a, float b) { return new crds3(a.x * b  , a.y * b  , a.z * b  );}
            static public crds3 operator * (crds3 a, crds3 b) { return new crds3(a.x * b.x, a.y * b.y, a.z * b.z); }

            static public crds3 approach(crds3 source, crds3 target, float max_approach)
            {
                crds3 d = target - source; var l = d.lenght;
                if (l < max_approach) return source;
                d.normalize(); return source + d * max_approach;
            }

            static public crds3 scatter(crds3 source, float amount)
            {
                var c = crds3.zero;
                do
                {
                    c.x = Random.range(-1f, 1f);
                    c.y = Random.range(-1f, 1f);
                    c.z = Random.range(-1f, 1f);
                    
                } while (c.len2>1f);
                return source + c * amount;                
            }
            static public crds3 pick_sphere_point(crds3 sphere_center, float min_radius, float max_radius)
            {
                var c = crds3.zero;
                // a) pick a point in unit sphere                
                do { c.x = Random.range(-1f, 1f); c.y = Random.range(-1f, 1f); c.z = Random.range(-1f, 1f); } while (c.len2 > 1f);
                // b) make the point be on the surface of the unit sphere
                c.normalize(); 
                // c) multiply with desired distance and add to sphere center
                return sphere_center +  c * Random.range(min_radius, max_radius);
            }

            public string print { get {
                return "[ " + x + " ; " + y + " ; " + z + " ]";
            } }
            
            
        }

        public struct crds4
        {
            public float x;
            public float y;
            public float z;
            public float w;

            public crds4    (float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
            
            public void set (float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
            public float lenght { get { return (float)System.Math.Sqrt(len2  ); } }
            public float len2   { get { return (x * x + y * y + z * z + w * w); } }
            public float dist(crds4 other) { return (this - other).lenght; }

            static public crds4 operator + (crds4 a, crds4 b) { return new crds4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w); }
            static public crds4 operator - (crds4 a, crds4 b) { return new crds4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w); }
            static public crds4 operator * (crds4 a, float b) { return new crds4(a.x * b,   a.y * b,   a.z * b,   a.w * b  ); }
            static public crds4 operator * (crds4 a, crds4 b) { return new crds4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w); }
            
            public crds3 to_crds3 { get { return new crds3(x, y, z); } }

            public void normalize()
            {
                float l = this.lenght; if (l < 0.0001f) x = 1f;
                else this *= 1f / l;
            }

            static public crds4 approach(crds4 source, crds4 target, float max_approach)
            {
                crds4 d = target - source; var l = d.lenght;
                if (l < max_approach) return target;
                d.normalize(); return source + d * max_approach;
            }

        }

        public struct interpolable
        {
            private float min_constraint;
            private float max_constraint;
            private float speed;

            private float value;
            private float target;
            private float delta;

            public void  set_constraints (float min = float.MinValue, float max = float.MaxValue)         { min_constraint = min; max_constraint = max; }
            public void  set_speed       (float new_speed)              { speed = new_speed; }
            public void  set             (float new_target)             { target = new_target; }
            public float get             (float interpolation = 0.0f)   { return value + delta * interpolation; }

            public void tick()
            {
                value += delta;

                target = target.choke(min_constraint, max_constraint);
                float d1 = target - value;
                if (System.Math.Abs(d1) < speed)    delta = d1;
                else                                delta = System.Math.Sign(d1) * speed;
            }
            public void tick_as_angle()
            {
                //target = target.choke(min_constraint, max_constraint);
                float da = XMath.delta_angle(XMath.wrap_angle(value), XMath.wrap_angle(target), speed);
                value += da;
                value = XMath.wrap_angle(value);                
                delta = da;
            }

        }
}