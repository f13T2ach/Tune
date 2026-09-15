using Tune.Models.Dialog;
using Tune.Models.Music;

namespace Tune.Generic.DialogGenerics
{
    /// <summary>
    /// Generic Implement of `DialogBase<object, Music>`
    /// Xaml doesn't support generic class
    /// </summary>
    public class DialogBaseObjectMusic : DialogBase<object, Music>
    {
        public DialogBaseObjectMusic() : base() { }
    }
}
