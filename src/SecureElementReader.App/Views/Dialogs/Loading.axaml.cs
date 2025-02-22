using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SecureElementReader.App.Views.Dialogs
{
    public partial class Loading : DialogWindowBase
    {
        public Loading()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
