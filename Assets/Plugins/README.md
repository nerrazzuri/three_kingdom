# Assets/Plugins — Domain/Presentation 桥接 DLL

本目录的 `ThreeKingdom.Domain.dll` 与 `ThreeKingdom.Presentation.dll` 是 `src/Domain` 与
`src/Presentation`（netstandard2.1）的**构建产物**，作为 Unity 表现层引用权威逻辑的桥。

## 权威源与重建

- **权威源是 `src/`，不是这两个 DLL。** dotnet 单元测试（BLOCKING）始终对 `src/` 源编译，永远新鲜。
- 改动 `src/Domain` 或 `src/Presentation` 后，须重建并替换本目录 DLL：

  ```sh
  dotnet build src/Presentation/ThreeKingdom.Presentation.csproj -c Release
  cp src/Presentation/bin/Release/netstandard2.1/ThreeKingdom.Domain.dll       Assets/Plugins/
  cp src/Presentation/bin/Release/netstandard2.1/ThreeKingdom.Presentation.dll Assets/Plugins/
  ```

## 为什么用 DLL 而非 asmdef 直引源

`src/` 下同时有 dotnet 的 `bin/obj`（生成 `.cs`），若在 `src/` 放 asmdef 让 Unity 直编译源，
Unity 会连 `bin/obj` 的生成代码一起编译 → 重复 AssemblyInfo 冲突。DLL 桥让 Unity 只见编译产物、
不扫 `src/` 源树，干净隔离两套构建。**Tech-debt**：未来可改为本地 UPM 包 + asmdef（需排除 bin/obj）或
CI 内 `dotnet build` 步骤注入 DLL，以消除二进制入库。
