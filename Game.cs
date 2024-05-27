using StbImageSharp;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Graphics;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using good_new_beggining;
using System.Management;
using System.Numerics;

namespace good_new_beggining
{
    // Game class that inherets from the Game Window Class
    internal class Game : GameWindow
    { 
        // camera
        Camera camera;

        // transformation variables
        float yRot = 0f;


        int width, height;


        //Model model_ikran;
        World_model model_world;
        Player_model player_Model;
        Enemy_model model_enemy;
        Fireball_model model_fireball;
        public Game(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.width = width;
            this.height = height;

            // center window
            CenterWindow(new Vector2i(width, height));
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
            this.width = e.Width;
            this.height = e.Height;
        }

        // called once when game is started
        protected override void OnLoad()
        {
            base.OnLoad();
            
            GL.Enable(EnableCap.DepthTest);
            //ikran spawn

            //model_ikran = new Model(0, "../../../object_ikran/ikran.obj", "../../../object_ikran/Ikran_Normal.png",Matrix4.CreateTranslation(-5f,0f,18f));
            //model_ikran.load();
            player_Model= new Player_model(0, "../../../object_ikran/ikran.obj", "../../../object_ikran/ikran_Normal.png", Matrix4.CreateTranslation(-10f,6f,0f));
            player_Model.load();

            Console.WriteLine("player loaded");
            //world spawn
            model_world=new World_model(0, "../../../object_world/world20.obj", "../../../object_world/mountain_soil.jpg", Matrix4.CreateTranslation(0f,0f,0f));
            model_world.load();
            Console.WriteLine("world loaded");
            //enemy spawn

            model_enemy=new Enemy_model(0, "../../../object_enemy/enemy.obj", "../../../object_enemy/2389905.png", Matrix4.CreateTranslation(10f, 6f, 0f));
            model_enemy.load();
            Console.WriteLine("Enemy loaded");
            //fireball spawn
            model_fireball = new Fireball_model(0, "../../../object_fireball/fireball.obj", "../../../object_fireball/red.png", Matrix4.CreateTranslation(0, 0, 0f));
            model_fireball.load();
            Console.WriteLine("Projectile loaded");

            camera = new Camera(width, height, OpenTK.Mathematics.Vector3.Zero);
            

            CursorState = CursorState.Grabbed;
        }
        // called once when game is closed
        protected override void OnUnload()
        {
            base.OnUnload();
            //model_ikran.svoboda();
            model_world.svoboda();
            model_enemy.svoboda();

        }
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            if (0>1)
            {
                //// Set the color to fill the screen with
                //GL.ClearColor(0.3f, 0.3f, 1f, 1f);
                //// Fill the screen with the color
                //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                //// draw our triangle
                //GL.UseProgram(shaderProgram); // bind vao
                //GL.BindVertexArray(vao); // use shader program
                //GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);

                //GL.BindTexture(TextureTarget.Texture2D, textureID);


                //// transformation matrices
                //Matrix4 model = Matrix4.Identity;
                //Matrix4 view = camera.GetViewMatrix();
                //Matrix4 projection = camera.GetProjectionMatrix();


                //model = Matrix4.CreateRotationY(yRot);
                //yRot += 0.001f;

                //Matrix4 translation = Matrix4.CreateTranslation(0f, 0f, -3f);

                //model *= translation;

                //int modelLocation = GL.GetUniformLocation(shaderProgram, "model");
                //int viewLocation = GL.GetUniformLocation(shaderProgram, "view");
                //int projectionLocation = GL.GetUniformLocation(shaderProgram, "projection");

                //GL.UniformMatrix4(modelLocation, true, ref model);
                //GL.UniformMatrix4(viewLocation, true, ref view);
                //GL.UniformMatrix4(projectionLocation, true, ref projection);

                //GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
                ////GL.DrawArrays(PrimitiveType.Triangles, 0, 3); // draw the triangle | args = Primitive type, first vertex, last vertex


                // swap the buffers
            }
            GL.ClearColor(Color4.AliceBlue);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            model_world.Draw(camera, false);
            player_Model.Draw(camera);
            model_enemy.Draw(camera);
            if (model_fireball.flag_ball)
            {
                float coord_x = player_Model.box[3];
                float coord_z = player_Model.box[5];
                coord_x*=(float)MathHelper.Cos(MathHelper.DegreesToRadians(Player_model.rotate_angle));
                coord_z*=(float)MathHelper.Sin(MathHelper.DegreesToRadians(Player_model.rotate_angle));
                coord_x= -10 + coord_x;
                coord_z= 6 + coord_z;
                model_fireball.set_ball(coord_x, coord_z);
                if (!model_fireball.flag_set_angle)
                {
                    model_fireball.angle_trajectory =Player_model.rotate_angle;
                    model_fireball.flag_set_angle=true;
                    
                }

                model_fireball.Draw(camera, model_enemy);

            }
            Context.SwapBuffers();

            base.OnRenderFrame(args);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            MouseState mouse = MouseState;
            KeyboardState input = KeyboardState;

            base.OnUpdateFrame(args);
            camera.InputController(input, mouse, args,model_fireball);
        }
       

    }
}