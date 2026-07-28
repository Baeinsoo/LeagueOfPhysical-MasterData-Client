using System.IO;
using Luban;
using NUnit.Framework;

namespace LOP.MasterData.Tests
{
    /// <summary>
    /// EditMode 데이터 무결성 테스트들이 공유하는 테이블 로더.
    /// <para>
    /// <c>LOPMasterData.LoadAsync()</c>는 내부적으로 <c>UnityWebRequest</c>를 쓰는데, 이는 EditMode에서
    /// 블로킹 대기하기에 안전하지 않다(코루틴/비동기 완료가 에디터 루프에 묶여 있음). 그래서 이 테스트들은
    /// 패키지가 실제로 배포하는 <c>.bytes</c> 파일을 <c>File.ReadAllBytes</c>로 직접 읽어 <see cref="Tables"/>를
    /// 구성한다.
    /// </para>
    /// </summary>
    internal static class MasterDataTestTableLoader
    {
        private const string StreamingAssetsRelative =
            "Packages/com.baegames.lop.masterdata.client/Runtime.Generated/StreamingAssets/MasterData";

        internal static Tables LoadTables()
        {
            string dir = Path.GetFullPath(StreamingAssetsRelative);
            Assert.IsTrue(Directory.Exists(dir), "StreamingAssets 폴더를 찾지 못했다: " + dir);

            return new Tables(name =>
            {
                string path = Path.Combine(dir, name + ".bytes");
                Assert.IsTrue(File.Exists(path), "테이블 파일을 찾지 못했다: " + path);
                return new ByteBuf(File.ReadAllBytes(path));
            });
        }
    }
}
