using System;
using System.Collections.Generic;
using System.IO;

using SFML.Graphics;

namespace XF
{
    
    static partial class Graphics
    {

        private static void parse_graphics_def_file(Texture tex, string file)
        {
            Debug.Log("Parsing def file " + file);

            using (StreamReader r = new StreamReader(file))
            {
                BitmapFont font = new BitmapFont(tex, file); // we create it now, but delete it at the end of procedure

                uint current_letter = 0;

                while (!r.EndOfStream)
                {
                    string l = r.ReadLine();

                    string[] words = l.Split(' ');

                    if (words.Length > 0)
                    {
                        switch (words[0])
                        {
                            case "sprites_x": tex.sequence.x = Convert.ToUInt32(words[1]);  break;
                            case "sprites_y": tex.sequence.y = Convert.ToUInt32(words[1]);  break;
                            case "margin"   : tex.margins    = Convert.ToInt32 (words[1]);  break;

                            case "smooth": tex.manipulate_smooth = true; break;

                            case "font"     : tex.is_font = true;break;
                            case "font_spacing_v": font.v_spacing = Convert.ToUInt32(words[1]); break;

                            case "font_wdt":
                            {                                
                                float w1 = (float)System.Convert.ToDouble(words[1]);
                                font.h_spacing = 0.0f;
                                for (uint ltr = 0; ltr < 255; ltr++)
                                {
                                    font.char_data[ltr].width = w1;
                                    font.char_data[ltr].seq_x = ltr % tex.sequence.x;
                                    font.char_data[ltr].seq_y = ltr / tex.sequence.x;
                                }

                            } break;

                            case "width_sequence":
                            {
                                string [] widths = words[1].Split(';');
                                for (int w = 0; w < widths.Length; w++)
                                {
                                    font.char_data[current_letter].width = (float)Convert.ToDouble(widths[w]);
                                    current_letter++;
                                }

                            } break;

                            case "extra_spacing":
                            {
                                foreach (var cd in font.char_data) cd.width += (float)Convert.ToDouble(words[1]);
                            } break;
                        }
                    }
                }

                if (tex.is_font)
                {
                    Graphics.fonts.Add(tex.identifier, font);
                    if (Graphics.fonts.Count == 1)
                    {
                        Graphics.default_font = font;
                        Graphics.current_font = font;
                    }
                }

            }                    
        }

        public static void load_batch_textures(string folder)
        {
            //Debug.Log("Loading textures from folder " + folder);
            Debug.StartLogGroup();

                DirectoryInfo dir = new DirectoryInfo(folder);

                if (dir.Exists)
                {
                    
                    FileInfo[] bmpfiles = dir.GetFiles("*.bmp", System.IO.SearchOption.AllDirectories);
                    FileInfo[] pngfiles = dir.GetFiles("*.png", System.IO.SearchOption.AllDirectories);
                    FileInfo[] files = new FileInfo[bmpfiles.Length + pngfiles.Length];

                    bmpfiles.CopyTo(files, 0);
                    pngfiles.CopyTo(files, bmpfiles.Length);

                    if (files.Length > 0) FileWatch.add_folder(folder);

                    for (int f = 0; f < files.Length; f++)
                    {
                        Texture tex = new Texture();
                        tex.assign_path(files[f].FullName);
                        textures.Add(tex.identifier, tex);
                    }
                }

            Debug.EndLogGroup();
        }

        public static void load_batch_shaders(string folder)
        {
            //Debug.Log("Loading shaders from folder " + folder);
            Debug.StartLogGroup();
                DirectoryInfo dir = new DirectoryInfo(folder);

                if (dir.Exists)
                {
                    FileInfo[] files = dir.GetFiles("*.sfx", System.IO.SearchOption.AllDirectories);                    
                    
                    for (int f = 0; f < files.Length; f++)
                    {
                        Debug.Log("Loading " + files[f].FullName);
                        bool succeeded = true; 
                        Shader shader = null;
                        try
                        {
                            shader = new Shader(files[f].FullName);
                        }
                        catch (SFML.LoadingFailedException e)
                        {
                            succeeded = false;
                            throw;
                        }

                        if (succeeded)
                        {
                            Debug.Log("Success!");
                            var temp = files[f].Name;
                            var identifier = files[f].Name.Remove(files[f].Name.LastIndexOf('.'));
                            shaders.Add(identifier, shader);
                        }
                        else
                        {
                            Debug.Log("Shader loading failed");
                        }
                        
                    }
                }
            Debug.EndLogGroup();

        }
    }
    
}