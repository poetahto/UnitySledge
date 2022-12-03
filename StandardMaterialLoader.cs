using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TrenchBroom
{
    public class StandardMaterialLoader : MaterialLoader
    {
        private Dictionary<string, string> _texturePathCache = new Dictionary<string, string>();

        protected override void Initialize()
        {
            base.Initialize();
            
            _texturePathCache.Clear();

            foreach (string lookupPath in pathsToSearch)
            {
                string[] filesToSearch = Directory.GetFiles(lookupPath);
                
                foreach (string filePath in filesToSearch)
                {
                    if (IsValidExtension(filePath))
                        _texturePathCache.Add(Path.GetFileNameWithoutExtension(filePath), filePath);
                }
            }
        }

        private static bool IsValidExtension(string path)
        {
            return Path.GetExtension(path) == ".exr" || Path.GetExtension(path) == ".jpg" ||
                   Path.GetExtension(path) == ".png" || Path.GetExtension(path) == ".tga";
        }

        public override Texture2D LoadTexture(string textureName)
        {
            return _texturePathCache.ContainsKey(textureName) 
                ? LoadPNG(_texturePathCache[textureName])
                : Texture2D.whiteTexture;
        }

        private static Texture2D LoadPNG(string filePath) 
        {
            Texture2D tex = null;

            if (File.Exists(filePath))
            {
                tex = new Texture2D(1, 1);
                byte[] fileData = File.ReadAllBytes(filePath);
                tex.LoadImage(fileData);
            }

            return tex;
        }
    }
}