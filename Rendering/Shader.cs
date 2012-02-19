using System;

namespace XF
{
    public static partial class Graphics
    {
        public class Shader
        {
            static private Shader _empty = new Shader();
            static internal Shader empty
            {
                get
                {
                    return _empty;
                }
            }
            private Shader()
            {
                _shader = null;
            }
            public Shader(string source)
            {   
                _shader = new SFML.Graphics.Shader(source);                
            }

            SFML.Graphics.Shader _shader;
            internal bool active { get { return _shader != null; } }
            [Obsolete("Please use sfml_shader_object instead")]
            internal SFML.Graphics.Shader shader { get { return _shader; } }
            internal SFML.Graphics.Shader sfml_shader_object { get { return _shader; } }

        }
    }
}
