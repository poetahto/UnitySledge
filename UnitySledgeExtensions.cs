using System;
using Sledge.Formats.Map.Objects;
using Sledge.Formats.Texture.Vtf;
using UnityEngine;
using Mesh = UnityEngine.Mesh;

namespace TrenchBroom
{
    public enum CoordinateSystem
    {
        Quake,
        Unity
    }
    
    public static class UnitySledgeExtensions
    {
        public static Texture2D ChangeFormat(this Texture2D oldTexture, TextureFormat newFormat)
        {
            Texture2D newTex = new Texture2D(oldTexture.width, oldTexture.height, newFormat, false);
            newTex.SetPixels(oldTexture.GetPixels());
            newTex.Apply();

            return newTex;
        }
        
        public static Texture2D UnityTexture2D(this VtfImage image)
        {
            var result = new Texture2D(image.Width, image.Height, image.Format.UnityTextureFormat(), false);
            result.LoadRawTextureData(image.Data);
            return result.ChangeFormat(TextureFormat.RGB24);
        }
        
        // Not sure if all of these are correctly mapped, plus many don't have Unity analogs...
        public static TextureFormat UnityTextureFormat(this VtfImageFormat format)
        {
            return format switch
            {
                VtfImageFormat.Rgba8888 => TextureFormat.RGBA32,
                VtfImageFormat.Rgb888 => TextureFormat.RGB24,
                VtfImageFormat.Rgb565 => TextureFormat.RGB565,
                VtfImageFormat.A8 => TextureFormat.Alpha8,
                VtfImageFormat.Argb8888 => TextureFormat.ARGB32,
                VtfImageFormat.Bgra8888 => TextureFormat.BGRA32,
                VtfImageFormat.Dxt1 => TextureFormat.DXT1,
                VtfImageFormat.Dxt5 => TextureFormat.DXT5,
                VtfImageFormat.Rgba16161616F => TextureFormat.RGBAHalf,
                VtfImageFormat.Rgba16161616 => TextureFormat.RGBA64,
                VtfImageFormat.R32F => TextureFormat.RFloat,
                VtfImageFormat.Rgba32323232F => TextureFormat.RGBAFloat,
                _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
            };
        }
        
        public static Vector3 UnityVector(this System.Numerics.Vector3 vector3, 
            CoordinateSystem system = CoordinateSystem.Unity
            )
        {
            return system switch
            {
                CoordinateSystem.Quake => new Vector3(vector3.X, vector3.Z, vector3.Y),
                CoordinateSystem.Unity => new Vector3(vector3.X, vector3.Y, vector3.Z),
                _ => throw new ArgumentOutOfRangeException(nameof(system), system, null)
            };
        }

        public static void ApplyUVs(this Mesh mesh, 
            Surface surface,
            float scale,
            float uvWidth, 
            float uvHeight,
            CoordinateSystem system = CoordinateSystem.Unity
            )
        {
            Vector3[] vertices = mesh.vertices;
            var uvs = new Vector2[mesh.vertexCount];
            
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 currentVertex = vertices[i] / scale;
                
                // Valve-format UV conversions, largely based off this book:
                // https://book.leveldesignbook.com/appendix/resources/formats/map
                Vector3 uAxis = (surface.UAxis / surface.XScale).UnityVector(system);
                Vector3 vAxis = (surface.VAxis / surface.YScale).UnityVector(system);
                uvs[i].x = (Vector3.Dot(currentVertex, uAxis) + surface.XShift) / uvWidth;
                uvs[i].y = (Vector3.Dot(currentVertex, vAxis) + surface.YShift) / uvHeight;
                
                // Apply corrective rotation to fix coordinate space differences.
                // todo: probably turn this into switch statement on CoordinateSystem
                uvs[i] = Quaternion.Euler(180, 0, 0) * uvs[i];
            }
            
            mesh.SetUVs(0, uvs);
        }
        
        public static Mesh UnityMeshAndUV(this Face face,
            float uvWidth, 
            float uvHeight,
            float scale = 1,
            CoordinateSystem system = CoordinateSystem.Unity
            )
        {
            Mesh result = UnityMesh(face, scale, system);
            result.ApplyUVs(face, scale, uvWidth, uvHeight, system);
            return result;
        }
        
        public static Mesh UnityMesh(this Face face, 
            float scale = 1,
            CoordinateSystem system = CoordinateSystem.Unity
            )
        {
            int vertexCount = face.Vertices.Count;

            var vertices = new Vector3[vertexCount];
            var normals = new Vector3[vertexCount];
            var triangles = new int[vertexCount * 3];

            Vector3 normal = face.Plane.Normal.UnityVector(system);

            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 currentVertex = face.Vertices[i].UnityVector(system);
                vertices[i] = currentVertex * scale;
                
                // All normals are the same for the vertices of a plane.
                normals[i] = normal;
            }

            // Build triangles for face mesh. (are there better ways to do this?)
            for (int i = 0; i < vertexCount * 3; i++)
                triangles[i] = i % vertexCount;

            // Applying all the calculated data to a new mesh.
            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            return mesh;
        }
    }
}