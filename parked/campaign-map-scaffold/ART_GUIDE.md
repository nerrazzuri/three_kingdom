# 战略大地图 · 美术制作指南（给美术/你自己）

代码层已就绪（数据流：世界态 → CampaignMapView → SessionRuntime → Adapter → scaffold 渲染，dotnet 已单测）。
美术只需按下表产出资产、按**约定命名**放进工程，代码即可认领。**不用改代码**——命名/坐标对上就行。

---

## 优先级（按"最少美术即可跑起来" → "精美"）

| 阶段 | 产出 | 没有它行不行 |
|---|---|---|
| **P0 能跑** | 一张地图底图 + 城用纯色圆点(代码生成) + 势力用纯色 | 行（先灰盒验证玩法） |
| **P1 有样** | 城节点图标(城/关) + 武将头像(占位灰头) + 势力旗色 | 建议 |
| **P2 精美** | 203 张武将立绘 + 领土多边形描边 + 天气 VFX + 书法字体 | 打磨期 |

---

## 1. 地图底图（Map Background）— 1 张
- **内容**：汉末中国地图（黄河/长江/山川/关隘），风格建议水墨/古地图。
- **规格**：4096×4096 或 8192×8192 PNG（正交俯视）；导入设为 Sprite 或大平面贴图。
- **摆放**：铺在 `TerritoryLayer` 之下的一个 Quad/Plane；世界坐标范围对齐 `CampaignMapDataAdapter` 的 `Positions`
  （现为约 x∈[-10,6]、z∈[-10,11] 的格局，可按底图缩放平移——**改 Positions 字典即可**，无需改逻辑）。

## 2. 城节点图标（Territory / City Icons）
代码把每座城摆在 `CampaignMapDataAdapter.Positions[cityId]`，共 **37 座**（id 见下）。美术给：
- **城图标**：普通城 `city_node.png`、君主治所 `city_capital.png`（大/带旗）。约 128×128。
- **归属着色**：图标做成**可染色**（白/灰底 + 代码按势力色 tint）。势力色见 `FactionColors` 字典（可改）。
- TerritoryView 上挂 SpriteRenderer，颜色由 `TerritoryViewModel.OwnerFactionId` → 势力色。

**37 城 id**（`AllWorldCities`）：city-xuchang/puyang/chenliu/juancheng（曹操）、ye/nanpi/pingyuan/jinyang（袁绍）、
shouchun/hulao/runan（袁术）、jianye/wujun/kuaiji/lujiang（孙）、xiaopei（刘备）、xiapi/xuzhou（吕布）、
xiangyang/jiangling/jiangxia/changsha（刘表）、chengdu/jiangzhou/zitong（刘璋）、xiliang/wuwei（马腾）、
hanzhong（张鲁）、beiping/jicheng（公孙瓒）、changan/luoyang（李傕）、wancheng（张绣）、beihai（孔融）、
hanyang（韩遂）、jiaozhou（士燮）。

## 3. 武将头像 / 立绘（Hero Portraits）— 最大工作量，203 张
- **数量**：约 203 名（当前谱）；先做**190 布防在场的 ~40 名**即可上地图（其余按需补）。
- **关键：Addressables 命名 = 武将 id**。代码在 `CampaignMapDataAdapter.ToHero` 里 `PortraitSprite=null`，
  你在美术接好后改为按 `HeroId` 异步加载。约定 Addressable key = **`portrait/{heroId}`**，如 `portrait/char-guanyu`。
- **规格**：头像 256×256（棋子用）；立绘 512×1024（详情面板用，可选）。透明 PNG。
- **id 命名规律**：`char-<拼音>`，见 `GeneralDossiers.cs` / `DisplayNames.cs`（关羽=char-guanyu、诸葛亮=char-zhugeliang…）。
- **省钱做法**：① AI 生成(统一 prompt 风格锁定) → 手修；② 买三国题材立绘包按 id 对应改名；③ 先用**统一占位灰头 + 中文名标签**（棋子已有 `HeroNameChinese`）跑通，再逐个替换。

## 4. 势力旗 / 配色（Faction Banners & Colors）
- 代码 `FactionColors` 已给 6 家默认色（金=玩家/蓝=曹/红=蜀/绿=吴/紫=袁绍/橙=吕布），**其余势力默认灰**——按需在字典补色。
- 美术可选给每家一面**旗帜 sprite** `banner_{factionId}.png`（如 banner_faction-cao），用于城头/棋子环/状态栏。

## 5. 武将棋子框（Hero Token Frame）
- 棋子 = 头像 + **势力色圆环**。给一张 `token_ring.png`（白色可染色环），代码按棋子 `FactionId` → 势力色 tint。
- HeroTokenLayerController 会按 `HeroPositionViewModel` 在城坐标生成棋子（prefab 需你在编辑器建：SpriteRenderer 头像 + 环 + TMP 名字）。

## 6. 天气 VFX（可选，P2）
- WeatherOverlayController 用 URP Volume + 粒子。美术给：雨/雪/雾/风暴的**粒子贴图**（小图 128²）+ 一个 URP Volume Profile（雾/色调）。

## 7. 字体 / UI
- 中文用**书法/楷体** TMP 字体资产（含常用字 + 人名生僻字：邓艾/毌丘俭/轲比能…建议全 GB2312 + 补字）。
- 面板/按钮 9-slice sprite（古卷轴风）。

---

## 你要动代码的唯一两处（美术接好后）
1. `CampaignMapDataAdapter.ToHero`：`PortraitSprite = null` → 改为 Addressables 按 `portrait/{HeroId}` 异步加载。
2. `Positions` / `FactionColors` 字典：对着底图微调坐标、补全势力色。

其余（棋子/城/领土 prefab、场景层级、包依赖）见 `SCAFFOLD_README.md` + `INTEGRATION.md`。

---

## 一句话
**先灰盒（P0）跑通玩法，再逐层替美术**。代码不挡你——所有资产靠**命名约定（id）**和**坐标字典**对接，产出多少就显示多少。
