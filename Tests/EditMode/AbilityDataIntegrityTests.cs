using NUnit.Framework;

namespace LOP.MasterData.Tests
{
    /// <summary>
    /// <c>TbAbility</c>가 참조하는 값들이 실제로 존재/유효한지 검사.
    /// <para>
    /// 클라(<c>AbilityDataProvider</c>)는 <c>StatusEffectApplyEffect.TargetType</c>을 런타임에
    /// <c>System.Enum.Parse</c>(대소문자 구분)로 파싱한다 — 데이터가 틀리면 즉시 예외로 죽는다.
    /// 이 테스트가 그물이기 때문에, 클라 쪽은 알 수 없는 id/값을 방어적으로 되묻지 않고 그대로 신뢰해도 된다.
    /// </para>
    /// <para>
    /// 이 패키지는 <c>LOP.TargetType</c>(LOP-Shared)을 참조하지 않는다(패키지 경계 — 새 asmdef 참조를
    /// 추가하지 않기로 함). 그래서 enum 타입 자체가 아니라 알려진 멤버 이름 문자열 목록으로 검사한다.
    /// </para>
    /// </summary>
    public class AbilityDataIntegrityTests
    {
        // LOP.TargetType(LeagueOfPhysical-Shared/Runtime/Scripts/Game/Ability/AbilityEffect.cs)의 멤버 이름과
        // 반드시 일치해야 한다. 그쪽에 멤버가 추가/변경되면 여기도 함께 갱신할 것.
        private static readonly string[] KnownTargetTypeNames = { "Self", "HitTargets" };

        [Test]
        public void AbilityStatusEffectApplyEffects_ReferenceExistingStatusEffectAndValidTargetType()
        {
            var tables = MasterDataTestTableLoader.LoadTables();

            foreach (var ability in tables.TbAbility.DataList)
            {
                foreach (var effect in ability.Effects)
                {
                    if (!(effect is StatusEffectApplyEffect apply))
                    {
                        continue;
                    }

                    Assert.IsTrue(
                        tables.TbStatusEffect.DataMap.ContainsKey(apply.StatusEffectId),
                        $"TbAbility id={ability.Id}({ability.Code})의 StatusEffectApplyEffect가 " +
                        $"존재하지 않는 StatusEffectId={apply.StatusEffectId}를 참조한다.");

                    Assert.Contains(
                        apply.TargetType, KnownTargetTypeNames,
                        $"TbAbility id={ability.Id}({ability.Code})의 StatusEffectApplyEffect.TargetType=" +
                        $"\"{apply.TargetType}\"이 LOP.TargetType의 알려진 값({string.Join(", ", KnownTargetTypeNames)})이 아니다. " +
                        "런타임에서 System.Enum.Parse가 예외를 던진다.");
                }
            }
        }
    }
}
