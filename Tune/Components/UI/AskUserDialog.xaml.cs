using System.Windows;
using Tune.Assets.Localization;
using Tune.Models.NewDialog;

namespace Tune.Components.UI
{
    /// <summary>
    /// The one dialog every question of the app goes through: a framed box with a message
    /// and either a pair of buttons or a single one that just closes it.
    /// </summary>
    public partial class AskUserDialog : DialogContentBase
    {
        public static readonly DependencyProperty DialogTitleProperty =
            DependencyProperty.Register(nameof(DialogTitle), typeof(string), typeof(AskUserDialog),
                new PropertyMetadata(string.Empty, OnContentChanged));

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(string), typeof(AskUserDialog),
                new PropertyMetadata(string.Empty, OnContentChanged));

        public static readonly DependencyProperty ConfirmTextProperty =
            DependencyProperty.Register(nameof(ConfirmText), typeof(string), typeof(AskUserDialog),
                new PropertyMetadata(string.Empty, OnContentChanged));

        public static readonly DependencyProperty CancelTextProperty =
            DependencyProperty.Register(nameof(CancelText), typeof(string), typeof(AskUserDialog),
                new PropertyMetadata(string.Empty, OnContentChanged));

        public static readonly DependencyProperty ShowCancelProperty =
            DependencyProperty.Register(nameof(ShowCancel), typeof(bool), typeof(AskUserDialog),
                new PropertyMetadata(true, OnContentChanged));

        /// <summary>Gets or sets the title shown in the frame of the dialog.</summary>
        public string DialogTitle
        {
            get { return (string)GetValue(DialogTitleProperty); }
            set { SetValue(DialogTitleProperty, value); }
        }

        /// <summary>Gets or sets the question shown in the middle of the dialog.</summary>
        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        /// <summary>Gets or sets the label of the confirming button; empty uses the default.</summary>
        public string ConfirmText
        {
            get { return (string)GetValue(ConfirmTextProperty); }
            set { SetValue(ConfirmTextProperty, value); }
        }

        /// <summary>Gets or sets the label of the cancelling button; empty uses the default.</summary>
        public string CancelText
        {
            get { return (string)GetValue(CancelTextProperty); }
            set { SetValue(CancelTextProperty, value); }
        }

        /// <summary>Gets or sets a value indicating whether the cancelling button is offered.</summary>
        public bool ShowCancel
        {
            get { return (bool)GetValue(ShowCancelProperty); }
            set { SetValue(ShowCancelProperty, value); }
        }

        /// <summary>Creates the dialog.</summary>
        public AskUserDialog()
        {
            InitializeComponent();
        }

        /// <summary>Asks a question the user can confirm or cancel.</summary>
        /// <param name="title">Title shown in the frame of the dialog.</param>
        /// <param name="message">Question shown in the middle of the dialog.</param>
        /// <returns>True when the user confirmed.</returns>
        public static bool Ask(string title, string message) =>
            Ask(title, message, string.Empty, string.Empty);

        /// <summary>Asks a question with labels of the caller's own.</summary>
        /// <param name="title">Title shown in the frame of the dialog.</param>
        /// <param name="message">Question shown in the middle of the dialog.</param>
        /// <param name="confirmText">Label of the confirming button; empty uses the default.</param>
        /// <param name="cancelText">Label of the cancelling button; empty uses the default.</param>
        /// <returns>True when the user confirmed.</returns>
        public static bool Ask(string title, string message, string confirmText, string cancelText) =>
            new AskUserDialog
            {
                DialogTitle = title,
                Message = message,
                ConfirmText = confirmText,
                CancelText = cancelText
            }.ShowDialog();

        /// <summary>Tells the user something, with a single button that closes the dialog.</summary>
        /// <param name="title">Title shown in the frame of the dialog.</param>
        /// <param name="message">Text shown in the middle of the dialog.</param>
        public static void Show(string title, string message) =>
            new AskUserDialog
            {
                DialogTitle = title,
                Message = message,
                ShowCancel = false
            }.ShowDialog();

        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            ((AskUserDialog)d).ApplyContent();

        /// <summary>Copies the properties onto the parts of the template.</summary>
        private void ApplyContent()
        {
            PART_Title.Title = DialogTitle;
            PART_Message.Text = Message;
            PART_Confirm.Text = string.IsNullOrEmpty(ConfirmText) ? LangStrings.Confirm : ConfirmText;
            PART_Cancel.Text = string.IsNullOrEmpty(CancelText) ? LangStrings.Cancel : CancelText;
            PART_Cancel.Visibility = ShowCancel ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e) => Confirm();

        private void OnCancelClick(object sender, RoutedEventArgs e) => Cancel();
    }
}
