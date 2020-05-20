使用说明：

请先运行Server与Agent，然后运行InteractionFinal.exe。

如果卡在初始场景，可能是缺少hpsocket4c，解决方法在群里。

如果进去卡在黑屏，说明一直在尝试连接agent，请检查端口对不对。

开始会有一些选项：窗口尺寸，是否全屏，画质等，自行勾选即可。

建立连接后进入游戏画面：

移动：WASD，组合键可以斜向走，如WD是向右上方走。

拾取：F

丢弃：丢弃食物是R，丢弃道具是T，若要指定投掷距离请同时按下对应数字键（1~4），不按数字键默认距离为0，扔到自己脚下。

使用：使用道具是U，使用工作台/提交点是I。

（由于API更新了，目前界面输入和server的API还没对接好，所以除了移动之外的操作会不正常，比如捡不起东西）

按退出键可以退出游戏。请先退出Client再关闭Server和Agent，否则Client将失去响应，只能强制结束进程。

在ClientConfig.json中设置agent端口，debug输出等级以及播放模式（0是实时对战，1是回放模式），与exe放在同一目录下。（Mac和Linux可能要自己研究下）

回放模式说明：将server.playback文件放在同目录下，playMode设置为1，playbackPath为回访文件路径，运行界面程序即可。按下数字1~8可以切换跟随视角。拖动滑块调整播放速度。通过鼠标滚轮调整缩放。按下Y键切换至自由视角，鼠标左键拖动视角。

并不是所有东西都能找到很合适的模型。。。先凑活着用吧。

unity的log路径参见https://docs.unity3d.com/Manual/LogFiles.html。

通过命令行参数指定log路径https://docs.unity3d.com/Manual/CommandLineArguments.html

