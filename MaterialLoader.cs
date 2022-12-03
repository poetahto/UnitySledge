using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TrenchBroom
{
    public abstract class MaterialLoader : MonoBehaviour
    {
        [SerializeField] protected string[] pathsToSearch;
        [SerializeField] protected Material baseMaterial;
        [SerializeField] protected Optional<string> cacheFolder;
        
        private Dictionary<string, Material> _materialCache = new Dictionary<string, Material>();
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        
        public abstract Texture2D LoadTexture(string path);
        
        private bool _initialized;
        
        protected virtual void Initialize()
        {
            _initialized = true;
            _materialCache.Clear();

            #if UNITY_EDITOR

                if (cacheFolder.ShouldBeUsed)
                {
                    foreach (string file in Directory.GetFiles(cacheFolder.Value))
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file);
                        string path = "Assets/" + Path.GetRelativePath(Application.dataPath, file);
                        
                        if (Path.GetExtension(file) == ".mat")
                            _materialCache[fileName] = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
                    }
                }

            #endif
        }
        
        public Material Lookup(string target)
        {
            if (!_initialized)
                Initialize();

            string textureName = Path.GetFileNameWithoutExtension(target);
            
            if (_materialCache.ContainsKey(textureName))
                return _materialCache[textureName];

            Material instance = new Material(baseMaterial);
            Texture2D result = LoadTexture(textureName);
            
            #if UNITY_EDITOR

                if (cacheFolder.ShouldBeUsed)
                {
                    string path = "Assets/" + Path.GetRelativePath(Application.dataPath, cacheFolder.Value);
                        
                    if (Directory.Exists(cacheFolder.Value))
                    {
                        var saveTexture = new Texture2D(result.width, result.height);
                        saveTexture.SetPixels(result.GetPixels());
                        File.WriteAllBytes($"{cacheFolder.Value}/{textureName}.jpg", saveTexture.EncodeToJPG());

                        UnityEditor.AssetDatabase.CreateAsset(instance, $"{path}/{textureName}.mat");
                        UnityEditor.AssetDatabase.ImportAsset($"{path}/{textureName}.jpg");
                        instance = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>($"{path}/{textureName}.mat");
                        result = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>($"{path}/{textureName}.jpg");
                    }
                }
                
            #endif
            
            instance.SetTexture(BaseMap, result);
            _materialCache[Path.GetFileNameWithoutExtension(target)] = instance;
            
            return instance;
        }
    }
}