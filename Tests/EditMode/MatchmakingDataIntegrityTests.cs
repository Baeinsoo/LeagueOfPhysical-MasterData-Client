
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace LOP.MasterData.Tests
{
    /// <summary>
    /// 큐·맵이 가리키는 게임 id가 실제로 존재하는지, 큐 정책 문자열이 유효한지 검사.
    /// 깨져도 컴파일은 통과하므로(데이터라서) 여기서 잡지 않으면 매칭 도중 터진다.
    /// </summary>
    public class MatchmakingDataIntegrityTests
    {
        // 선택 주체는 Luban enum이 아니라 string 컬럼이다(target_type 선례).
        // 어셈블리 경계 때문에 여기서 값을 못 박아 둔다 — LeagueOfPhysical-Client 레포의
        // docs/superpowers/specs/2026-07-27-matchmaking-standardization-design.md §4(큐 정책 컬럼과
        // Player/Server 값 정의)와 반드시 일치해야 한다. 그쪽 정책 값이 늘면 여기도 함께 갱신할 것.
        private static readonly HashSet<string> ValidSelectors = new() { "Player", "Server" };

        [Test]
        public void Queue_AllowedGameModeIds_ReferenceExistingGameModes()
        {
            var tables = MasterDataTestTableLoader.LoadTables();
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
            var tables = MasterDataTestTableLoader.LoadTables();
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
            var tables = MasterDataTestTableLoader.LoadTables();

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
            var tables = MasterDataTestTableLoader.LoadTables();

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
