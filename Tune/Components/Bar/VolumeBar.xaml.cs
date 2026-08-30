using System.Windows.Controls;

namespace Tune.Components
{
    public partial class VolumeBar : UserControl
    {
        // 内部可用最大宽度（总宽度 526 - 边框 6 - 画布边距 6 - 线宽 3.5，留出余地）
        private new const double MaxWidth = 510;

        public VolumeBar()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 实时更新音量电平（高性能实现）
        /// </summary>
        /// <param name="dbValue">dBFS 值，范围通常在 -60 到 0 之间</param>
        public void UpdateLevel(double dbValue)
        {
            // 1. 将 dBFS (-60 ~ 0) 归一化为 0 ~ 1 的百分比
            double normalized = Math.Clamp((dbValue + 60.0) / 60.0, 0.0, 1.0);

            // 2. 计算实际的像素宽度
            double targetWidth = normalized * MaxWidth;

            // 3. 切换到 UI 线程更新控件 (使用 InvokeAsync 避免阻塞音频回调线程)
            _ = Dispatcher.InvokeAsync(() =>
            {
                if (Progress != null)
                {
                    Progress.Width = targetWidth;
                    // 注意：Line 的 Canvas.Left 已经在 XAML 中绑定到了 Progress.Width
                    // 所以这里修改 Progress.Width 后，UI 会自动重绘将竖线推到正确位置，无需额外代码！
                }
            });
        }
    }
}

/*
💡 进阶提示（针对极致性能优化）：
如果你在极低延迟的音频处理环境（比如 5ms 回调），且不希望每次更新都走 Dispatcher.InvokeAsync，可以改为 “只在音量变化超过阈值时” 才触发 UI 更新。例如：

csharp
private double _lastRenderWidth = -1;
public void UpdateLevel(double dbValue)
{
    double targetWidth = Math.Clamp((dbValue + 60.0) / 60.0, 0.0, 1.0) * MaxWidth;
    
    // 只有当宽度波动超过 1 像素时才刷新 UI，大幅节省资源
    if (Math.Abs(targetWidth - _lastRenderWidth) > 1.0)
    {
        _lastRenderWidth = targetWidth;
        _ = Dispatcher.InvokeAsync(() => Progress.Width = targetWidth);
    }
}
 */