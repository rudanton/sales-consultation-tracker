using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace ConsultNote;

public partial class AddCustomerFileDialog : Window
{
    public static readonly string[] FileTypes =
    [
        "견적",
        "면허증",
        "사업자등록증",
        "법인사업자등록증앞면",
        "재무제표",
        "부가세과세표준증명원",
        "법인주주명부",
        "법인등기부등본",
        "법인 인감증명서",
        "대표자 개인인감증명서",
        "운전경력증명서",
        "주민등록등본",
        "소득증빙서류",
        "계약서",
        "기타",
    ];

    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".pdf",
    };

    public AddCustomerFileDialog()
    {
        InitializeComponent();
        FileTypeComboBox.ItemsSource = FileTypes;
        FileTypeComboBox.SelectedIndex = 0;
    }

    public string SourceFilePath => FilePathTextBox.Text.Trim();

    public string OriginalFileName => Path.GetFileName(SourceFilePath);

    public string DisplayName => TrimToNull(DisplayNameTextBox.Text) ?? OriginalFileName;

    public string FileType => FileTypeComboBox.SelectedItem?.ToString() ?? FileTypes[0];

    public string? CustomFileType => TrimToNull(CustomFileTypeTextBox.Text);

    public string? Memo => TrimToNull(MemoTextBox.Text);

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "지원 파일 (*.jpg;*.jpeg;*.png;*.pdf)|*.jpg;*.jpeg;*.png;*.pdf",
            Multiselect = false,
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        FilePathTextBox.Text = dialog.FileName;
        if (string.IsNullOrWhiteSpace(DisplayNameTextBox.Text))
        {
            DisplayNameTextBox.Text = Path.GetFileName(dialog.FileName);
        }
    }

    private void FileTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        CustomFileTypePanel.Visibility = FileType == "기타" ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SourceFilePath) || !File.Exists(SourceFilePath))
        {
            MessageBox.Show("추가할 파일을 선택해주세요.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var extension = Path.GetExtension(SourceFilePath);
        if (!SupportedExtensions.Contains(extension))
        {
            MessageBox.Show(".jpg, .jpeg, .png, .pdf 파일만 추가할 수 있습니다.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (FileType == "기타" && string.IsNullOrWhiteSpace(CustomFileType))
        {
            MessageBox.Show("기타 파일 유형명을 입력해주세요.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        DialogResult = true;
    }

    private static string? TrimToNull(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
