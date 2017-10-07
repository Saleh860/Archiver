using System.Windows;
using static Archiver.Model.Object;
using static Archiver.Model.Object.OverwriteOptions;

namespace Archiver.View
{
    /// <summary>
    /// Interaction logic for OverwriteFlags.xaml
    /// </summary>
    public partial class OverwriteOptionsWindow : Window
    {
        public OverwriteOptions Option => option;
        private OverwriteOptions option;

        public OverwriteOptionsWindow(OverwriteOptions option, Window owner)
        {
            InitializeComponent();
            Owner = owner;

            switch(option)
            {
                case ReplaceExistingKeepImages:
                    optReplaceExistingKeepImages.IsChecked = true;
                    break;
                case ReplaceExistingReplaceImages:
                    optReplaceExistingReplaceImages.IsChecked = true;
                    break;
                case KeepExistingIgnoreNew:
                    optKeepExistingIgnoreNew.IsChecked = true;
                    break;
                case KeepExistingRenameNew:
                    optKeepExistingRenameNew.IsChecked = true;
                    break;
                case KeepExistingAbortWarn:
                    optKeepExistingAbortWarn.IsChecked = true;
                    break;
                default:
                    break;
            }
        }

        public new OverwriteOptions ShowDialog()
        {
            base.ShowDialog();
            return option;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            {
                if (optReplaceExistingKeepImages.IsChecked is bool Checked && Checked)
                    option = ReplaceExistingKeepImages;
            }
            {
                if (optReplaceExistingReplaceImages.IsChecked is bool Checked && Checked)
                    option = ReplaceExistingReplaceImages;
            }
            {
                if (optKeepExistingIgnoreNew.IsChecked is bool Checked && Checked)
                    option = KeepExistingIgnoreNew;
            }
            {
                if (optKeepExistingRenameNew.IsChecked is bool Checked && Checked)
                    option = KeepExistingRenameNew;
            }
            {
                if (optKeepExistingAbortWarn.IsChecked is bool Checked && Checked)
                    option = KeepExistingAbortWarn;
            }

            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
