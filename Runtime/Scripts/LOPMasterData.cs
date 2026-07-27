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
        /// <summary>
        /// 이 패키지가 싣고 오는 테이블 파일 stem 목록. 생성물(<c>Tables.cs</c>의 loader 키 = 실제 <c>.bytes</c>)과
        /// 반드시 일치해야 하며, 새 Luban 테이블 추가 시 여기도 갱신해야 한다.
        /// 어긋나면 <see cref="LoadAsync"/>가 Entrance 단계에서 KeyNotFoundException으로 죽는다 — EditMode 테스트가 지킨다.
        /// </summary>
        public static readonly System.Collections.Generic.IReadOnlyList<string> TableFiles = new[]
        {
            "tbcharacter", "tbskin", "tbskinasset", "tbitem", "tbstatuseffect", "tbability",
            "tbcharacterloadout", "tbabilityview", "tbstatuseffectview",
            "tbgamemode", "tbmap", "tbqueue"
        };

        public Tables Tables { get; private set; }

        public async Task LoadAsync()
        {
            var blobs = new Dictionary<string, byte[]>(TableFiles.Count);
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
