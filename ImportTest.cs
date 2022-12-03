using System;
using System.IO;
using System.Linq;
using Sledge.Formats.Map.Formats;
using Sledge.Formats.Map.Objects;
using UnityEngine;
using Mesh = UnityEngine.Mesh;
using Path = System.IO.Path;

namespace TrenchBroom
{
    // todo: support more map formats, make selecting file path selection easier
    
    public class ImportTest : MonoBehaviour
    {
        public enum MapFormat
        {
            Quake,
            Hammer,
            Jackhammer,
            Worldcraft
        }
        
        [SerializeField] public MaterialLoader materialLoader;
        [SerializeField] private string pathToImport;
        [SerializeField] private float scale;
        [SerializeField] private MeshView meshViewPrefab;
        [SerializeField] private MeshCollisionView meshCollisionViewPrefab;
        [SerializeField] private MapFormat mapFormat = MapFormat.Quake;
        [SerializeField] private Material[] noDraw;
        [SerializeField] private bool importOnStart;
        
        private int _solids;
        
        private void Start()
        {
            if (importOnStart)
                ImportMap();
        }

        public void ImportMap()
        {
            using FileStream mapFile = File.OpenRead(pathToImport);

            IMapFormat format = ParseMapFormat();
            MapFile loadedMap = format.Read(mapFile);

            Transform mapRoot = new GameObject($"Imported {format.Name}: {Path.GetFileName(pathToImport)}").transform;

            foreach (Solid solid in loadedMap.Worldspawn.Children.OfType<Solid>())
                InstantiateSolid(solid).SetParent(mapRoot);
            
            StaticBatchingUtility.Combine(mapRoot.gameObject);
        }

        private IMapFormat ParseMapFormat()
        {
            return mapFormat switch
            {
                MapFormat.Quake => new QuakeMapFormat(),
                MapFormat.Hammer => new HammerVmfFormat(),
                MapFormat.Jackhammer => new JackhammerJmfFormat(),
                MapFormat.Worldcraft => new WorldcraftRmfFormat(),
            
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private Transform InstantiateSolid(Solid solid)
        {
            Transform parent = new GameObject($"Solid {_solids++}").transform;
            
            var meshes = new Mesh[solid.Faces.Count];

            for (int i = 0; i < solid.Faces.Count; i++)
                meshes[i] = InstantiateFace(solid.Faces[i], parent).Mesh;

            InstantiateCollision(meshes, parent);
            return parent;
        }

        private bool IsNoDrawMaterial(Material target)
        {
            foreach (var material in noDraw)
            {
                if (target == material)
                    return true;
            }

            return false;
        }

        private MeshView InstantiateFace(Face face, Transform parent)
        {
            Material material = materialLoader.Lookup(face.TextureName);
            Texture texture = material.GetTexture("_BaseMap");
            
            MeshView meshView = Instantiate(meshViewPrefab, parent);
            meshView.Mesh = face.UnityMeshAndUV(texture.width, texture.height, scale, CoordinateSystem.Quake);
            meshView.Material = material;
            
            if (IsNoDrawMaterial(material))
                meshView.gameObject.SetActive(false);

            return meshView;
        }

        private MeshCollisionView InstantiateCollision(Mesh[] meshes, Transform parent)
        {
            MeshCollisionView collisionView = Instantiate(meshCollisionViewPrefab, parent);
            collisionView.BuildCollider(meshes);
            return collisionView;
        }
    }
}