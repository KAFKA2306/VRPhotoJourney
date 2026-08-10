using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace YourNamespace
{
    public static class ExhibitionPackGenerator
    {
        public const string ToolRevision = "exhibition-pack-v1";

        [Serializable]
        private class Report
        {
            public int schemaVersion = 1;
            public string exhibitionId = "";
            public string title = "";
            public string organizer = "";
            public string rightsStatus = "";
            public string generatedAtUtc = "";
            public string unityVersion = "";
            public string toolRevision = ToolRevision;
            public string sourceCommit = "UNAVAILABLE";
            public int inputCount;
            public int acceptedCount;
            public int rejectedCount;
            public ReportItem[] photos = Array.Empty<ReportItem>();
            public string[] generatedAssets = Array.Empty<string>();
            public string[] humanReview = Array.Empty<string>();
        }

        [Serializable]
        private class ReportItem
        {
            public string id = "";
            public int order;
            public string title = "";
            public string caption = "";
            public string author = "";
            public string checksumSha256 = "";
            public string status = "REJECTED";
            public string reasonCode = "UNVALIDATED";
            public string generatedTexture = "";
            public string generatedMaterial = "";
            public string generatedPrefab = "";
        }

        public static bool Generate(string manifestAssetPath, string outputAssetDirectory, GameObject photoFramePrefab)
        {
            if (photoFramePrefab == null) throw new ArgumentNullException(nameof(photoFramePrefab));
            if (string.IsNullOrWhiteSpace(manifestAssetPath)) throw new ArgumentException("Manifest path is required.");
            if (string.IsNullOrWhiteSpace(outputAssetDirectory) || !outputAssetDirectory.StartsWith("Assets/", StringComparison.Ordinal))
                throw new ArgumentException("Output directory must be under Assets/.");

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string manifestFullPath = Path.GetFullPath(Path.Combine(projectRoot, manifestAssetPath));
            if (!File.Exists(manifestFullPath)) throw new FileNotFoundException("Exhibition manifest was not found.", manifestAssetPath);

            ExhibitionManifest manifest = JsonUtility.FromJson<ExhibitionManifest>(File.ReadAllText(manifestFullPath));
            IReadOnlyList<ExhibitionPhotoAudit> audits = ExhibitionManifestPolicy.Audit(manifest, projectRoot);

            string generatedAt = DateTime.UtcNow.ToString("O");
            string sourceCommit = Environment.GetEnvironmentVariable("GITHUB_SHA") ?? "UNAVAILABLE";
            string packRoot = outputAssetDirectory.TrimEnd('/') + "/" + manifest.exhibitionId;
            string textureRoot = packRoot + "/Textures";
            string materialRoot = packRoot + "/Materials";
            string prefabRoot = packRoot + "/Prefabs";
            EnsureAssetFolder(packRoot);
            EnsureAssetFolder(textureRoot);
            EnsureAssetFolder(materialRoot);
            EnsureAssetFolder(prefabRoot);

            var reportItems = new List<ReportItem>();
            var generatedAssets = new List<string>();
            var generatedFrames = new List<GameObject>();

            try
            {
                foreach (ExhibitionPhotoAudit audit in audits.OrderBy(item => item.Photo.order))
                {
                    var item = new ReportItem
                    {
                        id = audit.Photo.id,
                        order = audit.Photo.order,
                        title = audit.Photo.title,
                        caption = audit.Photo.caption,
                        author = audit.Photo.author,
                        checksumSha256 = audit.ChecksumSha256,
                        status = audit.Status,
                        reasonCode = audit.ReasonCode
                    };

                    if (audit.Status != "ACCEPTED")
                    {
                        reportItems.Add(item);
                        continue;
                    }

                    string sourceAssetPath = ExhibitionManifestPolicy.ProjectRelativePath(projectRoot, audit.SourcePath);
                    string extension = Path.GetExtension(sourceAssetPath).ToLowerInvariant();
                    string texturePath = $"{textureRoot}/{audit.Photo.id}{extension}";
                    string materialPath = $"{materialRoot}/{audit.Photo.id}.mat";
                    string prefabPath = $"{prefabRoot}/{audit.Photo.id}.prefab";

                    DeleteGeneratedAsset(texturePath);
                    DeleteGeneratedAsset(materialPath);
                    DeleteGeneratedAsset(prefabPath);

                    if (!AssetDatabase.CopyAsset(sourceAssetPath, texturePath))
                    {
                        item.status = "REJECTED";
                        item.reasonCode = "IMPORT_FAILED";
                        reportItems.Add(item);
                        continue;
                    }
                    AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceSynchronousImport);
                    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                    if (texture == null)
                    {
                        DeleteGeneratedAsset(texturePath);
                        item.status = "REJECTED";
                        item.reasonCode = "IMPORT_FAILED";
                        reportItems.Add(item);
                        continue;
                    }

                    GameObject frame = PrefabUtility.InstantiatePrefab(photoFramePrefab) as GameObject;
                    if (frame == null)
                    {
                        DeleteGeneratedAsset(texturePath);
                        item.status = "REJECTED";
                        item.reasonCode = "PREFAB_INSTANTIATION_FAILED";
                        reportItems.Add(item);
                        continue;
                    }

                    frame.name = audit.Photo.id;
                    Renderer renderer = frame.GetComponentInChildren<Renderer>();
                    if (renderer == null)
                    {
                        UnityEngine.Object.DestroyImmediate(frame);
                        DeleteGeneratedAsset(texturePath);
                        item.status = "REJECTED";
                        item.reasonCode = "FRAME_RENDERER_MISSING";
                        reportItems.Add(item);
                        continue;
                    }

                    Shader shader = renderer.sharedMaterial != null ? renderer.sharedMaterial.shader : Shader.Find("Standard");
                    if (shader == null)
                    {
                        UnityEngine.Object.DestroyImmediate(frame);
                        DeleteGeneratedAsset(texturePath);
                        item.status = "REJECTED";
                        item.reasonCode = "SHADER_UNAVAILABLE";
                        reportItems.Add(item);
                        continue;
                    }

                    var material = new Material(shader) { name = audit.Photo.id + "-material", mainTexture = texture };
                    AssetDatabase.CreateAsset(material, materialPath);
                    renderer.sharedMaterial = material;
                    PrefabUtility.SaveAsPrefabAsset(frame, prefabPath, out bool saved);
                    UnityEngine.Object.DestroyImmediate(frame);
                    if (!saved)
                    {
                        DeleteGeneratedAsset(prefabPath);
                        DeleteGeneratedAsset(materialPath);
                        DeleteGeneratedAsset(texturePath);
                        item.status = "REJECTED";
                        item.reasonCode = "PREFAB_SAVE_FAILED";
                        reportItems.Add(item);
                        continue;
                    }

                    item.generatedTexture = texturePath;
                    item.generatedMaterial = materialPath;
                    item.generatedPrefab = prefabPath;
                    item.reasonCode = "GENERATED";
                    generatedAssets.Add(texturePath);
                    generatedAssets.Add(materialPath);
                    generatedAssets.Add(prefabPath);
                    reportItems.Add(item);

                    GameObject generatedFrame = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (generatedFrame != null) generatedFrames.Add(generatedFrame);
                }

                int accepted = reportItems.Count(item => item.status == "ACCEPTED" && item.reasonCode == "GENERATED");
                if (accepted == 0)
                {
                    WriteReports(packRoot, manifest, reportItems, generatedAssets, generatedAt, sourceCommit,
                        new[] { "No photo was successfully generated; this pack is not deliverable." });
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.LogError("Exhibition generation failed closed: zero photos were generated.");
                    return false;
                }

                GameObject exhibitionRoot = new GameObject(manifest.exhibitionId + "-exhibition");
                try
                {
                    for (int i = 0; i < generatedFrames.Count; i++)
                    {
                        GameObject child = PrefabUtility.InstantiatePrefab(generatedFrames[i], exhibitionRoot.transform) as GameObject;
                        if (child != null)
                        {
                            child.name = generatedFrames[i].name;
                            child.transform.localPosition = new Vector3(i * 1.25f, 0f, 0f);
                        }
                    }
                    string exhibitionPrefabPath = packRoot + "/" + manifest.exhibitionId + ".prefab";
                    DeleteGeneratedAsset(exhibitionPrefabPath);
                    PrefabUtility.SaveAsPrefabAsset(exhibitionRoot, exhibitionPrefabPath, out bool rootSaved);
                    if (!rootSaved) throw new InvalidOperationException("Failed to save exhibition root prefab.");
                    generatedAssets.Add(exhibitionPrefabPath);
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(exhibitionRoot);
                }

                WriteReports(packRoot, manifest, reportItems, generatedAssets, generatedAt, sourceCommit,
                    new[]
                    {
                        "Confirm every displayed photo/caption/author against the client-approved manifest.",
                        "Verify VRChat world performance and visual layout in the target project before publication."
                    });
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return true;
            }
            catch
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                throw;
            }
        }

        private static void WriteReports(string packRoot, ExhibitionManifest manifest, List<ReportItem> items,
            List<string> generatedAssets, string generatedAt, string sourceCommit, string[] humanReview)
        {
            var report = new Report
            {
                exhibitionId = manifest.exhibitionId,
                title = manifest.title,
                organizer = manifest.organizer,
                rightsStatus = manifest.rightsStatus,
                generatedAtUtc = generatedAt,
                unityVersion = Application.unityVersion,
                sourceCommit = sourceCommit,
                inputCount = items.Count,
                acceptedCount = items.Count(item => item.status == "ACCEPTED" && item.reasonCode == "GENERATED"),
                rejectedCount = items.Count(item => item.status != "ACCEPTED" || item.reasonCode != "GENERATED"),
                photos = items.ToArray(),
                generatedAssets = generatedAssets.ToArray(),
                humanReview = humanReview
            };
            File.WriteAllText(AssetPathToFullPath(packRoot + "/exhibition-report.json"), JsonUtility.ToJson(report, true));

            var markdownAudits = items.Select(item => new ExhibitionPhotoAudit
            {
                Photo = new ExhibitionPhoto { id = item.id, order = item.order, title = item.title, caption = item.caption, author = item.author },
                ChecksumSha256 = item.checksumSha256,
                Status = item.status == "ACCEPTED" && item.reasonCode == "GENERATED" ? "ACCEPTED" : "REJECTED",
                ReasonCode = item.reasonCode
            });
            File.WriteAllText(AssetPathToFullPath(packRoot + "/exhibition-report.md"),
                ExhibitionManifestPolicy.ToMarkdown(manifest, markdownAudits, generatedAt, Application.unityVersion, ToolRevision));
        }

        private static string AssetPathToFullPath(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.GetFullPath(Path.Combine(projectRoot, assetPath));
        }

        private static void DeleteGeneratedAsset(string assetPath)
        {
            if (AssetDatabase.LoadMainAssetAtPath(assetPath) != null) AssetDatabase.DeleteAsset(assetPath);
        }

        private static void EnsureAssetFolder(string assetPath)
        {
            string[] parts = assetPath.Split('/');
            if (parts.Length == 0 || parts[0] != "Assets") throw new ArgumentException("Asset folder must be under Assets/.");
            string current = "Assets";
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
