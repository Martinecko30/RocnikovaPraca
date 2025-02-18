using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace RocnikovaPraca.Shaders;

public class Shader : IDisposable
{
    public int Handle;
    private int vertexShaderHandle, fragShaderHandle;

    private Dictionary<string, int> uniformLocations;

    
    public Shader(string vertexShaderPath, string fragmentShaderPath)
    {
        string shaderSource = File.ReadAllText(vertexShaderPath);
        vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShaderHandle, shaderSource);
        CompileShader(vertexShaderHandle);

        shaderSource = File.ReadAllText(fragmentShaderPath);
        fragShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragShaderHandle, shaderSource);
        CompileShader(fragShaderHandle);

        Handle = GL.CreateProgram();
        
        GL.AttachShader(Handle, vertexShaderHandle);
        GL.AttachShader(Handle, fragShaderHandle);
        
        LinkProgram();

        uniformLocations = new();
        
        GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

        for (int i = 0; i < numberOfUniforms; i++)
        {
            string key = GL.GetActiveUniform(Handle, i, out _, out _);
            int location = GL.GetUniformLocation(Handle, key);
            uniformLocations.Add(key, location);
        }
        
        
        
        GL.DetachShader(Handle, vertexShaderHandle);
        GL.DeleteShader(vertexShaderHandle);
        GL.DetachShader(Handle, fragShaderHandle);
        GL.DeleteShader(fragShaderHandle);
    }

    private void CompileShader(int shader)
    {
        GL.CompileShader(shader);
        
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            var infoLog = GL.GetShaderInfoLog(shader);
            throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
        }
    }

    private void LinkProgram()
    {
        GL.LinkProgram(Handle);
        
        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
        {
            string infoLog = GL.GetProgramInfoLog(Handle);
            throw new Exception($"Error occurred whilst linking Program {Handle}.\n\n{infoLog}");
        }
    }
    
    public void Use()
    {
        GL.UseProgram(Handle);
    }
    
    /*
     * It's unsafe to use this function to get attributes
     * Assign attributes location in VBO
     * IDK why but someone told me this
     */
    [Obsolete("It's unsafe to use this function to get attributes\nAssign attributes location in VBO")]
    public int GetAttribLocation(string attribName)
    {
        return GL.GetAttribLocation(Handle, attribName);
    }

    public int GetUniformLocation(string uniformName)
    {
        return GL.GetUniformLocation(Handle, uniformName);
    }
    
    
    public void SetInt(string name, int data)
    {
        GL.UseProgram(Handle);
        GL.Uniform1(UniformLocationsLookUp(name), data);
    }
    
        
    public void SetFloat(string name, float data)
    {
        GL.UseProgram(Handle);
        GL.Uniform1(UniformLocationsLookUp(name), data);
    }

        
    public void SetMatrix4(string name, Matrix4 data)
    {
        GL.UseProgram(Handle);
        GL.UniformMatrix4(UniformLocationsLookUp(name), true, ref data);
    }

    public void SetVector3(string name, Vector3 data)
    {
        GL.UseProgram(Handle);
        GL.Uniform3(UniformLocationsLookUp(name), data);
    }

    public void SetMatrix3(string name, Matrix3 data)
    {
        GL.UseProgram(Handle);
        GL.UniformMatrix3(UniformLocationsLookUp(name), true, ref data);
    }

    public void SetBool(string name, bool data)
    {
        SetInt(name, data ? 1 : 0);
    }
    
    

    private int UniformLocationsLookUp(string name)
    {
        if (uniformLocations.TryGetValue(name, out int value))
        {
            return value;
        }
        else
        {
            value = GetUniformLocation(name);
            return (uniformLocations[name] = value);
        }
    }
    
    
    
    private bool disposedValue = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            GL.DeleteProgram(Handle);

            disposedValue = true;
        }
    }

    ~Shader()
    {
        if (disposedValue == false)
        {
            Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
        }
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}