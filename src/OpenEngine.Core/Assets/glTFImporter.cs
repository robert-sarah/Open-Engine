// Created By Levi Enama
// glTF Importer for Loading glTF/glb Models
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using SharpGLTF.Schema2;

namespace OpenEngine.Core.Assets
{
    public class glTFImportResult
    {
        public string Name { get; set; }
        public List<MeshData> Meshes { get; set; }
        public List<AnimationClip> Animations { get; set; }
        public Dictionary<string, NodeTransform> Nodes { get; set; }
        public List<SkeletonData> Skeletons { get; set; }

        public glTFImportResult()
        {
            Meshes = new List<MeshData>();
            Animations = new List<AnimationClip>();
            Nodes = new Dictionary<string, NodeTransform>();
            Skeletons = new List<SkeletonData>();
        }
    }

    public class NodeTransform
    {
        public string Name { get; set; }
        public Vector3 Translation { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public string Parent { get; set; }
        public List<string> Children { get; set; }
        public int? MeshIndex { get; set; }

        public NodeTransform()
        {
            Translation = Vector3.Zero;
            Rotation = Quaternion.Identity;
            Scale = Vector3.One;
            Children = new List<string>();
        }
    }

    public class SkeletonData
    {
        public string Name { get; set; }
        public List<BoneData> Bones { get; set; }
        public Dictionary<string, Matrix4x4> InverseBindMatrices { get; set; }

        public SkeletonData()
        {
            Bones = new List<BoneData>();
            InverseBindMatrices = new Dictionary<string, Matrix4x4>();
        }
    }

    public class BoneData
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public Matrix4x4 InverseBindMatrix { get; set; }
        public string Parent { get; set; }
        public List<string> Children { get; set; }

        public BoneData()
        {
            Children = new List<string>();
        }
    }

    public class glTFImporter
    {
        public static glTFImportResult Import(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"glTF file not found: {filePath}");
            }

            ModelRoot model;
            
            // Determine file type and load accordingly
            if (Path.GetExtension(filePath).ToLower() == ".glb")
            {
                model = ModelRoot.LoadGLB(filePath);
            }
            else
            {
                model = ModelRoot.Load(filePath);
            }

            return ImportFromModel(model);
        }

        public static glTFImportResult ImportFromModel(ModelRoot model)
        {
            var result = new glTFImportResult
            {
                Name = model.Asset?.Generator ?? "ImportedModel"
            };

            // Load meshes
            LoadMeshes(model, result);

            // Load nodes and transforms
            LoadNodes(model, result);

            // Load skeletons
            LoadSkeletons(model, result);

            // Load animations
            LoadAnimations(model, result);

            return result;
        }

        private static void LoadMeshes(ModelRoot model, glTFImportResult result)
        {
            foreach (var mesh in model.LogicalMeshes)
            {
                foreach (var primitive in mesh.Primitives)
                {
                    var material = primitive.Material != null ? model.LogicalMaterials[primitive.Material] : null;
                    var meshData = Loader.MeshLoader.LoadMeshFromPrimitive(primitive, material);
                    meshData.Name = mesh.Name ?? $"Mesh_{result.Meshes.Count}";
                    result.Meshes.Add(meshData);
                }
            }
        }

        private static void LoadNodes(ModelRoot model, glTFImportResult result)
        {
            foreach (var node in model.LogicalNodes)
            {
                var transform = new NodeTransform
                {
                    Name = node.Name ?? $"Node_{node.LogicalIndex}",
                    Translation = node.LocalTransform.Translation,
                    Rotation = node.LocalTransform.Rotation,
                    Scale = node.LocalTransform.Scale,
                    MeshIndex = node.Mesh?.LogicalIndex
                };

                // Set parent
                if (node.Parent != null)
                {
                    transform.Parent = node.Parent.Name ?? $"Node_{node.Parent.LogicalIndex}";
                }

                // Set children
                foreach (var child in node.VisualChildren)
                {
                    transform.Children.Add(child.Name ?? $"Node_{child.LogicalIndex}");
                }

                result.Nodes[transform.Name] = transform;
            }
        }

        private static void LoadSkeletons(ModelRoot model, glTFImportResult result)
        {
            foreach (var skin in model.LogicalSkins)
            {
                var skeleton = new SkeletonData
                {
                    Name = skin.Name ?? $"Skeleton_{result.Skeletons.Count}"
                };

                // Load inverse bind matrices
                if (skin.InverseBindMatrices != null)
                {
                    var accessor = model.LogicalAccessors[skin.InverseBindMatrices];
                    var matrices = accessor.AsMatrix4x4Array();

                    for (int i = 0; i < skin.Joints.Count && i < matrices.Length; i++)
                    {
                        var jointNode = model.LogicalNodes[skin.Joints[i]];
                        var boneName = jointNode.Name ?? $"Bone_{i}";
                        
                        skeleton.InverseBindMatrices[boneName] = matrices[i];
                    }
                }

                // Load bones
                for (int i = 0; i < skin.Joints.Count; i++)
                {
                    var jointNode = model.LogicalNodes[skin.Joints[i]];
                    var bone = new BoneData
                    {
                        Name = jointNode.Name ?? $"Bone_{i}",
                        Index = i
                    };

                    if (skeleton.InverseBindMatrices.ContainsKey(bone.Name))
                    {
                        bone.InverseBindMatrix = skeleton.InverseBindMatrices[bone.Name];
                    }

                    // Set parent
                    if (jointNode.Parent != null)
                    {
                        bone.Parent = jointNode.Parent.Name ?? $"Bone_{jointNode.Parent.LogicalIndex}";
                    }

                    // Set children
                    foreach (var child in jointNode.VisualChildren)
                    {
                        bone.Children.Add(child.Name ?? $"Bone_{child.LogicalIndex}");
                    }

                    skeleton.Bones.Add(bone);
                }

                result.Skeletons.Add(skeleton);
            }
        }

        private static void LoadAnimations(ModelRoot model, glTFImportResult result)
        {
            foreach (var animation in model.LogicalAnimations)
            {
                var clip = AnimationLoader.LoadAnimation(animation, model);
                result.Animations.Add(clip);
            }
        }

        public static glTFImportResult ImportFromMemory(byte[] data, bool isBinary = false)
        {
            ModelRoot model;
            
            if (isBinary)
            {
                model = ModelRoot.ParseGLB(data);
            }
            else
            {
                model = ModelRoot.Parse(System.Text.Encoding.UTF8.GetString(data));
            }

            return ImportFromModel(model);
        }
    }
}
