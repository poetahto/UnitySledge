// using System.Linq;
// using Sledge.Formats.Map.Objects;
// using UnityEngine;
//
// namespace TrenchBroom
// {
//     public class MapFactory
//     {
//         private readonly MapFile _mapFile;
//         
//         public MapFactory(MapFile mapFile)
//         {
//             _mapFile = mapFile;
//         }
//
//         public GameObject SpawnMap()
//         {
//             GameObject mapRoot = new GameObject("Imported Map");
//
//             foreach (Solid solid in _mapFile.Worldspawn.Children.OfType<Solid>())
//                 InstantiateSolid(solid).SetParent(mapRoot.transform);
//             
//             StaticBatchingUtility.Combine(mapRoot);
//             return mapRoot;
//         }
//         
//         private Transform InstantiateSolid(Solid solid)
//         {
//             Transform parent = new GameObject($"Solid {_solids++}").transform;
//             
//             var meshes = new Mesh[solid.Faces.Count];
//
//             for (int i = 0; i < solid.Faces.Count; i++)
//             {
//                 var face = InstantiateFace(solid.Faces[i]);
//                 face.transform.SetParent(parent);
//                 meshes[i] = face.Mesh;
//             }
//
//             var collision = InstantiateCollision(meshes);
//             collision.transform.SetParent(parent);
//
//             return parent;
//         }
//         
//         private bool IsNoDrawMaterial(Material target)
//         {
//             foreach (var material in noDraw)
//             {
//                 if (target == material)
//                     return true;
//             }
//
//             return false;
//         }
//
//         private MeshFaceView InstantiateFace(Face face)
//         {
//             Material material = materialLoader.LoadTexture(face.TextureName);
//             var faceObject = Instantiate(meshFaceViewPrefab);
//             faceObject.Initialize(face, material, scale);
//             
//             if (IsNoDrawMaterial(material))
//                 faceObject.gameObject.SetActive(false);
//
//             return faceObject;
//         }
//
//         private MeshCollisionView InstantiateCollision(Mesh[] meshes)
//         {
//             var collisionObject = Instantiate(meshCollisionViewPrefab);
//             collisionObject.Initialize(meshes);
//             return collisionObject;
//         }
//     }
// }