using System;
using OpenTK.Graphics.OpenGL;
using System.IO;

namespace good_new_beggining
{
    class ShaderHelpers
    {
        ///<summary>
        ///Create a program object and make current
        ///</summary>
        ///<param name="vShader">a vertex shader program</param>
        ///<param name="fShader">a fragment shader program</param>
        ///<param name="program">created program</param>
        ///<returns>
        ///return true, if the program object was created and successfully made current
        ///</returns>
        public static bool InitShaders(string vShaderPath, string fShaderPath, out int program)
        {
            string vShaderSource, fShaderSource;
            LoadShaderFromFile(vShaderPath, out vShaderSource);
            LoadShaderFromFile(fShaderPath, out fShaderSource);

            program = CreateProgram(vShaderSource, fShaderSource);

            if (program == 0)
            {

                return false;
            }

            GL.UseProgram(program);

            return true;
        }

        private static int CreateProgram(string vShader, string fShader)
        {
            // Create shader object
            int vertexShader = CreateShader(ShaderType.VertexShader, vShader);
            int fragmentShader = CreateShader(ShaderType.FragmentShader, fShader);
            if (vertexShader == 0 || fragmentShader == 0)
            {
                return 0;
            }

            // Create a program object
            int program = GL.CreateProgram();
            if (program == 0)
            {
                return 0;
            }

            // Attach the shader objects
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);

            // Link the program object
            GL.LinkProgram(program);

            // Check the result of linking
            int status;
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out status);
            if (status == 0)
            {
                string errorString = string.Format("Failed to link program: {0}" + Environment.NewLine, GL.GetProgramInfoLog(program));

                GL.DeleteProgram(program);
                GL.DeleteShader(vertexShader);
                GL.DeleteShader(fragmentShader);
                return 0;
            }

            return program;
        }

        ///<summary>
        ///Load a shader from a file
        ///</summary>
        ///<param name="errorOutputFileName">a file name for error messages</param>
        ///<param name="fileName">a file name to a shader</param>
        ///<param name="shaderSource">a shader source string</param>
        public static void LoadShaderFromFile(string shaderFileName, out string shaderSource)
        {
            shaderSource = null;

            using (StreamReader sr = new StreamReader(shaderFileName))
            {
                shaderSource = sr.ReadToEnd();
            }
        }

        private static int CreateShader(ShaderType shaderType, string shaderSource)
        {
            // Create shader object
            int shader = GL.CreateShader(shaderType);
            if (shader == 0)
            {
                return 0;
            }

            // Set the shader program
            GL.ShaderSource(shader, shaderSource);

            // Compile the shader
            GL.CompileShader(shader);

            // Check the result of compilation
            int status;
            GL.GetShader(shader, ShaderParameter.CompileStatus, out status);
            if (status == 0)
            {
                string errorString = string.Format("Failed to compile {0} shader: {1}", shaderType.ToString(), GL.GetShaderInfoLog(shader));

                GL.DeleteShader(shader);
                return 0;
            }

            return shader;
        }
    }
}
