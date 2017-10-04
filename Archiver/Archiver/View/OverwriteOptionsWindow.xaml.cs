using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Archiver.Model;

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
                case OverwriteOptions.ReplaceExistingKeepImages:
                    this.ReplaceExistingKeepImages.IsChecked = true;
                    break;
                case OverwriteOptions.ReplaceExistingReplaceImages:
                    this.ReplaceExistingReplaceImages.IsChecked = true;
                    break;
                case OverwriteOptions.KeepExistingIgnoreNew:
                    this.KeepExistingIgnoreNew.IsChecked = true;
                    break;
                case OverwriteOptions.KeepExistingRenameNew:
                    this.KeepExistingRenameNew.IsChecked = true;
                    break;
                case OverwriteOptions.KeepExistingAbortWarn:
                    this.KeepExistingAbortWarn.IsChecked = true;
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
                if (this.ReplaceExistingKeepImages.IsChecked is bool Checked && Checked)
                    option = OverwriteOptions.ReplaceExistingKeepImages;
            }
            {
                if (this.ReplaceExistingReplaceImages.IsChecked is bool Checked && Checked)
                    option = OverwriteOptions.ReplaceExistingReplaceImages;
            }
            {
                if (this.KeepExistingIgnoreNew.IsChecked is bool Checked && Checked)
                    option = OverwriteOptions.KeepExistingIgnoreNew;
            }
            {
                if (this.KeepExistingRenameNew.IsChecked is bool Checked && Checked)
                    option = OverwriteOptions.KeepExistingRenameNew;
            }
            {
                if (this.KeepExistingAbortWarn.IsChecked is bool Checked && Checked)
                    option = OverwriteOptions.KeepExistingAbortWarn;
            }

            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
