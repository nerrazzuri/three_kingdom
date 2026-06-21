// VERTICAL SLICE - NOT FOR PRODUCTION
// Validation Question: 存档为显式版本化 DTO + JSON，经 Infrastructure 端口，原子写入（ADR-0005）
// Date: 2026-06-21

using System;
using System.IO;
using System.Text.Json;
using TkSlice.Domain.Config;
using TkSlice.Domain.Siege;

namespace TkSlice.Infrastructure.Save
{
    /// <summary>存档信封：schema 版本 + 游戏版本 + 权威状态快照。</summary>
    public sealed record SaveEnvelope(int SchemaVersion, string GameVersion, SiegeState.Memento State);

    /// <summary>
    /// 存档序列化端口（Infrastructure）。Domain 不知 JSON；本类负责 DTO↔JSON 与原子写入。
    /// 版本不兼容时走逆序逐版迁移链（slice 仅 v1，预留扩展点）。
    /// </summary>
    public static class SiegeSaveSerializer
    {
        public const int CurrentSchemaVersion = 1;
        public const string GameVersion = "slice-0.1.0";

        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,
        };

        public static string Serialize(SiegeState state)
        {
            var env = new SaveEnvelope(CurrentSchemaVersion, GameVersion, state.Capture());
            return JsonSerializer.Serialize(env, Options);
        }

        public static SiegeState Deserialize(string json, SiegeConfig cfg)
        {
            var env = JsonSerializer.Deserialize<SaveEnvelope>(json, Options)
                      ?? throw new InvalidDataException("存档解析失败：空信封。");
            var migrated = Migrate(env);
            return SiegeState.Restore(migrated.State, cfg);
        }

        /// <summary>逆序逐版迁移链占位（当前仅 v1）。未来版本在此逐级升级。</summary>
        private static SaveEnvelope Migrate(SaveEnvelope env)
        {
            if (env.SchemaVersion > CurrentSchemaVersion)
                throw new InvalidDataException(
                    $"存档 schema v{env.SchemaVersion} 高于当前 v{CurrentSchemaVersion}，无法读取（请升级）。");
            // v < CurrentSchemaVersion 时在此逐级迁移；slice 暂无历史版本。
            return env;
        }

        /// <summary>原子写入：先写临时文件再替换，避免半写损坏存档。</summary>
        public static void SaveToFile(SiegeState state, string path)
        {
            string tmp = path + ".tmp";
            File.WriteAllText(tmp, Serialize(state));
            File.Move(tmp, path, overwrite: true);
        }

        public static SiegeState LoadFromFile(string path, SiegeConfig cfg)
            => Deserialize(File.ReadAllText(path), cfg);
    }
}
