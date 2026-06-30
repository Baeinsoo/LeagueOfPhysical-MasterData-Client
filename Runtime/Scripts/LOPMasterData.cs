using Cysharp.Threading.Tasks;
using Luban;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace LOP.MasterData
{
    /// <summary>
    /// Thin client-side wrapper that owns the Luban-generated <see cref="Tables"/> and
    /// async-preloads the binary table files from StreamingAssets (Android-safe).
    /// No domain logic. Registered as a VContainer Singleton in LOP-Client.
    /// </summary>
    public class LOPMasterData
    {
        // loader keys == generated Tables.cs loader("...") keys == .bytes file stems
        private static readonly string[] TableFiles =
        {
            "tbcharacter", "tbskin", "tbskinasset", "tbitem", "tbstatuseffect", "tbability"
        };

        public Tables Tables { get; private set; }

        public async Task LoadAsync()
        {
            var blobs = new Dictionary<string, byte[]>(TableFiles.Length);
            foreach (var name in TableFiles)
            {
                blobs[name] = await LoadBytes($"MasterData/{name}.bytes");
            }
            Tables = new Tables(file => new ByteBuf(blobs[file]));
        }

        private static async Task<byte[]> LoadBytes(string relativePath)
        {
            string uri;
#if UNITY_EDITOR
            // In the editor, a package's StreamingAssets are NOT merged into
            // Application.streamingAssetsPath (that points at the project's Assets/StreamingAssets).
            // Resolve this package's own StreamingAssets via the virtual Packages/ path.
            // (In a player build, Unity copies package StreamingAssets into the build's
            //  StreamingAssets, so the streamingAssetsPath branches below are correct there.)
            uri = "file://" + Path.GetFullPath(
                $"Packages/com.baegames.lop.masterdata.client/Runtime.Generated/StreamingAssets/{relativePath}");
#elif UNITY_ANDROID
            uri = Path.Combine(Application.streamingAssetsPath, relativePath);
#else
            uri = "file://" + Path.Combine(Application.streamingAssetsPath, relativePath);
#endif
            using var www = UnityWebRequest.Get(uri);
            await www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[LOPMasterData] Failed to load {uri}: {www.error}");
                return Array.Empty<byte>();
            }
            return www.downloadHandler.data;
        }
    }
}
