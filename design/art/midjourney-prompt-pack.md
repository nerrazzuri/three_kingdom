# Midjourney 提示词包 + 风格锁定协议 — 背景骨架 & 签名样张 & 武将半身像

> **版本** v3.2 · **日期** 2026-07-09（**v3.2 新增 §5.30.10——四位招牌原型全部定稿封口**：武将立绘五位（关羽 + 诸葛亮/曹操/司马懿/吕布）全部确认定稿并入库至 `D:\Projects\三国演义\UI Test\UI\final`；§5.30.1/§5.30.3/§5.30.4 项目方确认无需再改，维持原文；§5.30.2（曹操）经历本轮唯一一次实质性重写——"外放霸主 v2"：诊断出曹操原版神韵措辞（`cold/reserved/calculating`）与司马懿高度重叠，在同一铁苍蓝 Wei 配色下有缩略图撞脸风险且滑向"光荣（Koei）冷脸谋士"刻板印象（新发现的第四种失效面——字面负面词只能拦"画风模仿"，拦不住"神韵/人设定位层面的雷同"），改判神韵为`bold, ambitious, charismatic, imperious, magnetic overlord, confident self-assured half-smile`（外放霸主），相貌加`robust, fuller composed face, NOT gaunt`，冠改为`tall chancellor's cap with gold crossbar AND gold rank ornament`（王者级双金饰），负面新增 8 项定向反拦；写入长期铁律"角色神韵互斥"——同势力同色系角色的神韵/相貌/头冠须显式互斥（曹操=外放霸主↔司马懿=内敛谋主），累加 §5.30.7/§5.30.8/§5.30.9 已有四条铁律共五条；§5.30.5 对照表曹操"表情基调"列同步更新；§6 登记表补齐五位原型的最终文件名/来源批次/势力色标记落点/缩略图辨识度评估/§9.4 原创依据；记录两项非阻断遗留待办（各立绘背景未统一；`bg3-1` 签名图 A 仍未 Upscale 入库））（v1.0 覆盖背景骨架与风格锁定协议；v1.1 新增 §5 武将半身像提示词模板系统，原 §5 素材登记表顺延为 §6；v1.2 新增 §7 首轮背景出图评审 + 签名图 A 修订版 prompt；v1.3 在 §5 内新增 5.8-5.11 首轮人像出图评审 + art-bible §3.1 补充裁定 + 人像模板 v1.2 修订；v1.4 新增 §5.12-5.16 第二轮人像出图评审 + 人像模板 v1.3 修订（渲染媒介回收）+ art-bible §3.1 第二条补充裁定，以及 §7.5-7.7 第二轮背景出图评审 + 锁定结论；v1.5 新增 §7.8-7.9 第三轮背景出图评审 + 锁定 `bg3-1` 为签名图 A，§5.17-5.20 第三轮人像出图评审（绢本方向，已被下条取代），§5.21 项目方方向异议裁定——人像改判"游戏立绘 register"、art-bible §1.1 新增资产门类范围说明、人像模板 v1.4（游戏立绘 + 区隔 Koei）；v1.6 新增 §5.22 第四轮人像出图评审（v1.4 模板首次验证，方向/表情/区隔 Koei 三项通过但整体偏暗）+ 人像模板 v1.4.1 打光修正 + `gy4-3` 因冷灰背景排除候选，签名图 B 仍未锁定；v1.7 新增 §5.23 第五轮人像出图评审——确认背景/整体明度问题已修复，但定位出新根因"脸部正面平面明度不足"（与整体明度是独立维度）+ `gy5-1` 单色橙罩染排除候选 + 人像模板 v1.4.2（色相/明度拆分描述 + 正面主光子句 + 强化压制单色罩染负面），签名图 B 仍未锁定；v1.8 新增 §7.10 MVP-4 类背景骨架出图评审（坚城/粮道/遮蔽/主菜单，挂 `bg3-1` 为 `--sref`）——坚城 `jc-3`/遮蔽 `zb-2`/主菜单 `zm-0` 三类通过并可锁定，粮道未过关待补负面重出一轮，新增黑边/画框通用负面提示 + 兵群/大车专属负面提示，§6 登记表对应四行已更新；v1.9 新增 §5.24 第六轮人像出图评审——v1.4.2 提亮修正确认生效，但暴露关羽黑长髯/黑发/东亚红脸三项身份锚点从未被显式钉死的新根因，四张全数排除，人像模板 v1.4.3（东亚汉人身份锚 + 黑长髯/黑发硬性锁定 + 红润东亚肤色与"提亮不带走人种"的措辞拆分 + 压制背景双色分栏负面），签名图 B 仍未锁定；新增 §7.10.7 粮道补负面重出一轮（批次 `ld2`）复核确认——黑边/画框问题已解决，`ld2-2` 无残留大车/人物，选定 `ld2-2` 锁定入库，MVP 背景四类全部锁定完成；v2.0 新增 §5.25 第七轮人像出图评审——v1.4.3 身份锚点验证成功，`gy7-0`/`gy7-1` 因平灰背景破坏纸本家族相似性底线排除（同 §5.22.2 `gy4-3` 判例），`gy7-3` 因画风偏扁平矢量排除，选定 `gy7-2` 四项复核+家族底线+§9.4 全数通过，正式锁定为签名图 B；**v2.1 新增 §5.25.6 背景修正复议——项目方提出用暖纸底修正版 `gy7-1` 替换 `gy7-2`（背景可分离修正，原否决理由仅涉及背景层），裁定原则同意但需先提交修正成品图按四项复核+家族底线重新验证才可改锁，建议首选 MJ Vary(Region)（散发丝边缘更自然）、PS 复用 `bg3-1`/`zm-0` 纸纹为兜底，给出暖纸底并排比对验收标准；`gy7-2` 暂保留"技术性可用"锁定状态但不建议现在大批量铺开 ~30 将，§6 登记表"签名图 B"行已更新为复议中状态；**v2.2 新增 §5.26 重大方向更新——项目方指出关羽经典形象须戴绿色幞头（v1.4.3/`gy7` 批是裸头束发，缺失该维度），并指认第四轮 `gy4-3` 为全程最神似关羽的一张（独立复核确认其唯三缺陷：脸偏暗/背景冷灰蓝双色分栏/幞头是黑非绿），据此 §5.25.6 的"gy7-1 背景修正复议"路线正式作废（gy7-1 无头饰，无法仅靠背景修正达标）；新增人像模板 v1.4.4——引入逐角色可配置的"头饰槽"机制（默认无、关羽定制为软质深绿幞头）+ 沿用 v1.4.2 提亮/v1.4.3 身份锚点/背景防分栏子句，给出完整正负面 prompt；同步在 art-bible §3.1 写入"补充裁定（三）"，裁定人物专属识别色（如关羽绿）从属于阵营色单块规则而非并列——面积更小、饱和度更低、色相避开 H60–100/H270–310 预留段（关羽绿定为 H150–170 冷调玉/孔雀石绿）；§6 登记表"签名图 B"与"关羽原型（签名）"两行更新为"待 v1.4.4 出图"，并注明验证通过后两行将共用同一 Upscale 源）；**v2.3 新增 §5.27 第八轮人像出图评审——v1.4.4 绿幞头稳定呈现，但暴露"过度矫正"（overcorrect）：红脸变普通棕褐、蜀汉红披风整个消失、背景变纯白、画风飘向扁平 cel/矢量，`gy8-1`/`gy8-2` 另出现胡须局部灰白丝缕；独立复核确认项目方四点诊断属实，并补充根因假说——头饰段落写成独立道具规格说明，误触发 MJ"角色设定稿/turnaround sheet"默认模板（白底+扁平+单一主色+弱化肤色戏剧性），四个症状同源而非各自独立；给出人像模板 v1.4.5——收紧头饰段落为从属句+显式限定"绿色仅限头部不得扩散全身"+红脸与绿巾显式解耦声明+红披显式夺回"面积最大/饱和度最高"主导地位+背景显式排除纯白/设定稿呈现+画风显式排除扁平/cel/漫画描线+黑髯纯度负面加固，给出完整正负面 prompt；`gy8` 四张全数排除且判定不具备可救性（红脸/红披/纸底三项系统性缺失，非可分离缺陷），`gy8-3` 仅作绿巾造型/黑髯纯度技法参考存档；§6 登记表"签名图 B"与"关羽原型（签名）"两行更新为"待 v1.4.5 出图"）；**v2.4 新增 §5.28 方向定稿——第九轮（`gy9-0~3`）验证确认红脸/画风修正生效，但绿巾/白底压不掉，项目方据此判断"关羽绿袍绿巾是角色本身的图标学而非 MJ 画错"，拍板不再压绿：绿（袍+巾）正式确立为关羽个人主色，红降为衣领/内衬/束绦小面积点缀，写入 art-bible §3.1 补充裁定（四）——覆盖裁定（三）"个人色须从属阵营色"默认规则，但仅限关羽一人例外；给出阵营辨识度双轨方案（画面内小面积红标记为次要信道 + 推荐的 UI 层阵营色边框/角标为主信道，后者需协调 ui-programmer/ux-designer 落地，遵循冗余编码原则）；给出大幅精简、正向拥抱绿袍的 v1.4.6 简化 prompt（删除全部反绿对抗性措辞，保留反白底/反扁平/反身份漂移核心项）；独立复核 `gy9-1`/`gy9-2`——`gy9-1` 判定为"只差暖纸底"的可直接背景修正候选（红脸/黑髯/画风/绿袍/红色标记五项已过），`gy9-2` 红脸最饱和但画风偏硬边平涂（非可分离缺陷），不建议直接补背景，仅作红脸色值参考；推进建议为优先对 `gy9-1` 走背景修正直锁路径，v1.4.6 作为该路径失败时的备用重出方案；§6 登记表"签名图 B"与"关羽原型（签名）"两行更新为"gy9-1 背景修正复核中"）；**v2.5 新增 §5.28.5 头饰结构判断修正——项目方指出并经独立复核确认 `gy9-1` 的绿色实为缠绕于发髻上的布巾（发际线、鬓角均外露，不成帽型），而 `gy9-2` 才是真正覆盖全头顶、额前有清晰帽箍分割线的成型软帽/幞头；同时 `gy9-2` 的赤面饱和度/气场也优于 `gy9-1`（`gy9-1` 仅厚涂笔触占优）；据此原 §5.28.4"`gy9-1` 只差暖纸底可直接背景修正"的判断作废（该结论已用 ~~删除线~~ 标记保留存档、未删除）——帽型结构与画风倾向均属主体像素内嵌/全局渲染技法层面的非可分离缺陷，不能像背景/材质层那样靠 Vary(Region)/PS 单独修补；v1.4.6 重新定向为明确合成靶——`gy9-2` 的内容（成型幞头 + 饱和赤面气场）+ `gy9-1` 的厚涂笔触 + 暖纸本背景，给出改写后的完整正负面 prompt（头饰段落改为"a structured soft cap/futou that sits over the crown...NOT a cloth wrapped around a topknot bun"+ 对应负面排除项；红脸子句提升至 `gy9-2` 级"opera red face"饱和描述；画风子句保留 `gy9-1` 的"loose expressive brush edges"厚涂措辞并加强负面压制硬边平涂）；§6 登记表"签名图 B"与"关羽原型（签名）"两行由"gy9-1 背景修正复核中"更新为"待 v1.4.6 出图（结构性重出，非背景修正）"）；**v2.6 新增 §5.29 第十轮人像出图评审——v1.4.6 合成靶命中，`gy10-0~3` 四张全部达成"成型绿幞头+戏曲赤面+及胸黑长髯+绿袍+暖纸底+威严闭口+厚涂笔触"七项目标，逐张过 §5.28.5 六点清单+§9.4 非Koei；正式裁定选定 `gy10-2` 为签名图 B / `--sref` 风格锚（红脸均匀饱和度与背景干净度本轮最佳、零水印风险、避免方向性侧光作为 DNA 传播损害未来 ~30 将缩略框可读性），`gy10-3`（项目方最爱、厚涂笔触最精但右下角有 MJ 幻觉水印"CUCCURS.COM"）经项目方拍板改走**解耦方案**（§5.29.4）——不再与签名图 B 共用同一源，`gy10-3` 去水印后单独作为关羽本人游戏内立绘专属资产，不进入 `--sref` 锚；给出去水印验收标准（PS 内容识别填充优先，简化复核不必整套六项重来）+ 锁定后 5 原型/核心~30将挂 `--sref` 出图步骤 + "sref 锁画风不锁五官"抽查提醒；§6 登记表"签名图 B"行更名为"签名图 B（风格锚，`--sref` 源）"并锁定 `gy10-2`，"关羽原型（签名）"行更名为"关羽原型（签名，游戏内立绘）"并选定 `gy10-3`（去水印版，待入库））；**v2.7 新增 §5.30 剩余 4 位招牌原型（诸葛亮/曹操/司马懿/吕布）出图 prompt**——复用 v1.4.6 正向风格句/打光句/暖纸底背景句/收尾句四段骨架逐字不改，每条只替换身份/头冠/服装+阵营标记/表情/肤色/须发，尾部追加 `--sref <gy10-2 URL 占位> --sw 300`；给出两处裁定：①诸葛亮不开个人色例外、走裁定三默认，因白色鹤氅为消色差材质天然不与阵营色竞争饱和度主导权，无需比照关羽降级处理；②吕布（群雄·独立客将）定色紫垒 H285 S38 L34（近似 #683678），因与关羽绿相距最远、不落入三大阵营色相家族、且"紫=非正统"语义与其"三姓家奴"定位相合，已回写 `art-bible.md` §4.4"其他势力"占位段并附边框/纹样/势力印记规格；司马懿与曹操同色系但用冠帽款式（低调款 vs 进贤冠）与滚边纹样（暗云纹 vs 银灰）区分，避免撞脸；§5.30.5 给出四条速览对照表；§6 登记表按项目方要求本轮不动（四位尚未出图）**，§1-§5/§6/§7.1-7.10 原文未改动）；**v2.8 新增 §5.29.5 + §5.30.6——首批 8 张实测（诸葛亮/曹操各 4）后的 v2 修订**：撤回 §5.29.3 第 3 点"`gy10-2` 挂 `--sref` 作 ~30 将跨角色风格锚"的决策，实测证明人物立绘做跨角色 `--sref` 源会连人物身份/神韵一起传播而非只传播画风，改判为核心 5 原型/~30 将均**不挂 `--sref`/`--sw`**、纯靠 v1.4.6 共享风格句兜底画风；曹操"iron-blue"措辞被 MJ 误读偏色到东吴碧江青（teal/cyan），改用排他式"steel slate blue-grey...NOT teal NOT cyan NOT turquoise NOT green"描述锁死色相，脸/冠/神情不动；司马懿预防性套用同一措辞修正（保留云纹滚边/灰白鬓须/鹰视狼顾区分点）；诸葛亮 6 处定点修正（背景提亮不入阴影/鹤氅改为无羽毛平滑布料/赤金朱限死小面积腰绦领缘/收紧头肩特写构图/表情改为温和睿智非武将/羽扇降权处理而非改卷轴）；§5.30.5 对照表加注措辞变化提醒，描述性归纳本身不变；§6 登记表本轮依旧不动）；**v2.9 新增 §5.30.7——MJ 自动审核拦截（"AI Moderator is unsure about this prompt...cautious with realistic images, especially of people"）后的过审安全清雷 v3**：四条 prompt 全清三类高危词——① 写实/照片类（`realistic`→`natural`，`naturalistic skin texture`→`painterly skin texture`，`hyper-realistic photo skin` 整条删除）；② 暴力/惊悚联想词（`gothic vampire count aesthetic` 删除，曹操 `dangerous, and duplicitous`→`cunning, and guarded`，诸葛亮 `menacing glare`→`harsh glare`/`aggressive villain sneer`→`unfriendly sneer`，吕布 `arrogant fierce...reckless...ferocity`→`proud, defiant...bold...fiery impulsiveness`）；③ 种族描述词（`pale caucasian skin`→`pale desaturated skin`，`western facial features`→`non-East-Asian features`，`light western skin`/`pale Western complexion` 删除或改写为非人种标签表达）；不改动任何设定/构图/阵营色/防平涂等既有内容，纯措辞清雷；新增通用备忘供后续 ~30 将出图时一律避开这三类高危词族；§6 登记表本轮依旧不动）；**v3.0 新增 §5.30.8——诸葛亮 v3 过审出图后暴露"画风回滑传统插画/故事书"问题的 v4 修订**：诊断根因为项目方场外测试时于开头追加的"traditional Chinese gouache illustration...not a photograph"框定语虽成功过审，但把画风拉向工笔绢本/祖宗像/故事书方向，丢失游戏厚涂立绘冲击力（确认为与"是否触发审核"完全独立的第三种失败面）；四条统一改为：①开头框定语换成"Bold stylized video game character key art...a rendered game splash-art illustration, not a photograph"（游戏原画框定，同样满足过审"声明非照片"要求）；②正向风格句追加 `thick impasto paint, dramatic cinematic lighting with rich saturated shadow depth and strong three-dimensional form, high-impact game character presence`；③负面追加十项挡传统绘画/故事书措辞（`traditional gongbi silk painting`/`ancestral shrine portrait`/`flat storybook illustration` 等）；确立铁律备忘——过审框定词必须用"video game key art/game character illustration/digital painting"，绝不能用"traditional Chinese gouache/ink painting"；诸葛亮六项/曹操司马懿铁苍蓝钉死/吕布紫垒等既有设定全部不动；本版为**预备版本**，待项目方确认诸葛亮场外测试结果有效后转正式版；§6 登记表本轮依旧不动）；**v3.1 新增 §5.30.9——诸葛亮正式定稿（用户确认 Upscale Creative `530823f0`）+ 三层失效面完整沉淀**：§5.30.1 锁定为定稿 prompt 全文，在 v4 基础上叠加七处最终修正——①年轻俊朗化（`refined, handsome, relatively young...late thirties, clear smooth healthy skin`替换`middle-aged`）；②短须化（`neat short-to-medium...NOT a thick heavy long warrior beard`）；③诸葛巾改写为`TALL upright structured flat-topped cloth cap...vertical pleated ridges`并加`low wrapped headscarf/floppy hood/turban`等负面（原"headscarf tied across crown"被 MJ 稳定误读成裹布）；④羽扇从"降权/隐约可见"改为`holding...clearly and prominently visible as his signature identifying object`；⑤脸部打光单独拆分为`soft even flattering key light...NOT harsh dramatic chiaroscuro`+ 剪影/披风升级为`strong...for game-art impact`，神韵负面加重（`fierce warrior/rugged warrior face/gaunt face/elderly old man`等）；⑥收尾工序确认走 Upscale (Creative)；⑦记录遗留观察（外袍偏枣红/白衣被压小，非阻断）。沉淀两条新通用铁律（累加 §5.30.7/§5.30.8 已有两条）——"年轻/文官角色须显式声明年轻俊朗+少须，不能只写身份词"、"招牌道具/头冠须写`clearly and prominently visible`，不能写`understated/glimpsed`这类降权措辞会导致 MJ 干脆画丢或误读"、"戏剧性冲击力用光与脸部可读性须分层声明，不能合并成一句"；§5.30.2-5.30.4（曹操/司马懿/吕布）套用其中与年龄/须发/头冠设定无关的通用项（脸部打光分层句+泛化猛将脸负面，按各自设定逐人挑选负面子集），保留三人各自威严/鹰视/骄横的既有设定不做"一刀切年轻化"，避免制造新的"千人一面美男子"问题；§6 登记表待四位实际出图通过复核后再登记）
> **对齐基线** `design/art/art-bible.md` v1.1（核心视觉规则 §1、色彩系统 §4、资产标准 §8、背景生产管线 Section 10）
> **决策前提**（2026-07-07 项目方裁定，不在本文件重开讨论）：
> ① 风格沿用 art bible v1.1「军府活图卷」，不改方向。
> ② 季节按"会跨"处理——背景骨架做**季节中性**，四季由覆盖层承担（见 art-bible §10.6 已更新的 MVP 表）。
> ③ 美术先用 **Midjourney** 试产，效果不足再转人工画师。
>
> **本文件的定位**：这是 art bible 的**执行工具**，不新增任何视觉决策。所有色彩、构图原则、原创性红线均来自 art-bible.md，本文件只负责把这些规则翻译成可以喂给 Midjourney 的具体提示词，以及一套非美术人员也能打勾判断的验收清单。

---

## 0. 使用须知（务必先读，避免预期错位）

### 0.1 Midjourney 只出「底层」，不出「中层批注」

art-bible §1.1 把每一帧画面定义为三层：**底层**手绘地貌/纸张质感、**中层**批注印记虚实线、**顶层**人物情绪。

**Midjourney 在本项目里只负责产出底层**：

- 干净的场景骨架底板（clean base plate）——地貌、建筑、道路，**不含任何路径箭头、不含虚线、不含文字标签、不含 UI 边框**。
- 干净的人物半身像/剪影底板——**不含姓名文字、不含状态图标、不含情报置信度标记**。

**中层的三种笔迹语气**（事实墨线 / 推测蓝灰虚线 / 命令朱红箭头 + 印章）**绝对不出现在任何 MJ prompt 里**——这一层是引擎在运行时按玩家的侦察/命令状态动态叠加的信息层（art-bible §3.3/§4.3/Section 10.5），画进底图里等于把动态信息焊死成静态图片，游戏没法用。

> **给项目方的提醒**：MJ 出的图**不是**可以直接截图当成游戏画面的"成品屏"，而是一张"半成品的干净底板"，后续还要在引擎里叠加地图标注、情报状态、UI 面板才是玩家实际看到的画面。评审 MJ 出图时，请只评审"这张纸质地貌画得对不对"，不要因为"看起来空空的、没有信息"而判定不合格——空，恰恰是对的，因为信息层是留给引擎叠加的。

### 0.2 原创性红线（art-bible §9.4，本文件严格遵守）

- **禁止**在任何 prompt 中点名在世/知名画师姓名、点名具体游戏/动画/影视作品名称去"仿其风格"。
- 一律用**画种、媒介、材质、光照、历史时期通用描述**来逼近"军府活图卷"的观感（例如"绢本设色""矿物颜料""汉代军事地图""泛黄宣纸"这类通用词汇），不借用任何可识别的现有作品视觉签名。
- 每张入库的 MJ 图，必须在本文件末尾的"已选定素材登记表"（§6）中登记其 prompt 版本与生成来源，作为 §9.4 要求的"原创依据可追溯"证据。存疑的图**不得入库**。

### 0.3 语言约定

Prompt 正文使用英文（Midjourney 对英文提示词的理解和出图质量显著优于中文，这是当前生成式工具的行业惯例，不违反 art-bible §9.4 的"中文叙述+技术参数可保留原语言"约定——prompt 字符串在此视同技术参数字符串）。说明性文字、检查清单、流程说明均为中文。

### 0.4 关于 Midjourney 参数语法的免责声明

Midjourney 的参数语法（`--sref`、`--sw`、`--cref`、`--ar` 等）会随版本更新调整。本文件给出的是**概念性用法与合理起点值**，代表撰写时（2026-07）已知的通用做法；正式执行前，请先核对当前 Midjourney 官方文档确认参数名称与取值范围仍然有效，如有出入以官方文档为准，并回写本文件更新记录。

---

## 1. 风格锁定协议（Style-Lock Protocol）——先做这个，最重要

### 1.1 流程

```
第一步：出 3 张「签名样张」（背景骨架 / 武将半身像 / UI 纸质材质）
    ↓
第二步：用 §3 的通过/不通过检查表逐条打勾评审
    ↓（3 张全部通过）
第三步：对每张签名图做 Upscale，取其图片 URL/Job 链接
    ↓
第四步：后续所有同类资产的 prompt 末尾追加 --sref <签名图URL> --sw <风格权重，起点值 250>
    ↓
第五步：批量出图 → 每张仍过 §3 检查表 → 不过则调整措辞重出，不无脑重复相同 prompt
```

**为什么先出签名图，而不是直接批量出全部 5 张 MVP 背景**：一次性把风格"焊死"在一张图上评审成本最低；如果第一版签名图没通过（比如画得太像数字插画而不够"纸感"），只需要改 3 张 prompt 重来，而不是等 5 张全出完才发现风格不对再全部推倒。

### 1.2 风格锁定成功判定标准

3 张签名图必须**同时**满足以下条件，才可以进入 `--sref` 锁定：

1. 三张图放在一起看，能看出"同一套视觉语言"（纸张色调、笔触质感、光照氛围一致），而不是三种风格拼凑。
2. 至少 2 名团队成员（含你自己）独立盲评，都能说出"这是军府图卷"式的观感，不需要额外解释。
3. 通过 §3 的全部客观检查项（无一条打叉）。

若某一张始终无法通过，**不要用另外两张"凑合锁定"**——先单独迭代那一张的 prompt，三张都过关再进入 `--sref` 阶段。

### 1.3 三张签名样张 Prompt

#### 签名图 A — 背景骨架（小城野外，季节中性）

```
A hand-painted illustration of a small ancient Chinese hilltop walled town seen from a mid-distance elevated angle, Han-dynasty era military map cartography style, ink outline linework combined with restrained mineral-pigment color washes (muted earth ochre, warm rice-paper beige, faded ink-gray), aged rice paper texture visible in the sky and open ground, weathered brush-stroke terrain contours, low rolling hills and a dirt road leading to the town gate, neutral season with no strong seasonal cues (avoid blossoms, snow, or vivid autumn foliage — use muted temperate greenery and bare earth tones instead), flat even soft lighting with no strong directional shadow, restrained composition with generous open negative space, no text, no signature, no watermark, no UI elements, no modern objects, painterly illustration texture, muted desaturated palette, wide shot --ar 16:9 --style raw
```

**负面提示**（追加在 `--no` 之后）：
```
--no text, watermark, signature, logo, UI, HUD, speech bubble, modern buildings, cars, roads with asphalt, neon light, glowing effects, anime style, chibi, 3D render look, photographic bokeh, lens flare, people close-up
```

#### 签名图 B — 武将半身像/剪影（通用武将原型，§3.1 剪影哲学对应）

```
A three-quarter portrait bust illustration of a stern ancient Chinese Han-dynasty general in lamellar armor, broad shoulders, tall plumed helmet silhouette, ink-line brush painting style combined with restrained mineral pigment color washes, muted warm palette (aged bronze, ochre, faded crimson accent), textured rice-paper background left mostly blank, dramatic but restrained lighting from upper front, serious weathered expression, painterly illustration not photographic, no text, no watermark, no signature, no modern elements, portrait crop from chest up, vertical composition --ar 3:4 --style raw
```

**负面提示**：
```
--no text, watermark, signature, logo, UI, name tag, health bar, anime style, chibi, hyper-realistic photo skin, 3D render, modern clothing, glasses, weapon glow effects
```

> 说明：本张只用于锁定"人物绘制的笔触/材质/光照"风格 DNA，不用于锁定具体某位历史人物的长相——武将原型选择遵循 art-bible §3.1 的宽肩铠甲/盔缨轮廓词汇，具体到某位武将（如刘备/关羽）的长相设计需另行单独出 prompt，不在本文件范围。

#### 签名图 C — UI 纸质材质/面板底纹（框皮基调）

```
A flat-lay texture study of an aged Chinese rice paper military dispatch document, subtle ink stains and brush-stroke border decoration in the corners only, warm beige-white paper tone, faint fibrous paper texture, minimal worn edges, no text or characters written on it, no modern printing artifacts, clean texture suitable as a UI panel background, restrained ornamentation confined to corner accents, flat frontal lighting with no directional shadow, square crop --ar 1:1 --style raw --tile
```

**负面提示**：
```
--no text, characters, calligraphy, watermark, signature, logo, modern paper texture, glossy paper, torn edges, burnt edges, photographic vignette
```

> `--tile` 用于测试材质是否可平铺；若平铺缝隙明显，可去掉 `--tile` 改为单张静态材质使用，不强求无缝平铺。

---

## 2. MVP-5 背景骨架逐张 Prompt

对齐 art-bible Section 10.6 更新后的 MVP 清单（5 张季节中性骨架）。**每张定稿后都追加 `--sref <签名图A的URL> --sw 250`**（背景类资产统一参照签名图 A 锁定；武将肖像类参照签名图 B 锁定，见 §5）。

### 2.1 小城城内

```
A hand-painted illustration of the interior of a small fortified Han-dynasty Chinese town seen from an elevated three-quarter angle, modest timber-and-tile rooftops, a central dirt marketplace square, low defensive earthen walls visible at the edges, ink outline linework with restrained mineral-pigment washes, aged rice-paper texture, neutral season with no strong seasonal cues (muted temperate tones, no blossoms or snow), flat even lighting with no strong directional shadow, generous open sky negative space at the top third of the frame, no text, no UI, no modern elements, wide establishing shot --ar 16:9 --style raw --sref <SIGNATURE_A_URL> --sw 250
```

**导出/命名提示**：拼合结果导出为 `bg_smallcity_neutral_1920.png`（季节中性版本；四季覆盖层另行按 §10.2 由人工在 PS 上基于此底图局部绘制，见 §4.5 衔接说明）。

### 2.2 战役地形——坚城（城防正面，对应 gdd-021「坚城」）

```
A hand-painted illustration of an ancient Chinese fortified city wall seen from outside at a low elevated angle, tall rammed-earth and stone city wall with a defended gate tower, banners on the ramparts left generic and blank (no faction emblem), open ground in front of the wall for approaching troops, ink outline linework with restrained mineral-pigment washes, aged rice-paper texture, neutral season, flat even lighting with no strong directional shadow, restrained composition with open negative space above the wall line, no text, no UI, no modern elements, wide shot --ar 16:9 --style raw --sref <SIGNATURE_A_URL> --sw 250
```

**说明**：城墙上旗帜必须留白/无纹样——势力旗色与纹样（§4.4）由引擎按势力动态叠加，不能烧进背景骨架。

### 2.3 战役地形——遮蔽（伏击林地，对应 gdd-021「遮蔽」）

```
A hand-painted illustration of a dense forested hillside terrain with scattered rock outcrops providing natural cover, narrow winding dirt path cutting through the trees, ink outline linework with restrained mineral-pigment washes, muted temperate forest tones with no strong seasonal cues, aged rice-paper texture, flat even lighting with no strong directional shadow, composition leaves open negative space along the path for future troop icon overlays, no text, no UI, no modern elements, wide shot --ar 16:9 --style raw --sref <SIGNATURE_A_URL> --sw 250
```

### 2.4 战役地形——粮道（补给线，对应 gdd-021「粮道」）

```
A hand-painted illustration of a long dirt supply road stretching across open countryside terrain, flanked by low farmland ridges and a distant tree line, a simple wooden cart-path visible, ink outline linework with restrained mineral-pigment washes, muted temperate tones with no strong seasonal cues, aged rice-paper texture, flat even lighting with no strong directional shadow, composition keeps the road clear of clutter to leave room for future supply-line markers, no text, no UI, no modern elements, wide shot --ar 16:9 --style raw --sref <SIGNATURE_A_URL> --sw 250
```

### 2.5 主菜单（一次性定妆图，无需季节层）

```
A hand-painted wide illustration of an unfurled ancient Chinese military campaign map scroll resting on a wooden field desk, ink brush and a red official seal stamp resting beside it, warm restrained candlelight-adjacent lighting concentrated on the scroll with the edges of the frame softly darkened, aged rice-paper texture with visible ink washes of distant mountains and rivers on the scroll itself, atmospheric and evocative but restrained composition suitable as a title screen backdrop, ample negative space reserved for a game logo placement in the upper third, no text, no existing logo, no UI, no modern elements --ar 16:9 --style raw --sref <SIGNATURE_A_URL> --sw 200
```

**说明**：这是唯一允许"更有氛围感"的一张（对应 art-bible §2.2「判断/布局」暖调集中光照描述），风格权重 `--sw` 略调低到 200，允许比其余 4 张背景骨架更强的光照戏剧性，因为主菜单不需要配合运行时时辰/季节叠加。

---

## 3. 通过 / 不通过检查表

给非美术的项目方使用的客观打勾清单。**任意一项打叉，该图拒收，回到 prompt 迭代或重新生成，不允许"看着还行就先凑合用"。**

| # | 检查项 | 判定方法 |
|---|---|---|
| 1 | 是否呈现纸质/矿物颜料质感，而不是光滑数字插画感或照片感 | 目视：能否看出类似"画在纸上"而非"屏幕渲染"的颗粒/笔触感 |
| 2 | 主色是否落在 art-bible §4.1 主调色板范围（宣纸白 `#E8E6E2` / 旧绢米 `#D8CDB4` / 山水墨 `#302D2A` / 矿土褐 `#7A5A3A` 附近色域） | 用取色工具（PS 吸管/在线取色器）吸取画面主要色块，与上述色值目视比对，明显偏离（如高饱和蓝紫、荧光色）判不通过 |
| 3 | 是否为季节中性——**不得**出现樱花/桃花、积雪、鲜明秋叶红黄等强季节符号 | 目视扫描整张图，出现任一季节强符号即打叉 |
| 4 | 光照是否为平光/柔光、无强烈方向性阴影（不能是夕阳侧光、正午顶光硬阴影等戏剧化用光） | 目视检查画面阴影方向是否强烈且单一（主菜单一张除外，见 §2.5 说明） |
| 5 | 构图是否留有充分负空间，没有把画面塞满细节 | 目视：地面/天空是否有大片相对干净区域，供后续引擎叠加路线/标注/图标 |
| 6 | 是否出现 MJ 自行生成的文字、水印、签名、UI 元素 | 目视逐角检查（MJ 经常在角落生成伪文字），出现即拒收重出 |
| 7 | 是否出现现代元素（电线、玻璃幕墙、汽车、柏油路等） | 目视检查 |
| 8 | 是否明显神似某个具体现有游戏/画师作品的可识别视觉签名（构图公式、招牌配色组合、标志性图形符号） | 人工主观判断，存疑一律拒收，不因"画得好看"放行——对应 §9.4 原创性红线，判定从严 |
| 9（仅武将像） | 剪影轮廓是否符合 art-bible §3.1 角色原型形状词汇（如武将=宽肩铠甲盔缨、谋士=窄肩纶巾羽扇） | 目视对照 §3.1 表格 |
| 10（仅背景类） | 城墙/旗帜等势力相关元素是否留白无纹样，没有被 MJ 自行画上具体旗帜图案 | 目视检查城墙/旗杆区域 |

---

## 4. 参数与工程建议

### 4.1 版本

使用当前可用的 Midjourney 最新稳定版本（撰写时惯例为 v6 系列，`--style raw` 参数用于减少 MJ 默认的高饱和度"网红风"倾向，更贴近本项目的克制淡设色需求）。**不建议使用 niji 系列**（该系列偏日系动漫风格，与"军府图卷"的历史写实基调不符）。执行前请在 Midjourney 官方文档核对当前版本号是否仍支持本文档所用参数。

### 4.2 `--ar`（画幅比例）

| 资产类型 | 比例 | 依据 |
|---|---|---|
| 背景骨架 | `16:9` | 对齐 art-bible §8.1 场景背景 1920×1080 规格 |
| 武将半身像 | `3:4` | 对齐 art-bible §3.1 人物半身像剪影哲学（256×340 标准比例） |
| UI 纸质材质样本 | `1:1` | 方形材质样本，便于后续任意裁切复用 |

### 4.3 `--sref` / `--sw`（风格参照与权重）

- `--sref <图片URL>`：把某张已生成图的"视觉 DNA"作为风格参照，让新 prompt 的出图向其靠拢。图片 URL 来自已上传/已生成图的可访问链接（如生成平台内的图片直链）。
- `--sw <0-1000>`：风格参照的权重强度。建议背景类起点值 **250**，人物类起点值 **300**（人物细节更需要一致性）；数值过高会导致新 prompt 的构图内容被参照图"带偏"（比如背景 prompt 明明写的是"粮道"，却因为 sref 权重过高而长得很像签名图 A 的"小城"构图），需要反复试验找到"像同一套风格、又不是同一张构图"的平衡点。
- **重要限制**：`--sref` 只能保证"倾向性接近"，不是 100% 锁死。每一张新出图仍需过 §3 检查表，不能因为挂了 `--sref` 就默认合格。
- 建议同时保留 3 张签名图原图作为人工评审基准——最终判断"风格是否统一"始终以人眼对照签名图为准，`--sref` 只是提高出图命中率的工具，不是自动质检。

### 4.4 负面提示（`--no`）通用清单

无论哪张资产，负面提示至少应包含：

```
text, watermark, signature, logo, UI, HUD, modern buildings, cars, paved asphalt roads, electrical wires, neon light, glowing effects, anime style, chibi, 3D render look, photographic lens flare, photographic bokeh
```

人物类额外追加：`name tag, health bar, hyper-realistic photo skin`
背景类额外追加（若出现季节符号）：`cherry blossom, snow, autumn red leaves`（若 prompt 已明确要求季节中性仍反复出现季节符号，可加入此负面提示强化）

### 4.5 批量迭代与筛选流程

```
出 4 宫格 → 目视初筛（§3 前 4 项快速排除明显不合格的）
    ↓
挑 1~2 张最佳做 Upscale
    ↓
过完整 §3 十项检查表
    ↓ 通过                          ↓ 不通过
登记入 §6 素材登记表            分析具体哪几项打叉 → 针对性调整 prompt 措辞（不是盲目重复相同 prompt）→ 重新出图
    ↓
导出给引擎侧使用
```

### 4.6 与 Unity 侧的衔接（诚实边界说明）

**必须向项目方说清楚的一点**：Midjourney 出的是一张**已经拼合完成的成品图**，不天然带有 art-bible §10.2 要求的图层分离（线稿层 / 固有色层 / 季节覆盖层 / AO 烘焙层是分开的 PSD 图层）。这意味着：

1. **骨架基底（本文件 §2 的 5 张）**：MJ 出图可以**直接作为 §10.2 定义的"L0+L1 拼合结果"**使用，导出命名为 `bg_[scene]_neutral_1920.png`，压缩规则沿用 §8.3（BC7）。这一步 MJ 可以直接顶上，不需要人工分层。
2. **季节覆盖层（L2）**：这是"仅覆盖植被/地面局部区域"的**带透明通道选区**贴图，Midjourney 不擅长输出精确对齐、干净透明背景的局部选区图层——**建议由人工在 PS 里基于 MJ 骨架图，手绘/合成局部季节色彩变化**，而不是让 MJ 单独生成一张"可以对齐拼接"的季节层。这部分仍然需要人工美术介入，不是 MJ 一键出四季。
3. **AO 烘焙层（L3）**：同理，建议用 PS 的图层混合模式（正片叠底 Multiply）手工画一层无方向性软阴影，或用图像处理工具从骨架图反推近似 AO，不指望 MJ 直接产出。
4. **@2x 放大（3840×2160）**：MJ 原生分辨率通常不足以直接满足 4K 需求，需要额外的图像放大工具处理，或按 art-bible §8.5 的"MVP 阶段 @1x 优先，验证后再补 @2x"策略推迟处理。

**结论给项目方**：Midjourney 能高效顶掉"骨架基底"这一大头（本文件 §2 的 5 张），但"季节覆盖层"和"AO 层"这类需要精确局部控制的资产，大概率仍需要人工在 PS 里基于 MJ 底图二次加工——这不是 MJ 失败，是分工合理：MJ 负责"画面从无到有"，人工负责"精确的局部控制"。这也是本轮先用 MJ 试产、若季节层这类精细活儿人工成本仍然偏高，再决定要不要为此单独找画师的判断依据。

---

## 5. 武将半身像 Midjourney 提示词模板系统

### 5.1 前提：与背景骨架同一套视觉语言，分叉的是"信息状态"，不是"画风"

武将半身像必须与背景骨架共享**同一套纸质/水墨/矿物颜料质感**（同一条视觉锚），差别只在于"这位武将玩家是否已经识别"这一**情报状态**，而不是换一种画法。因此本节的两态 prompt 模板刻意保留与 §1.3 签名图 B 完全一致的画种/材质/光照措辞，只在"是否上色、是否露出五官"这一层做区分——这对应 art-bible §3.1 的剪影哲学（形状语言恒定，信息随侦察状态揭示）与反全知核心机制（情报是逐步显影，不是开关式全知/全不知）。

### 5.2 两态设计：已识名将 vs 未探明之将

| 状态 | 视觉处理 | 揭示的信息 | 隐藏的信息 |
|---|---|---|---|
| **已识名将**（已侦察/已交战识别） | 全彩上色，五官/表情/服饰细节完整渲染，阵营色点缀清晰可见 | 姓名对应的具体长相、气质神态、阵营归属 | 无（信息已完全显影） |
| **未探明之将**（尚未侦察/情报不足） | 单色墨影剪影平涂，**无五官、无阵营色彩**，剪影边缘叠加一层淡淡的宣纸雾气纹理（与 art-bible §3.2 战略雾的纸质雾气语言保持一致，让"未知"在地图层和人像层用的是同一套视觉词汇） | 仅剪影轮廓所属的**身份大类**（武将/谋士/君主/女性——因为这属于 §3.1 定义的恒定形状语言，铠甲块面 vs 儒衫羽扇的轮廓差异在战场上本来就是肉眼可辨的，不算"情报"） | 具体是谁、气质标签、阵营归属色、面部表情 |

**为什么身份大类在未探明态仍然可见**：art-bible §3.1 把"形状"定义为角色原型的恒定识别特征（宽肩铠甲=武将、窄肩纶巾=谋士……），这是战场上肉眼就能分辨的"类别"，不属于需要侦察才能获得的"情报"；真正被反全知机制锁住的是**具体是谁、他的气质倾向、他属于哪一方阵营**——这些才需要通过侦察/交战逐步显影。两态共用同一套轮廓骨架，只是"平涂墨色剪影"还是"全彩具体渲染"的区别，这正是"同一套视觉逻辑、两种信息显影程度"的设计意图。

### 5.3 参数化模板（可套用到核心 ~30 将）——已识名将版

```
A three-quarter portrait bust illustration of a {AGE_DESCRIPTOR} {IDENTITY_DESCRIPTOR}, {TEMPERAMENT_POSE_AND_EXPRESSION}, wearing {FACTION_ARMOR_STYLE} with {FACTION_COLOR_ACCENT} accent tones, ink-line brush painting style combined with restrained mineral pigment color washes, muted warm base palette (aged bronze, ochre, faded paper tones), textured rice-paper background left mostly blank, dramatic but restrained lighting from upper front, painterly illustration not photographic, no text, no watermark, no signature, no modern elements, portrait crop from chest up, vertical composition --ar 3:4 --style raw --sref <SIGNATURE_B_URL> --sw 300
```

四个填空槽的替换方法：

**槽位 1 —— 气质标签（`{TEMPERAMENT_POSE_AND_EXPRESSION}`）**

| 气质标签 | 替换文本建议 |
|---|---|
| 莽勇 | `a broad aggressive fighting stance, brow furrowed, fierce direct forward gaze, mouth set in a shout-ready open expression` |
| 诡谋 | `a slightly hunched contemplative posture, narrowed calculating eyes looking slightly off-frame, a faint knowing half-smile` |
| 善守 | `a grounded steady stance with arms crossed or resting on a weapon hilt, calm watchful eyes, composed neutral expression` |
| 仁德 | `a relaxed open-shouldered stance, soft warm eyes with a gentle slight smile, welcoming approachable posture` |
| 傲物 | `a raised chin with shoulders pulled back, half-lidded dismissive gaze looking down and away from the viewer, faint disdainful smirk` |
| 狼顾 | `a slightly turned torso with the head twisted back over the shoulder toward the viewer, wary sidelong glance, tense guarded posture` |

**槽位 2 —— 阵营（`{FACTION_ARMOR_STYLE}` + `{FACTION_COLOR_ACCENT}`，对齐 §4.4 势力色）**

| 阵营 | `FACTION_ARMOR_STYLE` | `FACTION_COLOR_ACCENT` |
|---|---|---|
| 曹魏 | `dark lamellar armor with angular iron plate patterns` | `iron blue-gray (muted steel-blue)` |
| 蜀汉 | `lamellar armor with warm brocade trim` | `deep crimson-gold (warm red with gold accent)` |
| 东吴 | `lamellar armor with wave-motif trim` | `river teal (deep blue-green)` |
| 群雄/无阵营 | `plain unadorned lamellar armor with no faction motif` | `neutral muted bronze (no strong color identity)` |

**槽位 3 —— 年龄段（`{AGE_DESCRIPTOR}`，对应 EraStage）**

| EraStage | 替换文本建议 |
|---|---|
| 少壮 | `young` |
| 盛年 | `middle-aged` |
| 暮年 | `elderly, silver-streaked or fully grey-bearded` |

**槽位 4 —— 身份（`{IDENTITY_DESCRIPTOR}`，同时影响服饰基调）**

| 身份 | 替换文本建议 |
|---|---|
| 君主 | `ruler wearing an ornate ceremonial robe layered over light armor, a simple dark headpiece denoting rank` |
| 武将 | `general in full lamellar armor, broad-shouldered build, a plumed helmet` |
| 谋士 | `strategist-advisor in a flowing scholar's silk robe and gauze headscarf, holding a folded feather fan` |
| 女性 | `noblewoman in an elegant modest layered robe with subtle armor accents at the shoulders, hair bound in a simple period-appropriate style` |

**套用示例**：要做一名"盛年·蜀汉·武将·莽勇"的武将，把四槽替换值分别代入模板即可拼出完整 prompt，无需重新设计措辞。

### 5.4 "未探明剪影"专门 Prompt 变体

```
A flat monochrome ink-wash silhouette illustration of an unidentified figure in three-quarter portrait bust pose, {ROLE_SILHOUETTE_BUILD}, solid dark ink fill with no facial features, no color, and no faction markings, a faint rice-paper mist texture softly obscuring the lower edge and shoulders of the silhouette, generic guarded neutral posture with no specific expression, textured rice-paper background left mostly blank, restrained soft even lighting, painterly illustration not photographic, no text, no watermark, no signature, no modern elements, portrait crop from chest up, vertical composition --ar 3:4 --style raw --sref <SIGNATURE_B_URL> --sw 300
```

`{ROLE_SILHOUETTE_BUILD}` 复用 §5.3 槽位 4 的身份轮廓词汇（仅取轮廓，不取色彩/材质细节）：

| 身份大类 | `ROLE_SILHOUETTE_BUILD` |
|---|---|
| 君主 | `a tall silhouette with a layered robe draped over the shoulders and a simple rank headpiece outline` |
| 武将 | `a broad-shouldered armored build with a helmet silhouette` |
| 谋士 | `a slender robed build with a folded fan silhouette held at the chest` |
| 女性 | `a slender layered-robe silhouette with bound hair outline` |

**使用要点**：同一位武将的"已识名"版与"未探明剪影"版必须使用**同一个身份大类**（例如都用"武将"轮廓），只是 §5.3 版本套气质/阵营/年龄槽位、§5.4 版本只套身份轮廓槽位并整体转为墨色平涂——这样两态并排看才会像"同一人的两种情报状态"，而不是两个无关的人。

### 5.5 三张招牌武将签名 Prompt（锁定人像风格 + 跨图一致性）

> **原创性提醒**：以下 5 个原型选取三国演义中广为人知的历史/文学人物气质作为**内部代号**，但 prompt 正文一律用**特征描述**（脸型、胡须、兵刃轮廓、气质神态），不点名、不参照任何具体游戏/影视作品的既有造型设计——符合 art-bible §9.4 原创性红线。这些代号仅供团队内部沟通使用，不会出现在 prompt 字符串里。

**为什么先出 3-5 张再批量套模板**：和背景骨架一样，人像也需要先用少量高强度评审的签名图把"画笔触感/上色逻辑/光照"焊死，再用 `--sref` 批量套用到其余武将，避免出到第 20 张才发现风格已经漂移。

| 内部代号 | 特征描述 Prompt（正文） |
|---|---|
| 关羽原型 | `A three-quarter portrait bust illustration of a middle-aged general with a long flowing beard and a deep red-toned weathered face, wielding a long curved blade held upright beside the shoulder, wearing dark lamellar armor with warm brocade trim, a stern dignified direct gaze, ink-line brush painting style combined with restrained mineral pigment color washes, muted warm base palette, textured rice-paper background left mostly blank, dramatic but restrained lighting from upper front, painterly illustration not photographic, no text, no watermark, no signature, portrait crop from chest up, vertical composition --ar 3:4 --style raw --sref <SIGNATURE_B_URL> --sw 300` |
| 诸葛亮原型 | `A three-quarter portrait bust illustration of a young-to-middle-aged strategist-advisor in a flowing scholar's silk robe and gauze headscarf, holding a folded feather fan at the chest, a calm composed knowing expression with sharp intelligent eyes, ink-line brush painting style combined with restrained mineral pigment color washes, muted warm base palette, textured rice-paper background left mostly blank, dramatic but restrained lighting from upper front, painterly illustration not photographic, no text, no watermark, no signature, portrait crop from chest up, vertical composition --ar 3:4 --style raw --sref <SIGNATURE_B_URL> --sw 300` |
| 曹操原型 | `A three-quarter portrait bust illustration of a shrewd middle-aged ruler with a thin trimmed mustache, wearing an ornate dark ceremonial robe layered over light armor, a calculating half-lidded gaze and a faint controlled smirk, ink-line brush painting style combined with restrained mineral pigment color washes, muted warm base palette, textured rice-paper background left mostly blank, dramatic but restrained lighting from upper front, painterly illustration not photographic, no text, no watermark, no signature, portrait crop from chest up, vertical composition --ar 3:4 --style raw --sref <SIGNATURE_B_URL> --sw 300` |
| 吕布原型 | `A three-quarter portrait bust illustration of a flamboyant young cavalry general with sharp handsome features, wearing ornate lamellar armor with a tall pheasant-feather-plumed helmet crest, holding an ornate long-shafted halberd, a proud disdainful raised-chin gaze, ink-line brush painting style combined with restrained mineral pigment color washes, muted warm base palette, textured rice-paper background left mostly blank, dramatic but restrained lighting from upper front, painterly illustration not photographic, no text, no watermark, no signature, portrait crop from chest up, vertical composition --ar 3:4 --style raw --sref <SIGNATURE_B_URL> --sw 300` |
| 司马懿原型 | `A three-quarter portrait bust illustration of a reserved elderly strategist with a long grey beard and patient watchful eyes, wearing a plain dark scholar's robe with minimal ornamentation, a subtle wary sidelong glance with the head slightly turned back over the shoulder, ink-line brush painting style combined with restrained mineral pigment color washes, muted warm base palette, textured rice-paper background left mostly blank, dramatic but restrained lighting from upper front, painterly illustration not photographic, no text, no watermark, no signature, portrait crop from chest up, vertical composition --ar 3:4 --style raw --sref <SIGNATURE_B_URL> --sw 300` |

**一致性技巧——`--sref` 与 `--cref` 分工**：

- `--sref <签名图B的URL> --sw 300`：锁定**整套人像的画法风格 DNA**（笔触/上色逻辑/光照），5 个原型都挂同一个 `--sref`，保证"看起来是同一批人像"。
- `--cref <该武将某张已生成图的URL> --cw <权重，起点值 100>`：（若当前 Midjourney 版本支持角色参照功能，执行前请查官方文档确认参数名与可用性）用于保**同一位武将在多张图里长得像同一个人**——例如已经锁定"关羽原型"的盛年态签名图后，若还要产出他的"暮年态"或"负伤态"等变体图，在新 prompt 末尾追加 `--cref <盛年态签名图URL> --cw 100`，让新图的面部/build 尽量贴近参照图，而不是每次都长成不同的人。`--sref` 管"全局画风一致"，`--cref` 管"同一角色多图一致"，两者可以同时使用，互不冲突。

### 5.6 人像专属通过 / 不通过检查表

| # | 检查项 | 判定方法 |
|---|---|---|
| 1 | 气质标签是否**不看文字说明**就能从姿态/神态一眼读出（莽勇像莽勇、诡谋像诡谋） | 目视：遮住内部代号/说明文字，独立判断能否说出大致气质印象 |
| 2 | 身份（君主/武将/谋士/女性）是否通过服饰轮廓与姿态一眼可辨 | 目视：铠甲块面 vs 儒衫羽扇轮廓是否清晰区分 |
| 3 | 阵营色彩倾向是否落在 §4.4 势力色域内（若该武将已设定阵营归属） | 用取色工具吸取甲胄/服饰主色，对照 §4.4 色值 |
| 4 | 半身像构图与留白是否适配 UI 头像框（3:4 裁切后，圆形/方形二次裁切不会切掉头部或关键武器/道具特征） | 在设计工具里用圆形/方形遮罩预演裁切效果 |
| 5 | 未探明剪影态是否做到"神秘但轮廓仍可辨认身份大类"——**不是**一块纯黑看不出任何形状的色块 | 目视：能否至少分辨出"这是武将还是谋士"的大类轮廓 |
| 6 | 已识别态与未探明态并排放在一起，是否明显是"同一人物的两种信息显影程度"，而非两张风格不一致的图 | 目视对照：轮廓骨架/构图裁切是否一致，只是上色与五官详略不同 |
| 7 | 纸质/图卷基调是否与背景骨架（§1.3 签名图 A、§2 五张 MVP 背景）统一——**不能**变成另一种画风（如更偏动漫、更偏写实摄影感） | 把人像与背景骨架并排放在一起目视比对笔触/材质/光照是否一致 |
| 8 | 是否出现 MJ 自行生成的文字、水印、签名、名牌、状态图标等 UI 元素 | 目视逐角检查 |
| 9 | 是否明显神似某个具体现有游戏/影视作品的可识别角色造型（发型、兵器款式、招牌配色组合等） | 人工主观判断，存疑一律拒收，对应 §9.4 原创性红线，判定从严 |

任意一项打叉，该图拒收，回到 §5.3/5.4 模板调整对应槽位措辞后重出，不允许"五官画得好看就先凑合用"。

### 5.7 原创性红线再确认

本节所有 prompt（含 §5.5 的 5 个招牌原型）严格遵循 §0.2/§9.4：不点名在世画师、不点名具体游戏/影视作品、不复刻任何已有作品的可识别角色造型设计。若团队讨论中习惯用"关羽/诸葛亮"这类历史人物名字做内部代号，这本身是允许的（正史与《三国演义》原著属公有领域题材），**但一旦要落到 prompt 正文，必须换成本节示范的特征描述写法**，不得写入人物专名或具体作品的角色代号。

---

### 5.8 首轮人像出图评审（p-0 ~ p-3）

> 2026-07-07，项目方用 §5.5 招牌原型 prompt（关羽原型）出的第一轮 4 张图，反馈"感觉像水墨画多过游戏角色、不够立体"。美术总监亲自逐张目视复核，结论如下。

| 检查项 | p-0 | p-1 | p-2 | p-3 |
|---|---|---|---|---|
| 形体/体积（是否有方向光、核心暗部、高光，而非平涂线描） | 有——头盔金属高光、下颌与颈部阴影、虎纹披肩都有明显体积塑造 | 有——头盔/护肩有清晰高光和暗部渐变，龙首肩甲立体感强 | 有——同 p-1，光影结构清楚，非平涂 | 有——同上，铠甲折痕/暗部处理扎实 |
| 色彩/彩度（是否出现材质分区的局部色，而非通篇同一褐调） | **偏弱**——除左下一小块暗红披风外，全图几乎都在同一橙褐色相里，盔甲、皮肤、纸底几乎同色 | **偏弱**——同 p-0，通篇棕褐，仅背景一小块淡赭色斑 | **偏弱**——同上 | **偏弱**——同上，暗红披风面积比 p-0 还小 |
| MJ 擅自加文字/印章 | 未发现，干净 | **不通过**——左上角出现一列疑似毛笔手写汉字（伪汉字，非真实可读文字）+ 下方朱红印章，右下角另有一处疑似签名笔触 | **不通过**——左上角朱红印章，右下角出现一整行伪英文手写体（MJ 常见的"乱码花体字"），两处都违规 | 未发现，干净 |
| 画风是否统一（四张是否像同一批） | 是 | 是 | 是 | 是 |

**诊断结论——同意项目方主对话的判断，"不够立体"实质是"缺彩度/缺局部色"，不是形体问题**：四张图的铠甲、五官都有扎实的方向光和体积渲染，不是平涂水墨线描；但整张图（人物+纸底）被压在同一条低饱和橙褐色相里，人物和背景纸张几乎同色、材质之间（金属/皮革/布料/皮肤）也没有色彩区分，只有明暗层次没有色相层次——这在视觉心理上会被非美术使用者笼统描述成"扁""像画"，其实是"没有彩度对比造成的图-底不分离"，而非几何/光影缺失。另外 p-1、p-2 复现了背景那轮同样的问题——MJ 在"国风水墨"语境下有很强的自行加印章/伪文字倾向，这次连"伪英文花体字"这种新形式都出现了，说明负面词还需要再加强、覆盖更多变体写法。

### 5.9 关键裁定：人像允许比背景环境层级性提升局部饱和度

**裁定：同意，人像可以、也应当比环境背景更"鲜活"——但只开局部材质点缀色，不做整体提饱和。**

理由：
1. **视觉层级原则**：玩家注意力的焦点（人物）理应比舞台（环境背景）更饱和、更抓眼，这是常规的美术层级手法（前景/焦点提纯色度，背景/舞台压制色度），不是要推翻"军府图卷克制矿物色"的总方向。
2. **功能性理由更硬**：§4.4 势力识别色（阵营色）这套系统本来就是为了让玩家"看一眼就认出阵营"，但如果人像和环境用同一套压到底的褐调，势力色在游戏里出现频率最高的资产（头像）上反而没地方显色，等于势力色系统的核心应用场景被自己的画风压制了——这不是原创性或美学问题，是自我打架。
3. **代价可控**：只要把提饱和限制在"盔甲系带/披风内衬/肩带装饰"这类局部材质区域，铠甲主体板片和纸质背景仍然维持原来的克制褐调家族，就不会破坏"人像与背景同属一个纸感世界"的统一性——问题从"通篇同一褐调"变成"同一褐调家族里，人物身上多了几笔更饱和的势力色点缀"，观感上仍然协调。

**已同步裁定进 art-bible**：这是一条新的视觉决策（不只是"怎么写 prompt"），按本文件"不新增视觉决策，只翻译执行"的定位，不适合只写在这份 prompt 包里，已作为**补充条款**追加进 `design/art/art-bible.md` §3.1 人物半身像剪影哲学末尾（标注日期，未改动原有剪影形状语言/比例框/状态变体规则），并提示了蜀汉「赤金朱」与命令层「朱批朱红」色相接近、需实测核对同屏可辨识度这一点。

### 5.10 修订版人像模板 v1.2（在 §5.3 基础上追加局部提饱和 + 更强体积语言 + 印章/伪文字负面词）

> 本节**不删除** §5.3/§5.4 原模板，v1.2 是在其基础上的加强版，后续出图请优先使用 v1.2。

```
A three-quarter portrait bust illustration of a {AGE_DESCRIPTOR} {IDENTITY_DESCRIPTOR}, {TEMPERAMENT_POSE_AND_EXPRESSION}, wearing {FACTION_ARMOR_STYLE}, with vivid {FACTION_COLOR_ACCENT} color applied as bold localized color accents on the armor lacing cords, cloak lining, and shoulder sash only (the main armor plates and helmet remain in a restrained metallic bronze/iron base tone, matching a muted rice-paper world), warm naturalistic skin tone with visible healthy facial color contrast against the armor (skin must read as a distinct warm hue, not the same flat sepia as the armor or background), strong directional key light from upper-front-left creating a clear core shadow and a crisp rim-light along the armor edges and jawline, confident three-dimensional volumetric rendering with visible ambient occlusion in the cloth creases and a distinct specular highlight on metal edges, ink-line brush painting linework retained for outlines and hatching detail, aged rice-paper background left mostly blank, painterly illustration not photographic, no text, no watermark, no signature, no modern elements, portrait crop from chest up, vertical composition --ar 3:4 --style raw --sref <SIGNATURE_B_v1.2_URL> --sw 300
```

**负面提示 v1.2**（在 §4.4 通用负面清单基础上，比照 §7.3 背景修订同款加强印章/伪文字排除，并新增"防止通篇同色"的兜底词）：
```
--no text, watermark, signature, logo, UI, name tag, health bar, red seal, cinnabar seal, ink chop stamp, hanko, official stamp, fake calligraphy, fake chinese characters, brush-written characters, decorative cursive script, illegible handwritten text, artist signature mark, decorative corner seal, anime style, chibi, hyper-realistic photo skin, 3D render look, monochrome sepia rendering, single-tone color scheme, glasses, weapon glow effects
```

**未探明剪影态（§5.4）不受此次修订影响**：剪影态本来就是刻意压平为单色墨影（这是它"未识别"的信息含义），§5.4 原 prompt 保持不变；本次提饱和只作用于**已识名将**的全彩版本。

**示范：关羽原型（盛年·蜀汉·武将·莽勇）套用 v1.2 模板后的完整 prompt**：

```
A three-quarter portrait bust illustration of a middle-aged general in full lamellar armor, broad-shouldered build, a plumed helmet, a broad aggressive fighting stance, brow furrowed, fierce direct forward gaze, mouth set in a shout-ready open expression, wearing lamellar armor with warm brocade trim, with vivid deep crimson-gold color applied as bold localized color accents on the armor lacing cords, cloak lining, and shoulder sash only (the main armor plates and helmet remain in a restrained metallic bronze/iron base tone, matching a muted rice-paper world), warm naturalistic skin tone with visible healthy facial color contrast against the armor, strong directional key light from upper-front-left creating a clear core shadow and a crisp rim-light along the armor edges and jawline, confident three-dimensional volumetric rendering with visible ambient occlusion in the cloth creases and a distinct specular highlight on metal edges, ink-line brush painting linework retained for outlines and hatching detail, aged rice-paper background left mostly blank, painterly illustration not photographic, no text, no watermark, no signature, no modern elements, portrait crop from chest up, vertical composition --ar 3:4 --style raw --sref <SIGNATURE_B_v1.2_URL> --sw 300
```

### 5.11 下一步

**结论：这轮不能锁人像风格，需按 v1.2 重出一轮。** 而且有一个连带事项需要一起处理：

1. **签名图 B 本身也要重出**——§1.3 原有的签名图 B 是在"整体克制褐调"的假设下出的，如果只改 §5.3 的 30 将模板、却继续用旧签名图 B 做 `--sref` 源，`--sref` 会把新出的图重新"拉回"旧签名图 B 的低彩度基调，局部提饱和的效果会被部分抵消。建议**先用 §5.10 的 v1.2 措辞重新出一版签名图 B**，通过 §1.2 风格锁定标准 + 本文件 §3/§5.6 检查表后，再把新签名图 B 的 URL 作为后续 30 将出图的 `--sref` 源。
2. 新一轮出图重点复查三件事：① 局部提饱和是否只出现在系带/内衬/肩带这些局部区域，没有把整套盔甲都染色；② 印章/伪文字（含伪英文花体字这种新变体）是否被压下去；③ 人物和纸质背景放在一起看，是否既有彩度区分、又还是"同一个纸感世界"（不能因为提饱和就跳出统一画风）。
3. 三项都过、且 §5.6 人像专属检查表全项打勾后，才正式进入锁定：Upscale 取新签名图 B 的 URL → `--sref` 锁给后续核心 ~30 将 → 5 个招牌原型（§5.5）也建议按 v1.2 重出一遍，保持登记表（§6）里的签名样张与实际使用的 `--sref` 源一致。

### 5.12 第二轮出图评审（gy-0 ~ gy-3，检验 v1.2 修订效果）

> 2026-07-08，项目方用 §5.10 v1.2 模板（关羽原型示范 prompt，气质槽代入"莽勇"）出的第二轮 4 张图，反馈"颜色/立体解决了，但表情像在怒吼、画风偏"精致概念插画"、部分背景漏出山景"。美术总监亲自逐张目视复核，结论如下。

| 检查项 | gy-0 | gy-1 | gy-2 | gy-3 |
|---|---|---|---|---|
| 局部提饱和+体积感（§5.9 裁定是否达成） | 过——阵营红/金局部色鲜明，皮肤与铠甲分离清楚，体积感强 | 过，同上 | 过，同上 | 过，同上 |
| 表情是否符合"关羽=威严/傲物/stern dignified"（§5.5 原 prompt 原文措辞） | **不过**——张嘴怒吼状态 | **不过**，同上 | **不过**，同上 | **不过**，同上 |
| 背景是否"空白纸底"而非山水实景泄漏 | 过——仅做旧纸卡+边框，无实景 | 过，同上 | **不过**——人物身后清楚可见山峦/河流实景 | 有条件通过——目视未见明显山水实景，背景处理与 gy-1 类似（素色纸卡竖幅+边框），但不排除低分辨率/压缩掩盖了淡淡远景，**建议项目方放大原图复核** |
| 渲染媒介是否与背景骨架统一（墨线+纸本 vs 光滑数字概念插画/漫画感） | **不过**——线条锐利干净、金属高光强、皮肤上色偏写实上色质感，笔触/纸纹几乎看不出 | **不过**，同上（四张里相对最接近纸感的一张，仍不达标） | **不过**，同上 | **不过**，同上 |
| 呈现方式是否"活在同一张图卷上"而非"单独裁剪卡牌" | **不过**——明显的做旧卡片式独立边框，呈现成"角色单卡"而非"图卷一角" | 不过，同上 | 不过，同上 | 不过，同上 |
| **综合结论** | 色彩/立体问题稳定解决；表情、渲染媒介、卡片化呈现三项不过 | 同左，四张中相对最克制的一张（渲染媒介问题程度略轻） | 同左，另加背景山水泄漏问题 | 同左，背景问题存疑待复核 |

**与教练初判不同之处，如实记录**：教练初判 gy-2/gy-3 背后均有山景泄漏；我逐张目视复核后，**gy-2 确认属实**（山峦河流清晰可辨），但 **gy-3 未能确认**——gy-3 的背景处理观感上更接近 gy-1 的素色纸卡竖幅（有边框、无明显地貌轮廓），山水泄漏的判断在当前图片分辨率下证据不足，建议项目方放大 gy-3 原图核实后再决定这条负面提示要不要按"确诊"力度去写。

### 5.13 关键裁定：画风是否要往回收一档

**裁定：往回收。** 局部提饱和这个方向（§5.9 裁定）予以保留，但本轮暴露的"渲染媒介滑向精致概念插画/漫画感"必须收回，理由如下：

1. **这不是一个可以两难取舍的偏好问题，而是触碰了整个视觉体系最上位的规则**——art-bible §1.1 把游戏的核心视觉锚点定义为"军府案上展开的**同一张**活图卷"：底层地貌、中层批注、顶层人物，三层必须是**同一支笔画出来的**。如果人像换成了另一种渲染媒介（光滑数字上色、漫画式锐利描线、强金属高光），哪怕颜色再准、体积再对，人物看起来也像是"贴在图卷上的另一张纸"，图卷"同一张画"的核心叙事直接被打破。这个优先级高于"焦点角色该更精致"这类锦上添花的考虑。
2. **提饱和与渲染媒介是两件独立的事，不冲突**——§5.9 只裁定了"局部色可以更鲜活"，从未要求"渲染技法也要更写实/更光滑"。本轮背景骨架（bg2 组，见 §7.5）已经证明：同一套"墨线+矿物色平涂+纸纹"技法完全能承载足够的色彩变化和层次，不需要靠"数字概念插画"式的渲染才能有立体感。问题出在 v1.2 prompt 里新加的"volumetric rendering...distinct specular highlight on metal edges"这类措辞把 MJ 拉向了它训练集里"史诗奇幻角色概念图"的默认路径，而不是局部提饱和本身导致的。
3. **修法**：保留局部色彩指令，但把体积感的描述方式从"强方向光+清晰高光+3D volumetric"换成"笔触塑造的平面色块渐变"，并在负面提示里明确排除"3D 渲染感/漫画式锐边/玻璃感高光"；同时补一条排除"独立卡片式边框"的负面提示，解决呈现方式的卡片化问题（这一点不是渲染技法问题，是构图/裁切呈现的问题，需要单独一条负面提示）。这条裁定已同步写入 `design/art/art-bible.md` §3.1 的 2026-07-08 补充条款，不只留在本文件。

### 5.14 关于"莽勇"气质配关羽——示范用错，非签名 prompt 本身的问题

复核 §5.5 原始的"关羽原型"签名 prompt 原文，其中的表情措辞本来就是 `a stern dignified direct gaze`（威严、非张嘴怒吼），这是对的，符合关羽"傲物/威压"而非"莽勇"的人物气质。**问题出在我自己在 §5.10 举例套用参数化模板时，为了演示"气质槽"用法，选了"莽勇"这个槽位值去示范，而"莽勇"（`a broad aggressive fighting stance...mouth set in a shout-ready open expression`）本身对应的是张飞/许褚这类角色的气质，不适合用来示范关羽——这是我在写示范时的一处选值错误，不是气质词表本身有问题，也不需要修改 §5.3 气质槽的定义。** 修正方式：以后凡是"关羽原型"相关的示范/出图，气质槽一律代入**傲物**，并按下方 §5.15 的裁定把"faint disdainful smirk"这一句改得更贴近"威严不带轻蔑笑意"，避免读成得意/讥讽而非庄重。

### 5.15 修订版人像模板 v1.3（渲染媒介回收 + 表情修正 + 空白纸底加固）

> 本节**不删除** §5.10 v1.2 模板，v1.3 是在其基础上的定向修正版，后续出图请优先使用 v1.3。修正范围：① 渲染媒介措辞从"3D volumetric + 强高光"改回"笔触塑造的平面色块"；② 新增排除"独立卡片式边框"负面提示；③ 新增排除"背景山水/地貌泄漏"负面提示；④ 傲物气质槽的"disdainful smirk"改为庄重不轻蔑的措辞。

```
A three-quarter portrait bust illustration of a {AGE_DESCRIPTOR} {IDENTITY_DESCRIPTOR}, {TEMPERAMENT_POSE_AND_EXPRESSION}, wearing {FACTION_ARMOR_STYLE}, with vivid {FACTION_COLOR_ACCENT} color applied as bold localized color accents on the armor lacing cords, cloak lining, and shoulder sash only (the main armor plates and helmet remain in a restrained metallic bronze/iron base tone, matching a muted rice-paper world), warm naturalistic skin tone with visible healthy facial color contrast against the armor, traditional Chinese ink-wash brush painting technique with visible brush linework and flat mineral-pigment color washes building up form through layered flat color planes and soft brush-textured core shadow, not glossy digital airbrush rendering, not comic-book cel shading with hard vector outlines, not photorealistic 3D render, avoiding strong specular highlights or glossy metal reflections, plain continuous aged rice-paper background filling the entire frame with no distinct landscape, no mountains, no hills, no river, no horizon line, no scenery of any kind, the figure and paper background read as part of the same continuous scroll surface rather than a separate cut-out card, painterly illustration not photographic, no text, no watermark, no signature, no modern elements, portrait crop from chest up, vertical composition --ar 3:4 --style raw --sref <SIGNATURE_B_v1.3_URL> --sw 300
```

**负面提示 v1.3**（在 §5.10 v1.2 负面清单基础上追加，新增部分已加粗标注）：
```
--no text, watermark, signature, logo, UI, name tag, health bar, red seal, cinnabar seal, ink chop stamp, hanko, official stamp, fake calligraphy, fake chinese characters, brush-written characters, decorative cursive script, illegible handwritten text, artist signature mark, decorative corner seal, anime style, chibi, hyper-realistic photo skin, 3D render look, monochrome sepia rendering, single-tone color scheme, glasses, weapon glow effects, **glossy specular highlight, comic book ink shading, hard vector outline, digital airbrush gradient, photobash rendering, distant landscape, mountains, hills, river, scenery background, floating paper card, vignette border frame, torn deckled edge, cut-out card composition, open mouth shouting, screaming, roaring expression, aggressive shout-ready expression**
```

**气质槽定向修订——"傲物"（§5.3 原表，仅这一行措辞更新，其余气质槽不变）**：

| 气质标签 | 原措辞（§5.3） | v1.3 修订措辞 |
|---|---|---|
| 傲物 | `a raised chin with shoulders pulled back, half-lidded dismissive gaze looking down and away from the viewer, faint disdainful smirk` | `a raised chin with shoulders pulled back, a level composed gaze looking slightly down and away from the viewer, a stern closed-mouth expression, dignified and unsmiling` |

> 修订理由：原措辞的"disdainful smirk"容易被 MJ 渲染成挑衅/讥讽的表情，偏离"威严"这个核心印象；改为"stern closed-mouth...dignified and unsmiling"更贴近关羽这类"傲骨但不轻佻"的角色设定，同时也直接回应了项目方"表情该 stern/dignified、不该有攻击性"的诉求。

**示范：关羽原型（盛年·蜀汉·武将·傲物）套用 v1.3 模板后的完整 prompt**：

```
A three-quarter portrait bust illustration of a middle-aged general in full lamellar armor, broad-shouldered build, a plumed helmet, a raised chin with shoulders pulled back, a level composed gaze looking slightly down and away from the viewer, a stern closed-mouth expression, dignified and unsmiling, wearing lamellar armor with warm brocade trim, with vivid deep crimson-gold color applied as bold localized color accents on the armor lacing cords, cloak lining, and shoulder sash only (the main armor plates and helmet remain in a restrained metallic bronze/iron base tone, matching a muted rice-paper world), warm naturalistic skin tone with visible healthy facial color contrast against the armor, traditional Chinese ink-wash brush painting technique with visible brush linework and flat mineral-pigment color washes building up form through layered flat color planes and soft brush-textured core shadow, not glossy digital airbrush rendering, not comic-book cel shading with hard vector outlines, not photorealistic 3D render, avoiding strong specular highlights or glossy metal reflections, plain continuous aged rice-paper background filling the entire frame with no distinct landscape, no mountains, no hills, no river, no horizon line, no scenery of any kind, the figure and paper background read as part of the same continuous scroll surface rather than a separate cut-out card, painterly illustration not photographic, no text, no watermark, no signature, no modern elements, portrait crop from chest up, vertical composition --ar 3:4 --style raw --sref <SIGNATURE_B_v1.3_URL> --sw 300
```

**未探明剪影态（§5.4）不受此次修订影响**——理由同 §5.10。

### 5.16 下一步

**结论：这轮仍然不能锁人像风格，需按 v1.3 再出一轮，且这次连同签名图 B 一起重出。**

1. 签名图 B 沿用 §5.11 已经指出的连带事项——本来就该按提饱和的 v1.2 重出，现在还要叠加 v1.3 的"渲染媒介回收+空白纸底加固"措辞，两件事合并成一次重出即可，不用先出一版 v1.2 签名图 B 再出一版 v1.3，省一轮。
2. 新一轮出图重点复查四件事：① 表情是否回到 stern/dignified；② 渲染质感是否退回"看得出笔触和纸纹"的墨线水墨感，和 bg2-2 类型的背景放在一起看是否像同一支笔画的；③ 背景是否彻底空白无地貌、且不再是"独立卡片"观感；④ 局部提饱和（§5.9）的效果是否还保留（不能为了收渲染媒介又把颜色收回单色）。
3. 若这四点都过、且 §5.6 检查表全项打勾，才正式进入锁定阶段。届时签名图 B 的 URL 更新进 §6 登记表，5 个招牌原型（§5.5）里的关羽原型示范也一并按"傲物"气质重新出一版存档（用于替换目前示范文字里容易引起误解的"莽勇"套用示例）。

### 5.17 第三轮出图评审（gy3-0 ~ gy3-3，检验 v1.3 模板效果）

> 2026-07-08，项目方用 §5.15 v1.3 模板（关羽原型示范 prompt，气质槽代入"傲物"）出的第三轮 4 张图。美术总监亲自逐张目视复核 §5.6 人像专属检查表，结论如下。**核心结论：v1.3 的三处修订（表情改傲物 / 渲染媒介回收 / 空白纸底加固）全部生效**——4 张图一致呈现出"绢本设色·功臣像"（近明代帝王/功臣画像）的质感：笔触细腻但仍是平涂+晕染的传统技法，没有数字概念插画式的强高光/漫画锐边；表情全部是闭口、沉静、略带审视的庄重神态，无一张张嘴咆哮；背景是纯色绢底渐变，无山水/地貌泄漏。这是三轮迭代里第一次同时解决"立体感/表情/媒介统一"三项问题。

| §5.6 检查项 | gy3-0 | gy3-1 | gy3-2 | gy3-3 |
|---|---|---|---|---|
| 1 气质是否读出"傲物"而非"莽勇" | 过——沉静侧首、闭口，气质克制庄重 | 过，同上，五官刻画最锐利、气质最沉肃 | 过，眉头略紧、更显凝重，但仍是闭口非咆哮，属"傲物"光谱内的偏严厉一端 | 过，眉头紧蹙、目光更锐，同样闭口，属"傲物"偏威压一端 |
| 2 身份轮廓（武将=宽肩甲胄） | 过 | 过 | 过——但构图更饱满，肩甲几乎顶到左右画框边缘 | 过 |
| 3 阵营色落 §4.4 色域 | 过，但色块以细线状系带/绳结呈现 | 过，同上，红色系带层次最丰富（领口+肩带+腰间多处） | 过，胸甲中央绳结+右下角披帛，红色面积略大于 gy3-0/1 | 过，领口红绳+左肩至胸前的大幅橙红披帛，四张里红色可见面积**最大**的一张 |
| 4 3:4 UI 头像框裁切安全 | 过 | 过 | **需留意**——构图偏满，方形/圆形二次裁切时肩甲边缘可能被切到，建议实测 | 过 |
| 7 纸质/图卷基调与背景骨架统一 | **过，本轮质变达成** | 过，同上 | 过，同上 | 过，同上 |
| 8 无擅自文字/水印/UI元素 | 过 | 过 | 过 | 过 |
| 9 §9.4 原创性红线（是否神似具体现有作品） | 有条件过，见 §5.19 | 有条件过，同上 | 有条件过，同上 | 有条件过，同上 |
| **综合结论** | 通过 | **通过，本轮个人倾向选用（五官/系带层次/裁切安全性综合最佳）** | 通过（裁切需实测确认） | 通过（红色面积最大，可作"朱红强度是否要加"这一裁定的参考样本） |

### 5.18 关键裁定 (A)：阵营朱红的强度——不整体提饱和，改为"扩大色块连续面积"

**裁定：这一档"绢本设色"的克制程度本身是对的，不需要提高饱和度数值，但要解决一个功能性的隐患——点缀色的**呈现形状**不够"抗缩放"。**

理由：
1. §5.9/§5.13（art-bible §3.1 的两条既有裁定）要求局部提饱和是为了服务"UI 头像框里的阵营辨识度"这个**功能目标**，不是为了让画面本身更"鲜艳好看"。gy3-0~gy3-3 现在的红色大多呈现为**细线状**（系带、绳结、镶边），在出图原始分辨率下清晰美观，但美术资产最终要缩到 64×85px 的头像缩略框里使用——细线一旦缩小，线宽可能不足一个像素，视觉上会直接消失，起不到"一眼辨认阵营"的效果。这不是我凭空猜测的风险，是缩略图渲染的物理规律。
2. 因此**不建议往"整体调高饱和度数值"这个方向去修**——那会重新打开 §5.13 已经关闭的那扇门（滑向精致插画感的风险）。正确的修法是**保持当前的克制饱和度，但把红色点缀色画成一整块连续色块**（例如 gy3-3 那种"一整幅披帛"处理，而不是 gy3-1 那种"多处细系带"处理）——gy3-3 恰好提供了一个很好的参照：同样克制的绢本质感下，披帛这种大面积色块比系带更抗缩放。
3. 这条裁定已作为验收标准补充写入 `design/art/art-bible.md` §3.1 的 2026-07-08 第二条补充裁定（"阵营点缀色的功能达标标准是缩略图可辨识，不是原图可辨识"），不只留在本文件。

**prompt 修法**：把 v1.3 模板里 `applied as bold localized color accents on the armor lacing cords, cloak lining, and shoulder sash only` 这句，改为更强调"大面积连续色块"而非"多处细线"：

```
with vivid {FACTION_COLOR_ACCENT} color applied as one bold continuous color block on a single large accent area — either the full cloak lining draped visibly across one shoulder, or a single wide sash panel worn diagonally across the chest — rendered as one solid connected shape rather than multiple thin cords or scattered small trim lines (the main armor plates and helmet remain in a restrained metallic bronze/iron base tone, matching a muted rice-paper world)
```

### 5.19 关键裁定 (B)：§9.4 原创性红线复核——有条件通过，建议一次人工侧向核对

**裁定：有条件通过，不判定为违规，但因为这批图的"绢本设色·功臣像"观感异常接近真实馆藏文物，建议锁定 `--sref` 之前多做一步人工核对，而不是仅凭 prompt 文本合规就直接放行。**

理由：
1. **prompt 文本层面完全合规**：本轮沿用的 §5.15 v1.3 模板全程使用特征描述（`long flowing beard`、`deep red-toned weathered face`、`dark lamellar armor`等），未点名任何具体画作、画师或现有游戏/影视作品，符合 §0.2/§9.4/§5.7 的原创性红线要求。
2. **"绢本设色武将/帝王功臣半身像"是一个跨越数百年、成千上万件传世作品共享的**画种通用惯例**（如故宫南薰殿历代帝王圣贤名臣像一类的宫廷肖像传统），不是任何单一作品独占的视觉签名，参照这个画种本身不构成侵权，这一点与我们过往对"军府活图卷"整体画风的原创性判断逻辑一致。
3. **但需要正视一个新的、之前没遇到过的风险点**：由于这批图的写实程度、构图规整度都明显高于前两轮的"插画感"人像，且 MJ 的训练集里大概率吸收过大量具体的、可识别的关羽画像/塑像/年画（这是被广泛复制的公共形象，从画像到庙宇塑像都有极高的辨识度），**存在小概率 MJ 无意间高度趋同于某一张具体现存关羽画像的构图/配色/姿态的可能性**，这与"套用画种风格"是两回事——前者是我们主动要的，后者是需要规避的。这个风险在前两轮"偏插画感"的出图里较低（观感上离任何具体真迹都比较远），但这一轮"越像真迹，风险越该被多看一眼"，这正是艺术总监该主动把关的地方，不能因为"prompt 文本合规"就跳过这一步。
4. **建议动作（轻量，不阻塞锁定进度）**：在正式把 `gy3-1` 升采样锁为 `--sref` 签名图 B 之前，项目方或团队内任何人花几分钟做一次**人工侧向比对**——把 `gy3-1` 和 1-2 张最广为流传的关羽画像/庙宇塑像参考图并排看一眼，确认不是姿态、道具位置、配色布局的高度雷同（不需要专业査重工具，人眼一眼看出"这是同一构图"或"完全不是"即可）。这一步做完、结论是"不雷同"，即可视为 §9.4 复核完成，不需要再等一轮出图。

### 5.20 锁定确认：选定 `gy3-1` 为武将半身像签名图 B，正式进入 `--sref` 锁定阶段

**结论：人像本轮可以进入锁定阶段，但比背景多一个轻量前置步骤——完成 §5.19 的人工侧向核对后即可锁定，不需要再出一轮验证画风。**

`gy3-0`/`gy3-1`/`gy3-3` 三张 §5.6 检查表全项通过，`gy3-2` 因构图偏满在裁切安全性上需留意，四张里我倾向选 **`gy3-1`**——五官刻画最锐利清晰、红色系带层次最丰富、裁切安全性最好，综合代表性最佳，但这只是我的倾向，供项目方确认或换选。

**下一步具体动作**：
1. 完成 §5.19 的人工侧向核对（轻量、非阻塞性前置步骤）。
2. 核对通过后，对 `gy3-1` 执行 Upscale，取图片 URL，登记进 §6 素材登记表"签名图 B"行。
3. 用该 URL 作为 `--sref` 源，重出 5 个招牌原型（§5.5）——含关羽原型本身（这次气质槽正式改用"傲物"，见 §5.14），锁定核心 ~30 将的批量出图管线。
4. 批量出图时，正面 prompt 采用 §5.15 v1.3 模板，并按 §5.18 的措辞修法把阵营点缀色改成"单块连续色块"而非"多处细线"，解决缩略图可辨识度的隐患。
5. 未探明剪影态（§5.4）不受本轮任何修订影响，可直接沿用。

> **§5.21 起的内容修正/取代本节（§5.20）的锁定结论**：项目方作为最终决策者对"绢本 register"提出明确异议，经重新论证后方向改判，见下方 §5.21-§5.24。**`gy3-1` 不再是签名图 B 的锁定对象**，本节保留作为评审过程存档，不删除。

### 5.21 项目方关键异议与方向改判：从"绢本设色"改判为"游戏立绘"

> 2026-07-08，项目方对 §5.17-§5.20 的绢本方向提出明确异议：第二轮 `gy-2`（见 `scratchpad/round3/USER-PICK-game-portrait.png`，鲜活立体的游戏概念插画风格）**才最像"游戏里的武将头像"**；第三轮我选定的绢本 `gy3-1` **"不太像游戏武将头像、太像博物馆文物"**。项目方同时指出一个我此前评审中没有正面处理的架构事实：**武将头像在实际界面里是独立显示在头像框内的（角色名册、外交对话、出征部署面板等），并不会被合成叠加到地图图卷的纸面上**——这与背景骨架"必须是图卷本体的一部分"不是同一种资产使用方式。项目方同时立下硬红线：走立绘方向**绝不能撞《三国志》（Koei）或任何现有三国题材游戏的立绘画风**。

**逐条回答如下。**

#### 5.21.1 方向推荐（明确，不两可）：**采用"游戏立绘 register"，不再坚持绢本**

我重新论证后的结论是：**同意项目方的方向，明确推荐"游戏立绘"而非绢本**，理由一句话——**头像框是独立展示单元、不与地图纸面合成，"三层同一支笔"这条规则原本要解决的是"合成在同一画面时的媒介断裂"问题，武将头像这个资产门类根本不落在这个问题的适用范围内，所以没有必要为它牺牲角色识别度和游戏感。** 绢本方向在"评审好不好看""是否像功臣像"这个单一维度上确实做得很精致，但**用错了评判维度**——头像资产的首要功能是"名册/外交/部署这类界面里快速扫视辨认角色"，这个功能场景下，游戏惯例（Koei 三国志系列、Total War 系列、王朝无双等）普遍让角色立绘比环境美术更浓烈、更具辨识度，这不是偶然的行业惯性，而是"焦点资产需要在小尺寸/快速扫视下胜出"这一视觉层级规律的具体体现，与 §3.1 已有的"焦点可局部提饱和"裁定内在逻辑一致，只是这次要放开的维度从"色彩"扩大到"渲染技法"。

#### 5.21.2 §1.1 coherence 是否可以松：**可以，正面裁定为一条资产门类范围说明，不是废除 §1.1**

**裁定：§1.1"三层同一支笔"的强制力，范围限定为"实际合成在地图图卷画面上的元素"（地貌底板、图卷表面的标注笔迹、直接画在图卷上的事件/据点小图标等）；独立显示在专属 UI 框内、不与图卷纸面合成的资产门类（武将头像、外交/名册/部署面板里的半身像等）不受"同一支笔"字面约束，允许采用更浓烈的游戏立绘渲染技法。** 这条裁定已同步写入 `design/art/art-bible.md` §1.1 的资产门类范围补充说明（见下方art-bible编辑），不只留在本文件。

**但松开的只是"渲染技法"这一个维度，不是"世界观材质与色彩身份"——立绘资产仍必须满足以下"家族相似性"约束，否则会变成另一个世界观**：
1. 背景材质仍锚定纸本/绢本纹理（头像可以是"画在这种纸上的一幅更精致的画"，而不是脱离纸感的纯数字渲染背景）；
2. 主体色仍锚定 §4.1 矿物色系基调（青铜/铁灰/黑漆），阵营色仍严格取自 §4.4 色值，不允许因为"立绘化"就整体转向高饱和多彩的"游戏卡牌"配色；
3. 铠甲/服饰轮廓仍遵循 §3.1 汉晋铠甲形制词汇，不允许西式板甲、奇幻夸张部件；
4. 构图仍是半身像 + 3:4 裁切，不做 Koei 式全身动态出招姿势。

#### 5.21.3 "立绘 × 不撞 Koei"能否兼得：**能兼得**，给出 v1.4 完整 prompt，并明确差异化维度

不冲突。给出以下几个**可描述、可执行**的差异化维度，把"游戏立绘"和"Koei/三国无双式立绘"从笔法、材质、配色、构图、打光五个维度拉开距离（正面描述 + 负面排除双重保险，遵循 §0.2/§9.4 特征描述而非点名的原创性红线）：

| 维度 | Koei/三国无双式典型做法（不点名、仅描述特征以便负面排除） | 本项目 v1.4 的差异化选择 |
|---|---|---|
| 笔法/渲染 | 光滑数字上色、清晰硬边描线、近漫画/CG插画质感 | **可见笔触的水粉/水彩式厚涂**（gouache-like layered color blocking），保留笔触颗粒，不追求光滑 |
| 材质基底 | 干净的纯色/渐变背景板，像"角色卡"底 | **纸本/绢本纹理背景**（做旧纸纤维质感的柔和渐晕，不是纯色板） |
| 配色 | 全身高饱和多彩，每个角色一套鲜艳撞色 | **主体维持青铜/铁灰/黑漆矿物色基调，只在单块大色斑做阵营色**（§5.18 已定的"整块而非细线"原则），不整体转多彩 |
| 铠甲廓形 | 常见夸张化肩甲、异形头盔、装饰性极强的非历史造型 | **遵循 §3.1 汉晋札甲形制词汇**，装饰克制、历史可信优先 |
| 姿态/表情 | 大幅动态出招姿势，或夸张咆哮/得意等强烈表情 | **半身像静态构图 + 内敛克制的"威压"表情**（闭口、目光锐利、不咆哮） |

**人像模板 v1.4（正式版）**：

```
A three-quarter portrait bust illustration of a {AGE_DESCRIPTOR} {IDENTITY_DESCRIPTOR}, painted in a bold hand-painted game character illustration style with confident visible brushwork and gouache-like layered color blocking (not glossy 3D render, not cel-shaded anime, not photographic), {TEMPERAMENT_POSE_AND_EXPRESSION}, wearing {FACTION_ARMOR_STYLE} built from historically grounded Han-Jin lamellar plates and layered cloth, dramatic strong directional side lighting carving deep core shadow and a warm highlight along the brow, cheekbone, and armor ridge, vivid {FACTION_COLOR_ACCENT} rendered as one bold continuous color block on a single large accent area (a full cloak draped across one shoulder, or one wide diagonal chest sash) — a solid connected shape, not scattered thin cords or trim lines, with the main armor plates and helmet kept in a restrained bronze/iron/lacquer-black base tone drawn from the game's muted earth-mineral palette, warm weathered naturalistic skin with visible age and battle-worn texture, background is a plain aged rice-paper or silk parchment backdrop with soft vignette shading and visible paper fiber texture, no distinct scenery, no landscape, no mountains, no hills, no river, no horizon, character rendered with mature realistic human facial proportions, painterly illustration texture throughout, no text, no watermark, no signature, no modern elements, portrait crop from chest up, vertical composition --ar 3:4 --style raw --sref <SIGNATURE_B_v1.4_URL> --sw 300
```

**气质槽 v1.4（傲物/威压，融合"gy-2 的鲜活"与"gy3 的威严"，替换 §5.15 版本）**：
```
a fierce commanding gaze with brows lowered in intense focus, jaw set firm, mouth closed in a stern hard-set expression, head held high with controlled aggression — projecting authority and battlefield presence, dignified rather than shouting, no open mouth
```

**负面提示 v1.4**（在 §5.15 v1.3 负面清单基础上修订，新增/关键项已加粗）：
```
--no text, watermark, signature, logo, UI, name tag, health bar, red seal, cinnabar seal, ink chop stamp, hanko, official stamp, fake calligraphy, fake chinese characters, brush-written characters, decorative cursive script, illegible handwritten text, artist signature mark, decorative corner seal, chibi, hyper-realistic photo skin, monochrome sepia rendering, single-tone color scheme, glasses, weapon glow effects, distant landscape, mountains, hills, river, scenery background, open mouth shouting, screaming, roaring expression, **anime large eyes, cel-shaded anime style, glossy 3D render look, plastic skin, chrome metallic reflection, trading card frame border, oversized fantastical horned helmet, exaggerated oversized pauldrons, rainbow multi-color costume, western medieval plate armor, european knight armor, Koei Three Kingdoms game art style, Dynasty Warriors style, mobile gacha game splash art style**
```

**示范：关羽原型（盛年·蜀汉·武将·傲物-威压）套用 v1.4 模板后的完整 prompt**：

```
A three-quarter portrait bust illustration of a middle-aged general with a long flowing beard and a deep red-toned weathered face, painted in a bold hand-painted game character illustration style with confident visible brushwork and gouache-like layered color blocking (not glossy 3D render, not cel-shaded anime, not photographic), a fierce commanding gaze with brows lowered in intense focus, jaw set firm, mouth closed in a stern hard-set expression, head held high with controlled aggression — projecting authority and battlefield presence, dignified rather than shouting, no open mouth, wearing dark lamellar armor with warm brocade trim built from historically grounded Han-Jin lamellar plates and layered cloth, dramatic strong directional side lighting carving deep core shadow and a warm highlight along the brow, cheekbone, and armor ridge, vivid deep crimson red rendered as one bold continuous color block on a single large accent area (a full cloak draped across one shoulder, or one wide diagonal chest sash) — a solid connected shape, not scattered thin cords or trim lines, with the main armor plates and helmet kept in a restrained bronze/iron/lacquer-black base tone drawn from the game's muted earth-mineral palette, warm weathered naturalistic skin with visible age and battle-worn texture, background is a plain aged rice-paper or silk parchment backdrop with soft vignette shading and visible paper fiber texture, no distinct scenery, no landscape, no mountains, no hills, no river, no horizon, character rendered with mature realistic human facial proportions, painterly illustration texture throughout, no text, no watermark, no signature, no modern elements, portrait crop from chest up, vertical composition --ar 3:4 --style raw --sref <SIGNATURE_B_v1.4_URL> --sw 300
```

#### 5.21.4 锁定结论修正（取代 §5.20）

**一句话结论：不锁绢本 `gy3-1`；改走游戏立绘方向，用 v1.4 重新出一轮，验证通过后才锁签名图 B。背景 `bg3-1` 的锁定结论不受影响，维持 §7.9 的结论。**

新一轮出图需要重点复核：① 是否达到"鲜活立体、像游戏武将头像"的观感（对照 USER-PICK 参考图的鲜活度/打光/阵营色力度）；② 表情是否落在"威严克制"而非"咆哮"或"面无表情"；③ 是否与 Koei/三国无双式立绘拉开可描述的距离（笔触颗粒感、纸本背景、矿物色基调、历史铠甲廓形四项，对照 §5.21.3 的表格逐条核对）；④ §9.4 原创性红线复核（本轮风险点从"像绢本真迹"转移为"像现有三国游戏立绘"，复核方法不变——人工侧向比对，这次比对对象换成 1-2 张 Koei/三国无双的公开角色立绘做"确认不神似"）。

### 5.22 第四轮人像出图评审（gy4-0 ~ gy4-3，v1.4 立绘模板首次验证，不带 `--sref`）

> 本节记录用 §5.21.3 v1.4 模板（关羽·盛年·蜀汉·武将·傲物-威压）出的第四轮 4 张图（`gy4-0`~`gy4-3`），未挂 `--sref`（尚无签名图 B 可挂），目的是先验证"模板本身"是否已能独立达成鲜活立体+威严克制+区隔 Koei 三项目标，再决定要不要挂 `--sref` 正式锁定。评审由美术总监亲自逐张目视核对，独立于项目方初判进行；结论与项目方初判总体一致，但在"是否可以直接锁定"这一点上更保守，理由见下方裁定。

**5.22.1 §5.21.4 四项复核逐张打勾**

| 检查项 | gy4-0 | gy4-1 | gy4-2 | gy4-3 |
|---|---|---|---|---|
| ① 鲜活立体（对照 USER-PICK 的打光力度/阵营色浓度） | 通过——侧脸戏剧光+大块红披风 | 通过——正面三分侧脸，戏剧光最均衡 | 通过——但暗部占比过大，鲜活感被暗调吃掉一部分 | 通过——高对比侧光，红披帛块面清晰 |
| ② 威严克制、非咆哮 | 通过——闭口、神情沧桑肃穆 | 通过——皱眉闭口，最"临阵统帅"感 | 通过——闭口紧绷、目光锐利 | 通过——闭口，风霜感强 |
| ③ 与 Koei/三国无双拉开距离（§5.21.3 五维） | 通过——见下方五维小结 | 通过——五维最均衡 | 通过——但头盔顶部分叉冠饰造型偏"装饰化"，需留意别滑向"夸张奇幻头盔"边界（尚未越线，列为观察项） | 通过——但见 5.22.2 材质项扣分 |
| ④ §9.4 原创复核（对照 Koei/三国无双公开关羽立绘，非博物馆画像） | 通过 | 通过 | 通过 | 通过 |

**五维小结（③ 的展开依据，逐项对照 §5.21.3 表格）**：4 张在"笔触颗粒感"（可见厚涂笔触，非光滑渲染）、"矿物基调铠甲"（黑铁/青铜为主，无西式板甲）、"半身克制构图"（无夸张出招动态）三项上表现一致且稳定过关；"纸本背景材质"一项 4 张不完全一致（见 5.22.2）；"阵营色单一大色块"一项 4 张全部达标（红色披帛/斜披均为整块连贯形状，符合 §5.18 抗缩放裁定）。§9.4 复核基于品类特征比对（Koei/三国无双惯用干净赛璐璐上色+多彩戎装+夸张比例，本轮 4 张均为厚涂笔触+克制矿物色+单一大色块红披，风格总体可辨识为不同技法路线）；建议项目方仍做一次人工侧向比对（1-2 张 Koei/三国无双公开关羽立绘）作为轻量非阻断确认，理由同 §5.19 的方法论。

**5.22.2 家族相似性 4 条底线逐张打勾**

| 底线项 | gy4-0 | gy4-1 | gy4-2 | gy4-3 |
|---|---|---|---|---|
| 纸/绢本背景材质 | 通过——暖米色纸底，隐约竖向纹理 | 通过——暖奶油色纸本底，材质感最干净 | 通过——暖褐色纸本底（虽被暗角吃掉大半画面，但可辨识材质仍是暖纸本） | **不通过/存疑**——背景左右两侧明显偏冷灰蓝色调，与"暖纸本/绢本"基调脱节，中央头部背后小块暖褐色调纹理尚可辨认，但整体已不是统一的暖纸本观感 |
| §4.1 矿物基调 | 通过 | 通过 | 通过 | 通过 |
| §3.1 汉晋铠甲廓形 | 通过 | 通过 | 通过（头盔冠饰略装饰化，见上方观察项，暂判通过） | 通过 |
| 半身克制构图 | 通过 | 通过 | 通过 | 通过 |

**结论：`gy4-3` 的冷灰背景确认破坏了"纸本基调"这条家族相似性底线**，与项目方初判一致。其余三张材质项均通过。`gy4-3` 因此从签名图 B 候选中排除，不参与后续锁定比较（其表情/铠甲/构图仍合格，可作为"打光对比参考"留档，但不入候选池）。

**5.22.3 整体偏暗——裁定为缩略框合法隐患，需要 prompt 修正**

四张图的整体明度都偏低，尤其 `gy4-2`（暗部占画面比例最大，人脸在画面中占比小且大半落入阴影）与 `gy4-0`（侧脸转向幅度大+暗部广）。`gy4-1` 是四张里明度最均衡、脸部最清楚的一张，但仍属于中低调（moody）而非中高调（mid-key）画面。**裁定：这是真实的缩略框（64×85px）可辨识度隐患，不是主观美学偏好问题**——三国志类游戏名册/外交/部署面板需要玩家在缩略图尺度快速扫视辨认武将，若脸部在小尺寸下与背景暗部融为一团，会直接损害游戏性功能（辨识速度），必须在 prompt 层面修正，不能只靠后期 PS 提亮（提亮会破坏当前已经调好的暖色调阶层次）。

**修正措辞（v1.4 → v1.4.1，在打光子句中插入/替换加粗部分）**：
```
...dramatic strong directional side lighting carving deep core shadow and a warm highlight along the brow, cheekbone, and armor ridge, **the face lit clearly enough that no more than one-third of the face falls into deep shadow, with a soft warm fill light preventing the eyes and expression from being obscured, overall composition kept at a mid-to-high key brightness suitable for small thumbnail display, avoiding an overly dark or near-silhouette result**, vivid {FACTION_COLOR_ACCENT} rendered as one bold continuous color block...
```

**负面提示新增（追加进 v1.4 负面清单）**：
```
overly dark image, low-key silhouette, face obscured by heavy shadow, underexposed portrait, dark vignette covering face, face lost in shadow
```

**结论：此修正是必须项，不是可选项**——所有后续出图（含重出这一轮、含未来 ~30 位核心武将批量出图）都应带上这条修正，否则整套武将头像库都会继承同样的缩略框辨识度问题。

**5.22.4 锁定结论**

**一句话结论：暂不锁定签名图 B。v1.4 模板方向本身已验证通过（鲜活度、表情克制、与 Koei 拉开距离三项全部达标），但需要先按 5.22.3 的修正措辞（v1.4.1）补出一轮，再从修正后的结果里选定签名图 B——不能直接把 `gy4-1` 或本轮任何一张锁进 `--sref`。**

理由：`--sref` 一旦锁定会把该签名图的"打光基调"作为 DNA 传播给后续全部武将出图；本轮 4 张里明度最好的 `gy4-1` 仍偏中低调，若现在锁定，缩略框辨识度问题会被系统性复制到整个武将库，届时靠单张返工无法解决。这与背景骨架当初"先修折痕负面提示，验证 `bg3-1` 全部十项过关后才锁"的处理方式一致（见 §7.9），保持流程标准统一。

`gy4-1` 判定为**本轮最佳、下一轮的构图/表情/铠甲基准参考**（三分侧脸角度、皱眉闭口的克制威压表情、红披帛大色块的清晰度均为四张最优），但**不作为签名图 B 正式锁定**。下一轮建议：用 v1.4.1（本节新增的打光修正措辞 + 负面提示新增项）重出 4 张，不带 `--sref`（因为还没有可锁的签名），气质槽与构图参考 `gy4-1`；若新一轮明度问题解决、且四项复核+家族相似性底线全部过关，则从中选定签名图 B 正式挂 `--sref` 锁定，登记进 §6。

### 5.23 第五轮人像出图评审（gy5-0 ~ gy5-3，v1.4.1 打光修正验证 + 定位"脸暗"根因）

> 本节记录用 §5.22.3 v1.4.1 措辞出的第五轮 4 张图（`gy5-0`~`gy5-3`），验证上一轮"背景/整体偏暗"问题是否解决，以及项目方复核后提出的新诊断——"暗的是脸本身，不是背景"。评审由美术总监亲自逐张目视核对，独立于项目方诊断进行；结论与项目方诊断**一致**，且能进一步定位到具体的 prompt 因果链。

**5.23.1 背景/整体明度问题——已解决**

4 张的纸本背景都恢复为暖米色/暖褐色调，视觉上不再有 §5.22.2 里 `gy4-3` 那种冷灰蓝侧板问题，画面整体也不再是 §5.22.3 里 `gy4-2` 那种近剪影式大面积死黑。v1.4.1 的"整体中高调 + 暗部不超过三分之一"这条修正在**背景层面和整体构图层面**确认生效。

**5.23.2 新问题定位——脸部正面平面明度不足，与整体明度是两回事**

复核确认项目方的诊断准确：`gy5-0`、`gy5-2`、`gy5-3` 三张里，脸部的额头中央/双颊/下颌这些**正面朝向的大面积皮肤**，色值仍落在偏深的枣红棕色调，只有眉骨、鼻梁、颧骨的窄条状受光边缘被提亮成橙色高光——换句话说，v1.4.1 修正的是"背景亮不亮"和"暗部占比"，但没有改变"脸这块颜色本身的基础明度"，也没有把主光源从"侧光/边缘光"改成"能照亮正面五官的正面光"。根因在 v1.4 原模板里两处叠加：① `deep red-toned weathered face` 这个描述词组，"deep"和"weathered"都在把关羽标志性红脸推向深枣红棕而非鲜亮朱红；② 打光子句仍以"dramatic strong directional side lighting"为主语，侧光天然只照亮脸的一条边，正面大面积仍留在自身固有色的中低明度里。这是两个独立可修的点，不是同一个问题的重复。

**逐张诊断**：

| 图 | 背景明度 | 脸部正面平面明度 | 其他问题 |
|---|---|---|---|
| `gy5-0` | 通过——暖米色纸本，边缘干净 | 未通过——额头/双颊仍是深枣红棕，只有眉骨鼻梁窄条高光 | 画风比前几轮更偏"平涂+粗黑轮廓线"的图形化处理，笔触颗粒感变弱，建议观察项（非硬性失败，但已接近"扁平插画"边界，需留意别滑向 v1.4 负面排除的"glossy/flat"方向） |
| `gy5-1` | **不通过**——整图被单一橙红色罩染，背景本身也失去中性暖纸本感 | **不通过**——脸与胡须、铠甲几乎融进同一橙红色调里，正面平面完全不可辨 | **`single-tone color scheme` 负面未压住，判定单色罩染失败**，直接排除候选池，问题与项目方判断一致 |
| `gy5-2` | 通过——暖米色纸本，可见纤维/画布纹理，四张里材质感最好 | 未通过，但四张里**最好**——额头/颧骨/鼻梁有明确中间调亮面，正面平面比另外三张更亮、五官最快可辨认 | 无 |
| `gy5-3` | 通过——暖褐色背景，略偏饱和橙但仍在纸本可接受范围 | 未通过——正面平面提亮幅度小于 `gy5-2` | 无 |

**结论：v1.4.1 的"背景/整体明度"修正已验证有效，但必须追加一条专门针对"脸部正面平面明度"的措辞，且要单独强化压制"单色罩染"负面——这是两条独立修正，不能合并成一条笼统的"再亮一点"。**

**5.23.3 v1.4.2 定向措辞**

**核心思路**：把"红"（色相）和"亮"（明度）当成两个独立维度分别描述，不再用"deep red-toned weathered face"这一个词组同时决定两者；打光子句从"以侧光/边缘光为主"改为"正面主光 + 侧光辅助"，明确要求正面五官被主光照亮。

**面部描述子句（替换原 `{IDENTITY_DESCRIPTOR}` 后紧跟的肤色描述）**：
```
a vivid warm vermillion-red weathered face — the red leaning toward a saturated bright crimson rather than a dark maroon-brown — with the frontal plane of the face (forehead, both cheeks, nose bridge, chin) kept at a clearly lit warm mid-tone, so the face reads as the single brightest warm-colored area of the entire composition at a glance
```

**打光子句（替换原 v1.4/v1.4.1 的打光段落）**：
```
a soft warm frontal key light clearly illuminating the eyes, brows, cheekbones, nose, and mouth so the facial features are instantly legible even at small thumbnail size, complemented by a secondary directional side/rim light along the silhouette edge and armor ridge for volume and drama — the frontal plane of the face must never drop below a clear mid-tone brightness, and the face must always read brighter than the surrounding armor and background at first glance, functioning as the composition's clear visual focal point
```

**负面提示新增（追加进 v1.4.1 负面清单，专门压单色罩染）**：
```
monochromatic orange wash, single-hue color grading, entire image tinted one color, orange color cast over whole image, sepia-orange filter effect, uniform color overlay, muddy dark red-brown skin tone, face same darkness as armor
```

**完整 v1.4.2 关羽示范 prompt（盛年·蜀汉·武将·傲物-威压）**：
```
A three-quarter portrait bust illustration of a middle-aged general with a long flowing beard, a vivid warm vermillion-red weathered face — the red leaning toward a saturated bright crimson rather than a dark maroon-brown — with the frontal plane of the face (forehead, both cheeks, nose bridge, chin) kept at a clearly lit warm mid-tone, so the face reads as the single brightest warm-colored area of the entire composition at a glance, painted in a bold hand-painted game character illustration style with confident visible brushwork and gouache-like layered color blocking (not glossy 3D render, not cel-shaded anime, not photographic, not flat vector illustration), a fierce commanding gaze with brows lowered in intense focus, jaw set firm, mouth closed in a stern hard-set expression, head held high with controlled aggression — projecting authority and battlefield presence, dignified rather than shouting, no open mouth, wearing dark lamellar armor with warm brocade trim built from historically grounded Han-Jin lamellar plates and layered cloth, a soft warm frontal key light clearly illuminating the eyes, brows, cheekbones, nose, and mouth so the facial features are instantly legible even at small thumbnail size, complemented by a secondary directional side/rim light along the silhouette edge and armor ridge for volume and drama — the frontal plane of the face must never drop below a clear mid-tone brightness, and the face must always read brighter than the surrounding armor and background at first glance, functioning as the composition's clear visual focal point, vivid deep crimson red rendered as one bold continuous color block on a single large accent area (a full cloak draped across one shoulder, or one wide diagonal chest sash) — a solid connected shape, not scattered thin cords or trim lines, with the main armor plates and helmet kept in a restrained bronze/iron/lacquer-black base tone drawn from the game's muted earth-mineral palette, warm weathered naturalistic skin with visible age and battle-worn texture, background is a plain aged rice-paper or silk parchment backdrop with soft vignette shading and visible paper fiber texture, no distinct scenery, no landscape, no mountains, no hills, no river, no horizon, character rendered with mature realistic human facial proportions, painterly illustration texture throughout, no text, no watermark, no signature, no modern elements, portrait crop from chest up, vertical composition --ar 3:4 --style raw --sref <SIGNATURE_B_v1.4_URL> --sw 300
```

**负面提示 v1.4.2（在 v1.4.1 基础上追加，新增项已加粗）**：
```
--no text, watermark, signature, logo, UI, name tag, health bar, red seal, cinnabar seal, ink chop stamp, hanko, official stamp, fake calligraphy, fake chinese characters, brush-written characters, decorative cursive script, illegible handwritten text, artist signature mark, decorative corner seal, chibi, hyper-realistic photo skin, monochrome sepia rendering, single-tone color scheme, glasses, weapon glow effects, distant landscape, mountains, hills, river, scenery background, open mouth shouting, screaming, roaring expression, anime large eyes, cel-shaded anime style, glossy 3D render look, plastic skin, chrome metallic reflection, trading card frame border, oversized fantastical horned helmet, exaggerated oversized pauldrons, rainbow multi-color costume, western medieval plate armor, european knight armor, Koei Three Kingdoms game art style, Dynasty Warriors style, mobile gacha game splash art style, overly dark image, low-key silhouette, face obscured by heavy shadow, underexposed portrait, dark vignette covering face, face lost in shadow, **monochromatic orange wash, single-hue color grading, entire image tinted one color, orange color cast over whole image, sepia-orange filter effect, uniform color overlay, muddy dark red-brown skin tone, face same darkness as armor**
```

**5.23.4 关羽"红脸"传统与"提亮脸部"是否冲突——不冲突，因为红度与明度是两个独立维度**

不冲突。"红脸"是**色相**（hue）——关羽的视觉符号是"脸是红色的"，这一点由 `vermillion-red` / `crimson` 这类偏朱红/绯红的色相词汇保证，v1.4.2 里刻意把色相词从"deep red-toned...maroon-brown"改成"vermillion-red...saturated bright crimson"，红度不但没减弱，反而更靠近戏曲脸谱式的鲜明朱红。"提亮"动的是**明度**（value/lightness）——一张脸完全可以同时是"高饱和度的红"和"中等偏亮的明度"，这在色彩理论里是两个正交维度，不存在互斥关系（例如鲜红色的苹果高光处，颜色依然是红的，只是那一块比暗部亮）。v1.4.2 的写法刻意把"红"和"亮"拆成两句话分别下指令，就是为了避免 MJ 把两者绑在一起理解成"越红就必须越暗"。因此可以两全：脸既保持关羽标志性的鲜明红色辨识度，也满足缩略框看得清五官的功能性要求。

**5.23.5 锁定结论**

**一句话结论：暂不锁定签名图 B，需按 v1.4.2 再出一轮。`gy5-2` 是本轮最佳、下一轮的构图/明度基准参考，但不直接锁定。**

理由与 §5.22.4 一致：`--sref` 会把锁定源的明度/配色 DNA 复制给后续全部武将出图。`gy5-2` 虽是本轮脸部最清楚的一张，但其正面平面明度仍未达到"缩略框一眼辨认五官"的标准，且这一缺陷（脸部固有明度不足）是这一轮才被精确定位出来的新问题，尚未验证修正措辞（v1.4.2）是否有效——按照"先验证修正生效，再锁定"的一贯流程（§7.9 背景锁定 / §5.22.4 人像上一轮均如此处理），本轮也应保持同样标准，不应因为"已经出到第五轮、时间压力"而放宽锁定门槛。`gy5-1` 因单色罩染问题，直接排除候选池，不作为参考。

下一轮建议：用 v1.4.2（本节面部描述子句 + 打光子句 + 负面提示新增项）重出 4 张，仍不带 `--sref`；重点复核脸部正面平面是否达到"比盔甲/背景更亮的视觉焦点"要求，且不再出现单色罩染。若通过，从中选定签名图 B 正式挂 `--sref` 锁定，登记进 §6。

### 5.24 第六轮人像出图评审（gy6-0 ~ gy6-3，v1.4.2 提亮验证 + 身份特征丢失诊断）

> 本节记录用 §5.23.3 v1.4.2 措辞出的第六轮 4 张图（`gy6-0`~`gy6-3`）。项目方反馈"脸不暗了，但基本上不是关羽了"。评审由美术总监亲自逐张目视核对，独立于项目方诊断进行；结论与项目方诊断**一致**，且额外发现一处项目方诊断未提到的新问题（`gy6-0` 背景材质彻底丢失）。

**5.24.1 逐张复核——提亮已达标，身份特征全面丢失**

| 图 | 明度/正面平面提亮 | 黑长髯 | 黑发 | 红脸/东亚肤色 | 背景材质 |
|---|---|---|---|---|---|
| `gy6-0` | 通过——脸是全场最亮的暖色区域，五官清晰 | **不通过**——花白/灰色短须，齐胸浓密黑髯没了 | **不通过**——灰白色头发 | **不通过**——偏浅橙棕色皮肤，不再是关羽标志性的红脸 | **不通过**——纯色平涂灰绿背景，完全没有纸纹/暖纸本材质，比"偏冷灰"更严重，是彻底丢失纸本材质身份 |
| `gy6-1` | 通过——脸部明度最亮，五官最清楚 | **不通过**——花白短须，且整体气质接近西式伯爵/吸血鬼贵族造型 | **不通过**——银灰头发 | **不通过**——偏浅橙肤色，五官轮廓（深目高鼻、瘦长脸型）读起来更像欧洲贵族而非东亚武将 | **不通过**——深灰绿色矩形色块居中，两侧留白形成明显双色分栏，卡片化苗头明显回归 |
| `gy6-2` | 通过——脸部明度达标 | **不通过**——花白短须 | **不通过**——灰白头发 | **不通过**——偏浅橙棕肤色 | **不通过**——深褐色矩形色块居中，两侧米白色留白，双色分栏 |
| `gy6-3` | 通过——脸部明度达标 | **不通过**——花白短须（比另外三张略长，但仍非"齐胸浓密黑髯"） | **不通过**——银白头发 | **不通过**——偏浅橙棕肤色 | **不通过**——深灰绿色矩形色块居中，两侧米白色留白，双色分栏 |

**结论：v1.4.2 的提亮修正本身完全生效**（四张脸部明度、正面平面亮度、五官辨识度全部达标，问题不在"亮不亮"），**但关羽的四个核心身份特征——黑长髯/黑发/红脸(东亚肤色)/背景纸本材质——全数丢失**，且四张里有三张（`gy6-1`/`2`/`3`）背景回归了 §7.10.3 刚诊断过的"双色分栏/卡片化"问题，`gy6-0` 则是另一种更严重的背景失效（纯色平涂，无材质感）。项目方"基本上不是关羽了"的反馈准确。

**5.24.2 根因诊断——提亮牵动了从未被硬锚的身份变量**

同意项目方的诊断：v1.4/v1.4.1/v1.4.2 三代模板里，"黑长髯"、"黑发"、"东亚人种"这几个身份特征**从来没有被显式写进 prompt**——早几轮之所以"看起来还是关羽"，是因为模板整体色调偏暗偏红，MJ 在"低明度+红棕基调"的语境下**顺带**把须发也渲染成深色，人种特征也没有被强光凸显出来，属于"蒙对"而非"锁定"。v1.4.2 把肤色描述从`deep red-toned...maroon-brown`改成`vermillion-red...bright crimson`并加了"正面平面必须明显提亮"的指令后，MJ 找到的解决方案是**换一套它训练数据里"提亮的古典胡须老者"更常见的组合**——花白/银发须+浅橙肤色+深邃西式五官（这类组合在 MJ 的训练分布里大量存在，例如奇幻游戏的贵族/伯爵老年男性角色），而不是"东亚人红脸黑髯提亮版"这个更窄的组合。这不是同一根因的延续，而是修正提亮时暴露了一个**从未被处理过的、独立的身份锚点缺失问题**。

**5.24.3 v1.4.3 修正措辞**

**核心思路**：在保留 v1.4.2 已验证有效的"正面平面提亮"指令基础上，新增三个必须显式锚定的身份变量——① 东亚汉人男性；② 齐胸浓密黑长髯（黑色，不可花白/灰白）；③ 黑发（不可灰白/银色）；同时把肤色描述里"提亮"和"不能变西式浅肤"这两件事拆开写清楚，避免 MJ 把"更亮"直接联想成"更浅/更白"。背景则追加"必须是连续单一暖纸本色调，不允许双色分栏/色块分割"的显式禁止指令，并在负面提示里补强对应词汇。

**面部与身份描述子句（替换 v1.4.2 对应段落）**：
```
a middle-aged East Asian Han Chinese general with a long, thick, jet-black beard flowing down past the chin to the chest, jet-black hair swept back and neatly tied, a vivid warm ruddy reddish-bronze East Asian complexion — the red leaning toward a saturated bright crimson rather than a dark maroon-brown, with the frontal plane of the face (forehead, both cheeks, nose bridge, chin) kept at a clearly lit warm mid-tone so the face reads as the single brightest warm-colored area of the entire composition at a glance — the brightening must only raise the value/lightness of this warm reddish-bronze East Asian skin tone, it must never shift the skin toward a pale, ashen, or light Caucasian complexion, and the hair and beard must remain a solid deep black, never grey, white, or silver
```

**背景约束子句（追加进背景描述段落，紧跟"aged rice-paper or silk parchment backdrop"之后）**：
```
the backdrop must be a single continuous warm paper tone across the entire frame with no color blocking, no vertical color panel, no two-tone split, no card-frame or poster-frame division
```

**负面提示新增（追加进 v1.4.2 负面清单，专门压制人种漂移与背景分栏）**：
```
grey hair, white hair, silver hair, aged white beard, short beard, trimmed goatee, stubble beard, pale caucasian skin, ashen skin tone, light western skin, western facial features, aquiline nose profile, gaunt european face, gothic vampire count aesthetic, victorian high collar, stiff upright gothic collar, european nobleman, two-tone split background, vertical color panel background, grey card background, color block background division, flat solid non-paper background, geometric panel background, poster color blocking background
```

**完整 v1.4.3 关羽示范 prompt（盛年·蜀汉·武将·傲物-威压）**：
```
A three-quarter portrait bust illustration of a middle-aged East Asian Han Chinese general with a long, thick, jet-black beard flowing down past the chin to the chest, jet-black hair swept back and neatly tied, painted in a bold hand-painted game character illustration style with confident visible brushwork and gouache-like layered color blocking (not glossy 3D render, not cel-shaded anime, not photographic, not flat vector illustration), a fierce commanding gaze with brows lowered in intense focus, jaw set firm, mouth closed in a stern hard-set expression, head held high with controlled aggression — projecting authority and battlefield presence, dignified rather than shouting, no open mouth, wearing dark lamellar armor with warm brocade trim built from historically grounded Han-Jin lamellar plates and layered cloth, a vivid warm ruddy reddish-bronze East Asian complexion — the red leaning toward a saturated bright crimson rather than a dark maroon-brown, with the frontal plane of the face (forehead, both cheeks, nose bridge, chin) kept at a clearly lit warm mid-tone so the face reads as the single brightest warm-colored area of the entire composition at a glance — the brightening must only raise the value/lightness of this warm reddish-bronze East Asian skin tone, it must never shift the skin toward a pale, ashen, or light Caucasian complexion, and the hair and beard must remain a solid deep black, never grey, white, or silver, a soft warm frontal key light clearly illuminating the eyes, brows, cheekbones, nose, and mouth so the facial features are instantly legible even at small thumbnail size, complemented by a secondary directional side/rim light along the silhouette edge and armor ridge for volume and drama — the frontal plane of the face must never drop below a clear mid-tone brightness, and the face must always read brighter than the surrounding armor and background at first glance, functioning as the composition's clear visual focal point, vivid deep crimson red rendered as one bold continuous color block on a single large accent area (a full cloak draped across one shoulder, or one wide diagonal chest sash) — a solid connected shape, not scattered thin cords or trim lines, with the main armor plates and helmet kept in a restrained bronze/iron/lacquer-black base tone drawn from the game's muted earth-mineral palette, warm weathered naturalistic skin with visible age and battle-worn texture, background is a plain aged rice-paper or silk parchment backdrop with soft vignette shading and visible paper fiber texture — the backdrop must be a single continuous warm paper tone across the entire frame with no color blocking, no vertical color panel, no two-tone split, no card-frame or poster-frame division, no distinct scenery, no landscape, no mountains, no hills, no river, no horizon, character rendered with mature realistic human facial proportions, painterly illustration texture throughout, no text, no watermark, no signature, no modern elements, portrait crop from chest up, vertical composition --ar 3:4 --style raw --sref <SIGNATURE_B_v1.4_URL> --sw 300
```

**负面提示 v1.4.3（在 v1.4.2 基础上追加，新增项已加粗）**：
```
--no text, watermark, signature, logo, UI, name tag, health bar, red seal, cinnabar seal, ink chop stamp, hanko, official stamp, fake calligraphy, fake chinese characters, brush-written characters, decorative cursive script, illegible handwritten text, artist signature mark, decorative corner seal, chibi, hyper-realistic photo skin, monochrome sepia rendering, single-tone color scheme, glasses, weapon glow effects, distant landscape, mountains, hills, river, scenery background, open mouth shouting, screaming, roaring expression, anime large eyes, cel-shaded anime style, glossy 3D render look, plastic skin, chrome metallic reflection, trading card frame border, oversized fantastical horned helmet, exaggerated oversized pauldrons, rainbow multi-color costume, western medieval plate armor, european knight armor, Koei Three Kingdoms game art style, Dynasty Warriors style, mobile gacha game splash art style, overly dark image, low-key silhouette, face obscured by heavy shadow, underexposed portrait, dark vignette covering face, face lost in shadow, monochromatic orange wash, single-hue color grading, entire image tinted one color, orange color cast over whole image, sepia-orange filter effect, uniform color overlay, muddy dark red-brown skin tone, face same darkness as armor, **grey hair, white hair, silver hair, aged white beard, short beard, trimmed goatee, stubble beard, pale caucasian skin, ashen skin tone, light western skin, western facial features, aquiline nose profile, gaunt european face, gothic vampire count aesthetic, victorian high collar, stiff upright gothic collar, european nobleman, two-tone split background, vertical color panel background, grey card background, color block background division, flat solid non-paper background, geometric panel background, poster color blocking background**
```

**5.24.4 §9.4 合规说明**

以上身份锚点全部使用可描述的特征词（"East Asian Han Chinese"、"jet-black beard flowing to the chest"、"jet-black hair"），未点名任何真人历史人物姓名或具体艺术家/作品，符合 §9.4 特征描述而非专名指代的要求；"关羽"仅作为内部气质原型代号，不进入 prompt 正文，这一惯例本轮未改变。

**5.24.5 锁定结论**

**一句话结论：暂不锁定签名图 B，需按 v1.4.3 再出一轮验证身份特征是否稳定回归。** 本轮 4 张全部因身份特征丢失被判不通过，没有任何一张可作为候选，是目前六轮评审里第一次出现"全部排除、无最佳可参考"的情况——`gy6-0`~`gy6-3` 仅作为"提亮修正确认生效"的技术性验证留档，不作为下一轮的构图/表情参考（该职能仍由 `gy4-1`/`gy5-2` 承担）。下一轮建议：用 v1.4.3 重出 4 张，仍不带 `--sref`；重点复核黑长髯/黑发/东亚肤色三项身份锚点是否稳定出现，以及背景是否回到连续暖纸本单一色调、不再出现双色分栏。若通过且此前已验证的鲜活度/表情/铠甲/与 Koei 拉开距离/明度五项同时保持达标，才可正式选定签名图 B 锁定 `--sref`。

---

### 5.25 第七轮人像出图评审（gy7-0 ~ gy7-3，v1.4.3 身份锚点验证 + 最终锁定裁定）

项目方用 v1.4.3 出了 `gy7-0`~`gy7-3`，仍不带 `--sref`。美术总监逐张独立 Read 复核，结论如下。

**5.25.1 四项复核逐张打勾**

| 检查项 | gy7-0 | gy7-1 | gy7-2 | gy7-3 |
|---|---|---|---|---|
| ① 身份（黑长髯及胸／黑发／东亚红脸） | 通过——黑长髯及胸、黑发束髻，但脸部红润饱和度弱于 7-1/7-2，偏暖棕调 | **通过，本轮最强**——黑长髯最浓密、正面视角下红润饱和度最高、五官东亚特征最清晰 | 通过——黑长髯及胸、黑发束髻，脸部红润饱和度接近 7-1 | 通过（勉强）——黑长髯可辨，但扁平色块渲染下脸色偏橙黄，"红脸"辨识度是四张里最弱的 |
| ② 亮度（脸为最亮焦点、缩略框可辨） | 通过——三分侧脸，受光面清楚，但侧转角度较大，暗侧稍多 | **通过，本轮最强**——正面视角，脸部对比度和清晰度全场最佳 | 通过——三分侧脸，额头/颧骨/鼻梁高光清楚，对比度强 | 通过——高对比平涂让五官轮廓在缩略尺寸下依然醒目，但这是"图形对比度"而非"暖色调受光"带来的可辨识度 |
| ③ 画风（厚涂克制、非 cel、非 Koei/无双） | 通过——可见笔触，柔和过渡 | 通过——可见笔触，肩甲一处团花纹样略精致化，列为观察项（同 §5.22.1 头盔冠饰先例，尚未越线） | 通过——笔触感与图形化色块之间取得平衡，仍读作厚涂插画 | **偏弱**——四张里最扁平/矢量化，色块边缘锐利、缺乏笔触混合，风格滑向"扁平矢量海报插画"，与负面提示排除的"cel-shaded/oversized flat poster art"风险相邻，判定为本项四张最弱 |
| ④ 背景（暖纸本、无双色分栏） | **不通过**——纯灰色平涂，无暖色调、无纸纹，与 §5.22.2 曾判定 `gy4-3` 出局的"冷灰背景破坏纸本基调"是同一类失效 | **不通过**——浅灰/近白平涂，暖度极弱，同样判定为破坏纸本材质家族底线 | 通过——暖米黄/暖褐平涂，色相明确落在纸本暖色区间（虽无可见纤维纹理，但历轮判例里色相符合即视为过关，纤维颗粒感是加分项非硬性项） | 通过——暖褐平涂，色相同样落在纸本暖色区间 |

**5.25.2 家族相似性底线复核（回应"平灰背景算不算破底线"的关键取舍问题）**

**裁定：`gy7-0`/`gy7-1` 的平灰背景确认破坏"纸/绢本背景材质"这条家族相似性底线，属于硬性排除项，不是可以酌情放行的软性瑕疵。** 理由与 §5.22.2 对 `gy4-3` 的裁定完全一致、是同一规则的重复适用：家族相似性底线里的"纸/绢本背景材质"一项检验的是"这张图是否仍在同一个纸感世界里"，色相判定标准是"暖"而非"亮/暗"或"干净/杂乱"——中性灰或偏冷灰都不满足"暖纸本"要求，无论人脸画得多好、身份特征多准，背景材质一旦跳出纸感世界，就不能作为 `--sref` 的锁定源，因为 `--sref` 会把背景材质基因一并传播给后续全部 ~30 将出图，届时全库背景会系统性偏离"活图卷同一纸感世界"的核心视觉锚点。

这与"这张图本身好不好看"是两个独立问题：`gy7-1` 在①②两项（身份辨识度/正面亮度）确实是本轮乃至七轮以来的最佳单张，项目方判定的"神似最佳"经复核成立，但这不能豁免④背景材质这一条硬性底线——正如 §5.22.2 对 `gy4-1`（当轮最佳表情/构图）与 `gy4-3`（因背景出局）的处理方式一样，"最佳"和"可锁定"是两把不同的尺子。

**结论：`gy7-0`/`gy7-1` 排除出签名候选池**（`gy7-1` 判定为"表情/正面亮度的最佳参考留档"，供未来任何一轮微调时对照，但不作为锁定对象）。`gy7-2`/`gy7-3` 因背景通过，进入候选比较。

**5.25.3 `gy7-2` 与 `gy7-3` 的最终取舍**

`gy7-2` 与 `gy7-3` 在①②④三项均通过，差异集中在③画风一项：`gy7-2` 保持了"可见厚涂笔触"的插画质感，`gy7-3` 明显更扁平/矢量化、色块边缘锐利，风险是滑向"扁平海报插画/游戏图标风"这一相邻但同样需要避免的品类（负面提示里虽未逐字列出"flat vector poster"，但其精神与已排除的"cel-shaded anime style / mobile gacha game splash art style"一致——都是"过度光滑/图形化、缺乏历史厚重笔触感"的同类问题）。`gy7-2` 因此判定为四张里**唯一四项全部无保留通过**的候选，`gy7-3` 因画风项偏弱不作为锁定对象（可留作"高对比缩略框可辨识度"的技法参考，非锁定候选）。

**5.25.4 §9.4 原创复核**

对照 Koei/三国无双公开关羽立绘：`gy7-2` 为哑光厚涂色块 + 克制矿物色系 + 汉晋札甲廓形（无夸张龙纹/镀金浮雕/巨大肩甲），与 Koei 惯用的高光滑赛璐璐渲染+华丽镶金纹饰+夸张比例戎装在技法和视觉密度上均可清楚区分，判定**不神似**，通过 §9.4 复核。

**5.25.5 最终锁定裁定**

**一句话结论：锁定 `gy7-2` 为签名图 B，正式进入 `--sref` 锁定阶段，不需要再出一轮。** 这是七轮评审以来第一次出现"四项复核 + 家族相似性底线 + §9.4 原创复核"全数无保留通过、且没有任何缺陷需要靠下一轮修正的候选——按 `--sref` 锁定门槛"这张图的任何缺陷是否会被系统性复制到全部后续资产"检验，`gy7-2` 没有需要担心被复制的缺陷，没有必要为了换取 `gy7-1` 的正面角度/更强红脸饱和度而再赌一轮（`gy7-1` 唯一的问题是背景材质，属于确定性缺陷而非概率性瑕疵，重出一轮不保证解决且徒增一轮时间成本）。

**推进步骤**：
1. 对 `gy7-2` 执行 Upscale，取图片 URL。
2. 登记进 §6 素材登记表"签名图 B"行，标记为"通过，已锁定"。
3. 后续 §5.5 的 5 个招牌气质原型示范（关羽/诸葛亮/曹操/吕布/司马懿代号）与核心 ~30 将出图，一律追加 `--sref <gy7-2 的 Upscale URL> --sw 300`（沿用 §5.6/§5.21 已定义的权重建议）。
4. 首批挂 `--sref` 出图后，建议先出 3-5 张做"风格是否稳定复现"的抽查（重点复查：新出的将领是否仍保持黑发/东亚肤色等**这些本来就该因人而异的具体特征会不会被 `--sref` 过度锁死**——`--sref` 锁的是画风/材质/光照基因，不是五官本身，需要在抽查时确认新角色的脸型/发型/须型能正常按各自人物设定变化，不会被拉向"gy7-2 本人"的长相，这是风格参考图锁定的常见新手陷阱，抽查通过后再批量铺开）。

**5.25.6 背景修正复议：能否用暖纸底修正版 `gy7-1` 替换 `gy7-2`（项目方复议请求）**

项目方在看到 §5.25.5 结论后表态更偏好 `gy7-1` 的神韵（本节复核也确认 `gy7-1` 是①②两项本轮最强），并提出：既然否决 `gy7-1` 的唯一理由是背景材质，而背景是可单独替换的一层，能否修掉背景后改锁 `gy7-1`。

**裁定 1——方案是否成立：成立，原则同意，但不是"直接改锁"，而是"有条件重新进入候选"。** 否决 `gy7-1` 的理由（平灰背景破坏纸本材质家族相似性底线）是一个**局部材质层面的、可分离修正的缺陷**，不是身份/构图/表情这类烧录在人物主体像素里、无法单独替换的缺陷（对比：round 6 的身份漂移是须发/肤色/骨相全局性问题，无法只换一层解决；这次是背景单一图层问题，性质不同）。因此背景修正后的 `gy7-1` **理论上可以解除背景这一项的否决**，四项复核里的①②③三项本来就已经通过，问题只剩④——修正成立后，`gy7-1` 在纯粹的"候选质量"上确实优于 `gy7-2`（更强的正面亮度与红脸辨识度）。但这不代表可以跳过验证直接改锁：**必须先看到修正后的实际成品图，重新走一遍 §5.25.1 的四项复核 + §5.25.2 家族相似性底线，确认修正没有带来新的问题（换背景常见的边缘残留/色温不符/接缝），通过后才能正式改锁**，这与本项目一贯的"先验证、后锁定"标准一致，不能因为方案听起来合理就跳过验证环节。

**裁定 2——Vary (Region) vs PS，建议 Vary (Region) 为首选、PS 为兜底，与项目方的优先级判断一致，补充理由**：`gy7-1` 的头发/鬓角有多缕飘散的散发丝，边缘与灰背景直接相邻、形态细碎不规则——这种"毛发丝边缘"在 PS 里手动抠图/羽化极难做到无痕迹（散发丝一旦被硬边蒙版切断，会出现"剪影感"或"毛刺锯齿"，比背景本身的瑕疵更显眼），而 Vary (Region) 是同一生成引擎在同一像素空间内重新生成蒙版区域，对散发丝这类不规则边缘的过渡通常更自然、不会有"贴图感"。建议**首选 Vary (Region)**，蒙版框选人物轮廓外的全部灰色区域（含散发丝穿过灰底的部分留一点重叠余量，让 MJ 自己处理毛发与新背景的融合），prompt 只留背景描述子句（`single continuous warm rice-paper parchment backdrop, paper fiber texture, soft vignette, no scenery, no split panel, no letterbox` 一类，不需要重复人物描述）；**若 Vary (Region) 出现本项目此前反复诊断过的失效模式**（画框化/双色分栏/伪印章等，见 §7.10.3/§5.24），或者散发丝边缘出现明显新接缝，**再退到 PS 兜底**，PS 兜底时建议直接复用已锁定的 `bg3-1` 或 `zm-0` 纸纹素材做背景纹理来源，保证材质血统与已锁定背景族一致（不要另找新的纸纹图库，减少色相/颗粒不一致的风险）。

**裁定 3——验收标准（一句话）**：**修正后的 `gy7-1` 背景色相/明度/颗粒感必须与已锁定的背景骨架签名（`bg3-1`）及 `gy7-2` 的暖纸本基调并排目视比对"看得出是同一个纸感世界"**（同 §5.22.2/§7.10.3 一贯采用的并排比对方法），具体核对点包括：暖米黄至暖褐色相区间、无残留灰色光晕或色温断层、发丝与背景交界处无锐利接缝或色差、且不重新出现双色分栏/画框化/letterbox 等已诊断过的失效模式——四条中任意一条不满足，判定修正未通过，退回用 `gy7-2` 或再修一版。

**裁定 4——登记表处理：本次不直接把 §6 改成"`gy7-1` 修正版已锁定"，因为修正后的成品图尚未生成/尚未复核。** 处理方式：
- `gy7-2` 的锁定状态**暂时保留、不撤销**，作为"当前安全兜底签名"——若项目方希望在等待 `gy7-1` 修正版的同时不耽误进度，理论上可以先用 `gy7-2` 小批量试出 3-5 张核心武将验证 `--sref` 流程本身是否顺畅（技术验证，非最终定稿），但**不建议现在就大批量铺开 ~30 将**，因为签名图一旦大批量套用后再更换，之前出的图全部要按新签名重出，代价远高于现在多等一轮修正结果。
- §6"签名图 B"行状态改为"**已锁定 `gy7-2`（技术性可用），`gy7-1` 暖纸底修正版复议中，待修正成品图验证通过后可能替换**"，避免在最终人选未决时误导后续执行者直接开工大批量出图。
- 待项目方提交修正后的 `gy7-1` 成品图，按裁定 1-3 的流程复核，若通过则正式改锁、更新 §6 与本节结论；若不通过，则维持 `gy7-2` 为最终签名图 B，`gy7-1` 修正版作为技法参考存档。

---

### 5.26 方向更新：引入头饰槽（绿幞头）+ v1.4.4 模板 + 阵营色裁定（§5.25.6 复议路线作废）

项目方提出新方向：关羽经典形象须戴**绿色幞头/头巾**，v1.4.3（`gy7` 批）是光头束髻、没有头冠，这是一个此前所有轮次都没有显式处理过的缺失维度。项目方同时指认**第四轮 `gy4-3` 为全程最神似关羽的一张**，要求以它的头巾造型/表情/红脸/构图为目标出 v1.4.4。本节记录美术总监对 `gy4-3` 的独立复核、v1.4.4 修正设计、完整 prompt，以及阵营色裁定。

**5.26.1 `gy4-3` 独立复核（`scratchpad/round4/gy4-3.png`）**

美术总监重新 Read 该图确认：**软质深色幞头**（布质头巾，非硬质盔冠，两侧有布带垂落，束发于头巾内）+ **红脸**（右半侧脸受暖橙光照亮，左半侧落入较深阴影）+ **及胸长髯**（深色，偏黑但带一点灰绿色调，不算纯正的高饱和黑）+ **皱眉威严的表情**+ **红色斜披**大色块 + 深色甲胄。背景两侧确认是偏冷的浅灰蓝色调，头部正后方有一小块暖褐色调，整体确实呈现"中央暖、两侧冷"的双色分栏雏形，与项目方描述一致。**复核结论：项目方对 `gy4-3` 三项缺陷（脸偏暗、背景双色分栏冷灰、幞头是黑非绿）的诊断准确，予以采纳作为 v1.4.4 的修正目标；`gy4-3` 的头巾造型/威严表情/构图角度确认是全程最贴近"经典关羽"神韵的参考基准。**

**5.26.2 v1.4.4 修正设计**

在 v1.4.3（§5.24.3，身份锚点：东亚汉人/黑长髯/黑发/红润东亚肤色/暖纸本背景）基础上，做以下变更：

1. **新增头饰槽 `{HEADWEAR}`，替换 v1.4.3 的"黑发束髻"裸头描述**——关羽的槽值：软质深绿色幞头（布质头巾，两侧垂带，束发于内），发色仍为黑色（写在头巾描述里，避免头巾遮住头发后"黑发"锚点失去可见验证载体）。
2. **沿用 v1.4.2 提亮成果**——正面平面明度子句、"脸是全场最亮暖色焦点"子句原样保留，不改动（治 `gy4-3` 的脸暗问题）。
3. **沿用 v1.4.3 背景约束子句**——单一连续暖纸本色调、禁止双色分栏/画框化，原样保留（治 `gy4-3` 的冷灰蓝分栏问题）。
4. **沿用 v1.4.3 身份锚点**——东亚汉人、及胸浓密黑长髯、红润枣红/古铜东亚肤色（提亮不带向苍白西式），原样保留。
5. **沿用 v1.4 画风/表情/铠甲/阵营色块子句**——厚涂非cel、威严闭口非咆哮、汉晋铠甲、红色斜披/斗篷单块连续色斑，原样保留。
6. **新增头饰专属负面提示**：防止 MJ 把软头巾画回黑色（惯性来自 `gy4-3`/`gy6` 系列）、防止把软头巾画成硬质头盔/王冠、防止绿色过饱和/过大面积压过阵营红。

**5.26.3 完整 v1.4.4 关羽 prompt + 负面**

**头饰槽描述子句（替换 v1.4.3 中"jet-black hair swept back and neatly tied"一句）**：
```
wearing a soft dark jade-green cloth head-wrap (a futou-style soft fabric scarf tied at the back of the head, with two soft cloth ribbons draped down beside the temple, echoing a traditional soft cloth headwrap silhouette — not a rigid metal helmet, not an ornate crown, not a pointed spired helm), the head-wrap rendered in a muted, cool-leaning deep jade/malachite green (a blue-leaning green, never a yellow-green or lime-green hue), the green kept visibly smaller in area and lower in saturation than the red faction accent block so it reads as a secondary personal identifying color rather than competing with the main faction color, with jet-black hair neatly gathered underneath the head-wrap (a few black strands may be visible at the edge of the wrap to confirm the hair color)
```

**完整 v1.4.4 关羽示范 prompt（盛年·蜀汉·武将·傲物-威压）**：
```
A three-quarter portrait bust illustration of a middle-aged East Asian Han Chinese general with a long, thick, jet-black beard flowing down past the chin to the chest, wearing a soft dark jade-green cloth head-wrap (a futou-style soft fabric scarf tied at the back of the head, with two soft cloth ribbons draped down beside the temple, echoing a traditional soft cloth headwrap silhouette — not a rigid metal helmet, not an ornate crown, not a pointed spired helm), the head-wrap rendered in a muted, cool-leaning deep jade/malachite green (a blue-leaning green, never a yellow-green or lime-green hue), the green kept visibly smaller in area and lower in saturation than the red faction accent block so it reads as a secondary personal identifying color rather than competing with the main faction color, with jet-black hair neatly gathered underneath the head-wrap (a few black strands may be visible at the edge of the wrap to confirm the hair color), painted in a bold hand-painted game character illustration style with confident visible brushwork and gouache-like layered color blocking (not glossy 3D render, not cel-shaded anime, not photographic, not flat vector illustration), a fierce commanding gaze with brows lowered in intense focus, jaw set firm, mouth closed in a stern hard-set expression, head held high with controlled aggression — projecting authority and battlefield presence, dignified rather than shouting, no open mouth, wearing dark lamellar armor with warm brocade trim built from historically grounded Han-Jin lamellar plates and layered cloth, a vivid warm ruddy reddish-bronze East Asian complexion — the red leaning toward a saturated bright crimson rather than a dark maroon-brown, with the frontal plane of the face (forehead, both cheeks, nose bridge, chin) kept at a clearly lit warm mid-tone so the face reads as the single brightest warm-colored area of the entire composition at a glance — the brightening must only raise the value/lightness of this warm reddish-bronze East Asian skin tone, it must never shift the skin toward a pale, ashen, or light Caucasian complexion, and the beard must remain a solid deep black, never grey, white, or silver, a soft warm frontal key light clearly illuminating the eyes, brows, cheekbones, nose, and mouth so the facial features are instantly legible even at small thumbnail size, complemented by a secondary directional side/rim light along the silhouette edge and armor ridge for volume and drama — the frontal plane of the face must never drop below a clear mid-tone brightness, and the face must always read brighter than the surrounding armor and background at first glance, functioning as the composition's clear visual focal point, vivid deep crimson red rendered as one bold continuous color block on a single large accent area (a full cloak draped across one shoulder, or one wide diagonal chest sash) — a solid connected shape, not scattered thin cords or trim lines, with the main armor plates kept in a restrained bronze/iron/lacquer-black base tone drawn from the game's muted earth-mineral palette, warm weathered naturalistic skin with visible age and battle-worn texture, background is a plain aged rice-paper or silk parchment backdrop with soft vignette shading and visible paper fiber texture — the backdrop must be a single continuous warm paper tone across the entire frame with no color blocking, no vertical color panel, no two-tone split, no card-frame or poster-frame division, no distinct scenery, no landscape, no mountains, no hills, no river, no horizon, character rendered with mature realistic human facial proportions, painterly illustration texture throughout, no text, no watermark, no signature, no modern elements, portrait crop from chest up, vertical composition --ar 3:4 --style raw --sw 300
```

> 说明：本轮起不再挂 `--sref`（签名图 B 尚未最终定稁，见 §5.26.5），出图目标是先验证 v1.4.4 模板本身；一旦本轮选出干净候选，才升采样挂 `--sref` 正式锁定。

**负面提示 v1.4.4（在 v1.4.3 基础上追加，新增项已加粗）**：
```
--no text, watermark, signature, logo, UI, name tag, health bar, red seal, cinnabar seal, ink chop stamp, hanko, official stamp, fake calligraphy, fake chinese characters, brush-written characters, decorative cursive script, illegible handwritten text, artist signature mark, decorative corner seal, chibi, hyper-realistic photo skin, monochrome sepia rendering, single-tone color scheme, glasses, weapon glow effects, distant landscape, mountains, hills, river, scenery background, open mouth shouting, screaming, roaring expression, anime large eyes, cel-shaded anime style, glossy 3D render look, plastic skin, chrome metallic reflection, trading card frame border, oversized fantastical horned helmet, exaggerated oversized pauldrons, rainbow multi-color costume, western medieval plate armor, european knight armor, Koei Three Kingdoms game art style, Dynasty Warriors style, mobile gacha game splash art style, overly dark image, low-key silhouette, face obscured by heavy shadow, underexposed portrait, dark vignette covering face, face lost in shadow, monochromatic orange wash, single-hue color grading, entire image tinted one color, orange color cast over whole image, sepia-orange filter effect, uniform color overlay, muddy dark red-brown skin tone, face same darkness as armor, grey hair, white hair, silver hair, aged white beard, short beard, trimmed goatee, stubble beard, pale caucasian skin, ashen skin tone, light western skin, western facial features, aquiline nose profile, gaunt european face, gothic vampire count aesthetic, victorian high collar, stiff upright gothic collar, european nobleman, two-tone split background, vertical color panel background, grey card background, color block background division, flat solid non-paper background, geometric panel background, poster color blocking background, **black head-wrap, dark navy head-wrap, plain black scarf, rigid metal helmet, ornate metal crown, pointed spired helm, samurai-style helmet, bare head with no headwear, neon green, bright saturated lime green, yellow-green hue, oversized green area covering torso, green covering more than half the composition, green as the dominant color of the image**
```

**5.26.4 阵营色裁定：绿幞头是否与 §5.18"阵营色单块"规则冲突**

**裁定：不冲突，绿幞头判定为合法的"个人专属识别色"，但从属于阵营色单块规则，不是并列的第二阵营色。** 已写入 `design/art/art-bible.md` §3.1 补充裁定（三），核心结论：

1. **区分"缺陷/风险"与"层级从属"**：阵营色（蜀汉赤金朱）与人物专属色（关羽绿）不是互相竞争同一功能位的两个色块，而是两个不同层级的识别信息——阵营色负责"缩略框一眼辨认阵营"（最高优先级、最大面积、最高饱和度），人物专属色负责"熟悉角色后一眼认出具体是谁"（次级、面积更小、饱和度更低）。两者功能不同，不构成冲突，只要保证层级关系（专属色永远从属于阵营色）不被打破。
2. **落地方式已写进 v1.4.4 prompt**：头饰天然面积小于斜披/斗篷这类大色块（构图上容易满足"面积更小"）；饱和度通过明确指定"muted, cool-leaning deep jade/malachite green"（而非鲜绿/荧光绿）主动压低，并在负面提示里排除"neon green / oversized green area / green as dominant color"等失控情形。
3. **色相选择避开保留色相段**：色板设计原则里 H60–100（黄绿）、H270–310（紫）是预留给未来势力（董卓/袁绍/刘表等）的色相段。关羽绿巾指定为**偏冷的深玉/孔雀石绿（约 H150–170，蓝调绿）**，一是符合传统关公"绿巾"戏曲/年画视觉惯例本身就是冷调绿而非黄绿，二是不会提前占用未来势力的保留色相段，避免日后新势力上线与关羽个人色相"撞色"。
4. **通用影响（头饰槽对其他武将的意义）**：头饰槽是逐角色可配置项，**默认值**是现有裸头束发描述（不引入第二色），只有像关羽这种**具备广为人知的个人专属头饰/专属色的历史原型**才会在头饰槽里写入自定义颜色，且必须遵守本条"从属于阵营色"的层级规则（面积更小、饱和度更低、避开预留色相段）。这不是把"每个武将都可以有专属色"变成默认规则，而是给"极少数确有强烈个人图腾色彩的角色"开一条有约束条件的例外通道，多数武将（包括大部分蜀汉武将）仍应遵循"阵营色是唯一点缀色"的默认设计，避免整个武将库因为逐个加专属色而普遍削弱阵营辨识度。

**5.26.5 锁定路径影响：`gy7-1` 复议路线作废，`gy7-2` 维持技术性备选**

由于关羽的经典形象必须有绿幞头，而 `gy7-1`（及整个 `gy7` 批）是裸头束发、没有任何头冠，**§5.25.6 提出的"修正 `gy7-1` 背景后改锁"路线现予作废**——即便把 `gy7-1` 的背景修成暖纸本，它仍然缺少头饰这一关键身份维度，不再是"只差背景"的候选，而是"整体需要重出"。`gy7-2` 保持 §5.25.6 已定的"技术性可用、不建议大批量铺开"状态，作为 v1.4.4 验证期间的安全兜底，不因本次方向更新而撤销或提级。

**下一步**：用 v1.4.4（不带 `--sref`）出一轮关羽候选，重点复核：① 绿幞头是否稳定呈现（颜色/软质头巾造型/面积-饱和度从属关系）；② `gy4-3` 已验证的神韵（威严表情/构图/及胸黑髯）是否保留；③ v1.4.2 提亮、v1.4.3 身份锚点、v1.4.3 背景约束是否同时保持达标。若通过，**该轮选定的干净候选将同时承担"签名图 B"与"关羽原型（签名）"两个入库身份**（§6 两行合并指向同一 Upscale 源），不需要分别出图。

---

### 5.27 第八轮人像出图评审：绿幞头稳定呈现，但引入过度矫正（overcorrect），出 v1.4.5 再平衡

项目方用 v1.4.4 出了 `gy8-0~3`，逐张独立复核（`scratchpad/round8/gy8-0.png`~`gy8-3.png`）。

**5.27.1 逐张复核结论**

| 检查项 | gy8-0 | gy8-1 | gy8-2 | gy8-3 |
|---|---|---|---|---|
| 绿幞头造型/黑发 | 过（深绿软巾，束发） | 过 | 过 | 过（深绿软巾，色值最接近 `gy4-3` 目标） |
| 及胸黑长髯 | 过（纯黑） | **警示**：胡须尾端/髭须处出现明显灰白丝 | **不过**：髭须与须尾有清晰可见的灰白色调，比 gy8-1 更明显 | 过（纯黑，未见灰白） |
| 东亚汉人/表情威严闭口 | 过 | 过 | 过 | 过 |
| 红润东亚红脸 | **不过**：呈普通棕褐肤色，未见朱红/绯红 | **不过**：同上 | **不过**：同上 | **不过**：同上 |
| 蜀汉红披风/斜披大色块 | **不过**：画面完全无红色块，肩部/衣领是暗橄榄绿或灰绿色 | **不过**：同上 | **不过**：同上 | **不过**：同上（肩部露出一小块深橄榄灰） |
| 暖纸本背景 | **不过**：纯白背景，无纸纹/无暖色调 | **不过**：同上 | **不过**：同上 | **不过**：同上 |
| 画风（厚涂非cel/非扁平矢量） | **不过**：硬边黑色描线+大块平涂，偏漫画/cel | **不过**：同上，程度略轻 | **不过**：四张中最扁平/矢量化，边缘呈几何碎片状 | **接近过**：可见笔触堆叠/颜色层次，是四张里最接近厚涂 gouache 的一张，但仍不合格（无红/无纸底） |

**独立复核结论：项目方四点诊断（红脸丢失/红披消失/纯白背景/画风飘扁平）全部属实，逐张目视确认无异议。额外发现一项项目方未提及的新问题：`gy8-1`/`gy8-2` 的胡须尾端/髭须出现灰白色调残留（`gy8-2` 最明显）**——这是 v1.4.3 已锚定的"黑长髯，不可花白"规则出现的局部松动，虽不如第六轮的全面漂移严重（仅局部丝缕，不是整体变灰），但性质相同，需要在 v1.4.5 里一并加固负面提示。

**5.27.2 根因诊断（在项目方"措辞挤占"归因基础上追加更深一层解释）**

项目方的归因（大段绿巾措辞把整套服装/画面带偏）是正确的表层描述，独立复核后补充一个更具体的根因假说：v1.4.4 的头饰槽子句是一整段独立、详细描述服装构造细节的长句（材质/系带方式/飘带位置/色相禁忌等），这种"单独聚焦描述一件可穿戴道具的构造细节"的写法，与 MJ 训练分布里"角色设计参考图/角色设定稿（character design sheet / turnaround）"这一常见图像类型的 prompt 模式高度相似——而这类图默认惯例正好是：纯白/透明背景、扁平 cel 上色、以被强调描述的那件道具的颜色作为整张图的主色调、弱化面部肤色的戏剧性上色。也就是说，**四个症状（背景变白、画风变扁平、绿色扩散到全身、红脸红披被压制）很可能是同一个"图像类型误判"触发的连带效应，而不是四个独立的措辞失误**——头饰这段描述写得越像"产品/道具规格说明"，就越容易把整张图拖向"设定图"这个默认模板，而不是"游戏角色半身立绘"。这个诊断决定了 v1.4.5 的修正策略：**不能只是简单加长"红脸/红披/纸底"这几句的强调力度去和绿巾段落"拔河"，还需要同时缩短、收敛头饰段落本身的独立描述感**，把它写得更像一句从属描述而不是一段独立道具规格。

**5.27.3 v1.4.5 修正设计**

1. **头饰段落收紧收权**：改写为更短的从属句，去掉多重括注式的构造细节，明确追加"绿色严格限制在头部，绝不能扩散到肩部/躯干/服装"一句，直接切断"绿巾→全身绿袍"的联想链条。
2. **红脸与绿巾显式解耦**：紧跟头饰句之后，新增一句直接声明"尽管戴着绿头巾，脸依然是鲜明饱和的朱红东亚肤色——头巾颜色不得影响或稀释脸部的红色"，把两者作为两个独立色彩决策明确分开，防止模型把两种颜色信息"调和平均"成一种。
3. **红披阵营块显式夺回主导权**：在红披/斜披子句后追加"红色色块面积必须明显大于绿头巾面积，且始终是全画面里面积最大、饱和度最高的色块——绿头巾绝不能成为画面的主导色或最大色块"。
4. **暖纸底显式排除纯白**：背景子句后追加"绝不能呈现纯白色、灰白色或透明背景，绝不能呈现角色设定稿/转身图式的空白展示背景"。
5. **画风显式排除设定稿感**：画风子句后追加"具备可见的厚涂笔触与颜料层次，绝不能呈现扁平 cel 上色、硬边黑色描线漫画风格、或角色设定参考图/转身图式的呈现方式"。
6. **黑髯负面加固**：负面提示新增灰白胡须丝缕/斑驳/髭须发白等词，堵住本轮新发现的局部松动。
7. **保留**：绿幞头造型本身（futou 软巾+系带）、及胸黑长髯目标、东亚汉人身份锚点、威严闭口表情，均确认方向正确，不改动核心设定，只调整"权重分配"。

**5.27.4 完整 v1.4.5 关羽 prompt + 负面**

```
A three-quarter portrait bust illustration of a middle-aged East Asian Han Chinese general with a long, thick, jet-black beard flowing down past the chin to the chest, wearing a small soft dark jade-green futou-style cloth head-wrap tied at the back of the head with two short ribbons draped beside the temple (soft fabric, not a rigid metal helmet, not a crown, not a pointed spired helm), a muted cool-leaning deep jade/malachite green (never yellow-green or lime-green), the green strictly confined to the head area only — it must never spread onto the shoulders, torso, cloak, or garment, and stays clearly smaller in area and lower in saturation than the red faction accent block elsewhere in the composition, jet-black hair gathered underneath the wrap, and despite wearing this green head-wrap the face itself remains unmistakably a vivid saturated crimson-red East Asian complexion — the head-wrap color and the face color are two separate, independent color decisions, the green must never influence, dilute, or desaturate the red of the face, painted in a bold hand-painted game character illustration style with confident visible loaded brushstrokes and layered gouache-like painterly texture throughout — this must not resemble a flat cel-shaded illustration, a comic or manga ink-line drawing, or a flat character design reference sheet with hard black outlines and flat color fill regions, a fierce commanding gaze with brows lowered in intense focus, jaw set firm, mouth closed in a stern hard-set expression, head held high with controlled aggression — projecting authority and battlefield presence, dignified rather than shouting, no open mouth, wearing dark lamellar armor with warm brocade trim built from historically grounded Han-Jin lamellar plates and layered cloth, a vivid warm ruddy reddish-bronze East Asian complexion — the red leaning toward a saturated bright crimson rather than a dark maroon-brown, with the frontal plane of the face (forehead, both cheeks, nose bridge, chin) kept at a clearly lit warm mid-tone so the face reads as the single brightest warm-colored area of the entire composition at a glance — the brightening must only raise the value/lightness of this warm reddish-bronze East Asian skin tone, it must never shift the skin toward a pale, ashen, tan-brown, or light Caucasian complexion, and the beard must remain a solid deep black throughout, including the tips and mustache ends, never grey, white, silver, or streaked, a soft warm frontal key light clearly illuminating the eyes, brows, cheekbones, nose, and mouth so the facial features are instantly legible even at small thumbnail size, complemented by a secondary directional side/rim light along the silhouette edge and armor ridge for volume and drama — the frontal plane of the face must never drop below a clear mid-tone brightness, and the face must always read brighter than the surrounding armor and background at first glance, functioning as the composition's clear visual focal point, vivid deep crimson red rendered as one bold continuous color block on a single large accent area (a full cloak draped across one shoulder, or one wide diagonal chest sash) — a solid connected shape, not scattered thin cords or trim lines, this red area must be clearly larger than the green head-wrap and must always remain the single largest and most saturated color block in the entire composition — the green head-wrap must never become the dominant or largest color area of the image, with the main armor plates kept in a restrained bronze/iron/lacquer-black base tone drawn from the game's muted earth-mineral palette, warm weathered naturalistic skin with visible age and battle-worn texture, background is a plain aged rice-paper or silk parchment backdrop with soft vignette shading and visible paper fiber texture — the backdrop must be a single continuous warm paper tone across the entire frame, it must never render as a plain white, off-white, grey, or transparent background, and must never adopt a flat character-design-sheet or turnaround-sheet presentation style, with no color blocking, no vertical color panel, no two-tone split, no card-frame or poster-frame division, no distinct scenery, no landscape, no mountains, no hills, no river, no horizon, character rendered with mature realistic human facial proportions, painterly illustration texture throughout, no text, no watermark, no signature, no modern elements, portrait crop from chest up, vertical composition --ar 3:4 --style raw --sw 300
```

**负面提示 v1.4.5（在 v1.4.4 基础上追加，新增项已加粗）**：
```
--no text, watermark, signature, logo, UI, name tag, health bar, red seal, cinnabar seal, ink chop stamp, hanko, official stamp, fake calligraphy, fake chinese characters, brush-written characters, decorative cursive script, illegible handwritten text, artist signature mark, decorative corner seal, chibi, hyper-realistic photo skin, monochrome sepia rendering, single-tone color scheme, glasses, weapon glow effects, distant landscape, mountains, hills, river, scenery background, open mouth shouting, screaming, roaring expression, anime large eyes, cel-shaded anime style, glossy 3D render look, plastic skin, chrome metallic reflection, trading card frame border, oversized fantastical horned helmet, exaggerated oversized pauldrons, rainbow multi-color costume, western medieval plate armor, european knight armor, Koei Three Kingdoms game art style, Dynasty Warriors style, mobile gacha game splash art style, overly dark image, low-key silhouette, face obscured by heavy shadow, underexposed portrait, dark vignette covering face, face lost in shadow, monochromatic orange wash, single-hue color grading, entire image tinted one color, orange color cast over whole image, sepia-orange filter effect, uniform color overlay, muddy dark red-brown skin tone, face same darkness as armor, grey hair, white hair, silver hair, aged white beard, short beard, trimmed goatee, stubble beard, pale caucasian skin, ashen skin tone, light western skin, western facial features, aquiline nose profile, gaunt european face, gothic vampire count aesthetic, victorian high collar, stiff upright gothic collar, european nobleman, two-tone split background, vertical color panel background, grey card background, color block background division, flat solid non-paper background, geometric panel background, poster color blocking background, black head-wrap, dark navy head-wrap, plain black scarf, rigid metal helmet, ornate metal crown, pointed spired helm, samurai-style helmet, bare head with no headwear, neon green, bright saturated lime green, yellow-green hue, **oversized green area covering torso, green covering the shoulders, green covering the chest, green robe, green garment, green armor, green cloak, green cape, green as the outfit color, all-green color scheme, monochrome green palette, green as the dominant color of the image, green larger than red accent, missing red accent, absent red cloak, no red visible in the image, tan skin tone on face, ordinary brown skin without red tint, desaturated face color, pure white background, plain white backdrop, blank white background, off-white background, transparent background, character design sheet, character turnaround sheet, concept art reference sheet, color palette swatch card, isolated character study on white, flat cel shading, hard black outline linework, comic ink line art, manga line art, flat vector illustration, sharp geometric flat shapes, grey beard strands, white beard strands, silver mustache tips, streaked grey in beard, partially grey beard, bleached beard tips**
```

**5.27.5 gy8 批次可救性判断**

四张均未通过多维复核（红脸/红披/纸底三项全部不过，`gy8-1`/`gy8-2` 另有胡须局部灰白）。`gy8-3` 是四张里画风最接近厚涂目标、且黑髯保持纯黑的一张，但仍缺红脸/红披/暖纸底三项硬性维度，**不具备可救性、不能局部修补后升级为候选**——三项缺失是同源的整体色彩/背景系统性偏移（见 5.27.2 根因），不是可以像 §5.25.6 那样"只换一层"的可分离缺陷。`gy8-3` 可保留作"绿幞头造型+色值+黑髯纯度"的技法参考存档，不进入签名候选池。

**下一步**：用 v1.4.5（仍不带 `--sref`）重出一轮，重点复核六项——① 绿幞头是否收缩在头部不再扩散全身；② 红脸是否与绿巾解耦、独立回归饱和朱红；③ 红披/斜披是否重新成为面积最大色块；④ 背景是否回归暖纸本、无纯白/设定稿感；⑤ 画风是否回归厚涂、不再扁平化；⑥ 黑髯是否全程纯黑无灰白丝缕。全六项同时达标后方可从中选定签名图 B / 关羽原型（签名）的共用来源。

---

### 5.28 方向定稿：绿主红辅（不再压绿）+ 简化 v1.4.6 + gy9 可救性判断

项目方用 v1.4.5 出了 `gy9-0~3`（`scratchpad/round9/`），复核确认红脸/画风两项修正生效，但绿袍/白底两项"压不掉"——项目方据此判断"关羽绿袍绿巾本就是这个角色的图标学，不是 MJ 画错"，拍板顺势采纳绿袍，不再压绿。本节记录方向定稿、阵营辨识方案、简化后的 v1.4.6，以及对 `gy9-1`/`gy9-2` 的独立复核与可救性判断。

**5.28.1 方向定稿：绿主红辅**

裁定已写入 `design/art/art-bible.md` §3.1 补充裁定（四）：关羽的绿色（袍+巾）正式确立为主色，红降为小面积点缀（衣领/内衬/束绦），这是裁定（三）"个人色须从属于阵营色"默认规则的**具名例外**，仅适用于关羽一人，不改变裁定（三）对其他头饰槽角色的默认约束（其余蜀汉武将仍以红为主色）。

**5.28.2 阵营辨识度方案**

关羽绿色主导后，玩家如何在名册/外交等界面一眼认出他是蜀汉阵营，给出双轨方案：

1. **画面内小面积红标记（次要信道）**——衣领/内衬/束绦保留一条明确可见的深绯红色布料。`gy9-1` 已经自发呈现了这个方案的雏形（领口露出一小片米白+绯红内衬），可以直接作为 v1.4.6 的措辞参考。这一信道的定位是"熟悉角色后进一步确认"，不承担"一眼辨认"的首要功能。
2. **UI 层阵营色边框/角标（推荐的主信道）**——名册/外交/部署等面板的头像缩略框本身带系统性阵营色外框或角标，与插画内容解耦。理由是**冗余编码原则**：像"一眼辨认阵营"这种关键系统信息，不该只依赖一个因角色而异的信道（关羽这个例外证明了这个信道并不总可靠），应该有一个不随角色变化的固定信道兜底。这一条是**艺术规格要求**（缩略框需要独立于插画内容的阵营色标识位），具体实现需要协调 `ui-programmer`/`ux-designer` 落地头像框 UI 组件，不在本文件范围内展开。

**5.28.3 v1.4.6 简化 prompt（不再压绿，正向拥抱绿袍关羽）—— 已按 5.28.5 头饰结构修正定向为最终版**

在 v1.4.5 基础上删除全部"绿色限定在头部/红块必须更大/禁止绿袍绿甲"等对抗性措辞，改为正向描述传统绿袍绿巾关羽，新增小面积红色标记子句，保留红脸/黑髯/厚涂/暖纸本四项核心目标；**下方为 5.28.5 修正后的最终版本**（头饰改为"成型幞头/软帽"、红脸推到 `gy9-2` 级饱和度、画风锁定 `gy9-1` 的厚涂笔触）：

```
A three-quarter portrait bust illustration of a middle-aged East Asian Han Chinese general with a long, thick, jet-black beard flowing down past the chin to the chest, wearing the traditional green robe and structured soft green cap of the classic Guan Yu iconography — a muted deep jade/malachite green cloak draped over the shoulders paired with a structured soft cap/futou that sits fully over the crown of the head like a real hat, with a defined straight band or trim line across the forehead marking its front edge, and two short ribbons trailing from the back — this must read unmistakably as a finished, shaped cap covering the entire head, NOT as a loose cloth casually wrapped around a hair bun or topknot, with a small visible red accent at the collar or inner lining (a narrow strip of deep crimson red fabric peeking out at the neckline, or a red sash tie at the collar) serving as the character's Shu Han faction marker, painted in a bold hand-painted game character illustration style with confident visible loaded brushstrokes, soft blended color transitions, and layered gouache-like painterly texture throughout, thick impasto paint, dramatic cinematic lighting with rich saturated shadow depth and strong three-dimensional form, high-impact game character presence, with loose expressive brush edges at the silhouette boundary — not a flat cel-shaded illustration, not a comic or manga ink-line drawing, not a hard-edge graphic style with crisp flat color regions, not a character design reference sheet, a fierce commanding gaze with brows lowered in intense focus, jaw set firm, mouth closed in a stern hard-set expression, projecting authority and battlefield presence, dignified rather than shouting, a deeply saturated, uniform crimson-red East Asian complexion covering the entire face evenly (forehead, both cheeks, nose bridge, chin, jaw) — the classic vivid "opera red face" of Guan Yu, at full saturation and full commanding presence, not a pale or lightly-flushed version, functioning as the single brightest and most intense-colored focal point of the composition even at small thumbnail size, jet-black hair and beard throughout, never grey or white, a soft warm frontal key light on the face complemented by a secondary rim light along the silhouette edge, warm weathered naturalistic skin texture, background is a plain aged rice-paper or silk parchment backdrop with soft vignette shading and visible paper fiber texture across the entire frame — this backdrop must never render as a plain white, off-white, grey, or transparent background, character rendered with mature realistic human facial proportions, painterly illustration texture throughout, no text, no watermark, no signature, no modern elements, portrait crop from chest up, vertical composition --ar 3:4 --style raw --sw 300
```

**负面提示 v1.4.6（在原精简版基础上追加头饰结构与画风相关新增项，已加粗）**：
```
--no text, watermark, signature, logo, UI, name tag, health bar, red seal, cinnabar seal, ink chop stamp, hanko, official stamp, fake calligraphy, fake chinese characters, decorative cursive script, illegible handwritten text, artist signature mark, decorative corner seal, chibi, hyper-realistic photo skin, monochrome sepia rendering, glasses, weapon glow effects, distant landscape, mountains, hills, river, scenery background, open mouth shouting, screaming, roaring expression, anime large eyes, cel-shaded anime style, glossy 3D render look, plastic skin, chrome metallic reflection, trading card frame border, oversized fantastical horned helmet, exaggerated oversized pauldrons, rainbow multi-color costume, western medieval plate armor, european knight armor, Koei Three Kingdoms game art style, Dynasty Warriors style, mobile gacha game splash art style, overly dark image, low-key silhouette, face obscured by heavy shadow, underexposed portrait, dark vignette covering face, monochromatic orange wash, entire image tinted one color, muddy dark red-brown skin tone, pale face, lightly flushed face, desaturated red face, weak red saturation, grey hair, white hair, silver hair, aged white beard, short beard, trimmed goatee, stubble beard, grey beard strands, white beard strands, streaked grey in beard, pale caucasian skin, ashen skin tone, light western skin, western facial features, gothic vampire count aesthetic, victorian high collar, two-tone split background, vertical color panel background, grey card background, flat solid non-paper background, geometric panel background, poster color blocking background, rigid metal helmet, ornate metal crown, pointed spired helm, samurai-style helmet, bare head with no headwear, pure white background, plain white backdrop, blank white background, off-white background, transparent background, character design sheet, character turnaround sheet, concept art reference sheet, flat cel shading, hard black outline linework, comic ink line art, manga line art, flat vector illustration, sharp geometric flat shapes, no red visible anywhere, entirely green with no red accent, **cloth wrapped around a topknot bun, exposed hair bun under headwear, headscarf casually knotted on top of the head, visible hair between the wrap and the forehead, loose scarf without defined cap shape, hard-edge flat color rendering, crisp vector-like hair silhouette, sharp geometric beard shards, overly clean graphic linework**
```

**5.28.4 `gy9-1`/`gy9-2` 独立复核 + 可救性判断（初判，见 5.28.5 头饰结构修正）**

独立 Read 复核两张：

- **`gy9-1`**：红脸为暖橙红润调（额头较亮、颊鼻更红，符合正面主光逻辑），黑髯纯黑无灰白，绿袍偏灰绿的墨绿色调、画风是两张里笔触感最强的一张（湖蓝灰绿的厚涂层次、下摆有明显松动笔触），且**自发呈现衣领处一小片米白+绯红内衬**，恰好是 5.28.2 方案 1 的天然范例。逐项复核：红脸过（暖润，非最饱和但达标）／黑髯过／画风过（本轮最佳厚涂）／绿袍现已按新裁定接受／红色小标记已存在。**唯一未过项：背景为纯白**。**结论：`gy9-1` 确认是"只差背景"的候选**，可走背景修正路线。
- **`gy9-2`**：红脸是两张里最饱和、最经典的"关公正红脸"（额/颊/鼻均匀深红，项目方描述准确）。黑髯纯黑。但独立复核发现**一项项目方未提及的次要问题**：`gy9-2` 的渲染手法比 `gy9-1` 更偏硬边平涂——发际线、须尖、幞头褶皱的边缘处理更接近清晰的图形化描线，笔触堆叠感弱于 `gy9-1`，虽然没有到 `gy8-2` 那种完全扁平矢量/漫画描线的失败程度，但确实是"画风"这一维度上的一个**风格倾向性问题**，而非硬性失败。红色标记在 `gy9-2` 里也存在但比 `gy9-1` 更细微（领口一丝暗红），作为阵营标记稍嫌不够醒目。**结论：`gy9-2` 不是纯粹的"只差背景"——它还带一个画风倾向性的软性瑕疵，这个瑕疵是渲染技法层面的问题，不是可以像背景那样单独换一层的"图层缺陷"**，如果只做背景修正而不处理画风，修正后的 `gy9-2` 在"画风"这一项复核上仍然只能打"勉强/待定"而非"通过"。

**可救性判断**：
- **`gy9-1` 可以直接走"补暖纸底 → 复核 → 锁定"路径，不需要再出 v1.4.6 一轮**——背景是唯一未过项，且是经典可分离缺陷（同 §5.25.6 的处理方式）。技术路线沿用已确立的偏好：首选 MJ Vary(Region)（`gy9-1` 下摆有松散笔触边缘，同引擎重生成过渡更自然），PS 复用 `bg3-1`/`zm-0` 纸纹为兜底。验收标准同 §5.25.6：与 `bg3-1` 并排比对确认同一纸感世界，无残留白边/色温断层，边缘无接缝，不重现双色分栏。
- **`gy9-2` 不建议直接补背景就锁**——即便只修背景，画风倾向性瑕疵仍在，复核会在"画风"项打折扣；若项目方仍然想保留 `gy9-2` 这张"最正红脸"的成品，建议改用 v1.4.6 重出一轮去争取"红脸最饱和 + 画风也过关"同时达标的新候选，而不是在 `gy9-2` 基础上局部修补。`gy9-2` 目前定位为"红脸色值参考图"存档，不建议作为背景修正的直接对象。

~~**原推进建议（已作废，见 5.28.5）**：优先对 `gy9-1` 执行背景修正，可以省去 v1.4.6 整轮出图。~~

**5.28.5 修正：头饰结构判断修正——`gy9-1` 是缠髻布非成型帽，`gy9-2` 才是真正的幞头；`gy9-1` 背景直锁路径作废，改用 v1.4.6 定向重出**

项目方指出 5.28.4 的初判遗漏了一个关键结构差异：`gy9-1` 的绿色只是**缠在发髻上的布**，不成帽型；`gy9-2` 才是**成型的软帽/幞头**。独立重新 Read 复核两张确认属实：

- **`gy9-1` 头饰**：绿色软布从头顶偏后处扎起，呈圆鼓状发髻轮廓，两侧飘带垂下，**发际线（美人尖）与两侧头发在布结下方清晰可见**，读起来是"布裹发髻"而不是"戴帽子"——布料没有形成覆盖全头顶、额前收边的帽型结构。**脸部复核修正**：进一步比对确认脸虽有暖红润泽（额头偏亮、颧鼻偏红），但整体饱和度/气场明显弱于 `gy9-2`，不是"最正红脸"。
- **`gy9-2` 头饰**：绿色软帽**完整覆盖整个头顶**，额前有一条清晰的横向滚边（浅金/卡其色滚边线），帽顶有明显的折痕结构，**头发完全被帽子盖住、不可见**——这才是读起来"戴着一顶帽子"而非"扎了一块布"的结构。脸部：全脸（额/双颊/鼻梁/下颌）呈均匀高饱和的深红，是两张里"气场"最强、最接近戏曲关公红脸程式化形象的一张。

**结论修正：`gy9-2` 在"帽型"与"赤面气场"两个关羽本体识别要点上都优于 `gy9-1`，`gy9-1` 仅在画风笔触上占优（更松动的厚涂笔触，`gy9-2` 偏硬边平涂）**。因此：

- **不宜直接锁 `gy9-1`**——即便背景修好，帽型不成型、红脸气场不够，仍不合格，§5.28.4 判定的"只差背景"是**基于错误的头饰结构判断**得出的，现予撤回。
- **不宜直接锁 `gy9-2`**——硬边平涂画风一旦挂 `--sref` 会把这种渲染技法传播给后续全部 ~30 将，与 §5.27 已确立的"厚涂 gouache、非扁平/非硬边"画风标准冲突。
- **两张都不满足单层可分离修正的条件**（帽型是主体结构问题、画风是全局渲染技法问题，都不是背景那种可以单独换层的缺陷），**必须用 v1.4.6 重新出图**，不能再走"背景直锁"捷径。

v1.4.6（见上方 5.28.3 已更新的最终版）的定向目标非常明确：**`gy9-2` 的内容（成型绿幞头 + 饱和赤面气场）＋ `gy9-1` 的厚涂笔触 ＋ 暖纸本背景**——头饰段落已改写为"structured soft cap... sits fully over the crown... defined band across the forehead... NOT a loose cloth wrapped around a hair bun"，脸部子句已提升到"deeply saturated, uniform crimson-red... opera red face... not a pale or lightly-flushed version"，画风子句保留"loose expressive brush edges"以锚定 `gy9-1` 的笔触，并在负面提示新增"cloth wrapped around a topknot bun / exposed hair bun / hard-edge flat color rendering"等词分别堵住两张各自的缺陷复现。

**下一步**：用 v1.4.6（仍不带 `--sref`）出一轮，六项同时复核——①帽型是否读作"戴帽子"而非"扎布"（额前有无清晰收边/头发是否完全不可见）；②红脸是否达到 `gy9-2` 级别的均匀高饱和"戏曲赤面"；③画风是否保住 `gy9-1` 级别的松动厚涂笔触、不滑向硬边平涂；④背景是否为暖纸本非纯白；⑤绿袍/黑髯/表情/非Koei 是否保持；⑥红色小标记是否仍可见。全六项同时达标后方可选定为签名图 B / 关羽原型（签名）的共用来源。

### 5.29 第十轮出图评审 + 正式锁定裁定：v1.4.6 合成靶命中，选定 `gy10-2` 为签名图 B / 关羽原型（签名）

项目方用 v1.4.6 出了 `gy10-0~3`（`scratchpad/round10/`），独立 Read 复核四张，逐张过 §5.28.5 六点清单 + §9.4 非 Koei 复核。

**5.29.1 逐张六点清单**

| 检查项 | `gy10-0` | `gy10-1` | `gy10-2` | `gy10-3` |
|---|---|---|---|---|
| ①帽读作帽（非扎布，额前收边+头发不可见） | 过 | 过 | 过 | 过 |
| ②红脸达 `gy9-2` 级均匀高饱和 | 过（深红均匀） | 过 | **过，四张最佳**（全脸最均匀、最饱和，"戏曲赤面"气场最强） | 过，但有方向性阴影（右颊/下颌偏暗），均匀度不如 gy10-2 |
| ③厚涂非硬边 | 过 | 过（略偏工整，仍在合格线内） | 过 | **过，四张最佳**（笔触最松动、颜料堆叠感最强） |
| ④暖纸本背景非纯白 | 过（背景有一块偏几何的柔边色块，未到硬伤程度） | **待定**——背景后方有一块边缘偏清晰的矩形色块，接近此前诊断过的"画框/卡纸"失效模式边缘，未到硬性排除但是本轮唯一有此倾向的一张 | **过，四张最干净**（暖色晕影自然过渡，无矩形色块/边框痕迹） | 过（顶部/侧边有自然毛边笔触，无硬性矩形边框） |
| ⑤绿袍/黑髯/威严闭口/非Koei | 过 | 过 | 过 | 过 |
| ⑥小面积红标记可见 | 弱（被长髯遮挡，几不可见） | 弱（同上） | 弱偏中（下颌胡须分缝处可见一丝绯红内衬，四张中相对最可辨） | 弱（同源问题，长髯遮挡） |
| §9.4 非 Koei/原创性 | 过 | 过 | 过 | 过，**但右下角有 MJ 幻觉水印文字"CUCCURS.COM"，须去除后才可用作 `--sref` 源** |

**共性观察**：四张的"⑥小面积红标记"均偏弱——长而浓密的黑髯把领口/内衬区域几乎完全遮住，红色点缀只剩一丝缝隙可见。这不是本轮 prompt 的新退步（v1.4.6 措辞本身已包含该子句），而是关羽本体"长髯及胸"这一硬性身份特征与"衣领处红色点缀"这一构图位置之间存在天然的遮挡矛盾——**这印证了 §5.28.2 已经预判并给出的判断：画面内红标记只能作次要信道，UI 层阵营色边框/角标才是承担"一眼辨认阵营"主责的信道**，不需要为了让红标记更醒目而牺牲长髯的完整度去重新出图。

**5.29.2 最终锁定裁定：选定 `gy10-2`**

**一票：`gy10-2`。**

理由：
1. **六点清单里唯一在②③④三项均拿到"本轮最佳或并列最佳"的候选**——`gy10-3` 虽然画风笔触（③）略胜一筹，但②（红脸均匀度）和 §9.4（水印）两项都劣于 `gy10-2`；`gy10-0`/`gy10-1` 在②③两项都不如 `gy10-2`。
2. **零水印风险**——`gy10-3` 的"CUCCURS.COM"水印是 MJ 常见幻觉伪落款的变体（同 §5.8 系列"擅自加印章/伪文字"问题的水印版本），去除本身不难，但每多一道后期处理步骤就多一次"改完有没有引入新瑕疵"的复核成本；`gy10-2` 不需要这道工序，风险面最小。
3. **更关键的技术理由——面部光照均匀度直接影响缩略框可读性**：`gy10-3` 的方向性布光（左上暖光+右颊/下颌自然阴影）营造出更强的"画廊肖像画"气质，单张观感确实精彩，但这种明暗分布会在 64×85px 级别的名册缩略框里让半张脸偏暗、削弱"红脸是全画面最亮焦点"这条 v1.4.6 prompt 里明文写死的要求（"functioning as the single brightest and most intense-colored focal point of the composition even at small thumbnail size"）。这正是第四轮/第五轮评审反复处理过的"缩略框辨识度"问题的同类风险——如果把这种布光方式挂 `--sref` 传播给后续 ~30 将，等于把一个已经专门修过的问题重新引入 DNA，且不会在单张大图评审时被发现，只会在批量出图后的缩略框实测里才暴露，届时代价更高。`gy10-2` 的正面均匀布光完全避开这个风险。
4. `gy10-3` 的厚涂笔触质感确实是四张里最好的，**建议保留作为"画风质感"技法参考存档**（类比此前 `gy8-3`/`gy4-1` 的处理方式），但不作为 `--sref` 主锁源。

**若项目方仍倾向 `gy10-3`**（例如认为画廊肖像气质对武将资产库整体调性更重要，愿意承担缩略框布光风险）：去水印方式**优先选 PS 内容识别填充/仿制图章**（该角落背景是低细节的暖纸底纯色晕影区域，没有复杂纹理需要保留，修补风险低、比 Vary(Region) 更快且不会波及肩部/发丝等主体像素），其次才是 MJ Vary(Region) 局部重绘该角落。**去水印后必须再走一次简化复核**（不需要整套六项重来，只需确认：角落修补处与周边纸纹色温连续、无接缝/无残留文字痕迹），确认通过后才能升采样、挂 `--sref` 锁定——不能因为"只是去个水印"就跳过这一步复核，去水印操作本身有引入新瑕疵的风险，同 §5.25.6 确立的"可分离修正仍需重新验证"原则。

**5.29.3 锁定后步骤**

1. 对 `gy10-2` 执行 Upscale，取图片 URL。
2. 登记进 §6 素材登记表——"签名图 B"与"关羽原型（签名）"两行**正式合一锁定**，共用同一 Upscale 源。
3. ~~后续 5 个招牌原型（诸葛亮/曹操/吕布/司马懿 + 关羽本人）与核心 ~30 将出图，统一挂 `--sref <gy10-2 URL> --sw 300`。~~ **本条已于 §5.29.5 撤回**——实测证明用人物立绘做跨角色 `--sref` 锚会连人物身份一起传播，非本条设想的"只传播画风"，改判为纯文字 prompt，详见 §5.29.5。
4. **重申 `--sref` 锁定的注意事项**（同 §5.25 推进步骤）：`--sref` 锁的是画风/材质/笔触/布光基因，**不锁五官/脸型/头饰颜色**——新角色的脸型、须型、头饰造型与颜色理应因人而异（只有关羽本人保留绿幞头，其余武将走默认头饰槽或各自的专属定制值）。首批出图后**先抽查 3-5 张**，重点确认：① `--sref` 是否被误锁死成"gy10-2 本人的脸"（新角色是否也长成了关羽的样子）；②新角色的缩略框读法是否清晰（尤其是布光均匀度是否也随 DNA 传播一并达标）；③阵营色点缀在非关羽角色身上是否恢复为裁定（三）默认的"阵营色为主、专属色为辅"（不应该把关羽的例外也传播给其他角色）。抽查通过后再批量铺开剩余武将。

**5.29.4 补充裁定：解耦方案——签名图 B/`--sref` 风格锚与关羽本人游戏内立绘不再共用同一张图**

项目方就 5.29.2 提出的 `gy10-2` vs `gy10-3` 取舍拍板**解耦方案**，推翻本文件此前一贯默认的"签名图 B 与关羽原型（签名）共用同一 Upscale 源、双身份合一登记"处理方式（该默认最早见于 §5.26 结尾、并在 §5.28/§5.28.5/§6 历次更新中延续）：

- **签名图 B / `--sref` 风格锚 = `gy10-2`**——挂 `--sref` 锁给核心 ~30 将出图使用，理由同 5.29.2（均匀布光缩略框安全、无水印风险、三项指标本轮最佳）不变。
- **关羽本人游戏内立绘 = `gy10-3`（去水印版）**——项目方认可其厚涂笔触/画廊肖像质感是四张里最精的一张，希望关羽本尊使用这张成品；去水印后**单独作为关羽专属资产**，**不进入 `--sref` 锚**，从而规避 5.29.2 指出的核心顾虑（其方向性侧光如果挂进 `--sref` 会把"半脸偏暗"的布光基因传播给其余 ~30 将、损害缩略框可读性）——解耦后这个顾虑不再成立，因为 `gy10-3` 只服务于它自己这一张资产，不参与风格基因传播。

**这不是对 5.29.2 裁定的推翻，而是补充**：5.29.2 的核心论证——"`gy10-3` 的侧光不适合作为 `--sref` DNA 源"——完全成立且予以保留，解耦方案正是精确针对这一条论证给出的解法，而不是绕开它。两张图从"必须二选一"变成"分饰两个不同职能"：`gy10-2` 管画风基因传播（风格锚），`gy10-3` 管关羽本人的最终呈现（专属立绘）。`gy10-3` 之前 5.29.2 建议的"仅作画风质感参考存档"处理方式，现予升级为"直接入库作为关羽正式资产"。

**去水印验收**（沿用 5.29.2 fallback 方案，不因为"这次是要正式入库"而加严）：PS 内容识别填充/仿制图章优先处理右下角"CUCCURS.COM"文字（该角落背景是低细节暖纸底晕影区，修补风险低）；去除后只需确认角落修补处与周边纸纹色温连续、无残留文字笔画/接缝痕迹，**不需要重新走整套六项复核**——六项里其余五项（帽型/红脸/画风/绿袍黑髯/暖纸底大局）与水印缺陷无关，本来就已经通过，去水印是纯背景层局部修补，同 §5.25.6/§5.29.2 已确立的"可分离缺陷验收标准"一致。

**5.29.5 补充裁定（实测后追加）：撤回"`gy10-2` 挂 `--sref` 作 ~30 将跨角色风格锚"的决策——改判为纯文字 prompt 靠共享风格句兜底画风**

~~5.29.3 第 3 点原定"后续 5 个招牌原型...与核心 ~30 将出图，统一挂 `--sref <gy10-2 URL> --sw 300`"~~——本条经 §5.30 诸葛亮/曹操首批实测（各 4 张，共 8 张）证伪，正式撤回，理由如下：

1. **根因判定**：`gy10-2` 是一张**已经完成的人物立绘**（关羽本人，特定五官/神韵/情绪状态已经定型），不是一张抽离了人物身份的"纯风格参考板"。把这样一张图整体挂 `--sref` 传播给其他角色，即使调低 `--sw`（原计划从 300 试降到 100），传播的也不只是"笔触/材质/布光"这类可复用的技法基因，而是会连带"这个人的脸长什么样、这个人的表情/气场是什么"一起传播——8 张实测结果印证了这一点：诸葛亮、曹操出图后近似"穿着别人衣服的关羽"，五官/神韵明显趋同于源图，而非各自角色应有的样貌与性情。这比此前 5.29.2 讨论的"侧光布光基因传播"风险更根本——布光是技法层面、可以被"不锁五官"的期望部分隔离，但神韵/脸型是跟人物身份强绑定的，`--sref` 机制本身不区分"风格基因"和"身份基因"，无法只要前者不要后者。
2. **反证**：`gy10-2` 本人当初正是**完全不挂任何 `--sref`**、纯靠 v1.4.6 那段逐字锁定的风格文字段（`painted in a bold hand-painted game character illustration style with confident visible loaded brushstrokes...not a flat cel-shaded illustration...not a character design reference sheet...`）跑出来的第一张锚图。这说明该风格文字段本身已经具备独立支撑"军府活图卷"画风统一性的能力，不依赖 `--sref` 也能稳定复现，之前引入 `--sref` 是画蛇添足，反而引入了未预期的身份污染风险。
3. **新方案**：核心 5 原型/后续 ~30 将统一出图，**一律去掉 `--sref <URL> --sw NNN` 尾部参数，只用纯文字 prompt**——画风统一性完全交由 v1.4.6 逐字保留的共享风格句（正向风格句/打光句/暖纸底背景句/收尾句四段）兜底，人物神韵/性情差异化交由每条 prompt 各自的性情描写段落各自负责，不再共用同一张源图，从根源上切断"风格传播"与"身份传播"被同一机制捆绑的问题。
4. **`gy10-2` 的定位不变**：仍然是关羽本人的签名图 B / 画风参照，继续留档、继续作为"这套文字 prompt 稳定出图效果如何"的比对基准；只是**不再作为其他角色出图时挂载的 `--sref` 源**。
5. **后续如仍需要 `--sref` 兜底画风飘移风险的备选方案**：若未来纯文字 prompt 在更大批量出图（~30 将全量铺开）时仍出现画风漂移不收敛的情况，可以考虑另外制作一张**不含可识别人物五官的纯材质/笔触参考板**（例如无脸剪影、纯布料+纸底特写）专门用作跨角色 `--sref` 源，从源头上排除"身份基因传播"的可能性——但这只是预案，§5.30 v2 出图暂不启用，先验证纯文字方案的效果。

### 5.30 核心 5 原型剩余 4 位（诸葛亮/曹操/司马懿/吕布）出图 Prompt

本节复用 v1.4.6 关羽 prompt（§5.28.3）已被十轮验证专治"设计稿/白底/平涂"顽疾的骨架**逐字不改**——正向风格句（`painted in a bold hand-painted game character illustration style...not a character design reference sheet`）、打光句（`a soft warm frontal key light...rim light...naturalistic skin texture`）、暖纸底背景句（`background is a plain aged rice-paper...must never render as a plain white, off-white, grey, or transparent background`）、收尾句（`character rendered with mature realistic human facial proportions...--ar 3:4 --style raw`）四段原样保留；每条只替换身份/头冠/服装+阵营标记、性情表情、肤色、须发四处填空。**v2 起（见 5.30.6）不再挂 `--sref`/`--sw` 尾部参数，理由见 5.29.5。v4 起（见 5.30.8）正向风格句前追加"游戏原画框定语"、句中追加冲击力措辞，负面提示追加挡传统绘画/故事书措辞，四段骨架的其余部分与两处裁定不受影响。**

**5.30.0 两处裁定（先于四条 prompt 给出，作为撰写依据）**

**裁定一：诸葛亮不开个人色例外，走裁定三默认——但白色鹤氅因是消色差材质，天然不与阵营色竞争，无需替换成红色主袍。**

依据：关羽需要开例外的根本原因是**绿是强饱和度的有彩色，与蜀汉赤金朱在"谁是画面最跳的颜色"这件事上直接竞争**，两者不能共存于"阵营色永远最跳"这条规则下，只能二选一（选了绿，只好把红降级为点缀+靠 UI 层补辨识度）。诸葛亮的白鹤氅不存在这个竞争关系——白色饱和度趋近于零，不会去抢"最跳的颜色"这个位置，赤金朱依然可以是全画面唯一的高饱和色块（内衬/腰间丝绦/纶巾滚边处的鲜亮红金色），一眼看去仍是"整幅画里最鲜艳的颜色"。也就是说，裁定三"阵营色须是画面主色块"这条规则关心的是**饱和度竞争**，不是**像素覆盖面积**——白色鹤氅可以占据最大面积，只要它不与阵营色争夺"最饱和/最先被注意到"的视觉焦点位置，就完全符合裁定三的精神，不需要比照关羽另开一个具名例外。这与关羽案例的本质区别是：**有彩色 vs 有彩色才会产生"主色竞争"，有彩色 vs 消色差（白/灰/黑）不会**——以后任何角色若考虑是否要走"个人色例外"，都可以先问一句"这个专属色到底是不是一个会跟阵营色抢饱和度的强势色相"，是的话才需要走关羽这条例外路径，不是的话直接走裁定三默认即可，鹤氅经典造型完整保留、阵营辨识不受影响。

**裁定二：吕布定色——紫垒 H285 S38 L34（近似 #683678），采纳项目方倾向的紫色系，正式回写 art-bible §4.4"其他势力"占位。**

依据：① 黄绿段（H60–100）会与关羽的孔雀石绿（H150–170 虽不完全重叠，但同属"绿系"大类，色觉上容易被误读为同一阵营色系的深浅变体，尤其在缩略框小尺寸下）产生混淆风险，紫色系（H270–310）与冷调玉绿在色相环上相距最远，视觉上最不容易被误认成"关羽的绿"或"关羽的同系"；② 吕布在演义里"三姓家奴"、反复易主、不属于任何正统阵营的叙事定位，与紫色在传统色彩语义里"孤高、非正统、僭越"（如"红得发紫"暗含的僭越意味）的联想相合，紫色本身也不在曹魏铁苍蓝/蜀汉赤金朱/东吴碧江青任何一支的色相家族里，不会被误读为依附于某一方；③ H285（偏蓝紫，非偏红的紫罗兰/品红）与三大阵营色（蓝/红/青）都保持清晰色相距离，边框/纹样另定"独狼"识别语言（见下）。已将此裁定回写 `design/art/art-bible.md` §4.4"其他势力"段落。

**5.30.1 诸葛亮 · 蜀汉军师**

```
Bold stylized video game character key art, a dramatic hand-painted digital illustration character portrait — a rendered game splash-art illustration, not a photograph, a three-quarter portrait bust illustration of a refined, handsome, relatively young East Asian Han Chinese scholar-strategist in his late thirties, with clear smooth healthy skin and elegant graceful refined features, a neat short-to-medium well-groomed elegant beard along the chin — NOT a thick heavy long warrior beard, finer and more scholarly than a thick warrior's beard, wearing a plain smooth pale ivory-white scholar's robe and long draped cloak of soft matte cloth in the classic hè chǎng silhouette — smooth fabric only, NO feathers, NOT made of feathers, NO feathered shoulder pieces, NO wings, paired with a TALL upright structured flat-topped cloth cap (lún jīn, the classic Zhuge headwear) with vertical pleated ridges running up the front, pale silver-grey in tone, sitting high over the crown like a proper tall hat — not a low wrapped scarf, with soft ribbons trailing at the sides, holding his iconic large crane-feather fan raised near the chest, clearly and prominently visible as his signature identifying object — the robe, cloak, and cap read as understated pale ivory and silver-grey tones, with only a narrow crimson-gold sash at the waist and a thin crimson-gold collar trim serving as the single saturated color accent and Shu Han faction marker — the inner garment glimpsed beneath the cloak is pale grey-blue or ivory, NOT a large red robe, the crimson-gold accent stays small and confined to the sash and collar trim only, the brightest and most colorful element in an otherwise restrained ivory-and-silver-grey palette, painted in a bold hand-painted game character illustration style with confident visible loaded brushstrokes, soft blended color transitions, and layered gouache-like painterly texture throughout, thick impasto paint, dramatic cinematic lighting with rich saturated shadow depth and strong three-dimensional form, high-impact game character presence, with loose expressive brush edges at the silhouette boundary — not a flat cel-shaded illustration, not a comic or manga ink-line drawing, not a hard-edge graphic style with crisp flat color regions, not a character design reference sheet, a gentle, warm, wise and serene expression with a relaxed unfurrowed brow, calm narrowed thoughtful eyes, and a faint kind knowing almost-smile at the corner of the mouth, unmistakably a composed scholar-strategist and not a warrior, projecting quiet strategic brilliance and unshakeable composure rather than aggression, a natural warm East Asian complexion with a refined scholarly tone, clearly lit and clearly readable even at small thumbnail size — not the vivid red "opera face" reserved for Guan Yu, jet-black hair and beard throughout, never grey or white, a soft even flattering key light on the face with smooth gentle rendering — NOT harsh dramatic chiaroscuro on the face — complemented by a strong secondary rim light along the silhouette edge and robe for game-art impact, warm weathered painterly skin texture, background is a plain aged rice-paper or silk parchment backdrop with soft vignette shading and visible paper fiber texture across the entire frame, rendered in a bright luminous pale warm tone clearly lighter than the figure, with the face brightly and evenly lit and clearly NOT in shadow, NOT dark, NOT murky — this backdrop must never render as a plain white, off-white, grey, dark, murky, or transparent background, character rendered with mature natural human facial proportions, painterly illustration texture throughout, tight head-and-shoulders bust crop, the face large and dominant filling the upper portion of the frame, no text, no watermark, no signature, no modern elements, portrait crop from chest up, vertical composition --ar 3:4 --style raw
```

负面提示：
```
--no text, watermark, signature, logo, UI, name tag, health bar, red seal, cinnabar seal, ink chop stamp, hanko, official stamp, fake calligraphy, fake chinese characters, decorative cursive script, illegible handwritten text, artist signature mark, decorative corner seal, chibi, monochrome sepia rendering, glasses, weapon glow effects, distant landscape, mountains, hills, river, scenery background, open mouth shouting, screaming, roaring expression, anime large eyes, cel-shaded anime style, glossy 3D render look, plastic skin, chrome metallic reflection, trading card frame border, oversized fantastical horned helmet, exaggerated oversized pauldrons, rainbow multi-color costume, western medieval plate armor, european knight armor, Koei Three Kingdoms game art style, Dynasty Warriors style, mobile gacha game splash art style, overly dark image, low-key silhouette, face obscured by heavy shadow, underexposed portrait, dark vignette covering face, monochromatic orange wash, entire image tinted one color, deeply saturated opera red face, vivid red complexion, pale desaturated skin, ashen skin tone, non-East-Asian features, victorian high collar, two-tone split background, vertical color panel background, grey card background, flat solid non-paper background, geometric panel background, poster color blocking background, rigid metal helmet, ornate metal crown, pointed spired helm, samurai-style helmet, bare head with no headwear, low wrapped headscarf, floppy hood, loose topknot cloth wrap, bandana, turban, soft slouch cap, pure white background, plain white backdrop, blank white background, off-white background, transparent background, character design sheet, character turnaround sheet, concept art reference sheet, flat cel shading, hard black outline linework, comic ink line art, manga line art, flat vector illustration, sharp geometric flat shapes, hard-edge flat color rendering, crisp vector-like hair silhouette, sharp geometric beard shards, overly clean graphic linework, grey hair, white hair, aged white beard, streaked grey in beard, entirely red or gold robe with no white/ivory visible, martial warrior armor, muscular battle-ready physique, dark murky background, heavy shadow on face, dim underexposed lighting, olive brown background, feathered pauldrons, feather epaulettes, feathers on shoulders, angel wings, feathered cloak, bird-feather costume, feathered armor, large red robe, predominantly red garment, mostly red clothing, full-body robe shot, waist-up wide framing, small distant face, fierce scowl, harsh glare, angry warrior expression, unfriendly sneer, scroll, book, tablet, plaque, sign board, banner held in hand, weapon in hand, sword, spear, staff, traditional gongbi silk painting, antique silk scroll portrait, ancestral shrine portrait, museum artifact painting, flat storybook illustration, children's picture book illustration, fairy-tale book illustration, muted washed-out watercolor, delicate thin ink-line drawing, flat traditional chinese painting, fierce warrior, grim tough soldier, furrowed angry brow, thick bushy angry eyebrows, rugged warrior face, gaunt face, hollow sunken cheeks, craggy harsh face, elderly old man, severe stern face, sinister
```

> **定稿说明**：本条已由项目方确认最终 Upscale (Creative) 版本 `530823f0` 定稿，以上为对应的最终 prompt 全文，锁定为诸葛亮官方立绘生产依据。精修工序：出图后走 Midjourney **Upscale (Creative)**（而非标准 Upscale）收尾，Creative 档位会在放大同时补强厚涂笔触细节，比原始四宫格更精细，是本角色定稿采用的收尾方式，建议后续核心 ~30 将定稿前也默认走 Creative 档位收尾。**遗留观察（非阻断，供未来复议参考）**：定稿图外袍实际呈现偏枣红、白衣占比被压小，与 5.30.0 裁定一"白色鹤氅为主、赤金朱只做点缀"的原始设计意图有出入；若日后需要一版"白袍为主"的备选，可在正向追加 `predominantly clean ivory-white robe, red accent stays tiny` + 负面追加 `large red robe, predominantly red/maroon garment` 再出一轮，本轮不因这条观察阻断定稿。

**5.30.2 曹操 · 曹魏枭雄**

```
Bold stylized video game character key art, a dramatic hand-painted digital illustration character portrait — a rendered game splash-art illustration, not a photograph, a three-quarter portrait bust illustration of a middle-aged East Asian Han Chinese ruler with a robust, fuller composed face — NOT gaunt, a neatly trimmed, dignified black mustache and short beard — cleaner and more groomed than a flowing warrior's beard, per his historical reputation for a fine beard, wearing a tall chancellor's cap with a horizontal gold crossbar AND a gold rank ornament, denoting supreme high rank, paired with a deep desaturated steel slate blue-grey Wei faction cloak (a cool weathered steel tone, distinctly blue-grey — NOT teal, NOT cyan, NOT turquoise, NOT green) draped over the shoulders as the single largest and most saturated color block in the composition, trimmed with silver-grey accent piping at the collar and cuffs, a small dark steel-blue-grey faction emblem clasp at the collar serving as the Wei faction marker, painted in a bold hand-painted game character illustration style with confident visible loaded brushstrokes, soft blended color transitions, and layered gouache-like painterly texture throughout, thick impasto paint, dramatic cinematic lighting with rich saturated shadow depth and strong three-dimensional form, high-impact game character presence, with loose expressive brush edges at the silhouette boundary — not a flat cel-shaded illustration, not a comic or manga ink-line drawing, not a hard-edge graphic style with crisp flat color regions, not a character design reference sheet, a bold, confident, self-assured half-smile, eyes bright with a glint of ruthless ambition and magnetic charisma, projecting an imperious, commanding, larger-than-life overlord's presence — openly ambitious and captivating rather than withdrawn or secretive, dignified but never quiet, a natural warm East Asian complexion, weathered but composed, clearly lit and clearly readable even at small thumbnail size — not flushed or reddened, jet-black hair and beard throughout, never grey or white, a soft warm frontal key light on the face keeping the face clearly and evenly readable — not obscured by harsh dramatic chiaroscuro — complemented by a strong secondary rim light along the silhouette edge and robe for game-art impact, warm weathered painterly skin texture, background is a plain aged rice-paper or silk parchment backdrop with soft vignette shading and visible paper fiber texture across the entire frame, rendered in a bright warm tone clearly lighter than the figure — this backdrop must never render as a plain white, off-white, grey, dark, murky, or transparent background, character rendered with mature natural human facial proportions, painterly illustration texture throughout, no text, no watermark, no signature, no modern elements, portrait crop from chest up, vertical composition --ar 3:4 --style raw
```

负面提示：
```
--no text, watermark, signature, logo, UI, name tag, health bar, red seal, cinnabar seal, ink chop stamp, hanko, official stamp, fake calligraphy, fake chinese characters, decorative cursive script, illegible handwritten text, artist signature mark, decorative corner seal, chibi, monochrome sepia rendering, glasses, weapon glow effects, distant landscape, mountains, hills, river, scenery background, open mouth shouting, screaming, roaring expression, anime large eyes, cel-shaded anime style, glossy 3D render look, plastic skin, chrome metallic reflection, trading card frame border, oversized fantastical horned helmet, exaggerated oversized pauldrons, rainbow multi-color costume, western medieval plate armor, european knight armor, Koei Three Kingdoms game art style, Dynasty Warriors style, mobile gacha game splash art style, overly dark image, low-key silhouette, face obscured by heavy shadow, underexposed portrait, dark vignette covering face, monochromatic orange wash, entire image tinted one color, deeply saturated opera red face, vivid red complexion, pale desaturated skin, ashen skin tone, non-East-Asian features, victorian high collar, two-tone split background, vertical color panel background, grey card background, flat solid non-paper background, geometric panel background, poster color blocking background, rigid metal helmet, pointed spired helm, samurai-style helmet, bare head with no headwear, pure white background, plain white backdrop, blank white background, off-white background, transparent background, character design sheet, character turnaround sheet, concept art reference sheet, flat cel shading, hard black outline linework, comic ink line art, manga line art, flat vector illustration, sharp geometric flat shapes, hard-edge flat color rendering, crisp vector-like hair silhouette, sharp geometric beard shards, overly clean graphic linework, grey hair, white hair, aged white beard, streaked grey in beard, long flowing waist-length beard, green robe, purple robe, teal robe, cyan robe, turquoise robe, jade green robe, sea-green robe, emerald robe, dark murky background, dim underexposed lighting, traditional gongbi silk painting, antique silk scroll portrait, ancestral shrine portrait, museum artifact painting, flat storybook illustration, children's picture book illustration, fairy-tale book illustration, muted washed-out watercolor, delicate thin ink-line drawing, flat traditional chinese painting, grim tough soldier, thick bushy angry eyebrows, rugged warrior face, gaunt face, hollow sunken cheeks, craggy harsh face, elderly old man, sinister, koei romance of the three kingdoms portrait style, generic cold three kingdoms strategist, cold reserved elder schemer, quiet withdrawn strategist, hawk-eyed wolf-turning sidelong look, gaunt cold recluse, thin wispy beard, low plain scholar cap
```

> **定稿说明（外放霸主 v2）**：本条为最终定稿版，供 batch 73ae56db 的 c_2 → Upscale 生产使用。此版相较上一轮"锐利多疑谋士"式的曹操做了整体神韵重写——原版与司马懿同着铁苍蓝袍、同为"冷峻谋士"神韵，在缩略图尺度与同势力配色下几乎撞脸，且滑向"光荣（Koei）三国志立绘"式冷脸谋士刻板印象。现改为：神韵从 `cold/reserved/calculating/cold-smirk` 全面替换为 `bold, ambitious, charismatic, imperious, magnetic overlord, confident self-assured half-smile, glint of ruthless ambition`；相貌新增 `robust, fuller composed face, NOT gaunt`；头冠从"进贤冠+金横簪"升级为 `tall chancellor's cap with gold crossbar AND gold rank ornament`（王者级双重金饰，与司马懿低炭灰素冠区分开）；负面新增 `koei romance of the three kingdoms portrait style / generic cold three kingdoms strategist / cold reserved elder schemer / quiet withdrawn strategist / hawk-eyed wolf-turning sidelong look / gaunt cold recluse / thin wispy beard / low plain scholar cap` 八项，专门反向拦截"滑回司马懿式内敛谋士脸"。
>
> **裁定（本项目铁律，需长期遵守）**：**曹操 = 外放霸主（外露野心）↔ 司马懿 = 内敛谋主（内藏算计）**——两条 prompt 的神韵、相貌措辞、头冠形制须显式互斥，永久防止同势力同色系（铁苍蓝 Wei 袍）撞脸。后续新增曹魏阵营角色时，须先核对与本条、与 §5.30.3 是否存在类似的神韵/头冠重叠。
>
> **非阻断观察**：`dramatic`/`imperious`/`overlord` 等外放霸气词容易被 MJ 联想到战场/战火意象，偏好把背景带向红底或火焰意象。若下一轮生成偏红底，可视为呼应曹操"战意"设定而保留；若需强制统一米纸底，可加负面 `red/crimson flame background, battlefield fire background` 再 roll。

**5.30.3 司马懿 · 曹魏谋主**

```
Bold stylized video game character key art, a dramatic hand-painted digital illustration character portrait — a rendered game splash-art illustration, not a photograph, a three-quarter portrait bust illustration of an elder East Asian Han Chinese strategist with a long, thin, well-kept beard streaked naturally with grey and white among the black, reflecting his advanced age — some grey and white in the beard and at the temples is appropriate and intentional here, wearing a low, dark charcoal-grey Han-Jin scholar-official's cap (guān) of understated design — simpler and less ornamented than a ruler's cap, befitting a reserved strategist, paired with a deep desaturated steel slate blue-grey Wei faction cloak (a cool weathered steel tone, distinctly blue-grey — NOT teal, NOT cyan, NOT turquoise, NOT green) draped over the shoulders as the single largest and most saturated color block in the composition, trimmed with subdued dark cloud-pattern piping at the collar and cuffs (visually distinct from another Wei general's silver-grey piping), a small dark steel-blue-grey faction emblem clasp at the collar serving as the Wei faction marker, painted in a bold hand-painted game character illustration style with confident visible loaded brushstrokes, soft blended color transitions, and layered gouache-like painterly texture throughout, thick impasto paint, dramatic cinematic lighting with rich saturated shadow depth and strong three-dimensional form, high-impact game character presence, with loose expressive brush edges at the silhouette boundary — not a flat cel-shaded illustration, not a comic or manga ink-line drawing, not a hard-edge graphic style with crisp flat color regions, not a character design reference sheet, a cold, deeply reserved, watchful gaze — the classic "hawk-eyed, wolf-turning" look of sidelong suspicion, eyes that seem to track everything without moving the head, mouth closed in a thin unreadable patient line, projecting quiet calculating menace and long-game patience rather than open aggression, a natural weathered East Asian complexion of an older man, composed and clearly lit, clearly readable even at small thumbnail size — not flushed, hair swept back beneath the cap with visible grey strands at the temples consistent with his elder age — never a stark-white full-elderly look nor a pale non-East-Asian complexion, a soft warm frontal key light on the face keeping the face clearly and evenly readable — not obscured by harsh dramatic chiaroscuro — complemented by a strong secondary rim light along the silhouette edge and robe for game-art impact, warm weathered painterly skin texture, background is a plain aged rice-paper or silk parchment backdrop with soft vignette shading and visible paper fiber texture across the entire frame, rendered in a bright warm tone clearly lighter than the figure — this backdrop must never render as a plain white, off-white, grey, dark, murky, or transparent background, character rendered with mature natural human facial proportions, painterly illustration texture throughout, no text, no watermark, no signature, no modern elements, portrait crop from chest up, vertical composition --ar 3:4 --style raw
```

负面提示：
```
--no text, watermark, signature, logo, UI, name tag, health bar, red seal, cinnabar seal, ink chop stamp, hanko, official stamp, fake calligraphy, fake chinese characters, decorative cursive script, illegible handwritten text, artist signature mark, decorative corner seal, chibi, monochrome sepia rendering, glasses, weapon glow effects, distant landscape, mountains, hills, river, scenery background, open mouth shouting, screaming, roaring expression, anime large eyes, cel-shaded anime style, glossy 3D render look, plastic skin, chrome metallic reflection, trading card frame border, oversized fantastical horned helmet, exaggerated oversized pauldrons, rainbow multi-color costume, western medieval plate armor, european knight armor, Koei Three Kingdoms game art style, Dynasty Warriors style, mobile gacha game splash art style, overly dark image, low-key silhouette, face obscured by heavy shadow, underexposed portrait, dark vignette covering face, monochromatic orange wash, entire image tinted one color, deeply saturated opera red face, vivid red complexion, stark white full elderly hair, bald head, completely white beard with no black remaining, pale desaturated skin, ashen skin tone, non-East-Asian features, victorian high collar, two-tone split background, vertical color panel background, grey card background, flat solid non-paper background, geometric panel background, poster color blocking background, rigid metal helmet, pointed spired helm, samurai-style helmet, bare head with no headwear, pure white background, plain white backdrop, blank white background, off-white background, transparent background, character design sheet, character turnaround sheet, concept art reference sheet, flat cel shading, hard black outline linework, comic ink line art, manga line art, flat vector illustration, sharp geometric flat shapes, hard-edge flat color rendering, crisp vector-like hair silhouette, sharp geometric beard shards, overly clean graphic linework, green robe, purple robe, muscular battle-ready physique, martial gold crossbar cap, teal robe, cyan robe, turquoise robe, jade green robe, sea-green robe, emerald robe, dark murky background, dim underexposed lighting, traditional gongbi silk painting, antique silk scroll portrait, ancestral shrine portrait, museum artifact painting, flat storybook illustration, children's picture book illustration, fairy-tale book illustration, muted washed-out watercolor, delicate thin ink-line drawing, flat traditional chinese painting, grim tough soldier, thick bushy angry eyebrows, rugged warrior face, gaunt face, hollow sunken cheeks, craggy harsh face, sinister
```

> **v-next 预调说明**：套用诸葛亮定稿沉淀的通用项——脸部打光加"clearly and evenly readable / not obscured by harsh chiaroscuro"，剪影/袍改"strong...for game-art impact"；负面追加防泛化猛将脸词族，但**不加 `elderly old man`**——司马懿的年长鹰视狼顾本就是核心设定（灰白鬓须/年长复合肤色已显式声明），不能连"年长"本身也一并压掉，只压"糊脸/粗粝/凶恶"这类会抹掉他鹰视狼顾细腻神情的泛化倾向。云纹滚边/低调冠款/铁苍蓝钉死均保留不动。收尾建议同诸葛亮走 **Upscale (Creative)** 精修。

**5.30.4 吕布 · 群雄（独立客将，紫垒色，见 5.30.0 裁定二）**

```
Bold stylized video game character key art, a dramatic hand-painted digital illustration character portrait — a rendered game splash-art illustration, not a photograph, a three-quarter portrait bust illustration of a young to early-middle-aged East Asian Han Chinese warrior with a short, well-groomed black mustache and light stubble along the jaw — younger and less bearded than the other generals, reflecting his relative youth and vigor, full head of black hair bound in a topknot, wearing an ornate gold-inlaid deep purple coronet (zǐ jīn guān) over the bound topknot with a single long pheasant tail feather trailing upward from the crown, paired with intricately detailed lamellar armor featuring etched beast-motif shoulder guards — historically grounded Han-Jin ornate armor, not exaggerated fantasy oversized pauldrons — and a deep royal purple warlord cloak draped over the shoulders as the single largest and most saturated color block in the composition, a small dark purple-and-gold sash accent at the collar reinforcing his identity as an independent warlord aligned with none of the three major factions, painted in a bold hand-painted game character illustration style with confident visible loaded brushstrokes, soft blended color transitions, and layered gouache-like painterly texture throughout, thick impasto paint, dramatic cinematic lighting with rich saturated shadow depth and strong three-dimensional form, high-impact game character presence, with loose expressive brush edges at the silhouette boundary — not a flat cel-shaded illustration, not a comic or manga ink-line drawing, not a hard-edge graphic style with crisp flat color regions, not a character design reference sheet, a proud, defiant, hot-blooded gaze with brows raised in open disdain, a curling contemptuous half-smirk, projecting bold martial pride and fiery impulsiveness — supremely confident and looking down on all rivals, dignified swagger rather than shouting, a natural warm East Asian complexion with a healthy vigorous ruddiness from physical exertion, clearly lit and clearly readable even at small thumbnail size — not the vivid red "opera face" reserved for Guan Yu, jet-black hair throughout, never grey or white, a soft warm frontal key light on the face keeping the face clearly and evenly readable — not obscured by harsh dramatic chiaroscuro — complemented by a strong secondary rim light along the silhouette edge and cloak for game-art impact, warm weathered painterly skin texture, background is a plain aged rice-paper or silk parchment backdrop with soft vignette shading and visible paper fiber texture across the entire frame — this backdrop must never render as a plain white, off-white, grey, or transparent background, character rendered with mature natural human facial proportions, painterly illustration texture throughout, no text, no watermark, no signature, no modern elements, portrait crop from chest up, vertical composition --ar 3:4 --style raw
```

负面提示：
```
--no text, watermark, signature, logo, UI, name tag, health bar, red seal, cinnabar seal, ink chop stamp, hanko, official stamp, fake calligraphy, fake chinese characters, decorative cursive script, illegible handwritten text, artist signature mark, decorative corner seal, chibi, monochrome sepia rendering, glasses, weapon glow effects, distant landscape, mountains, hills, river, scenery background, open mouth shouting, screaming, roaring expression, anime large eyes, cel-shaded anime style, glossy 3D render look, plastic skin, chrome metallic reflection, trading card frame border, oversized fantastical horned helmet, exaggerated oversized pauldrons, rainbow multi-color costume, western medieval plate armor, european knight armor, Koei Three Kingdoms game art style, Dynasty Warriors style, mobile gacha game splash art style, overly dark image, low-key silhouette, face obscured by heavy shadow, underexposed portrait, dark vignette covering face, monochromatic orange wash, entire image tinted one color, deeply saturated opera red face, vivid red complexion, pale desaturated skin, ashen skin tone, non-East-Asian features, victorian high collar, two-tone split background, vertical color panel background, grey card background, flat solid non-paper background, geometric panel background, poster color blocking background, rigid metal helmet, pointed spired helm, samurai-style helmet, bare head with no headwear, pure white background, plain white backdrop, blank white background, off-white background, transparent background, character design sheet, character turnaround sheet, concept art reference sheet, flat cel shading, hard black outline linework, comic ink line art, manga line art, flat vector illustration, sharp geometric flat shapes, hard-edge flat color rendering, crisp vector-like hair silhouette, sharp geometric beard shards, overly clean graphic linework, grey hair, white hair, aged white beard, streaked grey in beard, long flowing waist-length beard, iron-blue robe, crimson-gold robe, teal robe, traditional gongbi silk painting, antique silk scroll portrait, ancestral shrine portrait, museum artifact painting, flat storybook illustration, children's picture book illustration, fairy-tale book illustration, muted washed-out watercolor, delicate thin ink-line drawing, flat traditional chinese painting, grim tough soldier, thick bushy angry eyebrows, gaunt face, hollow sunken cheeks, craggy harsh face, elderly old man
```

> **v-next 预调说明**：套用诸葛亮定稿沉淀的通用项——脸部打光加"clearly and evenly readable / not obscured by harsh chiaroscuro"，剪影/披风改"strong...for game-art impact"；负面追加防泛化猛将脸词族，但**不加 `fierce warrior`/`rugged warrior face`**——吕布本身设定就是骄矜猛将，这两个词若加入负面会反过来压掉他的角色定位，只压"呆板僵硬的普通士兵脸"（`grim tough soldier`）与"粗粝老态"（`craggy harsh face`/`elderly old man`）这类会让他显老显钝、而非显年轻骄横的泛化倾向；紫垒色钉死、雉羽紫金冠、`proud, defiant`神情等前几轮修正保留不动。收尾建议同诸葛亮走 **Upscale (Creative)** 精修。

**5.30.5 逐条差异速览（供出图前自查）**

| 角色 | 阵营/定位 | 头冠 | 主色块 | 阵营标记落点 | 须发年龄段 | 表情基调 |
|---|---|---|---|---|---|---|
| 诸葛亮 | 蜀汉·军师 | 靛蓝纶巾 | 象牙白鹤氅（消色差，见裁定一） | 赤金朱腰绦+内衬 | 中年，黑髯修长儒雅 | 沉静睿智，若有似无的自信 |
| 曹操 | 曹魏·君主 | 高冠+金横簪+金阶饰（王者级双金饰，区别司马懿） | 铁苍蓝袍（银灰滚边） | 领口铁苍蓝纹章扣 | 中年，须髯修剪整齐，面容饱满非清瘦 | 外放霸主，野心毕露，自信魅惑（v2 更新，见 5.30.10） |
| 司马懿 | 曹魏·谋主 | 深炭灰冠（低调款，区别曹操） | 铁苍蓝袍（暗云纹滚边，区别曹操） | 领口铁苍蓝纹章扣 | 年长，鬓须自然灰白 | 冷峻深沉，鹰视狼顾 |
| 吕布 | 群雄·独立客将 | 嵌宝紫金冠+雉羽 | 紫垒 H285 S38 L34（新定，见裁定二） | 领口紫金束绦 | 偏年轻，短须/胡茬 | 骄矜暴烈，目中无人 |

> 四条 prompt 均未挂关羽的"绿主红辅"例外，均走裁定三默认（阵营色为唯一高饱和主色块，个人识别色/材质从属其下），与 art-bible §3.1 补充裁定（三）（四）的适用范围划分一致——（四）明确声明"仅限关羽一人"，本节四位不套用。

> **v2 更新提醒**：曹操/司马懿"主色块"一列的中文概括（铁苍蓝袍）指的仍是 art-bible §4.4 定义的阵营色本身，未变；变化的只是 prompt 里描述这个颜色时用的英文措辞（见 5.30.6），因为首批实测发现"iron-blue"这个措辞会被 MJ 误读偏色到东吴的碧江青（teal/cyan），故 v2 起改用更具体的排他式描述锁死色相，本表无需因此改列。诸葛亮"表情基调"一列的中文概括也保持"沉静睿智"不变（v2 只是把英文措辞写得更明确、并加了"非武将"的排他限定，中文层面的定位没有改变）。

---

**5.30.6 v2 修订记录（首批 8 张实测后的定点修正）**

首批出图（诸葛亮 + 曹操各 4 张，共 8 张）暴露以下问题，经诊断后给出 v2：

1. **共同根因（诸葛亮/曹操/司马懿/吕布全部适用）**：v1 版尾部统一挂了 `--sref <gy10-2> --sw 300`。实测证明这不是"权重高低"的问题，而是"用一张已经定型的关羽人物立绘做跨角色风格锚"这件事本身有问题——`--sref` 机制无法只复制画风基因而不复制人物身份基因，诸葛亮/曹操出图后都不同程度地"长成了关羽的样子"。**裁定：v2 起彻底移除 `--sref`/`--sw` 尾部参数，四条 prompt 一律改为纯文字出图**，画风统一性交由 v1.4.6 逐字保留的共享风格句兜底（该风格句本身就是关羽 `gy10-2` 当初不挂任何 `--sref`、纯文字跑出来的验证结果，已证明足够撑住统一画风）。完整依据见 §5.29.5。
2. **曹操（最致命）**：铁苍蓝袍子被 MJ 渲染成东吴碧江青同色系的青绿/teal 色。诊断为"iron-blue"这个英文措辞被 MJ 拉向 teal 语义邻域。修正：改用排他式描述——"a deep desaturated steel slate blue-grey...distinctly blue-grey NOT teal NOT cyan NOT turquoise NOT green"，并在负面提示加 `teal robe, cyan robe, turquoise robe, jade green robe, sea-green robe, emerald robe`；脸/进贤冠/奸雄神情原样不动，仅改袍色描述并顺带加了背景提亮的措辞（原版背景语句在实测中也偏暗）。
3. **司马懿（预防性，尚未出图测试）**：与曹操共享同一段"iron-blue"措辞，判断会命中同一失败模式，故预防性套用与曹操相同的修正措辞与负面追加，同时保留他云纹滚边/年长灰白鬓须/鹰视狼顾三处区分点不变，避免与曹操撞脸。
4. **诸葛亮（6 处定点修正）**：① 背景跑暗/脸入阴影——加入"bright luminous...face brightly and evenly lit, NOT in shadow"正向句 + 对应负面；② 鹤氅被画成奇幻鸟羽护肩——鹤氅描述整段替换为"smooth fabric...NO feathers...NO wings"+ 对应负面；③ 红色内衬面积过大——赤金朱限死为"腰间窄绦+领口滚边"，内衬明确为"pale grey-blue or ivory"+ 对应负面；④ 构图过远脸过小——加入"tight head-and-shoulders bust crop, the face large and dominant"+ 对应负面；⑤ 神情过凶——表情整句替换为"gentle, warm, wise and serene...unmistakably a composed scholar-strategist NOT a warrior"+ 对应负面；⑥ 羽扇丢失/被画成卷轴——**本项由我判断处理**：不改为卷轴（避免误触发"文件/卷宗"类误读），改为将羽扇降权处理——只描述"折扇上沿在胸前下方隐约可见，不作视觉焦点"，并在负面加入 `scroll, book, tablet, plaque, sign board, banner held in hand, weapon in hand, sword, spear, staff` 排除卷轴/兵器类误读；此外这次同步收紧的"头肩特写构图"客观上也让手部/道具区域更靠近画面下边缘甚至部分出框，进一步降低了这个道具被误读的概率。
5. §5.30.5 对照表除已加的措辞更新提醒外，其余描述性归纳保持不变（详见上方提醒块）。§6 登记表本轮依旧不动。

**5.30.7 v3 修订记录（MJ 审核拦截后的过审安全清雷）**

出图时遭遇 Midjourney 自动审核拦截，报错提示"AI Moderator is unsure about this prompt. AI Moderation is cautious with realistic images, especially of people."——即审核模型对"写实人物"类描述敏感，v1/v2 版本里若干措辞属于高危触发词，本轮做纯清雷处理（不改设定/构图/阵营色等既有内容，只替换措辞），四条 prompt（诸葛亮/曹操/司马懿/吕布）全部适用：

1. **照片写实类词汇**：正向 `mature realistic human facial proportions` → `mature natural human facial proportions`（"realistic" 删除，用 "natural" 替代，语义不变但避开"写实/照片感"关联词）；正向 `warm weathered naturalistic skin texture` → `warm weathered painterly skin texture`（"naturalistic" 同属该风险词族，改为强调"绘画感"的 "painterly"，与整段"手绘游戏立绘"定位更一致）；负面 `hyper-realistic photo skin` 整条删除（"photo" + "realistic" 叠加是高危组合，且该负面项本就只是防止"照片级皮肤"，用同段落里已有的 `plastic skin`/`chrome metallic reflection` 等词已足够防止跳向照片写实，不留缺口）。
2. **暴力/惊悚联想词**：负面 `gothic vampire count aesthetic` 整条删除（"vampire" 高危，且该词本就只是防止吕布/曹操被误读成哥特贵族造型，已有 `victorian high collar` 兜底同一防呆目的，不需要单独保留 vampire 措辞）；曹操正向 `projecting brilliant, dangerous, and duplicitous authority` → `projecting brilliant, cunning, and guarded authority`（"dangerous" 弱化为 "cunning"/"guarded"，"奸雄多疑"的核心气质通过 "cold smirk"/"eyes narrowed with cunning and suspicion" 等其余从句已充分传达，不依赖这一个词）；诸葛亮负面 `menacing glare` → `harsh glare`、`aggressive villain sneer` → `unfriendly sneer`（同属"威胁性/反派感"过审风险词，替换为语义更中性的近义表达，排除效果不变）；吕布正向 `an arrogant fierce hot-blooded gaze...projecting reckless martial pride and impulsive ferocity` → `a proud, defiant, hot-blooded gaze...projecting bold martial pride and fiery impulsiveness`（"arrogant fierce"/"reckless"/"ferocity" 一组词连续叠加渲染攻击性，判定为叠加风险，软化为"骄矜自负但不渲染攻击性"的措辞，"骄矜暴烈目中无人"这一角色定位由 "brows raised in open disdain"/"curling contemptuous half-smirk"/"looking down on all rivals" 等其余从句继续承担，不依赖这组高危词）。
3. **种族描述词**：负面 `pale caucasian skin` → `pale desaturated skin`（改为纯粹的"肤色饱和度"描述，不点名任何具体人种，与本系列一贯的"用色相/明度描述而非人种标签"原则一致）；负面 `western facial features` → `non-East-Asian features`（原意是防止五官漂向非目标人种，用否定式的中性表达即可达成同一防呆目的，不需要点名具体人种）；负面 `light western skin` 整条删除（"pale desaturated skin" 已覆盖同一防呆目的，不需要再单独保留一条带人种标签的近义项）；司马懿正向 `never a stark-white full-elderly look nor a pale Western complexion` → `never a stark-white full-elderly look nor a pale non-East-Asian complexion`（同一原则）。
4. **未改动项**：v1/v2 已确立的防平涂/防设计稿/防白底/防暗调背景骨架、阵营色钉死措辞、诸葛亮六项定点修正（去羽毛/压红/收紧构图/提亮背景/温和神韵/羽扇降权）——全部原样保留，本轮只做过审清雷这一层，不重新打开任何内容讨论。
5. **§6 登记表本轮依旧不动**（四位尚未出图）。

> **通用备忘（写入供后续新角色 prompt 参考）**：Midjourney 的 AI Moderator 对"写实人物"类 prompt 从严审查，实测确认以下词族容易触发或叠加触发拦截——① `realistic`/`photo`/`photo-realistic` 等强调照片写实感的词；② `vampire`/其他暴力惊悚联想词；③ `caucasian`/`western`（或其他具体人种标签）这类种族描述词，即便用于"排除"该特征的负面提示里出现，也可能被判定为敏感内容。**后续新角色（核心 ~30 将全量出图）撰写 prompt 时，一律直接避开这三类词族**——写实感诉求改用 `natural`/`painterly`/`illustration` 等强调"绘画感"的替代词；人种排除类负面提示改用中性的色相饱和度描述（如 `pale desaturated skin`）或否定式表达（如 `non-East-Asian features`），不点名具体人种或地域标签；情绪张力类描述避免多个高危强度词连续堆叠（如 "arrogant fierce...reckless...ferocity" 这类连续渲染攻击性的组合），改为用其中 1-2 个具体的表情/肢体细节从句承担"性格识别度"，而不是靠形容词的强度堆叠。

**5.30.8 v4 修订记录（诸葛亮过审出图后暴露的"画风回滑传统插画"问题，及框定用词铁律）**

诸葛亮 v3 顺利过审并出图，但暴露一个与"能否过审"完全独立的**第三种失败面**：项目方为帮助过审，在实测时于 prompt 开头自行加了一句"声明非照片"的框定语——`A traditional Chinese hand-painted gouache illustration ... not a photograph`（该框定语并非我在 v2/v3 中写入本文件的文字，是项目方/用户在实际提交 Midjourney 时的场外追加，过审本身没问题，但引出了新根因）。诊断结果：这句话虽然成功绕开了"photograph"触发词，但"traditional Chinese...gouache illustration"这个框定措辞本身把 MJ 对画风的理解拉向了**工笔绢本/祖宗像/故事书插画**方向而不是"游戏原画"方向，导致出图（4 张里 2 张，`_0`/`_2`）直接变成传统工笔祖宗像，丢失了关羽那种游戏厚涂立绘该有的冲击力——用户反馈"更像小说/故事书插画，不是游戏"。这与 v3 清雷的"是否触发审核"是两个完全不同的失败维度：**v3 解决的是"过不过审"，v4 解决的是"过审用词本身是否把画风带偏"**——一句话即使成功避开了审核触发词，措辞选择依然可能系统性地污染画风方向，这两件事必须分开验证，不能因为"过审了"就默认框定语没问题。

修正为四条 prompt 统一套用以下三处改动（角色专属造型/神韵/阵营色设定完全不动，只换这一套"游戏原画框定+冲击力词+挡故事书负面"）：

1. **开头框定语替换**：不再使用（或依赖项目方场外追加）"traditional Chinese gouache illustration...not a photograph"这类"传统中国画"框定语，改为在正向 prompt 最前端显式写入 `Bold stylized video game character key art, a dramatic hand-painted digital illustration character portrait — a rendered game splash-art illustration, not a photograph,` 再接原有的"A/a three-quarter portrait bust illustration of a...`开头——同样声明"非照片"以保过审，但框定方向拉回"游戏原画/游戏立绘"而非"传统绘画"。
2. **正向冲击力词追加**：在原有"...and layered gouache-like painterly texture throughout,"之后追加 `thick impasto paint, dramatic cinematic lighting with rich saturated shadow depth and strong three-dimensional form, high-impact game character presence,`，再接原有的"with loose expressive brush edges..."，强化"游戏厚涂立绘"的光影层次与画面冲击力，抵消框定语调整后仍可能残留的"平淡插画感"倾向。
3. **负面追加挡传统/故事书**：四条负面提示统一追加 `traditional gongbi silk painting, antique silk scroll portrait, ancestral shrine portrait, museum artifact painting, flat storybook illustration, children's picture book illustration, fairy-tale book illustration, muted washed-out watercolor, delicate thin ink-line drawing, flat traditional chinese painting`，正面堵死"工笔绢本/祖宗像/故事书插画/水彩故事书"这条画风回滑路径。

> **铁律备忘（供本项目全部 ~30 将 prompt 遵循）**：MJ 过审要求"声明非照片/绘画"，但框定词必须用"video game key art / game character illustration / digital painting"，绝不能用"traditional Chinese gouache/ink painting"——后者会把画风滑向工笔绢本/故事书插画，丢失游戏立绘冲击力。这是本项目 30 将立绘的通用铁律。

**四条完全一致，仅角色专属造型/神韵/阵营色不动**：诸葛亮六项 v2 定点修正、曹操/司马懿铁苍蓝钉死措辞、吕布紫垒色与 v3 软化神情用词，均原样保留，未因本轮改动。

**状态说明（已被下条 §5.30.9 更新，并由 §5.30.10 最终收口）**：本轮原为**预备版本**——项目方已用诸葛亮做过一次同思路的场外测试，但当时尚未收到用户对该测试结果的最终确认。诸葛亮已于 §5.30.9 定稿；~~曹操/司马懿/吕布三条仍维持"已备妥、待验证"状态，等各自实测确认后再转正式定版~~——**此状态已过时**：四位原型已于 §5.30.10 全部确认定稿入库，曹操另经"外放霸主 v2"整体重写（§5.30.2），司马懿/吕布沿用本轮预调版本未再改动。

**5.30.9 诸葛亮定稿（用户确认 Upscale Creative `530823f0`）+ 三层失效面完整沉淀 + 方法预调曹操/司马懿/吕布**

诸葛亮 v4 出图后，项目方与用户逐轮试出以下**在 v4 基础上再叠加**的最终生效改动，用户已接受 **Upscale (Creative)** 版本 `530823f0` 作为定稿，§5.30.1 已锁定为该定稿对应的完整 prompt：

1. **主体年龄相貌**：`middle-aged` 改为 `refined, handsome, relatively young...in his late thirties, clear smooth healthy skin, elegant graceful refined features`——治"MJ 默认把东亚大胡子男配 dramatic game splash 画成粗粝老武将"这一根本倾向，年轻俊朗的设定必须显式写出，不能只靠"scholar/strategist"这类身份词被动期待 MJ 推断出文弱气质。
2. **须**：长须改为 `neat short-to-medium well-groomed elegant beard, NOT a thick heavy long warrior beard`——浓长垂须本身就是"硬汉武将"联想的强触发词，与第 1 点同源，需一并处理。
3. **头冠（本轮最关键的辨识度修正）**：原"headscarf...tied neatly across the crown"的写法被 MJ 稳定误读成软塌裹布，完全不成"诸葛巾"的经典高直平顶形制；改写为 `TALL upright structured flat-topped cloth cap...vertical pleated ridges running up the front...sitting high over the crown like a proper tall hat`，并在负面追加 `low wrapped headscarf, floppy hood, loose topknot cloth wrap, bandana, turban, soft slouch cap` 反向堵死"裹布"这条误读路径。**原创性红线确认**：诸葛巾是真实存在的历史冠帽形制（高直平顶竖褶），不专属于任何现有游戏或画师，用户提供的示意图疑似出自商业游戏立绘，本次修正只提取"帽子形制"这一客观历史事实描述，未参照该图的姿势/袍纹/配色/画面构成，符合 §9.4 原创性红线。
4. **羽扇**：从"折扇上沿隐约可见、降权处理"改为 `holding his iconic large crane-feather fan raised near the chest, clearly and prominently visible as his signature identifying object`——这是本轮发现的一条通用教训（见下方沉淀），招牌道具一旦写成"understated/glimpsed"这类降权措辞，等于主动放弃了这个道具本该承担的角色辨识职能，正确做法是让它明确可见。
5. **脸部打光单独拆分**：新增 `soft even flattering key light on the face...NOT harsh dramatic chiaroscuro on the face`，同时把剪影/披风边缘光升级为 `strong secondary rim light...for game-art impact`——这是本轮第二条通用教训：v4 引入的"dramatic cinematic lighting with rich saturated shadow depth"是服务于"游戏冲击力"这一诉求的措辞，但如果不拆分声明"脸部单独走柔光"，这句戏剧性用光描述会连脸一起吃阴影，与文官儒雅气质的诉求打架；拆分后戏剧性光影全部让渡给剪影/披风/材质层承担，脸部保持清晰易读。神韵负面同步加重压制（`fierce warrior`/`grim tough soldier`/`furrowed angry brow`/`thick bushy angry eyebrows`/`rugged warrior face`/`gaunt face`/`hollow sunken cheeks`/`craggy harsh face`/`elderly old man`/`severe stern face`/`sinister`）。
6. **精修工序**：确认收尾走 **Upscale (Creative)**（而非标准 Upscale）——Creative 档位会在放大同时补强厚涂笔触细节，是本次定稿实际采用且验证有效的收尾方式。
7. **遗留观察（非阻断）**：定稿图外袍偏枣红、白衣被压小，与裁定一"白袍为主、赤金朱点缀"的原始设计意图有出入，已在 §5.30.1 定稿说明中记录为"未来若需白袍为主版本"的备用调整方向，不阻断本轮定稿。

**本轮沉淀的通用铁律（写入供本项目全部 ~30 将 prompt 遵循，累积 §5.30.7/§5.30.8 已有的两条）**：

- **过审框定词铁律（§5.30.8 已定，本轮再次验证有效）**：MJ 审核拦"写实人物"→prompt 必须声明"绘画非照片"，框定词只能用 `video game key art / game character illustration / digital painting`，绝不能用 `traditional Chinese gouache/ink painting`（会滑向工笔绢本/故事书插画）。
- **过审词表（§5.30.7 已定，本轮再次确认适用范围）**：正向避免 `realistic/photo/hyper-realistic`；负面避免堆叠 `caucasian/western skin` 这类人种标签词与 `weapon/sword/spear/screaming/roaring/muscular/vampire` 这类暴力惊悚词，即便出现在"排除"用途的负面清单里也可能触发审核。
- **（本轮新增）年轻/文官角色须显式声明"年轻俊朗+少须"，不能只写身份词**：MJ 对"东亚男性+胡须+dramatic game splash art"这个组合的默认联想强烈偏向"粗粝武将"，即便身份词已经写明"strategist/scholar"，只要没有显式给出年龄段（避免 `middle-aged` 这类中性但容易被 MJ 解读偏老的词）、肌肤状态、须型长度的具体描述，就可能被拉回武将脸。这条规则不仅适用于诸葛亮，任何未来"儒将/谋士/文官"类角色都要预先假设 MJ 有这个默认偏置，主动写年轻/须短/肌肤健康这几项，不能省略。
- **（本轮新增）招牌道具/头冠必须写"明确可见/正戴"，不能写"understated/glimpsed"这类降权措辞**：降权措辞的本意通常是"不想让道具喧宾夺主"，但实测发现这类措辞会导致 MJ 干脆把道具画丢或错误简化（诸葛亮的羽扇曾被"降权"到几乎消失，帽子曾被写成"tied across crown"这种含糊措辞而被误读成裹布）——道具的"辨识度职能"与"是否喧宾夺主"其实是两个独立维度，前者要靠"clearly and prominently visible"类措辞正面保证，后者可以靠构图位置（如"raised near the chest"而非举过头顶）、面积占比等其他从句控制，不需要靠模糊整体描述的方式牺牲辨识度来换取克制感。
- **（本轮新增）"戏剧性冲击力"用光描述需要与"脸部可读性"分层声明，不能合并成一句**：v4 为了游戏冲击力引入的 `dramatic cinematic lighting with rich saturated shadow depth` 这类措辞，若不额外拆出一句"脸部单独维持柔光/清晰可读"，会连脸一起吃重阴影，与角色需要的"睿智/文气"气质冲突；分层写法是给"脸"一句、给"剪影/披风/材质"另一句，两句可以同时存在于同一条 prompt 里，互不干扰。
- **收尾统一走 Upscale (Creative)**，不靠反复重 roll 四宫格来碰运气，节省批量出图成本。

**方法预调曹操/司马懿/吕布（§5.30.2-5.30.4 已套用，本节记录取舍依据）**：三人**不需要**诸葛亮那种"极致柔和年轻化"改造——曹操中年枭雄的沉稳威严、司马懿年长鹰视狼顾、吕布年轻骄横猛将，这三种气质本身就需要保留一定的硬朗感，区分度靠各自头冠款式/神韵措辞/阵营色（曹操进贤冠+金横簪+铁苍蓝、司马懿低调冠+云纹滚边+灰白鬓须、吕布紫金冠+雉羽+紫垒色）来承担，不是靠把所有人都柔化成同一种"年轻俊美"脸谱化解决——**如果不分青红皂白地把"年轻化+少须+柔光脸"套给全部角色，反而会制造新的"千人一面"问题（这次是"千人一面美男子"而非"千人一面猛将"）**。因此三人只套用了两项与年龄/须发/头冠设定无关的通用项：①脸部打光的"clearly and evenly readable / not obscured by harsh chiaroscuro"拆分句（防脸部糊读，与年龄无关，普适）；②负面提示追加防"泛化猛将脸/僵硬士兵脸"词族（`grim tough soldier`/`rugged warrior face`/`craggy harsh face` 等，逐人按其设定意图挑选子集——司马懿不加 `elderly old man` 因为他本就该显年长，吕布不加 `fierce warrior`/`rugged warrior face` 因为他本就该显猛将气）。招牌道具显著化这一条对三人不适用——三人的头冠（进贤冠/云纹低调冠/紫金雉羽冠）在原 prompt 里从未使用"understated/glimpsed"这类降权措辞，不存在诸葛亮那种"藏起来"的问题，无需改动。收尾统一建议走 Upscale (Creative)。~~§6 登记表待四位都实际出图并通过复核后再登记~~——**已于 §5.30.10 完成登记**。

**5.30.10 四位原型全部定稿封口（§6 登记完成 + 曹操"外放霸主 v2"+ 全轮教训终版沉淀）**

武将立绘五位（关羽 + 本节四位原型）已全部定稿并入库至 `D:\Projects\三国演义\UI Test\UI\final`：诸葛亮.png（批次 `a9587a53` → Upscale Creative `530823f0`）、曹操.png（批次 `73ae56db` 候选 `c_2` → Upscale）、司马懿.png（批次 `b0595d56` 候选 `s_3` → Upscale）、吕布.png（批次 `d477df46` 候选 `l_1` → Upscale）、关羽.png + 关羽_ref.png（`gy10-3` / `gy10-2`）。§5.30.1/§5.30.3/§5.30.4 经项目方确认无需再改，维持原文；§5.30.2 经历本轮唯一一次实质性重写，见下。

1. **曹操"外放霸主 v2"重写（本轮核心变更）**：出图复核发现曹操原版（"锐利多疑谋士"神韵 + 进贤冠+金横簪）与司马懿在同一铁苍蓝 Wei 配色下，神韵措辞高度重叠（`cold/reserved/calculating` 系词族两条都在用），缩略图尺度下有撞脸风险，且整体滑向"光荣（Koei）三国志立绘"式冷脸谋士刻板印象——而这恰恰是本项目 art-bible 与 prompt-pack 一直在防的既定负面清单项（`Koei Three Kingdoms game art style` 本就在负面提示里，但神韵措辞的"神似"绕过了字面负面词的拦截，说明**字面负面词只能拦"画风"层面的模仿，拦不住"神韵/人设定位"层面的雷同**——这是本轮新发现的第三种失效面，与"内容质量/平台审核/框定词语义漂移"三层并列，可视为第四层）。修正为神韵 `bold, ambitious, charismatic, imperious, magnetic overlord, confident self-assured half-smile, glint of ruthless ambition`（外放霸主）、相貌 `robust, fuller composed face, NOT gaunt`、头冠 `tall chancellor's cap with gold crossbar AND gold rank ornament`（王者级双金饰），负面新增 8 项定向反拦"滑回司马懿式内敛谋士脸"。
2. **司马懿/吕布无需改动**：项目方复核确认 §5.30.3（铁苍蓝+低炭灰冠+云纹滚边+鹰视狼顾）与 §5.30.4（紫垒+紫金冠雉羽+骄矜）均已稳定命中设计意图，维持原文不动。
3. **§6 登记表已完成登记**：五位原型（含关羽）各自的最终文件名、来源批次/候选、势力色标记落点、缩略图焦点辨识度评估、§9.4 原创依据均已按行填入，见上方"6. 已选定素材登记表"。

**本轮沉淀的新增铁律（第五条，累积 §5.30.7/§5.30.8/§5.30.9 已有的四条）**：

- **角色神韵互斥铁律（本项目长期铁律）**：同势力/同阵营色的多个角色（例如曹魏的曹操与司马懿共享铁苍蓝），其 prompt 的神韵措辞、相貌基调、头冠形制必须**显式互斥**，不能只靠头冠款式的细微差异去区分——神韵词族本身若高度重叠（例如两人都写"cold/calculating/reserved"），即便字面负面提示里已经排除了"Koei 画风"，仍会在观感上滑向同一种刻板印象脸谱。**曹操 = 外放霸主（外露野心）↔ 司马懿 = 内敛谋主（内藏算计）**是这一原则在本项目中的具体落地案例，后续任何同势力多角色场景（例如未来若为蜀汉/东吴新增第二位谋士型角色）都应先核对是否与既有角色存在类似的神韵重叠风险。
- 与此并列，供全部 ~30 将 prompt 长期遵循的既有四条铁律（分别定于 §5.30.7/§5.30.8/§5.30.9，本轮验证依旧全部有效，一并列出便于查阅）：
  1. **过审框定词铁律**：MJ 审核拦"写实人物"→ 声明"绘画非照片"须用 `video game key art / game character illustration / digital painting`，绝不能用 `traditional Chinese gouache/ink painting`（滑向工笔绢本/故事书插画）。
  2. **过审词表**：正向避免 `realistic/photo/hyper-realistic`；负面避免堆叠人种标签词与暴力惊悚词，即便用于"排除"语境也可能触发审核。
  3. **年轻/文官角色须显式声明"年轻俊朗+少须+肌肤健康"**，不能只靠身份词（`strategist/scholar`）被动期待 MJ 推断气质——反之，本就该显年长/显猛将的角色（如司马懿/吕布）不应盲目套用同一套年轻化负面清单，需逐人核对"这条修正针对的症状是否恰好是该角色应有的设定特征"。
  4. **招牌道具/头冠须写"clearly and prominently visible / 正戴"，不能写"understated/glimpsed"降权措辞**；戏剧性冲击力用光需与脸部可读性分层声明（脸一句、剪影/披风材质另一句）。
  5. **收尾统一走 Upscale (Creative)**，不靠反复重 roll 四宫格碰运气。

**遗留待办（非阻断，记录供未来处理）**：
- ① 五位立绘目前背景未统一（例如曹操偏红底、司马懿偏白底等），可在后续批次里追加统一的米纸底负面/正面措辞，批量重新走一次 Upscale 或轻量重绘对齐，非当前阻断项。
- ② 签名图 A 背景骨架锚点 `bg3-1` 仍未执行 Upscale 并正式收录进 `final` 目录（§6 登记表"签名图 A"行的 Job/图片链接仍为占位），需项目方后续在 Midjourney 后台补做 Upscale 并回填链接。

至此，五位核心签名立绘（关羽/诸葛亮/曹操/司马懿/吕布）原型定稿工作全部完成并封口。

---

## 6. 已选定素材登记表（供 §9.4 原创性可追溯性存档）

> 每张正式入库使用的 MJ 出图，在此登记一行，作为"原创依据与参考来源"的存档证据。

| 场景/用途 | Prompt 版本（本文件章节号） | 生成日期 | Job/图片链接 | 审核结果（§3 检查表） | 入库文件名 |
|---|---|---|---|---|---|
| 签名图 A（背景骨架） | §7.3 v1.2 正面 + §7.6 杀折痕负面 | 2026-07-08 | 待项目方从 Midjourney 后台补填 Upscale URL（本地评审文件：`bg3-1.png`，见 §7.8/§7.9） | **通过，已锁定 `--sref`**（§7.9，十项检查含折痕/人物项全数打勾） | `char_bg_signature_a_1920.png`（示例名，正式入库时按 art-bible §8.4 重命名） |
| 签名图 B（风格锚，`--sref` 源） | §5.15 v1.3（绢本方向，已否决）→ §5.21 v1.4（游戏立绘方向，模板已验证）→ §5.22 v1.4.1（打光修正）→ §5.23 v1.4.2（脸部正面明度修正，暴露身份漂移）→ §5.24 v1.4.3（东亚汉人/黑长髯/黑发身份锚定）→ §5.25.5（第七轮验证通过，暂锁 `gy7-2`）→ ~~§5.25.6（gy7-1 暖纸底复议路线，已作废）~~ → §5.26 v1.4.4（加绿幞头头饰槽，神韵锚定 `gy4-3`；第八轮验证暴露 overcorrect）→ §5.27 v1.4.5（再平衡，第九轮验证：红脸/画风修正生效但绿巾/白底压不掉）→ §5.28 方向定稿（绿主红辅，不再压绿）→ ~~§5.28.4（`gy9-1` 背景直锁路径，已作废）~~ → §5.28.5 头饰结构判断修正（定向 v1.4.6 合成靶）→ **§5.29 第十轮验证通过，正式锁定 `gy10-2` 为签名图 B / `--sref` 风格锚**（§5.29.4 补充裁定：本行**自本轮起不再与"关羽原型"共用同一源**，`gy10-2` 专职管画风/材质/笔触/布光基因传播，关羽本人立绘改用 `gy10-3`，见下方"关羽原型（签名）"行） | 待项目方补填 Upscale 日期 | 待项目方从 Midjourney 后台补填 Upscale URL（本地评审文件：`gy10-2.png`，见 §5.29） | **通过，已锁定 `--sref`**（§5.29.1 六点清单 + §9.4 非 Koei 全数打勾，四张候选中②红脸均匀饱和度、④背景干净度均为本轮最佳，且无水印风险）；`gy7-2` 兜底状态解除，`gy9`/`gy8` 批次维持已记录的排除结论不变 | `char_signature_b_styleref_680.png`（示例名，正式入库时按 art-bible §8.4 重命名） |
| 签名图 C（UI 纸质材质） | §1.3 C | 待填 | 待填 | 待填 | 待填 |
| 小城城内 | §2.1 | 待填 | 待填 | 待填 | `bg_smallcity_neutral_1920.png` |
| 战役地形-坚城 | §2.2 + §7.3 v1.2 正面 + §7.6 杀折痕负面 | 2026-07-08 | 待项目方补填 Upscale URL（本地评审文件：`jc-3.png`，见 §7.10.1） | **通过，可锁定**（§7.10.1，十项含旗帜留白/人物排除全数打勾） | `bg_fortified_neutral_1920.png` |
| 战役地形-遮蔽 | §2.3 + §7.3 v1.2 正面 + §7.6 杀折痕负面 | 2026-07-08 | 待项目方补填 Upscale URL（本地评审文件：`zb-2.png`，见 §7.10.4） | **通过，可锁定**（§7.10.4，四张均无硬伤，`zb-2` 留白/纵深最佳） | `bg_cover_neutral_1920.png` |
| 战役地形-粮道 | §2.4 + §7.3 v1.2 正面 + §7.6 杀折痕负面 + §7.10.3 黑边/画框通用负面 + §7.10.2 大车专属负面 | 2026-07-08 | 待项目方补填 Upscale URL（本地评审文件：`ld2-2.png`，见 §7.10.7） | **通过，可锁定**（§7.10.7，黑边/画框问题已解决，复核确认无残留大车/人物剪影） | `bg_supplyroad_neutral_1920.png` |
| 主菜单 | §2.5 + §7.3 v1.2 正面 + §7.6 杀折痕负面 | 2026-07-08 | 待项目方补填 Upscale URL（本地评审文件：`zm-0.png`，见 §7.10.5） | **通过，可锁定**（§7.10.5，十项全过，案头朱印为合理道具不违反反落款规则） | `bg_mainmenu_default_1920.png` |
| 关羽原型（签名，游戏内立绘） | §5.5（占位）→ §5.26 v1.4.4（第八轮 overcorrect，排除）→ §5.27 v1.4.5（第九轮验证：红脸/画风过，绿巾/白底未过）→ §5.28 方向定稿（绿主红辅）→ §5.28.5（v1.4.6 合成靶定向）→ **§5.29 第十轮出图，选定 `gy10-3`（去水印版）**（§5.29.4 补充裁定：本行**自本轮起与"签名图 B"行不再共用同一源**——`gy10-3` 厚涂笔触/画像质感为四张最佳，项目方指定作关羽本尊专属立绘，但因其方向性侧光不适合作 `--sref` 传播源，不进风格锚，风格锚职能改由 `gy10-2` 单独承担） | 2026-07-09 | 已完成去水印（本地文件：`final/关羽.png`，源 `gy10-3`）+ 自身风格参照存档（`final/关羽_ref.png`，源 `gy10-2`，供未来同角色再出图/续作对齐用，非跨角色 `--sref` 锚） | **通过，已定稿**。阵营色标记落点：绿主红辅（见裁定四，仅限关羽一人的例外规则）；缩略图焦点：绿巾轮廓+红脸在小尺寸下辨识度达标；§9.4 原创依据：见 §5.29.4 全轮沉淀，纯文字迭代十轮跑出，未使用任何角色的现有商业立绘作为图像参照 | `char_guanyu_portrait_680.png`（示例名，正式入库时按 art-bible §8.4 重命名；展示位源文件 `final/关羽.png`） |
| 诸葛亮原型（签名，游戏内立绘） | §5.30.1（v4 基础上七点最终修正，已定稿） | 2026-07-09 | 批次 `a9587a53` → **Upscale (Creative)** `530823f0`（本地文件：`final/诸葛亮.png`） | **通过，已定稿**。阵营色标记落点：领口赤金朱窄绦+内衬（蜀汉主色为象牙白鹤氅，见裁定一）；缩略图焦点：诸葛巾高直冠形+羽扇明确可见，小尺寸下辨识度达标；§9.4 原创依据：诸葛巾据史实高直平顶竖褶冠帽形制文字描述创作，未喂图、未照搬任何现有作品的姿势/袍纹/构图 | `char_zhugeliang_portrait_680.png`（示例名，源文件 `final/诸葛亮.png`） |
| 曹操原型（签名，游戏内立绘） | §5.30.2（外放霸主 v2，已定稿） | 2026-07-09 | 批次 `73ae56db` 候选 `c_2` → Upscale（本地文件：`final/曹操.png`） | **通过，已定稿**。阵营色标记落点：领口铁苍蓝纹章扣，铁苍蓝袍为唯一高饱和主色块；缩略图焦点：高冠双金饰（金横簪+金阶饰）与司马懿低炭灰素冠形成小尺寸可辨的头冠差异，神韵改为外放霸主自信半笑，与司马懿内敛谋主互斥（见 §5.30.2 定稿说明的裁定）；§9.4 原创依据：王者级双金饰头冠、外放霸主神韵均据史实身份定位的文字描述创作，未喂图、未照搬任何现有作品构图 | `char_caocao_portrait_680.png`（示例名，源文件 `final/曹操.png`） |
| 司马懿原型（签名，游戏内立绘） | §5.30.3（定稿，铁苍蓝+低炭灰冠+云纹滚边+鹰视狼顾） | 2026-07-09 | 批次 `b0595d56` 候选 `s_3`（灰白须显年长版）→ Upscale（本地文件：`final/司马懿.png`） | **通过，已定稿**。阵营色标记落点：领口铁苍蓝纹章扣，铁苍蓝袍暗云纹滚边（区别曹操银灰滚边）；缩略图焦点：低炭灰素冠+云纹滚边+灰白鬓须三点在小尺寸下与曹操形成可辨差异，内敛谋主神韵与曹操外放霸主互斥；§9.4 原创依据：低调冠形/云纹滚边/鹰视狼顾神情均据史实身份的文字描述创作，未喂图、未照搬任何现有作品构图 | `char_simayi_portrait_680.png`（示例名，源文件 `final/司马懿.png`） |
| 吕布原型（签名，游戏内立绘） | §5.30.4（定稿，紫垒 H285 S38 L34+紫金冠雉羽+骄矜） | 2026-07-09 | 批次 `d477df46` 候选 `l_1` → Upscale（本地文件：`final/吕布.png`） | **通过，已定稿**。阵营色标记落点：领口紫金束绦，紫垒为唯一高饱和主色块（群雄·独立客将，见裁定二）；缩略图焦点：嵌宝紫金冠+雉羽尾翎在小尺寸下轮廓清晰可辨，骄矜暴烈神情与曹操/司马懿的曹魏两位形成阵营间差异；§9.4 原创依据：紫金冠嵌宝+雉羽形制、骄矜神韵均据史实身份的文字描述创作，未喂图、未照搬任何现有作品构图 | `char_lubu_portrait_680.png`（示例名，源文件 `final/吕布.png`） |
| 未探明剪影通用模板 | §5.4 | 待填 | 待填 | 待填 | `char_unknown_silhouette_340.png` |

> 上表命名仅为示例格式，正式入库时请按 art-bible §8.4 命名规则（`[category]_[name]_[variant]_[size].[ext]`）替换为实际武将 ID/代号，并在"变体"位标注 `id`（已识名）或 `silhouette`（未探明）。

---

*本文件为 art-bible.md v1.1 的执行工具，不改变任何已签核的视觉决策。若试产过程中发现 Midjourney 无法稳定达成 §3/§5.6 检查表标准（背景类尤其容易在"纸质感"与"季节中性"两项翻车；人像类尤其容易在"未探明剪影是否仍可辨身份大类"与"两态是否统一"两项翻车），建议在 §6 登记表中如实记录失败原因，作为"是否转人工画师"决策的依据，而非反复无目的地重试同一套 prompt。*

---

## 7. 首轮出图评审与签名图 A 修订版 Prompt（v1.1 → v1.2）

> 本节记录 2026-07-07 用 §1.3 签名图 A 原始 prompt 出的第一批 4 张图（`bg-a-0` ~ `bg-a-3`）的正式评审结论，以及据此修订出的签名图 A prompt v1.2。评审由美术总监亲自逐张目视核对 §3 检查表，独立于项目方的初判进行，结论有一处与项目方初判不同（见 7.1 footnote），已如实记录。

### 7.1 逐张 §3 检查表打勾结果

| 检查项 | bg-a-0 | bg-a-1 | bg-a-2 | bg-a-3 |
|---|---|---|---|---|
| 1 纸质/矿物颜料质感 | 过 | 过 | 过（色彩层偏淡，接近单色渲染，程度弱于其余三张，但仍在可接受范围） | 过 |
| 2 色板落在 §4.1 范围 | 过 | 过 | 过 | 过（植被绿偏深/偏饱和，处于色域边缘，需留意） |
| 3 季节中性（无樱花/雪/秋叶） | 过（树木呈枯枝/暗绿，未见花色） | **存疑，判不过**——中景城门右侧树丛与中央建筑群下方的树冠上有若干浅粉/浅白色小色块，形态接近折枝花朵而非单纯高光，需放大原图复核；若确认为花色则违反本条 | 过——本轮四张里季节中性表现最干净的一张，植被为均质暗绿/褐，无花色 | 过 |
| 4 光照平光柔光 | 过 | 过 | 过（本轮最平最柔） | 过 |
| 5 构图负空间是否充足（供后续叠信息层） | **不过**——建筑群从中景一直堆到画面近三分之二宽度，且最高塔顶几乎顶到画面上边缘，底部又被前景土坡占掉一部分，可用留白明显少于 a-1/a-2 | **过，本轮最佳**——建筑群整体偏右上安排，左侧三分之一大片天空+远山留白，前景坡地和土路干净开阔，最利于后续叠加路线箭头/情报标注 | 过——前景山谷+土路大片留白，构图克制 | 中等——建筑群居中偏满，两侧被林木山体夹紧，留白略少于 a-1/a-2，但前景坡地留白尚可 |
| 6 无 MJ 擅自加文字/印章/签名 | **不过**——左上角出现一列类似毛笔手写汉字的竖排文字/印章组合，是本轮最严重的违规（不只是印章图形，接近"写了字"） | **不过**——左上角出现朱红色印章堆叠（2-3 枚），明显的"擅自盖章"问题 | **过——本轮唯一一张画面中未发现任何印章/文字/水印痕迹的图** | **不过**——右下角出现一枚小型朱红印章 |
| 7 无现代元素 | 过 | 过 | 过 | 过（画面中另有两个极小的人形剪影出现在庭院中，虽非"现代元素"，但背景骨架层按 §1.1 三层原则不应包含人物——人物属于顶层情绪层，建议后续负面提示里补充排除） |
| 8 画风是否统一/不神似现有作品 | 过 | 过 | 过（风格略偏"淡彩线描"，与 a-0/a-1/a-3 的"更浓墨重彩"版本放在一起对比，饱和度是四张里最低的，若定为签名图需注意后续出图统一往这个淡彩方向靠，或反过来把这张的色彩强度调高再用） | 过 |
| **综合结论** | **不通过**（双重问题：擅自文字/印章 + 负空间不足） | **有条件通过**（构图与功能性全场最佳，但擅自印章 + 疑似花色两项待处理，不能直接定为最终签名图） | **通过，本轮唯一全项打勾的图**（但色彩饱和度偏淡、构图偏保守是需要留意的风格代价） | **不通过**（擅自印章 + 出现人物剪影 + 构图偏满） |

> **与项目方初判的一处不同，如实记录**：项目方最初判断"樱花感"出现在 `bg-a-0`，我逐张目视复核后认为——`bg-a-0` 的植被是暗绿/枯枝色调，未见花色；反倒是 `bg-a-1`（项目方倾向选用的那张）的中景树丛上有浅粉/浅白色小色块，形态更接近折枝花朵。这个判断在当前分辨率下仍有一定不确定性，**强烈建议项目方在原图上放大 `bg-a-1` 中景树丛区域再次确认**——这一点直接决定 `bg-a-1` 能不能直接拿来锁定，还是需要重出。

### 7.2 签名图选择：不完全同意直接锁定 bg-a-1，建议"以 a-1 构图为准、再出一轮"

**同意项目方的部分**：从"这张图将来要在上面叠三种笔迹信息层（事实墨线/推测虚线/命令朱红箭头）"这个**功能性**标准出发，`bg-a-1` 的构图确实是四张里最合适的——建筑群偏右上安排、左侧大片天空远山留白、前景土坡干净，未来叠加路线、标注、图标都有地方放，不会和建筑细节打架。这一点我认同项目方的判断，`bg-a-2` 虽然全项打勾但构图偏保守居中、可叠加区域不如 a-1 灵活。

**不完全同意的部分——不建议把 `bg-a-1` 这张原图直接拿去做 `--sref` 签名源**：

1. 印章问题本身不难处理（是画面角落的小范围色块，可以直接裁掉或用 MJ 的 Vary(Region) / Photoshop 内容识别填补去掉，不影响整体构图），**但**花色问题（若 7.1 复核为真）不是角落小问题，而是散布在中景植被里的色彩倾向，裁不掉、补不干净，一旦把这张图当作 `--sref` 源，后续所有背景骨架都会被"带偏"出一点点粉色调，这是我们最想避免的"季节中性被破坏"的核心红线，不能让它进签名源。
2. 即便花色问题最终复核为"其实只是高光、不是花"，这张图角落的印章仍然需要先裁除/去除干净了再上传做 `--sref` 源，不建议原图直接用。

**建议动作（二选一，都不是"直接锁定"）**：
- **方案 A（更稳）**：用 §7.3 的修订版 prompt 重新出一轮 4 宫格，专门检验"擅自印章"和"花色"两个问题是否被修订版的负面提示压下去；新一轮里选一张构图形态接近 `bg-a-1`（偏右上安排+左侧留白）且干净通过全部 10 项的，正式定为签名图。
- **方案 B（更快，若急着推进）**：把 `bg-a-2`（本轮唯一全项打勾）先临时定为签名图去跑通后续 MVP-5 的流程验证管线是否顺畅，同时并行用修订版 prompt 再出一轮去追一张"a-1 构图 + 全项通过"的图，跑通后用更好的那张替换 `--sref` 源重新出 MVP-5（背景骨架数量少，替换成本可控）。

我个人倾向**方案 A**——现在多花一轮出图成本，好过把有问题的图焊进 `--sref` 之后，5 张 MVP 背景全部要重来。

### 7.3 修订版 Prompt v1.2（签名图 A）

在原 §1.3 签名图 A 的基础上，做了三处修订：① 正面描述里加入更强的构图偏置指令（复刻 `bg-a-1` 的"偏右上留白"构图优点）；② 正面描述与负面提示双重加强季节中性措辞；③ 负面提示新增"印章/文字/人物"排除项。

```
A hand-painted illustration of a small ancient Chinese hilltop walled town seen from a mid-distance elevated angle, Han-dynasty era military map cartography style, ink outline linework combined with restrained mineral-pigment color washes (muted earth ochre, warm rice-paper beige, faded ink-gray), aged rice paper texture visible in the sky and open ground, weathered brush-stroke terrain contours, low rolling hills and a dirt road leading to the town gate, composition positions the walled town cluster within the right two-thirds of the frame with a margin of open sky above the tallest rooftop, leaving the left third of the frame as open sky and distant hazy mountains and keeping the lower foreground third relatively clear with only soft hill contours and a single clean dirt road, reserving open negative space for future map annotation overlays, strictly season-neutral with no flowering trees of any kind (no pink or white plum, cherry, or peach blossoms anywhere in the scene), no snow, no vivid red or orange autumn foliage — render all trees and shrubs as plain bare-branch silhouettes or plain muted olive-green foliage only, flat even soft lighting with no strong directional shadow, no human figures anywhere in the scene, painterly illustration texture, muted desaturated palette, wide shot --ar 16:9 --style raw
```

**负面提示**（追加在 `--no` 之后，相较 v1.1 新增印章/文字/花色/人物相关词汇，新增部分已加粗标注供核对）：
```
--no text, watermark, signature, logo, UI, HUD, speech bubble, modern buildings, cars, roads with asphalt, neon light, glowing effects, anime style, chibi, 3D render look, photographic bokeh, lens flare, people close-up, **red seal, cinnabar seal, ink chop stamp, hanko, official stamp, calligraphy inscription, brush-written characters, artist signature mark, decorative corner seal, pink blossom, white blossom, plum blossom, cherry blossom, peach blossom, flowering tree, human figures, people, tiny figures**
```

**修订要点说明（供非美术项目方理解"改了什么"）**：
1. **治印章**：在负面提示里把"印章"这个概念拆成好几种同义表达（red seal / cinnabar seal / ink chop stamp / hanko / official stamp / decorative corner seal）一起排除——MJ 有时候只认其中某一两个词，同义词堆叠是常见的工程手段，不是啰嗦。
2. **强化季节中性**：不只在负面提示里加"no blossom"，还在正面描述里明确要求"所有树木只能是光秃枝干剪影或纯橄榄绿叶丛"，用正面强约束 + 负面排除双保险，比 v1.1 只写一句"avoid blossoms"更不容易被 MJ 忽略。
3. **复刻 a-1 的留白构图**：新增一整句显式构图指令，把"建筑偏右上、左三分之一天空远山留白、前景土路干净"这个 a-1 表现最好的优点写成明确指令，而不是碰运气等 MJ 自己抽到类似构图。
4. **新增排除人物**：针对 `bg-a-3` 出现的院内小人形问题，新增"no human figures"系列负面词——背景骨架层按 art-bible §1.1 不该包含人物，人物属于顶层情绪层，由引擎另行叠加。

### 7.4 下一步

**结论：本轮"基本可锁定风格方向"，但还没到"直接选一张定 `--sref`"的程度，还差一轮迭代**——四张图证明了 MJ 完全吃得下"军府图卷"这套纸质水墨观感（风格锁定协议 §1.2 的第 1、2 条标准已经达成：一眼能看出同一套视觉语言），但每一张都至少有一项检查表硬伤（印章×3、花色疑似×1、构图偏满×1、人物剪影×1），不满足 §1.2 第 3 条"通过 §3 全部客观检查项"的要求，**不能直接把任意一张升采样后拿去做 `--sref` 签名源**。

**下一步具体动作**：
1. 用 §7.3 的修订版 prompt 重新出一轮 4 宫格。
2. 重点复查两件事：印章是否消失、`bg-a-1` 曾出现的疑似花色区域这次是否干净。
3. 新一轮里选一张**构图接近 a-1（右上偏置+左侧留白）且十项全过**的，做 Upscale，取图片 URL，正式进入 `--sref` 锁定阶段（对照 §1.1 第三、四步）。
4. 若新一轮仍然反复出现印章问题，说明这是 MJ 在"古风水墨"语境下的强惯性行为，届时可以考虑把"清理角落印章"当作一个固定的、每张图出图后都要做的人工 Photoshop 收尾步骤写进 §4.5 流程，而不是无限迭代 prompt 去赌 MJ 完全不画——工程上"生成后固定清一次角落"有时比"死磕 prompt 消灭某个 MJ 强惯性"更省时间。

---

### 7.5 第二轮背景出图评审（bg2-0 ~ bg2-3，检验 v1.2 修订效果）

> 本节记录 2026-07-08 用 §7.3 修订版 prompt v1.2 出的第二批 4 张背景图（`bg2-0` ~ `bg2-3`）的评审结论。评审由美术总监亲自逐张目视核对，独立于项目方初判进行。**核心结论：v1.2 的三处修订（治印章 / 强化季节中性 / 复刻右上留白构图 + 排除人物）全部生效**——4 张图均未发现印章、文字、花色、人物，构图全部落在"建筑偏右上 + 左侧留白"，风格锁定协议 §1.2 第 1、2、3 条中"季节中性 / 无擅自元素 / 构图功能性"三项本轮首次全数达成。但暴露出一个 v1.1/v1.2 都未预料的**新失效模式：横向折痕线**（见 §7.6），因此仍不能直接锁定，须再走一轮定向修正。

| 检查项 | bg2-0 | bg2-1 | bg2-2 | bg2-3 |
|---|---|---|---|---|
| 印章/文字/水印 | 干净 | 干净 | 干净 | 干净 |
| 季节中性（无花/雪/秋叶） | 过 | 过 | 过 | 过 |
| 无人物 | 过 | 过 | 过 | 过 |
| 构图（右上偏置 + 左侧留白，供叠信息层） | 过 | 过 | **过，本轮最佳**——左侧大片开阔，负空间最足 | 过 |
| 色板落 §4.1 | 过 | 过 | 过 | 过（前景绿略重，处色域边缘，尚可接受） |
| **横向折痕线（新增检查项，见 §7.6）** | **不过**——两条明显横向折痕线贯穿画面（做旧纸被误解为折叠图卷） | **不过（本轮最重）**——整图裂成上下两块，上约 4/5 与底部浅色带的色调明显对不上，非单纯折痕而是"两块拼接"级瑕疵 | **过（可接受）**——仅底边一条无害细线，PS 一擦即除 | **过（可接受）**——底边一条无害细线 |
| **综合结论** | **不通过**（折痕线中度瑕疵） | **不通过**（上下拼接色调断裂，最严重） | **通过，本轮最佳**（构图最优 + 仅无害底边线） | **通过**（良，仅无害底边线，前景绿偏重需留意） |

> **与项目方初判一致**：项目方倾向 `bg2-2`（留白最足），美术总监复核后确认同一判断——`bg2-2` 是四张里构图最优、瑕疵最轻的一张，是本轮的签名图候选。

### 7.6 新失效模式诊断：横向折痕线 + 负面提示修正

**现象**：v1.2 出的 4 张里，`bg2-0`（两条折痕）与 `bg2-1`（上下拼接色断）出现横向的纸张折痕/分块线，`bg2-2`/`bg2-3` 则退化为底边一条无害细线。

**诊断**：这是 MJ 对 prompt 里"aged rice paper texture / military map cartography"这类"做旧图卷"语义的过度演绎——它把"古旧地图"理解成了一张**物理上被折叠过、可以摊开的卷轴纸**，于是自作主张画出折痕/拼接缝。这与印章问题同属"MJ 的古风强惯性"，处理手法一致：**正面弱化"折叠卷轴"暗示 + 负面堆同义词排除**。

**背景负面提示追加**（接在 §7.3 的 `--no` 串之后，专杀折痕/分块）：
```
horizontal fold line, crease line, scroll fold, paper fold seam, folded paper, panel division, split panel, horizontal band, torn paper strip, diptych, color grading mismatch between panels
```

> 若下一轮仍反复出现折痕线，同 §7.4 第 4 点的工程判断：把"拉平/去折痕"并入固定的出图后 Photoshop 收尾步骤，不必无限死磕 prompt。`bg2-2`/`bg2-3` 那种"仅底边一条细线"本就属于收尾一擦即除的等级，不构成重出理由；真正需要 prompt 修正压制的是 `bg2-0`/`bg2-1` 那种贯穿全图的折痕/拼接。

### 7.7 第二轮锁定结论与下一步

**结论：仍不锁，但已非常接近——只差"消掉折痕"这一轮。** v1.2 已把印章/花色/人物/构图四项历史硬伤全部解决，`bg2-2` 除一条无害底边线外已全项通过；唯一拦路的是本轮新暴露的折痕线。由于 `--sref` 签名源等于后续全部背景骨架（MVP-5 → 远期 ~44 张，见 art-bible §10.6）的"风格 DNA"，一旦把带折痕的图焊进签名源，后续每张背景都会被带出折痕倾向，返工成本远高于现在多出一轮。故**不将 `bg2-2` 原图直接锁定**。

**下一步具体动作**：
1. 用 §7.3 的 v1.2 正面 prompt + §7.6 追加的杀折痕负面，重出一轮 4 宫格。
2. 重点复查：折痕线是否消失（尤其不再出现 `bg2-1` 那种上下拼接色断）。
3. 选一张构图接近 `bg2-2`（右上偏置 + 左侧留白）、折痕干净的，做 Upscale，取图片 URL，正式进入 `--sref` 锁定阶段（对照 §1.1 第三、四步），登记入 §6 素材登记表。
4. 若 `bg2-2` 型构图这轮未复现，退而用本轮 `bg2-2` 做一次 Photoshop 去底边线 + 拉平，作为临时签名源先跑通 MVP-5 管线，并行再追一张干净的替换（同 §7.2 方案 B 思路）。

### 7.8 第三轮出图评审（bg3-0 ~ bg3-3，检验杀折痕负面提示效果）

> 2026-07-08，项目方用 §7.3 v1.2 正面 prompt + §7.6 杀折痕负面追加词出的第三轮 4 张图。美术总监亲自逐张目视复核 §3 检查表（含 §7.5/§7.6 新增的折痕/拼接检查项），结论如下。

| 检查项 | bg3-0 | bg3-1 | bg3-2 | bg3-3 |
|---|---|---|---|---|
| 印章/文字/水印 | 干净 | 干净 | 干净 | 干净 |
| 季节中性/光照/色板/构图留白（1-5 项综合） | 全过 | **全过，本轮留白最足、构图最佳** | 全过 | 全过（前景轻微天然裂纹质感，属正常纸感，非折痕） |
| 折痕线/上下拼接色断（§7.6 检查项） | 过——未见折痕或拼接断层 | 过，同上 | **不过**——画面上下边缘出现明显的撕纸毛边（torn deckle edge），是杀折痕负面提示的过度补偿变体，形态从"折痕/拼接"变成了"四周做旧撕边"，属于新变体而非同一问题复发 | 过，仅极轻微天然墨渍质感，属正常纸感 |
| 无人物剪影 | **有条件通过**——画面中段小径上有一处极小的竖直暗色标记，位置约在城门前小径中部，形态含糊，不能完全排除是极小人形或路标/灯柱一类道具，建议放大原图确认 | **过**——逐角检查未见任何人物或含糊标记 | 过 | 过 |
| **综合结论** | 有条件通过（唯一顾虑是那处含糊标记，非硬伤，且未选它做签名源） | **通过，本轮最佳，全项无保留打勾** | 不通过（撕纸毛边，是杀折痕负面的偶发副作用） | 通过，可作备用 |

**与教练初判的印证与差异**：教练判断"杀折痕负面生效，bg3-0/bg3-1/bg3-3 干净，bg3-2 变成撕纸毛边、bg3-0 路上有极小人影"——逐张复核后**基本认同**，唯一补充是 bg3-0 那处标记的性质我判断为"含糊、不能确诊"，比教练"有个极小人影"的措辞更保守一些，但结论一致：不影响把 `bg3-1` 定为签名源的判断。

### 7.9 锁定确认：选定 `bg3-1` 为背景骨架签名图 A，正式进入 `--sref` 锁定阶段

**结论：背景骨架本轮可以直接锁定，不需要再等一轮。** `bg3-1` 本轮所有检查项（含 §7.5/§7.6/§7.8 新增的折痕/人物项）全部干净打勾，构图留白也是三轮里最佳的一张，满足风格锁定协议 §1.2 第三条"通过 §3 全部客观检查项"的要求——上一轮 §7.7 判断"暂不锁"的唯一理由（折痕/拼接）本轮已被证实解决（`bg3-0`/`bg3-1`/`bg3-3` 三张独立验证），可以放心推进。

**下一步具体动作**：
1. 对 `bg3-1` 执行 Upscale，取图片 URL，登记进 §6 素材登记表"签名图 A"行（本文件已在 §6 登记本轮结论，Job URL 待项目方从 Midjourney 后台补填）。
2. 用该 URL 作为 `--sref` 源，正式出剩余 4 张 MVP 背景骨架（坚城/遮蔽/粮道/主菜单），沿用 §7.3 正面 prompt + §7.6 杀折痕负面追加词。
3. `bg3-0` 那处含糊标记不阻塞本次锁定（未选它做签名源），但建议批量出图时在负面提示里再加一条预防性词汇，降低同类含糊标记复现概率：`tiny figure on path, tiny walking figure, small silhouette on road, distant lone figure`。
4. `bg3-2` 的撕纸毛边不需要再单独开一轮验证——`bg3-1`/`bg3-0`/`bg3-3` 三张已经证明杀折痕负面本身有效，撕纸毛边只是这一张的偶发副作用，弃用即可；若批量出图阶段这个副作用反复出现，再回来加一条`torn deckle edge, ragged paper border`负面词处理，不必现在就动已验证有效的 prompt。

### 7.10 MVP-4 类背景骨架出图评审（坚城/粮道/遮蔽/主菜单，挂 `bg3-1` 为 `--sref`）

> 本节记录用 §7.3 正面 prompt + §7.6 杀折痕负面、挂 `bg3-1` 的 `--sref` 出的 4 类 MVP 背景骨架各 4 张（`jc-0~3` 坚城、`ld-0~3` 粮道、`zb-0~3` 遮蔽、`zm-0~3` 主菜单），逐张对照 §3 十项检查表（含第10项旗帜留白）评审。评审由美术总监亲自逐张目视核对，独立于项目方初判进行；结论与项目方初判总体一致，但在"坚城小人算不算硬伤"和"粮道能否现在就锁"两点上给出更明确/略有出入的裁定。

**7.10.1 坚城（`jc-0`~`jc-3`）**

| 图 | 旗帜留白（第10项） | 背景层是否有人物/兵群 | 结论 |
|---|---|---|---|
| `jc-0` | 通过 | **不通过**——城门左下角有清晰可辨的密集兵群剪影（成排竖线状矛/旗形态） | 不选 |
| `jc-1` | 通过 | 存疑——右下角城门附近有一小片模糊暗斑，形态不如 `jc-0`/`jc-2` 清晰，无法确认是兵群还是烟尘碎石 | 保守判不选（原因见下） |
| `jc-2` | 通过 | **不通过**——右侧小城门根部同样有清晰可辨的密集兵群剪影 | 不选 |
| `jc-3` | 通过 | 通过——未见清晰可辨的人物/兵群形态；左下角有极淡的模糊暗斑，判断更可能是远景灌木/地形笔触，非人形轮廓 | **选定** |

**结论：`jc-0`/`jc-2` 判定为硬伤，不是"背景层可容忍"的程度。** 这不是审美偏好问题，而是 §1.1 三层分工契约的直接违反——底层地貌图层的职责边界就是"不画人"，人物/事件的情绪温度归顶层，玩家看到的每一帧都是引擎把顶层人物动态叠加在这层地貌之上；如果底层图里已经烧录了一群静态的兵，等引擎在同一位置叠加真正的（会移动、会随战局变化的）军队图标或角色立绘时，会出现"背景里凭空多一批和当前战局无关的兵"这种叙事/信息层面的错误，等同于 §1.1 反全知/三语气体系里"信息即美术"原则的破坏。`jc-1` 从严处理，即便形态不如另两张清晰，人物排除是硬性规则而非"看着还行就算了"的软指标，不值得为了多一个候选而承担风险，判定不选。

`jc-3` 全部十项打勾通过，**本轮可以直接 Upscale 锁定入库，不需要再等一轮**——不必因为另外 3 张失败就连累这一类重出，处理方式与 §7.9 背景骨架签名图 A 锁定时"一张干净的图即可推进"的标准一致。

**入库命名**：`bg_fortified_neutral_1920.png`（对齐 §6 登记表既有命名）。

**7.10.2 粮道（`ld-0`~`ld-3`）**

| 图 | 旗帜/装饰留白 | 黑边/画框类硬伤 | 路面含糊小型印记 | 结论 |
|---|---|---|---|
| `ld-0` | 不适用（无旗帜元素） | 无 | 路面中段有一处模糊小方形暗色标记，形态偏车厢/推车轮廓，比例不确定，无法排除是"大车"这类隐含人物操作的载具 | 待复核 |
| `ld-1` | 不适用 | **不通过**——顶部整条黑色 letterbox 色带 | 不适用（黑带已构成硬伤，不再评後续项） | 不选 |
| `ld-2` | 不适用 | **不通过**——图像被处理成"带外框的展示画作"，上下各一道装饰边框线，底部还留出一块空白题字栏（卡片化/画框化） | 不适用 | 不选 |
| `ld-3` | 不适用 | 无 | 路面左上方远处同样有一处模糊小型印记，形态与 `ld-0` 类似（疑似大车/推车），距离更远、占比更小 | 待复核 |

**结论：粮道本轮不能直接判定全部通过，建议合并三个问题重出一轮，而不是在存疑状态下勉强选定 `ld-3`。** 与坚城不同——坚城已经有一张（`jc-3`）十项全过的干净候选，可以直接推进；粮道现存的两张"看起来没有黑边/画框问题"的图（`ld-0`/`ld-3`）都带着同一处尚未确认性质的路面小标记，如果这处标记在高分辨率下被确认是"运粮的大车"，它同样违反 §1.1 底层不画人物/事件载具的分工契约（大车暗示有人在赶车，是事件/动态层的内容，不该烧录进静态地貌层）；如果只是石块/木桩之类的地物细节，则完全合规。在没有把握之前，不建议把 `ld-3` 直接推进 Upscale——这与 §7.8 里 `bg3-0` 的"含糊标记，暂不选它做签名源、但不阻塞其他图锁定"处理方式不同，因为粮道这一类目前**没有第三张干净候选可以替代**，只能在存疑图里二选一，风险等级更高，值得多花一轮把三个问题一次性解决。

**下一步**：合并三项问题重出一轮——① 在负面提示里追加 7.10.3 的黑边/画框通用负面（解决 `ld-1`/`ld-2`）；② 追加下方"路面含糊小型印记"专属负面（解决 `ld-0`/`ld-3` 的疑似大车问题）；③ 重出后若 4 张里出现全项通过的干净候选，直接选定 Upscale 锁定，不需要再等第三轮。

**路面含糊小型印记专属负面提示（追加进 §7.3/§7.6 现有负面清单）**：
```
cart, wagon, oxcart, handcart, wheeled vehicle, cart on road, distant cart silhouette, small vehicle on path, figure pulling cart
```

**入库命名（待新一轮通过后使用）**：`bg_supplyroad_neutral_1920.png`。

**7.10.3 黑色 letterbox 色带 + 外框/画框化——通用成因诊断与负面提示**

`ld-1`、`ld-2`、`zm-1`、`zm-2` 四张（跨"粮道"和"主菜单"两个类目）都出现了同一类失效模式的不同变体：整条黑色色带（类似电影宽银幕遮幅）、或整图被处理成"带装饰边框+底部/顶部留白题字区"的展示画作。**成因诊断**：这与此前"折痕线"、"擅自加印章"两类失效模式同源——都是 MJ 在"古风/做旧/卷轴"语境下，把"这是一张地貌骨架图"误解成"这是一幅需要被装裱展示的艺术品"，于是自动脑补了艺术品展示时才会出现的装置（画框、题字签条、遮幅色带），而不是老老实实画一张干净的地貌底图。这类失效模式在"素材本身写实感强、构图偏'广角全景'"的图里更容易触发（`ld`/`zm` 两类都是横向大场景，比坚城/遮蔽的中近景构图更容易被 MJ 联想成"一整幅挂画"）。

**通用负面提示追加（比照当初杀折痕负面的处理方式，作为背景骨架标准负面清单的新增条目，非仅本轮临时用）**：
```
letterbox bars, black bars, black horizontal bands, cinematic bars, framed border, picture frame, ornate border, decorative frame, card border, poster frame, mounted artwork frame, museum placard, title card, caption plate, text banner area, blank signage area, bordered vignette frame, matting border, gallery frame, exhibition frame
```

**结论：此修正为必须项**，建议直接并入 §7.6 已有的杀折痕负面清单，作为背景骨架的标准负面提示常驻条目（覆盖坚城/粮道/遮蔽/主菜单及未来全量 44 张出图），不只是这一轮临时打补丁。

**7.10.4 遮蔽（`zb-0`~`zb-3`）**

四张风格统一，均未出现黑边/画框/清晰可辨人物问题，第10项（本类无旗帜元素，不适用）不影响判定。`zb-0` 小路中段有一小簇模糊暗色笔触，形态更接近灌木/碎石丛（山林小径场景里这类地物很常见，且不构成任何可辨认的人形轮廓），风险等级明显低于坚城/粮道那两类——判定为地物细节，不影响通过。`zb-2` 全项最干净、小路留白/纵深感最好，与 `zb-0` 相比没有需要复核的模糊标记。

**结论：本类可以直接选定 `zb-2` Upscale 锁定入库，`zb-0` 留作备选（若 `zb-2` 后续 Upscale 出现意外问题可用 `zb-0` 顶替，其模糊笔触已判定为低风险地物细节，不阻塞使用）。**

**入库命名**：`bg_cover_neutral_1920.png`。

**7.10.5 主菜单（`zm-0`~`zm-3`）**

| 图 | 黑边/画框类硬伤 | 顶部留白（供 UI/logo 使用） | 桌上道具 | 结论 |
|---|---|---|---|
| `zm-0` | 无 | 通过——上方留有干净的暖色空白区域，足够放置 logo/标题 | 案头一枚朱红印玺道具，属于"军府案头"场景里合理的实体道具（不是画面上凭空多出的落款印记，两者性质不同，不违反反印章/伪落款规则） | **选定** |
| `zm-1` | **不通过**——顶部整条黑色 letterbox 色带 | 不适用（黑带已占据顶部） | 不适用 | 不选 |
| `zm-2` | **不通过**——顶部深色色带 + 画面四周带装饰性画框边线，读起来像"被装裱展示的画作" | 不适用 | 案头一枚棕色印玺道具，尚可，但被上面两项硬伤连累 | 不选 |
| `zm-3` | 无黑边/画框 | **不通过**——背景群山铺满整个上半区域，没有留出任何干净空白供 logo/标题使用 | 案头一枚朱红印玺道具，尚可 | 不选（留白项不达标） |

**结论：`zm-0` 全项通过，可以直接 Upscale 锁定入库，不需要再等一轮。** `zm-1`/`zm-2` 属于 7.10.3 诊断的同一类黑边/画框问题，`zm-3` 则是单独的"留白不足"问题（山景铺满构图，UI 无处安放）——三张失败图各自的问题都不影响 `zm-0` 已经全项通过的事实，处理方式与坚城/遮蔽一致：一张干净图即可推进。

**入库命名**：`bg_mainmenu_default_1920.png`。

**7.10.6 四类汇总结论**

| 类目 | 能否现在锁定 | 选定图 | 一句话结论 |
|---|---|---|---|
| 坚城 | **能** | `jc-3` | 十项全过，`jc-0`/`jc-2` 因清晰可辨兵群剪影判硬伤不选，不影响本类直接锁定 |
| 粮道 | **不能** | 暂无 | `ld-1`/`ld-2` 因黑边/画框硬伤不选，`ld-0`/`ld-3` 路面有疑似大车的含糊标记待确认，建议合并三个问题重出一轮 |
| 遮蔽 | **能** | `zb-2` | 四张均无硬伤，`zb-2` 留白/纵深最佳，`zb-0` 可作备选 |
| 主菜单 | **能** | `zm-0` | 十项全过（含桌头朱印道具，不违反反落款规则），`zm-1`/`zm-2`/`zm-3` 分别因黑边/画框/留白不足不选 |

坚城、遮蔽、主菜单三类可以立即执行 Upscale，取图片 URL 登记进 §6 素材登记表对应行。粮道需先按 §7.10.2/§7.10.3 补充负面提示重出一轮，通过后再登记。7.10.3 的黑边/画框通用负面提示建议直接并入背景骨架标准负面清单，覆盖全部后续出图（含未来全量 44 张阶段）。

**7.10.7 粮道补负面重出一轮复核（批次 `ld2`，`ld2-0`~`ld2-3`）**

> 按 §7.10.2/§7.10.3 追加"黑边/画框通用负面"+"路面含糊小型印记专属负面"重出一轮，本节为美术总监亲自逐张 Read 复核的结论，独立于项目方初判进行。

| 图 | 黑边/画框类硬伤 | 大车/载具残留 | 结论 |
|---|---|---|---|
| `ld2-0` | 无 | 路面中远段有一处圆桶/桶状轮廓的模糊暗色标记，性质不明确，不能排除是"大车/货物"载具 | 不选（存疑） |
| `ld2-1` | 无 | **不通过**——路旁清晰可辨一辆带弧形篷顶的大车，车轮轮廓明确 | 不选 |
| `ld2-2` | 无 | 通过——通篇复核未见任何车辆/桶状物/人形剪影；路右侧的十字形木桩为地物类栅栏/界桩细节，非载具也非人物，风险等级与 §7.10.4 `zb-0` 的灌木笔触判例一致，不阻塞判定 | **选定** |
| `ld2-3` | 无 | 通过——路两侧仅可见树木，未见车辆或人物剪影 | 尚可，留作备选 |

**结论：黑边/画框问题已彻底解决**（4 张均无 letterbox/装饰画框，证明 §7.10.3 通用负面提示对本类同样生效，且不影响画面构图完整性）。**大车问题：`ld2-2` 复核确认干净**，无残留车辆/桶状物/人物剪影，与项目方初判一致；`ld2-1` 因清晰可辨的带篷大车判定不选（同 §7.10.1 坚城兵群剪影的判定逻辑——载具/人物暗示"事件动态层"内容，不该烧录进静态地貌层）；`ld2-0` 因桶状标记性质存疑，保守不选；`ld2-3` 基本干净，留作备选。

**一句话结论：选定 `ld2-2` Upscale 锁定入库，粮道类通过。至此 MVP 4 类背景骨架（坚城 `jc-3` / 粮道 `ld2-2` / 遮蔽 `zb-2` / 主菜单 `zm-0`）全部锁定完成。**

**入库命名**：`bg_supplyroad_neutral_1920.png`（对齐 §6 登记表既有命名）。

---

## 8. 战场地形背景补充 Prompt（隘口 Pass / 渡口 Ford / 平原 Plain）

> 2026-07-10 新增，**2026-07-10 二次修订**：经项目方校正，`TerrainKind` 真正缺失的战场背景是**隘口 / 渡口 / 平原**三类（坚城已有 `jc-3`、遮蔽已有 `zb-2`），此前 v1 版本"平原复用粮道背景"的方案已作废——"粮道"是区（zone）角色名非地形，二者不应共用同一资产，本节补充第 3 条独立的平原 Prompt（§8.3）。三条新 Prompt 均对齐 `design/art/battle-scene-art-spec.md` §1 地形清单，覆盖 `TerrainKind` 全部 5 值。沿用已锁定的签名图 A `--sref`（`bg3-1`）+ §7.3 v1.2 正面描述方法论（季节中性双重保险 / 无人物排除 / 克制留白构图）+ §7.6 杀折痕负面 + §7.10.3 黑边画框通用负面。**三条新 Prompt 均为「地貌骨架」，不烧录任何已成型战术事件**（火攻火焰、水攻决堤、伏兵、夜袭动态等）——这些属于 `battle-scene-art-spec.md` §7「涌现标记」层，由引擎运行时叠加，不得预先画进背景（沿用 §1.1 三层分工契约 + §7.10 系列裁决标准）。
>
> **命名约定变更**：本节三条新背景导出命名统一采用 `design/gdd/visual-battle-scene.md` §6 资产接口契约表定义的 `bg_zonebattle_[terrain]_1920.png` 格式（而非本文件此前 §7 系列使用的 `bg_[scene]_neutral_1920.png` 格式）。已入库的坚城/遮蔽/主菜单/粮道 4 张背景仍保留其原有 legacy 文件名，是否统一改名详见 `battle-scene-art-spec.md` §1.6，本文件不单方决定，仅在本节新出的 3 张背景上直接采用新命名。

### 8.1 隘口 Pass（山间小路，伏击地形，对应 `TerrainKind.Pass`）

```
A hand-painted illustration of a narrow mountain pass trail cutting between two steep rocky ridgelines, Han-dynasty era military map cartography style, ink outline linework combined with restrained mineral-pigment color washes (muted earth ochre, warm rice-paper beige, faded ink-gray), aged rice paper texture visible in the sky and exposed rock faces, weathered brush-stroke rock contours, the trail winds narrow between the ridges with steep slopes rising on both sides suggesting a natural ambush choke point, scattered boulders and sparse dry scrub along the slopes, composition keeps the trail itself clear and open in the lower-center of the frame with negative space reserved for future troop icon overlays, open sky visible as a narrow strip above the ridgeline, strictly season-neutral with no flowering trees of any kind (no pink or white plum, cherry, or peach blossoms anywhere in the scene), no snow, no vivid red or orange autumn foliage — render all vegetation as plain bare-branch silhouettes or plain muted olive-green scrub only, flat even soft lighting with no strong directional shadow, no human figures anywhere in the scene, no hidden soldiers, no ambush action depicted, painterly illustration texture, muted desaturated palette, wide shot --ar 16:9 --style raw --sref <SIGNATURE_A_URL> --sw 250
```

**负面提示**（通用清单 + 杀折痕 + 黑边画框 + 本场景专属排除）：
```
text, watermark, signature, logo, UI, HUD, speech bubble, modern buildings, cars, roads with asphalt, neon light, glowing effects, anime style, chibi, 3D render look, photographic bokeh, lens flare, people close-up, red seal, cinnabar seal, ink chop stamp, hanko, official stamp, calligraphy inscription, brush-written characters, artist signature mark, decorative corner seal, pink blossom, white blossom, plum blossom, cherry blossom, peach blossom, flowering tree, human figures, people, tiny figures, horizontal fold line, crease line, scroll fold, paper fold seam, folded paper, panel division, split panel, horizontal band, torn paper strip, diptych, color grading mismatch between panels, letterbox bars, black bars, black horizontal bands, cinematic bars, framed border, picture frame, ornate border, decorative frame, card border, poster frame, mounted artwork frame, museum placard, title card, caption plate, text banner area, blank signage area, bordered vignette frame, matting border, gallery frame, exhibition frame, tiny figure on path, tiny walking figure, small silhouette on road, distant lone figure, hidden soldiers, ambush action, weapons, banners, flags
```

**导出命名**：`bg_zonebattle_pass_1920.png`

### 8.2 渡口 Ford（水岸渡口，火攻+水攻地形，对应 `TerrainKind.Ford`）

```
A hand-painted illustration of a wide river ford crossing point seen from a low elevated bank angle, a broad shallow river with a natural fording spot, low reed banks on the near shore and a distant opposite shore with gentle terrain, Han-dynasty era military map cartography style, ink outline linework combined with restrained mineral-pigment color washes (muted slate blue-gray water tone, warm rice-paper beige banks, faded ink-gray), aged rice paper texture visible in the sky and water surface, weathered brush-stroke water ripple contours, calm still water surface with no waves or turbulence, composition keeps the water and near bank clear of clutter in the lower half of the frame with negative space reserved for future troop and event icon overlays, open sky above the far shore, strictly season-neutral with no flowering trees of any kind (no pink or white plum, cherry, or peach blossoms anywhere in the scene), no snow, no vivid red or orange autumn foliage — render all reeds and shrubs as plain muted olive-green only, flat even soft lighting with no strong directional shadow, no human figures anywhere in the scene, no boats, no ships, no fire, no burning, no flooding action depicted, painterly illustration texture, muted desaturated palette, wide shot --ar 16:9 --style raw --sref <SIGNATURE_A_URL> --sw 250
```

**负面提示**（通用清单 + 杀折痕 + 黑边画框 + 渡口专属"事件预烧"排除）：
```
text, watermark, signature, logo, UI, HUD, speech bubble, modern buildings, cars, roads with asphalt, neon light, glowing effects, anime style, chibi, 3D render look, photographic bokeh, lens flare, people close-up, red seal, cinnabar seal, ink chop stamp, hanko, official stamp, calligraphy inscription, brush-written characters, artist signature mark, decorative corner seal, pink blossom, white blossom, plum blossom, cherry blossom, peach blossom, flowering tree, human figures, people, tiny figures, horizontal fold line, crease line, scroll fold, paper fold seam, folded paper, panel division, split panel, horizontal band, torn paper strip, diptych, color grading mismatch between panels, letterbox bars, black bars, black horizontal bands, cinematic bars, framed border, picture frame, ornate border, decorative frame, card border, poster frame, mounted artwork frame, museum placard, title card, caption plate, text banner area, blank signage area, bordered vignette frame, matting border, gallery frame, exhibition frame, boats, ships, war ships, rafts, fire, flames, burning, smoke, flood, rushing water, waves, splashing, cart, wagon, oxcart, handcart, wheeled vehicle
```

**导出命名**：`bg_zonebattle_ford_1920.png`

### 8.3 平原 Plain（开阔草原骑冲地形，对应 `TerrainKind.Plain`，独立新增，不复用「粮道」背景）

> **与 §2.4「粮道」背景的刻意区隔**：`bg_supplyroad_neutral_1920.png`（源图 `ld2-2`）画面主体含"两侧低矮农田脊 + 土路"的地貌线索，服务的是粮道（Supply 区）语义；本条 Prompt 在正负面提示词中都**显式排除道路/车辙/农田脊**等元素，确保画面读作"无路径的开阔草原"，不与粮道图产生语义混淆。

```
A hand-painted illustration of a vast open grassland plain stretching toward a distant low horizon, wide flat terrain suited for sweeping cavalry maneuvers, Han-dynasty era military map cartography style, ink outline linework combined with restrained mineral-pigment color washes (muted grass-green ochre, warm rice-paper beige, faded ink-gray), aged rice paper texture visible in the sky and across the grass field, weathered brush-stroke grass-tuft texture, gently rolling low grass mounds with completely open unbroken ground — no dirt road, no cart track, no wagon path, no trail, no farmland ridges of any kind crossing the field, a few sparse low shrubs and isolated rock outcrops scattered across the open field, composition keeps the central and lower two-thirds of the frame open and uncluttered with negative space reserved for future troop icon overlays, distant low horizon line with open sky filling the upper third of the frame, strictly season-neutral with no flowering trees of any kind (no pink or white plum, cherry, or peach blossoms anywhere in the scene), no snow, no vivid red or orange autumn foliage — render all vegetation as plain muted olive-green grass and scrub only, flat even soft lighting with no strong directional shadow, no human figures anywhere in the scene, no cavalry, no horses, no dust cloud, no charge action depicted, painterly illustration texture, muted desaturated palette, wide shot --ar 16:9 --style raw --sref <SIGNATURE_A_URL> --sw 250
```

**负面提示**（通用清单 + 杀折痕 + 黑边画框 + 平原专属"排除路径/事件预烧"）：
```
text, watermark, signature, logo, UI, HUD, speech bubble, modern buildings, cars, roads with asphalt, neon light, glowing effects, anime style, chibi, 3D render look, photographic bokeh, lens flare, people close-up, red seal, cinnabar seal, ink chop stamp, hanko, official stamp, calligraphy inscription, brush-written characters, artist signature mark, decorative corner seal, pink blossom, white blossom, plum blossom, cherry blossom, peach blossom, flowering tree, human figures, people, tiny figures, horizontal fold line, crease line, scroll fold, paper fold seam, folded paper, panel division, split panel, horizontal band, torn paper strip, diptych, color grading mismatch between panels, letterbox bars, black bars, black horizontal bands, cinematic bars, framed border, picture frame, ornate border, decorative frame, card border, poster frame, mounted artwork frame, museum placard, title card, caption plate, text banner area, blank signage area, bordered vignette frame, matting border, gallery frame, exhibition frame, dirt road, cart track, wagon path, road, path, trail, farmland ridges, plowed field furrows, carts, wagons, oxcart, handcart, wheeled vehicle, horses, cavalry, dust cloud, charging riders, hoofprints, banners, flags, weapons
```

**导出命名**：`bg_zonebattle_plain_1920.png`

### 8.4 与 §6 素材登记表的衔接

本节三张背景（隘口/渡口/平原）出图通过 §3 检查表（含折痕/黑边/人物排除三项既有检查项）后，登记方式对齐 §6 现有体例，新增三行（**registry §6 表格本身暂不动，待出图后由项目方或美术总监正式填入**）：

| 场景/用途 | Prompt 版本 | 生成日期 | Job/图片链接 | 审核结果 | 入库文件名 |
|---|---|---|---|---|---|
| 战役地形-隘口 | §8.1 | 待填 | 待填 | 待填 | `bg_zonebattle_pass_1920.png` |
| 战役地形-渡口 | §8.2 | 待填 | 待填 | 待填 | `bg_zonebattle_ford_1920.png` |
| 战役地形-平原 | §8.3 | 待填 | 待填 | 待填 | `bg_zonebattle_plain_1920.png` |

---
