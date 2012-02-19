using System;
using System.Collections.Generic;

namespace XF
{

    /////////////////////////////////////////////////////////////////////////////////////////
    
    #region Shader parameter class declarations

    public abstract class shader_param
    {
        public string name;
        internal abstract void apply(SFML.Graphics.Shader source);
    }

    public class shader_param_float : shader_param
    {
        public float value;
        public shader_param_float(string name, float value)
        {
            this.name = name;
            this.value = value;
        }
        internal override void apply(SFML.Graphics.Shader source)
        {
            source.SetParameter(name, value);
        }
    }

    public class shader_param_ref_float : shader_param
    {
        public Utility.Ref<float> referenced_value;
        public shader_param_ref_float(string name, Utility.Ref<float> value) { this.name = name;  this.referenced_value = value; }
        internal override void apply(SFML.Graphics.Shader source)            { source.SetParameter(name, referenced_value.value);}        
    }

    public class shader_param_float4 : shader_param
    {
        public crds4 value;
        public shader_param_float4(string name, crds4 value)
        {
            this.name = name;
            this.value = value;
        }
        internal override void apply(SFML.Graphics.Shader source)
        {
            source.SetParameter(name, value.x, value.y, value.z, value.w);
        }
    }
    public class shader_param_buffer : shader_param
    {
        public Graphics.ScreenBuffer buffer;
        public shader_param_buffer(string name, Graphics.ScreenBuffer buff)
        {
            this.name = name;
            this.buffer = buff;
        }
        internal override void apply(SFML.Graphics.Shader source)
        {
            source.SetTexture(name, buffer.get_image);
        }
    }
    public class shader_param_tex : shader_param
    {
        public Graphics.Texture texture;
        public shader_param_tex(string name, Graphics.Texture tex)
        {
            this.name = name;
            this.texture = tex;
        }
        internal override void apply(SFML.Graphics.Shader source)
        {
            source.SetTexture(name, texture.image);
        }
    } 
    #endregion

    /////////////////////////////////////////////////////////////////////////////////////////

    public class EffectInstance
    {
        /////////////////////////////////////////////////////////////////////////////////////

        #region Constructors
        public EffectInstance()
        {
            shader = Graphics.Shader.empty;
            parameters = new List<shader_param>();
        }
        public EffectInstance(EffectInstance copy)
        {
            shader = copy.shader;
            parameters = new List<shader_param>(copy.parameters);
        } 
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region Fields
        private Graphics.Shader _shader;
        public Graphics.Shader shader
        {
            get { return _shader; }
            set { _shader = value; }
        }
        public List<shader_param> parameters; 
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region Main stuff
        public void prepare()
        {
            if (shader.sfml_shader_object == null) return;

            foreach (var param in parameters)
            {
                param.apply(shader.sfml_shader_object);
            }
        } 
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region Clear instance of all data:
        internal void clear()
        {
            shader = Graphics.Shader.empty;
            parameters.Clear();
        } 
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region To get an empty instance : 
        static private EffectInstance _empty = new EffectInstance();
        static public EffectInstance empty { get { return _empty; } } 
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////
    }
}
