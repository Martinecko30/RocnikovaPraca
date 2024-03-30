using Assimp;
using OpenTK.Graphics.OpenGL;
using StbImageSharp;
using TextureWrapMode = OpenTK.Graphics.OpenGL.TextureWrapMode;

namespace RocnikovaPraca.Objects;

public class Texture
{
    public int textureID;

    public string type;
    public string path;
    
    public Texture(string filePath, string type)
    {
        this.type = type;
        this.path = filePath;
        textureID = GL.GenTexture();
        
        //GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, textureID);
        
        StbImage.stbi_set_flip_vertically_on_load(1);

        using (Stream stream = File.OpenRead(filePath))
        {
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            
            GL.TexImage2D(
                TextureTarget.Texture2D, 
                0, 
                PixelInternalFormat.Rgba, 
                image.Width, 
                image.Height, 
                0, 
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                image.Data
            );
        }
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
    }

    public Texture(string type, Color4D color)
    {
        this.type = type;
        this.path = "";
        textureID = GL.GenTexture();
        
        GL.BindTexture(TextureTarget.Texture2D, textureID);

        GL.TexImage2D(
            TextureTarget.Texture2D, 
            0, 
            PixelInternalFormat.Rgba, 
            1, 
            1, 
            0, 
            PixelFormat.Rgba,
            PixelType.Float,
            ref color
        );
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
    }

    public void Use(TextureUnit unit = TextureUnit.Texture0)
    {
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.Texture2D, textureID);
    }
}