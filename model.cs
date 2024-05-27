using StbImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Audio.OpenAL;
using System.Xml.Linq;
using System.Reflection.Metadata;
using System.Drawing;
using System.Reflection;
using good_new_beggining;
using System.Globalization;
using System.Drawing.Imaging;
using System.Configuration;
using System.Transactions;
using OpenTK.Compute.OpenCL;
using OpenTK.Platform.Windows;

namespace good_new_beggining
{
    public class Model_base
    {

        private protected int vao;
        private protected int vbo;
        private protected int textureVBO;
        private protected int ebo;
        private protected int normalsVBO;
        private protected int textureID;

        private protected int _program;
        private protected string pathfile;
        private protected string picture_path;
        private protected float yRot = 5f;

        private protected List<Vector3> _vertices = new List<Vector3>();
        private protected List<Vector2> _texCoords = new List<Vector2>();
        private protected List<Vector3> _normals = new List<Vector3>();
        private protected List<int> _indices = new List<int>();
        private protected Matrix4 start_transition;

        public List<float> box= new List<float>();

        private protected Matrix4 model;
        private protected Matrix4 view;
        private protected Matrix4 projection;


        public float max_left = 0f;
        public float max_right = 0f;
        public float max_top = 0f;
        public float max_down = 0f;
        public Model_base(int program, string path_obj, string png, Matrix4 start_transition)
        {
            _program = program;
            this.pathfile= path_obj;
            this.picture_path = png;
            this.start_transition = start_transition;
        }
        private void LoadObj(string filename)
        {
            float minX = 1000f, maxX = -1000f, minY = 1000f, maxY = -1000f, minZ = 1000f, maxZ = -1000f;

            List<String> objLines = new List<string>(File.ReadAllLines(filename));

            

            foreach (string line in objLines)
            {
                
                string[] tokens = line.Split(' ');
                CultureInfo culture = new CultureInfo("en-US");
                switch (tokens[0])
                {
                    case "v":
                        {
                            var temp = new Vector3(
                            float.Parse(tokens[1], culture),
                            float.Parse(tokens[2], culture),
                            float.Parse(tokens[3], culture));
                            _vertices.Add(temp);
                            minX = Math.Min(minX, temp.X);
                            minY = Math.Min(minY, temp.Y);
                            minZ = Math.Min(minZ, temp.Z);

                            maxX = Math.Max(maxX, temp.X);
                            maxY = Math.Max(maxY, temp.Y);
                            maxZ = Math.Max(maxZ, temp.Z);
                        }

                        break;
                    case "vt":
                        _texCoords.Add(new Vector2(
                            float.Parse(tokens[1], culture),
                            float.Parse(tokens[2], culture)
                        ));
                        break;
                    case "vn":
                        _normals.Add(new Vector3(
                            float.Parse(tokens[1], culture),
                            float.Parse(tokens[2], culture),
                            float.Parse(tokens[3], culture)
                        ));
                        break;
                    case "f":
                        for (int i = 1; i <= 3; i++)
                        {
                            string[] faceTokens = tokens[i].Split('/');
                            int vertexIndex = int.Parse(faceTokens[0], culture) - 1;
                            int texCoordIndex = int.Parse(faceTokens[1], culture) - 1;
                            int normalIndex = int.Parse(faceTokens[2], culture) - 1;
                            _indices.Add((int)vertexIndex);
                        }
                        break;
                }

                


            }

            box.Add(minX);
            box.Add(minY);
            box.Add(minZ);
            box.Add(maxX);
            box.Add(maxY);
            box.Add(maxZ);

        }
        public void load()
        {
            LoadObj(pathfile);

            // Create Vertex Array Object
            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);


            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Count * Vector3.SizeInBytes, _vertices.ToArray(), BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);



            // Create  Buffer Object
            textureVBO=GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, textureVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, _texCoords.Count * Vector2.SizeInBytes, _texCoords.ToArray(), BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // Create Normal Buffer Object

            normalsVBO=GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, normalsVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, _normals.Count * Vector3.SizeInBytes, _normals.ToArray(), BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);

            // Create Element Buffer Object for indices
            ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            var temp = _indices.ToArray();
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Count * sizeof(int), temp, BufferUsageHint.StaticDraw);


            // Set up vertex attributes
            // Unbind buffers
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            use_shader_for_model("../../../shaders/default.vert", "../../../shaders/default.frag");
            InitTextures();

        }

        
        public void InitTextures()
        {

            textureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,(float)OpenTK.Graphics.OpenGL4.All.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)OpenTK.Graphics.OpenGL4.All.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)OpenTK.Graphics.OpenGL4.All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)OpenTK.Graphics.OpenGL4.All.Linear);

            string imageFileName = picture_path;

            Bitmap image;
            try
            {
                image = new Bitmap(imageFileName);
            }
            catch (Exception)
            {

                return;
            }
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
            
            BitmapData data = image.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0,
                PixelInternalFormat.Rgb, image.Width, image.Height,
                0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba,
                PixelType.UnsignedByte, data.Scan0);

            image.UnlockBits(data);
        }

        public void use_shader_for_model(string v_shader_path, string f_shader_path)
        {
            good_new_beggining.ShaderHelpers.InitShaders(v_shader_path, f_shader_path, out _program);
        }
        public void Draw(good_new_beggining.Camera camera, bool flag)
        {
            GL.BindVertexArray(vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, ebo);
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            view = camera.GetViewMatrix();
            projection = camera.GetProjectionMatrix();
            model=start_transition;
            

            //model *= start_transition;
            int modelLocation = GL.GetUniformLocation(_program, "model");
            int viewLocation = GL.GetUniformLocation(_program, "view");
            int projectionLocation = GL.GetUniformLocation(_program, "projection");

            GL.UniformMatrix4(modelLocation, true, ref model);
            GL.UniformMatrix4(viewLocation, true, ref view);
            GL.UniformMatrix4(projectionLocation, true, ref projection);



            GL.DrawElements(PrimitiveType.Triangles, _indices.Count, DrawElementsType.UnsignedInt, 0);
        }

        public void svoboda()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffer(vbo);
            GL.DeleteBuffer(ebo);
            GL.DeleteVertexArray(vao);
            GL.DeleteTexture(textureID);
            GL.DeleteProgram(_program);
            GL.DeleteBuffer(normalsVBO);
        }

    }


    sealed class Player_model:Model_base 
    {
        Matrix4 rotation = Matrix4.Identity;
        public static float rotate_angle = 0f;
        public void rotate_player(float angle)
        {
            rotation= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rotate_angle));
        }

        public Player_model(int program, string path_obj, string png, Matrix4 start_transition) : base(program, path_obj, png, start_transition)
        {
            _program = program;
            this.pathfile= path_obj;
            this.picture_path = png;
            this.start_transition = start_transition;
        }

        public void Draw(good_new_beggining.Camera camera)
        {
            GL.BindVertexArray(vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, ebo);
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            view = camera.GetViewMatrix();
            projection = camera.GetProjectionMatrix();
            rotate_player(rotate_angle);
            
            model = rotation;
            model *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(90f));
            model*=start_transition;

            int modelLocation = GL.GetUniformLocation(_program, "model");
            int viewLocation = GL.GetUniformLocation(_program, "view");
            int projectionLocation = GL.GetUniformLocation(_program, "projection");

            GL.UniformMatrix4(modelLocation, true, ref model);
            GL.UniformMatrix4(viewLocation, true, ref view);
            GL.UniformMatrix4(projectionLocation, true, ref projection);



            GL.DrawElements(PrimitiveType.Triangles, _indices.Count, DrawElementsType.UnsignedInt, 0);
        }

    }


    sealed class World_model : Model_base
    {



        public World_model(int program, string path_obj, string png, Matrix4 start_transition) : base(program, path_obj, png, start_transition)
        {
            _program = program;
            this.pathfile= path_obj;
            this.picture_path = png;
            this.start_transition = start_transition;
        }

        public void Draw(good_new_beggining.Camera camera)
        {
            GL.BindVertexArray(vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, ebo);
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            view = camera.GetViewMatrix();
            projection = camera.GetProjectionMatrix();
           

            model = Matrix4.CreateRotationY(yRot);
            model*=start_transition;
            yRot += 0.001f;

            

            int modelLocation = GL.GetUniformLocation(_program, "model");
            int viewLocation = GL.GetUniformLocation(_program, "view");
            int projectionLocation = GL.GetUniformLocation(_program, "projection");

            GL.UniformMatrix4(modelLocation, true, ref model);
            GL.UniformMatrix4(viewLocation, true, ref view);
            GL.UniformMatrix4(projectionLocation, true, ref projection);



            GL.DrawElements(PrimitiveType.Triangles, _indices.Count, DrawElementsType.UnsignedInt, 0);
        }

    }


    sealed public class Enemy_model : Model_base
    {
        public bool flag_set_angle = false;
        public float angle_trajectory;
        public float start_x;
        public float start_z;
        public bool flag_dead = false;
        public Enemy_model(int program, string path_obj, string png, Matrix4 start_transition) : base(program, path_obj, png, start_transition)
        {
            _program = program;
            this.pathfile= path_obj;
            this.picture_path = png;
            this.start_transition = start_transition;
            Vector4 coords = Vector4.One * start_transition;
            start_x = coords[0];
            start_z = coords[1];
        }

        public void Draw(good_new_beggining.Camera camera)
        {
            if (!flag_dead)
            {


            GL.BindVertexArray(vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, ebo);
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            view = camera.GetViewMatrix();
            projection = camera.GetProjectionMatrix();

            model = Matrix4.Identity;
            model*=start_transition;

            
            
            int modelLocation = GL.GetUniformLocation(_program, "model");
            int viewLocation = GL.GetUniformLocation(_program, "view");
            int projectionLocation = GL.GetUniformLocation(_program, "projection");

            GL.UniformMatrix4(modelLocation, true, ref model);
            GL.UniformMatrix4(viewLocation, true, ref view);
            GL.UniformMatrix4(projectionLocation, true, ref projection);



            GL.DrawElements(PrimitiveType.Triangles, _indices.Count, DrawElementsType.UnsignedInt, 0);
            }
        }

    }

    sealed public class Fireball_model : Model_base
    {

        public bool flag_ball = false;
        public bool flag_set_angle = false;

        public float speed = 0;
        public float angle_trajectory = 0;

        float start_x;
        float start_z;

        float time = 0f;
        public Fireball_model(int program, string path_obj, string png, Matrix4 start_transition) : base(program, path_obj, png, start_transition)
        {
            _program = program;
            this.pathfile= path_obj;
            this.picture_path = png;
            this.start_transition = start_transition;
        }

        public void set_ball(float x,float z)
        {
            start_x=x;
            start_z=z;
        }
        public void Draw(good_new_beggining.Camera camera, Enemy_model enemy)
        {
            if (flag_ball)
            {
                GL.BindVertexArray(vao);




                GL.BindBuffer(BufferTarget.ArrayBuffer, ebo);
                GL.BindTexture(TextureTarget.Texture2D, textureID);
                view = camera.GetViewMatrix();
                projection = camera.GetProjectionMatrix();

                model = Matrix4.Identity;

                float tr_x = (float)(start_x + (speed * Math.Cos(MathHelper.DegreesToRadians(angle_trajectory)) * time));
                float tr_z = (float)(start_z + 0.3f * Math.Sin(MathHelper.DegreesToRadians(angle_trajectory)) + (speed * Math.Sin(MathHelper.DegreesToRadians(angle_trajectory) * time) - 0.3 * Math.Pow(time, 2) / 2));


                model *= Matrix4.CreateTranslation(tr_x, tr_z, 0f);
                int modelLocation = GL.GetUniformLocation(_program, "model");
                int viewLocation = GL.GetUniformLocation(_program, "view");
                int projectionLocation = GL.GetUniformLocation(_program, "projection");

                GL.UniformMatrix4(modelLocation, true, ref model);
                GL.UniformMatrix4(viewLocation, true, ref view);
                GL.UniformMatrix4(projectionLocation, true, ref projection);



                GL.DrawElements(PrimitiveType.Triangles, _indices.Count, DrawElementsType.UnsignedInt, 0);

                float enemy_left = enemy.box[0];
                float enemy_bottom = enemy.box[2];
                float enemy_top = enemy.box[5];
                float t = tr_z- box[2];
                if (tr_x + box[3] >= enemy.start_x + enemy_left && (tr_z + box[5] >= enemy.start_z  && tr_z <= enemy.start_z))
                {
                    enemy.flag_dead = true;
                    flag_ball=false;
                    time = 0;

                }
                time += 0.01f;
                if (tr_x>40|| tr_z <0)
                {
                    flag_ball=false;
                    flag_set_angle = false;
                    time = 0;
                }
            }
        }

    }
}


