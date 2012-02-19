using System;
using System.Collections.Generic;

namespace XF
{
    static partial class Graphics
    {
        static private Layer backup_layer;

        public static void begin_targeted_render_sequence(ScreenBuffer buffer)
        {   
            var d = Graphics.Pipeline.add_directive(Pipeline.Directives.clear_buffer, target_buffer : buffer, color: 0x0);
            d.temporary = true;
            d = Graphics.Pipeline.add_directive(Pipeline.Directives.render_layers, target_buffer: buffer);
            d.temporary = true;
            buffer.dedicated_layer = new Layer("");
            d.include_layer(buffer.dedicated_layer);
            if (backup_layer == null) backup_layer = Graphics.current_layer;
            Graphics.current_layer = buffer.dedicated_layer;
        }

        public static void end_targeted_render_sequence()
        {
            Graphics.current_layer = backup_layer;
        }
    }    
}
