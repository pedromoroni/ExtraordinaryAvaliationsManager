using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EamBackOffice01;

public static class Helper {
    private static readonly Regex _emailRegex = new("^(?:[a-z0-9!#$%&'*+\\/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+\\/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])$");
    public static bool IsValidEmail(string email) => _emailRegex.IsMatch(email);

    private static readonly Regex _firstNameRegex = new("^^[A-ZÁ-ÚÀ-ÙÃ-ŨÄ-ËÏ-ÏÖ-Ü][a-záàãäåèéêëìíîïòóõöøùúüýÿàá]{0,254}$");
    public static bool IsValidFirstName(string firstName) => _firstNameRegex.IsMatch(firstName);

    private static readonly Regex _lastNameRegex = new("^([A-ZÁ-ÚÀ-ÙÃ-ŨÄ-ËÏ-ÏÖ-Ü][a-záàãäåèéêëìíîïòóõöøùúüýÿ']*|[a-zàáãäåèéêëìíîïòóõöøùúüýÿ]+)( [A-ZÁ-ÚÀ-ÙÃ-ŨÄ-ËÏ-ÏÖ-Üa-záàãäåèéêëìíîïòóõöøùúüýÿ'-]+)*$");
    public static bool IsValidLastName(string lastName) => _lastNameRegex.IsMatch(lastName);

    private static readonly Regex _abbreviationRegex = new("[A-Z]{1,10}");
    public static bool IsValidAbbreviation(string abbreviation) => _abbreviationRegex.IsMatch(abbreviation);

    public static bool IsValidNif(string nif) {
        if (!Regex.IsMatch(nif, @"^\d{9}$")) {
            return false;
        }
        if (nif[0] == '0') {
            return false;
        }

        int checkSum = 0;
        for (int i = 0; i < 8; i++) {
            checkSum += (nif[i] - '0') * (9 - i);
        }

        int checkDigit = (11 - checkSum % 11) % 10;

        return checkDigit == (nif[8] - '0');
    }
    public static string FilterName(string name) => name.Trim().Length <= 1
        ? name.Trim()
        : name
            .Trim()
            .Split([' '], StringSplitOptions.RemoveEmptyEntries)
            .Aggregate((a, b) => a + " " + b);

    public static Image GetStretchedImage(string imagePath, int width, int height) {
        using Image originalImage = Image.FromFile(imagePath);
        var resizedImage = new Bitmap(width, height);
        using (Graphics g = Graphics.FromImage(resizedImage)) {
            g.DrawImage(originalImage, 0, 0, width, height);
        }
        return resizedImage;
    }
    public static void InjectToComboBox(ComboBox comboBox, IEnumerable<string> list) {
        comboBox.Items.Clear();
        comboBox.Items.AddRange([.. list]);
    }
    public static void InjectToComboBox<T>(
        ComboBox comboBox,
        IEnumerable<T> list,
        Expression<Func<T, object>> selector
    ) {
        comboBox.Items.Clear();
        var compiledSelector = selector.Compile();
        foreach (var item in list) {
            comboBox.Items.Add(compiledSelector(item).ToString()!);
        }
    }
    public static byte[] ImageToByteArray(Image image) {
        using var memoryStream = new MemoryStream();
        var encoder = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()
            .First(c => c.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);

        // por motivos desconhecidos, a image fazia 💥, por isso usei bitmap
        var bitmap = new Bitmap(image);

        bitmap.Save(memoryStream, encoder, null);
        return memoryStream.ToArray();
    }
    public static void InjectToListView<T>(
        ListView listView,
        IEnumerable<T> list,
        IEnumerable<(ColumnHeader Column, Expression<Func<T, object>> Property)> selectors
    ) {
        listView.Items.Clear();
        listView.Columns.Clear();
        listView.Columns.AddRange(selectors
            .Select(s => s.Column)
            .ToArray()
        );
        foreach (var item in list) {
            if (item != null) {
                var compiledSelectors = selectors
                    .Select(s => s.Property.Compile());

                var subItems = compiledSelectors
                    .Select(cs => cs(item).ToString()!);

                listView.Items.Add(new ListViewItem([.. subItems]));
            }
        }
    }
    public static void ListViewCheckedItemsClear(ListView listView) {
        for (int i = 0; listView.CheckedItems.Count > 0; i++) {
            if (listView.Items[i].Checked) {
                listView.Items[i].Checked = false;
                i--;
            }
        }
    }

    public static Image? ByteArrayToImage(byte[] byteArray) {
        using var ms = new MemoryStream(byteArray);

        try {
            return Image.FromStream(ms);
        } catch {
            MessageBox.Show(
                "Couldn't load the image, select other one",
                "COULDN'T LOAD THE IMAGE",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return null;
        }
    }
}