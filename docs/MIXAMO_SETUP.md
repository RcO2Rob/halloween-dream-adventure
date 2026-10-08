# 在新克隆的项目中恢复 Timmy

本机现有项目已包含素材，无需重新设置。此说明用于从公开 GitHub 仓库克隆到另一台机器的情况。

Mixamo 允许在游戏中使用角色和动画，但其 [分发说明](https://community.adobe.com/questions-696/mixamo-faq-licensing-royalties-ownership-eula-and-tos-589400?lang=en) 不允许向非团队成员公开分发原始角色和动画文件。因此公开仓库保留脚本、场景、Prefab、动画控制器、导入设置及素材来源记录，排除 Mixamo FBX 和提取的 PNG。未补齐素材时，角色模型和动画无法正常显示，不能直接试玩或构建完整游戏。

## 为你自己的另一台电脑恢复

在首次用 Unity 打开新克隆项目之前，从你本机的项目复制 `Assets/ThirdParty/Mixamo/Timmy` 文件夹到新项目的相同位置，保留仓库中的 `.meta` 文件。这会恢复五个 FBX 和四张贴图，并保持场景的资源引用。素材仍会被 Git 忽略，不会随下次提交公开上传。不要提交 `Library` 或 `Builds`。

如果已经用 Unity 打开过缺少素材的克隆项目，先关闭 Unity，再恢复素材以及仓库中的原始 `.meta` 文件，之后重新打开项目。

## 其他使用者自行从 Mixamo 获取

登录自己的 [Mixamo](https://www.mixamo.com/) 账号，选择 **Timmy**，使用 **FBX for Unity** 格式导出。模型选择 T-pose、With Skin。四个动画选择 Without Skin、30 FPS、无关键帧缩减；Running 启用 In Place。对应路径如下：

| 下载内容 | 项目中的文件路径 |
| --- | --- |
| Timmy，T-pose | `Assets/ThirdParty/Mixamo/Timmy/Models/Timmy.fbx` |
| Standing Idle | `Assets/ThirdParty/Mixamo/Timmy/Animations/Idle.fbx` |
| Running With Intention | `Assets/ThirdParty/Mixamo/Timmy/Animations/Running.fbx` |
| Standing Aim Idle 01 | `Assets/ThirdParty/Mixamo/Timmy/Animations/BowAim.fbx` |
| Throw | `Assets/ThirdParty/Mixamo/Timmy/Animations/Throw.fbx` |

先把五个文件放入上述位置，保留原始 `.meta`，再用 **Unity 6000.6.2f1** 打开项目。在 Timmy 模型的 Inspector → Materials 中使用 **Extract Textures**，输出到 `Assets/ThirdParty/Mixamo/Timmy/Textures`。该文件夹中的原始 `.meta` 对应 `Ch09_1001_Diffuse.png`、`Ch09_1001_Normal.png`、`Ch09_1001_Glossiness.png` 和 `Ch09_1001_Specular.png`。如果打开 Unity 时缺少 PNG 导致它清理了这些 `.meta`，在关闭 Unity 后从仓库恢复原始 `.meta`，再重新导入贴图。

保存的场景、角色 Prefab、控制器和导入设置已提供，恢复素材时无需重新生成地图。之后打开 `Assets/Scenes/01_FirstLight.unity` 试玩，并使用 **Lost Dream → Validate prototype scenes** 检查角色和各关卡引用。

原始下载设置和 SHA-256 记录在 `Assets/ThirdParty/Mixamo/Timmy/SOURCE.json`。Mixamo 后续导出的文件可能不同；若验证未通过，应先检查模型、动画名称、导入设置和资源引用。上述自行下载流程是恢复说明，本次同步没有在第二台电脑重新下载验证。
