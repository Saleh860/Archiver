using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using IO = System.IO;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using Archiver.Model;

namespace Archiver.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Model.Object> localClipboard;

        public MainWindow()
        {
            InitializeComponent();
            Title = "الأرشيف :";
            mnuSave.IsEnabled = false;
            mnuSaveAs.IsEnabled = false;
            mnuIntegrityCheck.IsEnabled = false;

            localClipboard=null;
        //            CreateArchive();
    }

    private void CreateArchive()
        {
            if (CloseArchiveIfOpen())
            {
                SaveFileDialog dlg = new SaveFileDialog
                {
                    FileName = Archive.FileName,
                    OverwritePrompt = false
                };

                if (dlg.ShowDialog() == true)
                {
                    string pathName = dlg.FileName;
                    IO.DirectoryInfo path = new IO.FileInfo(pathName).Directory;
                    IO.FileInfo[] files = path.GetFiles();
                    IO.DirectoryInfo[] dirs = path.GetDirectories();
                    if (files.GetLength(0) > 0 || dirs.GetLength(0) > 0)
                    {
                        if(MessageBox.Show(this, "المجلد المختار ليس فارغا، هل أنت مصمم على مسح كافة محتويات المجلد؟", "أرشيف جديد", MessageBoxButton.YesNo)==MessageBoxResult.Yes)
                        {
                            foreach(IO.FileInfo f in files)
                            {
                                f.Delete();
                            }
                            foreach(IO.DirectoryInfo d in dirs)
                            {
                                d.Delete(true);
                            }
                        }
                        else
                        {
                            return;
                        }
                    }

                    Archive.Create(path.FullName);

                    Title = "الأرشيف :" + Archive.PathName;
                    mnuSave.IsEnabled = true;
                    mnuSaveAs.IsEnabled = true;
                    mnuIntegrityCheck.IsEnabled = true;
                    localClipboard = null;

                    FoldersView.DataContext = Archive.RootDirectory;
                    FoldersView.ItemsSource = Archive.RootDirectory.Subdirectories;
                }
            }
        }

        private void RefreshFoldersView()
        {
            FoldersView.ItemsSource = null;
            FoldersView.ItemsSource = Archive.RootDirectory.Subdirectories;
        }

        private void MenuNew_Click(object sender, RoutedEventArgs e)
        {
            CreateArchive();
        }

        private void MenuLoad_Click(object sender, RoutedEventArgs e)
        {
            if (CloseArchiveIfOpen())
            {
                OpenFileDialog dlg = new OpenFileDialog
                {
                    FileName = "archive.bin",
                    ReadOnlyChecked = true,
                    Multiselect = false
                };

                if (dlg.ShowDialog() == true)
                {
                    string Filename = dlg.FileName;
                    Archive.Load(Filename);

                    Title = "الأرشيف : " + Archive.PathName;
                    mnuSave.IsEnabled = true;
                    mnuSaveAs.IsEnabled = true;
                    mnuIntegrityCheck.IsEnabled = true;
                    localClipboard = null;

                    FoldersView.DataContext = Archive.RootDirectory;
                    FoldersView.ItemsSource = Archive.RootDirectory.Subdirectories;
                }
            }
        }

        private void MenuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveArchiveAs();
        }

        //return true if the saving operation was complete, or false if canceled
        private bool SaveArchiveAs()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            if (dlg.ShowDialog() == true)
            {
                string Filename = dlg.FileName;

                Archive.SaveAs(Filename);

                Title = "الأرشيف :" + Archive.PathName;
                mnuSave.IsEnabled = true;
                mnuSaveAs.IsEnabled = true;
                mnuIntegrityCheck.IsEnabled = true;

                return true;
            }
            return false;
        }

        private void MenuSave_Click(object sender, RoutedEventArgs e)
        {
            SaveArchive();
        }

        //returns true if the Save operation was successful
        private bool SaveArchive()
        {
            FoldersView.ItemsSource = null;

            bool result = Archive.Save() ;

            FoldersView.ItemsSource = Archive.RootDirectory.Subdirectories;

            return result;
        }

        //returns false if canceled
        //returns true if no archive is open or the user chose not to save it or if it is already saved.
        private bool CloseArchiveIfOpen()
        {
            if (Archive.IsOpen())
            {
                switch (MessageBox.Show(this, "هل تريد حفظ الأرشيف الحالي؟", "أرشيف جديد", MessageBoxButton.YesNoCancel))
                {
                    case MessageBoxResult.Yes:
                        if (!SaveArchive())
                            return false;
                        break;

                    case MessageBoxResult.Cancel:
                        return false;
                }
                Archive.Close();

                Title = "الأرشيف :";
                mnuSave.IsEnabled = false;
                mnuSaveAs.IsEnabled = false;
                mnuIntegrityCheck.IsEnabled = false;

                localClipboard = null;
                FoldersView.ItemsSource = null;
            }
            return true;
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !CloseArchiveIfOpen();
        }

        private void DropFiles(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement Source && 
                Source.DataContext is Directory targetDirectory &&
                e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                OverwriteOptions option = OverwriteOptions.KeepExistingAbortWarn;

                foreach (string file in files)
                {
                    if (targetDirectory.GetChild(new IO.FileInfo(file).Name) is Model.Object obj)
                    {
                        OverwriteOptionsWindow dlg = new OverwriteOptionsWindow(option, this);

                        option = dlg.ShowDialog();

                        break;
                    }
                }
                Archive.ArchiveObjects(files, targetDirectory, option);

                RefreshFoldersView();
            }
        }

        private void FoldersView_Drop(object sender, DragEventArgs e)
        {
            if (Archive.IsOpen())
            {
                DropFiles(sender, e);

                e.Handled = true;
            }
            else
            {
                MessageBox.Show(this, "لا يوجد أرشيف مفتوح. افتح أرشيفا أو أنشئ أرشيفا جديدا", "إضافة ملفات للأرشيف", MessageBoxButton.OK);
            }
        }


        private void FoldersView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            FilesView.ItemsSource = null;

            if (FoldersView.SelectedItem is Directory directory)
                FilesView.ItemsSource = directory.Files;
        }

        private void ShowMaster_Click(object sender, RoutedEventArgs e)
        {
            if (FoldersView.SelectedItem is ImageDirectory image)
            {
                image.IsSelected = false;
                //MessageBox.Show(image.Master.FullName);
                (image.Master.Parent as Directory).ExpandPath();
                (image.Master as Directory).IsSelected = true;
                e.Handled = true;
            }
        }

        private void ImageDirectory_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && e.LeftButton == MouseButtonState.Pressed)
            {
                ShowMaster_Click(sender, e);
            }
        }

        private void ShowFolderContextMenu(object sender, RoutedEventArgs e)
        {

        }

        private SaveFileDialog ExportFolderDialog;

        private void ExportFolder_Click(object sender, RoutedEventArgs e)
        {
            if (FoldersView.SelectedItem is Model.Object obj)
            {
                try
                {
                    ExportFolderDialog = new SaveFileDialog()
                    {
                        Title = "تصدير مجلد",
                        FileName = obj.Name,
                        Tag = obj
                    };

                    ExportFolderDialog.FileOk += ExportFolderDlg_FileOk;
                    ExportFolderDialog.ShowDialog();
                }
                finally
                {
                    ExportFolderDialog = null;
                }
            }
        }

        private void ExportFolderDlg_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Assume success
            e.Cancel = false;

            //Check if a folder with the same name exists
            if (IO.Directory.Exists(ExportFolderDialog.FileName))
            {
                if (MessageBox.Show(this, "يوجد مجلد بنفس الاسم في هذا المكان، هل تريد استبداله بالمجلد الذي يتم تصديره؟", "تصدير مجلد", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    //Try to delete the folder, and if unsuccessful abort
                    try
                    {
                        IO.Directory.Delete(ExportFolderDialog.FileName, true);
                    }
                    catch
                    {
                        e.Cancel = true;
                    }
                }
            }

            if(!e.Cancel)
            {
                if(ExportFolderDialog.Tag is Model.Object obj)
                {
                    obj.Export(ExportFolderDialog.FileName);
                }
            }
        }

        private bool isCut;

        private void CopyFolder_Click(object sender, RoutedEventArgs e)
        {
            if (FoldersView.SelectedItem is Model.Object obj)
            {
                localClipboard = new List<Model.Object>()
                {
                    obj
                };

                isCut = false;
            }
        }

        private void CutFolder_Click(object sender, RoutedEventArgs e)
        {
            if (FoldersView.SelectedItem is Model.Object obj)
            {
                localClipboard = new List<Model.Object>()
                {
                    obj
                };

                isCut = true;
            }
        }

        private void PasteFolder_Click(object sender, RoutedEventArgs e)
        {
            if (FoldersView.SelectedItem is Directory parent)
            {
                if(localClipboard!= null)
                {
                    OverwriteOptions option = OverwriteOptions.NotSet;

                    foreach (Model.Object obj in localClipboard)
                    {
                        if (parent.GetChild(obj.Name) is Model.Object existing)
                        {
                            if (option == OverwriteOptions.NotSet)
                            {
                                OverwriteOptionsWindow dlg = new OverwriteOptionsWindow(option, this);

                                option = dlg.ShowDialog();

                                break;
                            }
                        }
                        try
                        {
                            if (isCut)
                                obj.Parent = parent;
                            else
                                obj.CopyTo(obj.Name, parent);

                            RefreshFoldersView();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(this, ex.Message);
                        }

                    }
                }
            }
        }

        private void DeleteFolder_Click(object sender, RoutedEventArgs e)
        {
            if (FoldersView.SelectedItem is Model.Object obj)
            {
                if(obj is Master master)
                {
                    if(!master.Delete(MasterDeleteOptions.AbortIfHasImages))
                    {
                        switch(MessageBox.Show("قد يكون لهذا المجلد أو الملف صور تعتمد عليه، هل تريد الإبقاء على تلك الصور؟", "حذف مجلد", MessageBoxButton.YesNoCancel))
                        {
                            case MessageBoxResult.Yes:
                                master.Delete(MasterDeleteOptions.MakeFirstImageMaster);
                                break;
                            case MessageBoxResult.No:
                                master.Delete(MasterDeleteOptions.DeleteAllImages);
                                break;
                            default:
                                break;
                        }

                    }
                }
                else
                {
                    obj.Delete();
                }
                RefreshFoldersView();
            }
        }

        private void DeleteFolderImage_Click(object sender, RoutedEventArgs e)
        {
            if (FoldersView.SelectedItem is ImageDirectory image)
            {
                image.Delete();
            }
        }

        private void MakeMaster_Click(object sender, RoutedEventArgs e)
        {
            if (FoldersView.SelectedItem is ImageDirectory image)
            {
                //Swap the master with the image
                image.MakeMaster();

                RefreshFoldersView();
            }
        }

        private void MnuIntegrityCheck_Click(object sender, RoutedEventArgs e)
        {
            if(Archive.IsOpen())
            {
                IntegrityCheckResults results = new IntegrityCheckResults();
                Archive.IntegrityCheck(results);

                string message = results.ToString();

                if (message == string.Empty)
                    message = "لم يتم العثور على أي أخطاء";

                if (MessageBox.Show(this, message, "إصلاح الأخطاء", MessageBoxButton.OKCancel)==MessageBoxResult.OK)
                {
                    foreach(IntegrityCheckResult result in results)
                    {
                        result.Fix();
                    }
                }

            }
        }
    }
}
