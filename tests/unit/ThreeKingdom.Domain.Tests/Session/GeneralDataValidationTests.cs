using System.Text;
using NUnit.Framework;
using ThreeKingdom.Application.Scenarios;

namespace ThreeKingdom.Domain.Tests.Session
{
    /// <summary>武将内容数据完整性守卫（ADR-0016 内容守卫）：全谱扫描无悬空引用/非法生卒/重复布防。CI 拦一类静默数据 bug。</summary>
    [TestFixture]
    public class GeneralDataValidationTests
    {
        [Test]
        public void test_general_data_has_no_integrity_violations()
        {
            // Act
            var violations = GeneralDataValidation.Validate();
            // Assert
            if (violations.Count > 0)
            {
                var sb = new StringBuilder($"武将内容数据校验发现 {violations.Count} 处违规：\n");
                foreach (var vio in violations) sb.AppendLine("  " + vio);
                Assert.Fail(sb.ToString());
            }
            Assert.That(violations.Count, Is.EqualTo(0), "武将内容数据健康。");
        }
    }
}
