
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Luban;
using NUnit.Framework;

namespace LOP.MasterData.Tests
{
    /// <summary>
    /// 큐·맵이 가리키는 게임 id가 실제로 존재하는지, 큐 정책 문자열이 유효한지 검사.
    /// 깨져도 컴파일은 통과하므로(데이터라서) 여기서 잡지 않으면 매칭 도중 터진다.
    /// </summary>
    public class MatchmakingDataIntegrityTests
    {
        private const string StreamingAssetsRelative =
            "Packages/com.baegames.lop.masterdata.client/Runtime.Generated/StreamingAssets/MasterData";

        // 선택 주체는 Luban enum이 아니라 string 컬럼이다(target_type 선례).
        // 어셈블리 경계 때문에 여기서 값을 못 박아 둔다 — 정책 값이 늘면 함께 갱신할 것.
        private static readonly HashSet<string> ValidSelectors = new() { "Player", "Server" };

        private static Tables LoadTables()
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

        [Test]
        public void Queue_AllowedGameModeIds_ReferenceExistingGameModes()
        {
            var tables = LoadTables();
            var gameModeIds = tables.TbGameMode.DataList.Select(x => x.Id).ToHashSet();

            foreach (var queue in tables.TbQueue.DataList)
            {
                Assert.IsNotEmpty(queue.AllowedGameModeIds,
                    $"큐 {queue.Code}(id={queue.Id})의 허용 게임 목록이 비었다 — 아무도 매칭될 수 없다.");

                foreach (var id in queue.AllowedGameModeIds)
                {
                    Assert.IsTrue(gameModeIds.Contains(id),
                        $"큐 {queue.Code}가 없는 게임 id {id}를 가리킨다.");
                }
            }
        }

        [Test]
        public void Map_GameModeId_ReferencesExistingGameMode()
        {
            var tables = LoadTables();
            var gameModeIds = tables.TbGameMode.DataList.Select(x => x.Id).ToHashSet();

            foreach (var map in tables.TbMap.DataList)
            {
                Assert.IsTrue(gameModeIds.Contains(map.GameModeId),
                    $"맵 {map.Code}(id={map.Id})가 없는 게임 id {map.GameModeId}를 가리킨다.");
            }
        }

        [Test]
        public void Queue_SelectorValues_AreValid()
        {
            var tables = LoadTables();

            foreach (var queue in tables.TbQueue.DataList)
            {
                Assert.IsTrue(ValidSelectors.Contains(queue.GameModeSelector),
                    $"큐 {queue.Code}의 game_mode_selector가 '{queue.GameModeSelector}' — Player/Server만 유효.");
                Assert.IsTrue(ValidSelectors.Contains(queue.MapSelector),
                    $"큐 {queue.Code}의 map_selector가 '{queue.MapSelector}' — Player/Server만 유효.");
            }
        }

        [Test]
        public void GameMode_PlayerCounts_AreSane()
        {
            var tables = LoadTables();

            foreach (var gameMode in tables.TbGameMode.DataList)
            {
                Assert.Greater(gameMode.MinPlayers, 0,
                    $"게임 {gameMode.Code}의 최소 인원이 0 이하다.");
                Assert.GreaterOrEqual(gameMode.MaxPlayers, gameMode.MinPlayers,
                    $"게임 {gameMode.Code}의 최대 인원이 최소보다 작다.");
            }
        }
    }
}
