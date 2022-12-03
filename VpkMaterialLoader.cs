using System;
using System.Collections.Generic;
using System.IO;
using Sledge.Formats.Packages;
using Sledge.Formats.Texture.Vtf;
using UnityEngine;

namespace TrenchBroom
{
    public class VpkMaterialLoader : MaterialLoader
    {
        private Dictionary<string, CacheData> _texturePathCache = new Dictionary<string, CacheData>();

        private struct CacheData
        {
            public VpkPackage Package;
            public PackageEntry Entry;
        }

        protected override void Initialize()
        {
            base.Initialize();
            _texturePathCache.Clear();
            
            foreach (string searchDirectory in pathsToSearch)
            {
                VpkPackage package = new VpkPackage(searchDirectory);

                foreach (var packageEntry in package.Entries)
                    _texturePathCache[packageEntry.Name] = new CacheData {Entry = packageEntry, Package = package};
            }
        }

        public override Texture2D LoadTexture(string textureName)
        {
            if (_texturePathCache.ContainsKey($"{Path.GetFileNameWithoutExtension(textureName).ToLower()}.vtf"))
            {
                var cache = _texturePathCache[$"{Path.GetFileNameWithoutExtension(textureName).ToLower()}.vtf"];
                VtfFile test = new VtfFile(cache.Package.Open(cache.Entry));
                Texture2D result = test.Images[^1].UnityTexture2D();
                Color[] pix = result.GetPixels();
                
                for (int row = 0; row < result.width; ++row)
                    Array.Reverse(pix, row * result.width, result.height);
                
                Array.Reverse(pix, 0, pix.Length);
                result.SetPixels(pix);
                return result;
            }

            return Texture2D.whiteTexture;
        }
    }
}