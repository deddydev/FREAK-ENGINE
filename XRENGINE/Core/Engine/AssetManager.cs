﻿using Microsoft.DotNet.PlatformAbstractions;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using XREngine.Core.Files;
using XREngine.Data;
using YamlDotNet.Serialization;

namespace XREngine
{
    public class AssetManager
    {
        public const string AssetExtension = "asset";

        public AssetManager(string? engineAssetsDirPath = null)
        {
            if (!string.IsNullOrWhiteSpace(engineAssetsDirPath) && Directory.Exists(engineAssetsDirPath))
                EngineAssetsPath = engineAssetsDirPath;
            else
            {
                string? basePath = ApplicationEnvironment.ApplicationBasePath;
                //Iterate up the directory tree until we find the Build directory
                while (basePath is not null && !Directory.Exists(Path.Combine(basePath, "Build")))
                    basePath = Path.GetDirectoryName(basePath);
                if (basePath is null)
                    throw new DirectoryNotFoundException("Could not find the Build directory in the application path.");
                string buildDirectory = Path.Combine(basePath, "Build");
                EngineAssetsPath = Path.Combine(buildDirectory, "CommonAssets");
            }

            VerifyPathExists(GameAssetsPath);

            Watcher.Path = GameAssetsPath;
            Watcher.Filter = "*.*";
            Watcher.IncludeSubdirectories = true;
            Watcher.EnableRaisingEvents = true;
            Watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            Watcher.Created += FileCreated;
            Watcher.Changed += FileChanged;
            Watcher.Deleted += FileDeleted;
            Watcher.Error += FileError;
            Watcher.Renamed += FileRenamed;
        }

        private bool VerifyPathExists(string? directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                return false;

            if (Directory.Exists(directoryPath))
                return true;

            string? parent = Path.GetDirectoryName(directoryPath);

            if (VerifyPathExists(parent))
                Directory.CreateDirectory(directoryPath);

            return true;

        }

        void FileCreated(object sender, FileSystemEventArgs args)
        {
            Debug.Out($"File '{args.FullPath}' was created.");
        }
        void FileChanged(object sender, FileSystemEventArgs args)
        {
            Debug.Out($"File '{args.FullPath}' was changed.");
        }
        void FileDeleted(object sender, FileSystemEventArgs args)
        {
            Debug.Out($"File '{args.FullPath}' was deleted.");
            //Leave files intact
            //if (LoadedAssetsByPathInternal.TryGetValue(args.FullPath, out var list))
            //{
            //    foreach (var asset in list)
            //        asset.Destroy();
            //    LoadedAssetsByPathInternal.Remove(args.FullPath);
            //}
        }
        void FileError(object sender, ErrorEventArgs args)
        {
            Debug.LogWarning($"An error occurred in the file system watcher: {args.GetException().Message}");
        }
        void FileRenamed(object sender, RenamedEventArgs args)
        {
            Debug.Out($"File '{args.OldFullPath}' was renamed to '{args.FullPath}'.");

            if (!LoadedAssetsByPathInternal.TryGetValue(args.OldFullPath, out var asset))
                return;

            LoadedAssetsByPathInternal.Remove(args.OldFullPath, out _);
            LoadedAssetsByPathInternal.TryAdd(args.FullPath, asset);
        }

        private void CacheAsset(XRAsset asset)
        {
            string path = asset.FilePath ?? string.Empty;
            XRAsset UpdatePathDict(string existingPath, XRAsset existingAsset)
            {
                if (existingAsset is not null)
                {
                    if (existingAsset != asset && !existingAsset.EmbeddedAssets.Contains(asset))
                        existingAsset.EmbeddedAssets.Add(asset);
                    return existingAsset;
                }
                else
                    return asset;
            }
            LoadedAssetsByPathInternal.AddOrUpdate(path, asset, UpdatePathDict);

            if (asset.ID == Guid.Empty)
            {
                Debug.LogWarning("An asset was loaded with an empty ID.");
                return;
            }

            XRAsset UpdateIDDict(Guid existingID, XRAsset existingAsset)
            {
                Debug.Out($"An asset with the ID {existingID} already exists in the asset manager. The new asset will be added to the list of assets with the same ID.");
                return existingAsset;
            }
            LoadedAssetsByIDInternal.AddOrUpdate(asset.ID, asset, UpdateIDDict);
        }

        public FileSystemWatcher Watcher { get; } = new FileSystemWatcher();
        /// <summary>
        /// This is the path to /Build/CommonAssets/ in the root folder of the engine.
        /// </summary>
        public string EngineAssetsPath { get; }
        public string GameAssetsPath { get; set; } = Path.Combine(ApplicationEnvironment.ApplicationBasePath, "Assets");
        public string PackagesPath { get; set; } = Path.Combine(ApplicationEnvironment.ApplicationBasePath, "Packages");
        public ConcurrentDictionary<string, XRAsset> LoadedAssetsByPathInternal { get; } = [];
        public ConcurrentDictionary<Guid, XRAsset> LoadedAssetsByIDInternal { get; } = [];
        
        public XRAsset? GetAssetByID(Guid id)
            => LoadedAssetsByIDInternal.TryGetValue(id, out XRAsset? asset) ? asset : null;

        public bool TryGetAssetByID(Guid id, [NotNullWhen(true)] out XRAsset? asset)
            => LoadedAssetsByIDInternal.TryGetValue(id, out asset);

        public XRAsset? GetAssetByPath(string path)
            => LoadedAssetsByPathInternal.TryGetValue(path, out var asset) ? asset : null;

        public bool TryGetAssetByPath(string path, [NotNullWhen(true)] out XRAsset? asset)
            => LoadedAssetsByPathInternal.TryGetValue(path, out asset);

        public async Task<T?> LoadEngineAssetAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(params string[] relativePathFolders) where T : XRAsset, new()
            => await LoadAsync<T>(ResolveEngineAssetPath(relativePathFolders));

        public async Task<T?> LoadGameAssetAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(params string[] relativePathFolders) where T : XRAsset, new()
            => await LoadAsync<T>(ResolveGameAssetPath(relativePathFolders));

        public T? LoadEngineAsset<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(params string[] relativePathFolders) where T : XRAsset, new()
            => Load<T>(ResolveEngineAssetPath(relativePathFolders));

        public T? LoadGameAsset<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(params string[] relativePathFolders) where T : XRAsset, new()
            => Load<T>(ResolveGameAssetPath(relativePathFolders));

        //public async Task<T?> LoadEngine3rdPartyAssetAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(params string[] relativePathFolders) where T : XR3rdPartyAsset, new()
        //    => await XR3rdPartyAsset.LoadAsync<T>(Path.Combine(EngineAssetsPath, Path.Combine(relativePathFolders)));

        //public async Task<T?> LoadGame3rdPartyAssetAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(params string[] relativePathFolders) where T : XR3rdPartyAsset, new()
        //    => await XR3rdPartyAsset.LoadAsync<T>(Path.Combine(GameAssetsPath, Path.Combine(relativePathFolders)));

        //public T? LoadEngine3rdPartyAsset<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(params string[] relativePathFolders) where T : XR3rdPartyAsset, new()
        //    => XR3rdPartyAsset.Load<T>(Path.Combine(EngineAssetsPath, Path.Combine(relativePathFolders)));

        //public T? LoadGame3rdPartyAsset<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(params string[] relativePathFolders) where T : XR3rdPartyAsset, new()
        //    => XR3rdPartyAsset.Load<T>(Path.Combine(GameAssetsPath, Path.Combine(relativePathFolders)));

        /// <summary>
        /// Creates a full path to an asset in the engine's asset directory.
        /// </summary>
        /// <param name="relativePathFolders"></param>
        /// <returns></returns>
        public string ResolveEngineAssetPath(params string[] relativePathFolders)
            => Path.Combine(EngineAssetsPath, Path.Combine(relativePathFolders));

        /// <summary>
        /// Creates a full path to an asset in the game's asset directory.
        /// </summary>
        /// <param name="relativePathFolders"></param>
        /// <returns></returns>
        public string ResolveGameAssetPath(params string[] relativePathFolders)
            => Path.Combine(GameAssetsPath, Path.Combine(relativePathFolders));

        public void Dispose()
        {
            foreach (var asset in LoadedAssetsByIDInternal.Values)
                asset.Destroy();
            LoadedAssetsByIDInternal.Clear();
            LoadedAssetsByPathInternal.Clear();
        }

        public event Action<XRAsset>? AssetLoaded;
        public event Action<XRAsset>? AssetSaved;

        private void PostSaved(XRAsset asset, bool newAsset)
        {
            if (newAsset)
                CacheAsset(asset);
            AssetSaved?.Invoke(asset);
        }

        private void PostLoaded<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(string filePath, T? file) where T : XRAsset
        {
            if (file is null)
                return;

            file.FilePath = filePath;
            CacheAsset(file);
            AssetLoaded?.Invoke(file);
        }

        public async Task<T?> LoadAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(string filePath) where T : XRAsset, new()
        {
            T? file;
#if !DEBUG
            try
            {
#endif
            if (TryGetAssetByPath(filePath, out XRAsset? existingAsset))
                return existingAsset is T tAsset ? tAsset : null;

            file = !File.Exists(filePath)
                ? null
                : await DeserializeAsync<T>(filePath);
            PostLoaded(filePath, file);
#if !DEBUG
            }
            catch (Exception e)
            {
                return null;
            }
#endif
            return file;
        }

        public T? Load<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(string filePath) where T : XRAsset, new()
        {
            T? file;
#if !DEBUG
            try
            {
#endif
            if (TryGetAssetByPath(filePath, out XRAsset? existingAsset))
                return existingAsset is T tAsset ? tAsset : null;

            file = !File.Exists(filePath)
                ? null
                : Deserialize<T>(filePath);
            PostLoaded(filePath, file);
#if !DEBUG
            }
            catch (Exception e)
            {
                return null;
            }
#endif
            return file;
        }

        public async Task SaveAsync(XRAsset asset)
        {
            if (asset.FilePath is null)
            {
                Debug.LogWarning("Cannot save an asset without a file path.");
                return;
            }
#if !DEBUG
            try
            {
#endif
            await File.WriteAllTextAsync(asset.FilePath, Serializer.Serialize(this));
            PostSaved(asset, false);
#if !DEBUG
            }
            catch (Exception e)
            {

            }
#endif
        }

        public void Save(XRAsset asset)
        {
            if (asset.FilePath is null)
            {
                Debug.LogWarning("Cannot save an asset without a file path.");
                return;
            }
#if !DEBUG
            try
            {
#endif
            File.WriteAllText(asset.FilePath, Serializer.Serialize(this));
            PostSaved(asset, false);
#if !DEBUG
            }
            catch (Exception e)
            {

            }
#endif
        }

        public void SaveTo(XRAsset asset, string directory)
        {
#if !DEBUG
            try
            {
#endif
            string path = Path.Combine(directory, $"{(string.IsNullOrWhiteSpace(asset.Name) ? GetType().Name : asset.Name)}.{AssetExtension}");
            File.WriteAllText(path, Serializer.Serialize(this));
            asset.FilePath = path;
            PostSaved(asset, true);
#if !DEBUG
            }
            catch (Exception e)
            {

            }
#endif
        }

        public async Task SaveToAsync(XRAsset asset, string directory)
        {
#if !DEBUG
            try
            {
#endif
            string path = Path.Combine(directory, $"{(string.IsNullOrWhiteSpace(asset.Name) ? GetType().Name : asset.Name)}.{AssetExtension}");
            await File.WriteAllTextAsync(path, Serializer.Serialize(this));
            asset.FilePath = path;
            CacheAsset(asset);
            PostSaved(asset, true);
#if !DEBUG
            }
            catch (Exception e)
            {

            }
#endif
        }

        private static readonly ISerializer Serializer =
            new SerializerBuilder()
            .WithTypeConverter(new DataSourceYamlTypeConverter())
            .WithTypeConverter(new XRAssetYamlTypeConverter())
            .Build();
        private static readonly IDeserializer Deserializer =
            new DeserializerBuilder()
            .WithTypeConverter(new DataSourceYamlTypeConverter())
            .WithTypeConverter(new XRAssetYamlTypeConverter())
            .Build();

        private static T? Deserialize<T>(string filePath) where T : XRAsset, new()
        {
            string ext = Path.GetExtension(filePath)[1..].ToLowerInvariant();
            if (ext == AssetExtension)
                return Deserializer.Deserialize<T>(File.ReadAllText(filePath));
            else
            {
                var extensions3rdParty = typeof(T).GetCustomAttribute<XR3rdPartyExtensionsAttribute>()?.Extensions;
                if (extensions3rdParty?.Contains(ext) ?? false)
                {
                    var asset = new T();
                    asset.Load3rdParty(filePath);
                    return asset;
                }
                else
                {
                    Debug.LogWarning($"The file extension '{ext}' is not supported by the asset type '{typeof(T).Name}'.");
                    return null;
                }
            }
        }

        private static async Task<T?> DeserializeAsync<T>(string filePath) where T : XRAsset, new()
        {
            string ext = Path.GetExtension(filePath)[1..].ToLowerInvariant();
            if (ext == AssetExtension)
                return await Task.Run(async () => Deserializer.Deserialize<T>(await File.ReadAllTextAsync(filePath)));
            else
            {
                var exts = typeof(T).GetCustomAttribute<XR3rdPartyExtensionsAttribute>()?.Extensions;
                if (exts?.Contains(ext) ?? false)
                {
                    var asset = new T();
                    await asset.Load3rdPartyAsync(filePath);
                    return asset;
                }
                else
                {
                    Debug.LogWarning($"The file extension '{ext}' is not supported by the asset type '{typeof(T).Name}'.");
                    return null;
                }
            }
        }
    }

    /// <summary>
    /// These are the extensions that will be recognized by the asset manager as 3rd-party loadable for this asset.
    /// </summary>
    /// <param name="extensions"></param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class XR3rdPartyExtensionsAttribute(params string[] extensions) : Attribute
    {
        /// <summary>
        /// These are the 3rd-party file extensions that this asset type can load.
        /// </summary>
        public string[] Extensions { get; } = extensions;
    }
}
