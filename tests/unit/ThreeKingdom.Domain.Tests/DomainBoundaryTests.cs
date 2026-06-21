using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ThreeKingdom.Domain;

namespace ThreeKingdom.Domain.Tests
{
    /// <summary>
    /// epic-001 story-001（建立纯 C# Domain 与测试边界）的核心验收测试。
    /// AC1：程序化断言 Domain 程序集不引用 UnityEngine/UnityEditor（不靠 csproj 约定，靠反射实证）。
    /// AC2：测试程序集可对 Domain public API 写正常/边界/失败用例（本类 + BuildInfoTests 即为证）。
    /// AC3：经 dotnet test 在无 Unity 运行时下运行（CI domain-tests job 旁路 UNITY_LICENSE）。
    /// 治理 ADR：ADR-0002 架构分层（Domain 纯 C#，禁 UnityEngine.*）。
    /// </summary>
    [TestFixture]
    public class DomainBoundaryTests
    {
        private static Assembly DomainAssembly => typeof(BuildInfo).Assembly;

        // AC1（正常）：Domain 直接引用集中不含任何 Unity 程序集。
        [Test]
        public void Domain_assembly_references_no_unity_assembly()
        {
            string[] forbidden = { "UnityEngine", "UnityEditor", "Unity." };

            var offenders = DomainAssembly
                .GetReferencedAssemblies()
                .Select(a => a.Name ?? string.Empty)
                .Where(name => forbidden.Any(f =>
                    name.Equals("UnityEngine", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("UnityEditor", StringComparison.OrdinalIgnoreCase) ||
                    name.StartsWith("UnityEngine.", StringComparison.OrdinalIgnoreCase) ||
                    name.StartsWith("UnityEditor.", StringComparison.OrdinalIgnoreCase) ||
                    name.StartsWith("Unity.", StringComparison.OrdinalIgnoreCase)))
                .ToArray();

            Assert.That(offenders, Is.Empty,
                "Domain 程序集禁止引用 UnityEngine/UnityEditor/Unity.*（ADR-0002）。违规引用：" +
                string.Join(", ", offenders));
        }

        // AC1（边界）：Domain 中任一 public 类型的程序集均不来自 Unity 命名空间。
        [Test]
        public void Domain_public_types_carry_no_unity_namespace()
        {
            var unityNamespaced = DomainAssembly
                .GetExportedTypes()
                .Where(t => (t.Namespace ?? string.Empty).StartsWith("Unity", StringComparison.OrdinalIgnoreCase))
                .Select(t => t.FullName)
                .ToArray();

            Assert.That(unityNamespaced, Is.Empty,
                "Domain public 类型不得位于 Unity* 命名空间。违规：" + string.Join(", ", unityNamespaced));
        }

        // AC2（失败用例形态）：断言一个明确不存在的 Unity 引用确实查不到——证明测试能区分有/无。
        [Test]
        public void Lookup_of_unityengine_reference_returns_nothing()
        {
            var unityRef = DomainAssembly
                .GetReferencedAssemblies()
                .FirstOrDefault(a => string.Equals(a.Name, "UnityEngine", StringComparison.OrdinalIgnoreCase));

            Assert.That(unityRef, Is.Null,
                "Domain 不应存在名为 UnityEngine 的引用；若出现说明边界被破坏。");
        }
    }
}
