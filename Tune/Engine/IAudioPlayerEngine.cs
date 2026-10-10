using System.ComponentModel;

namespace Tune.Engine
{
    public interface IAudioPlayerEngine : INotifyPropertyChanged
    {
        /// <summary>
        /// 当前播放时间（秒）
        /// </summary>
        double CurrentSeconds { get; set; }


        /// <summary>
        /// 音频总长度（秒）
        /// </summary>
        double TotalSeconds { get; }


        /// <summary>
        /// 是否正在播放
        /// </summary>
        bool IsPlaying { get; }


        /// <summary>
        /// 是否已经加载文件
        /// </summary>
        bool IsFileLoaded { get; }


        /// <summary>
        /// 播放进度更新
        /// </summary>
        event EventHandler<double>? ProgressUpdated;

        /// <summary>
        /// 当音乐播放到结尾时触发
        /// </summary>
        event EventHandler? PlaybackEnded;


        /// <summary>
        /// 加载音乐文件
        /// </summary>
        void LoadFile(string filePath);


        /// <summary>
        /// 播放
        /// </summary>
        void Play();

        //void Seek(double seconds);


        /// <summary>
        /// 暂停
        /// </summary>
        void Pause();


        /// <summary>
        /// 停止播放并回到开头，保留当前音乐
        /// </summary>
        void Stop();


        /// <summary>
        /// 卸载当前音乐并释放资源
        /// </summary>
        void Unload();
    }
}