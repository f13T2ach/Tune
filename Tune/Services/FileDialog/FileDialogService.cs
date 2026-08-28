using Microsoft.Win32;

namespace Tune.Services.FileDialog
{
    public class FileDialogService : IFileDialogService
    {
        public string? OpenMusicFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter =
                    "Music File|*.mp3;*.wav;*.flac;*.m4a|All Files|*.*"
            };

            return dialog.ShowDialog() == true
                ? dialog.FileName
                : null;
        }
    }
}
