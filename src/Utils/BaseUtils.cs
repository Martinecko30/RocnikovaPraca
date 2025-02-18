using OpenTK.Mathematics;
using StbImageSharp;

namespace RocnikovaPraca.Utils;

using OpenTK.Graphics.OpenGL;

public class BaseUtils
{
    public static int LoadCubemap(List<string> faces)
    {
        int textureID = 0;
        textureID = GL.GenTexture();
        GL.BindTexture(TextureTarget.TextureCubeMap, textureID);
        
        StbImage.stbi_set_flip_vertically_on_load(0);

        for (var i = 0; i < faces.Count; i++)
        {
            string path = Path.GetFullPath(faces[i]);
            if (!File.Exists(path))
                throw new FileNotFoundException("Could not find file: " + faces[i] + " \nWas the file set to content?");
            
            using Stream stream = File.OpenRead(path);
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlue);
                
            if(image == null || image.Data == null || image.Data.Length == 0)
                throw new NullReferenceException($"An exception occured while loading cubemap. \n" +
                                                 $"{faces[i]} at path {path} \n" +
                                                 $"Face number = {i}.");

            GL.TexImage2D(
                TextureTarget.TextureCubeMapPositiveX + i,
                0,
                PixelInternalFormat.Rgb,
                image.Width,
                image.Height,
                0,
                PixelFormat.Rgb,
                PixelType.UnsignedByte,
                image.Data
            );
        }
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
        
        return textureID;
    }

    public static List<float> GetViewFrustum(Matrix4 projectionMatrix)
    {
        List<float> planes = new List<float>(6 * 4); // Initialize list with enough capacity for 24 floats (6 planes * 4 values each)

        for (int i = 0; i < 4; i++)
        {
            planes.Add(projectionMatrix[i, 3] + projectionMatrix[i, 0]); // left
            planes.Add(projectionMatrix[i, 3] - projectionMatrix[i, 0]); // right
            planes.Add(projectionMatrix[i, 3] + projectionMatrix[i, 1]); // bottom
            planes.Add(projectionMatrix[i, 3] - projectionMatrix[i, 1]); // top
            planes.Add(projectionMatrix[i, 3] + projectionMatrix[i, 2]); // near
            planes.Add(projectionMatrix[i, 3] - projectionMatrix[i, 2]); // far
        }

        return planes;
    }
    
    /*
    public static bool DoesFrustumSeeBox(Model model, Matrix4 projectionMatrix)
    {
        return DoesFrustumSeeBox(model.GetBoundingBox(), projectionMatrix);
    }

    public static bool DoesFrustumSeeBox(Box3 boundingBox, Matrix4 projectionMatrix)
    {
        List<float> frustumPlanes = GetViewFrustum(projectionMatrix);
        List<Vector3> boxPoints = GetBoundingBoxCorners(boundingBox);
        
        foreach (var plane in frustumPlanes)
        {
            bool allPointsOutside = true;

            //float A = plane[0], B = plane[1], C = plane[2], D = plane[3];

            foreach (var point in boxPoints)
            {
                float distance = A * point.X + B * point.Y + C * point.Z + D;
                
                if (distance >= 0)
                {
                    allPointsOutside = false;
                    break;
                }
            }

            if (allPointsOutside)
            {
                return false;
            }
        }
        return true;
    }
    */
    
    public static List<Vector3> GetBoundingBoxCorners(Box3 box)
    {
        List<Vector3> corners = new List<Vector3>(8);

        corners.Add(new Vector3(box.Min.X, box.Min.Y, box.Min.Z)); // Bottom-left-near
        corners.Add(new Vector3(box.Max.X, box.Min.Y, box.Min.Z)); // Bottom-right-near
        corners.Add(new Vector3(box.Min.X, box.Max.Y, box.Min.Z)); // Top-left-near
        corners.Add(new Vector3(box.Max.X, box.Max.Y, box.Min.Z)); // Top-right-near
        corners.Add(new Vector3(box.Min.X, box.Min.Y, box.Max.Z)); // Bottom-left-far
        corners.Add(new Vector3(box.Max.X, box.Min.Y, box.Max.Z)); // Bottom-right-far
        corners.Add(new Vector3(box.Min.X, box.Max.Y, box.Max.Z)); // Top-left-far
        corners.Add(new Vector3(box.Max.X, box.Max.Y, box.Max.Z)); // Top-right-far

        return corners;
    }
    
    public static List<Vector3> GetFrustumCornersWorldSpace(Matrix4 projectionMatrix, Matrix4 viewMatrix)
    {
        Matrix4 inv = Matrix4.Invert(projectionMatrix * viewMatrix);

        List<Vector3> frustumCorners = new List<Vector3>();

        for (int x = 0; x < 2; ++x)
        {
            for (int y = 0; y < 2; ++y)
            {
                for (int z = 0; z < 2; ++z)
                {
                    Vector4 pt = new Vector4(
                        2.0f * x - 1.0f,   // x: -1 or 1
                        2.0f * y - 1.0f,   // y: -1 or 1
                        2.0f * z - 1.0f,   // z: -1 or 1
                        1.0f             // homogeneous coordinate
                    );

                    Vector4 worldSpacePt = Vector4.Transform(pt, inv.ExtractRotation());
                    
                    Vector3 worldSpaceCorner = new Vector3(
                        worldSpacePt.X / worldSpacePt.W,
                        worldSpacePt.Y / worldSpacePt.W,
                        worldSpacePt.Z / worldSpacePt.W
                    );

                    frustumCorners.Add(worldSpaceCorner);
                }
            }
        }

        return frustumCorners;
    }
}