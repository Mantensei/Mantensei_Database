using Mantensei_Database.Common;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Mantensei_Database.Windows
{
    public partial class ImageCropWindow : Window
    {
        private bool _isDragging = false;
        private Point _startPoint;
        private BitmapSource _originalImage;
        private double _imageScaleX = 1.0;
        private double _imageScaleY = 1.0;
        private Point _imageOffset;

        public BitmapSource CroppedImage { get; private set; }

        public ImageCropWindow(string imagePath)
        {
            InitializeComponent();
            LoadImage(imagePath);
        }

        private void LoadImage(string imagePath)
        {
            try
            {
                _originalImage = new BitmapImage(new Uri(imagePath));
                SourceImage.Source = _originalImage;

                // キャンバスサイズに合わせて画像をスケーリング
                Loaded += (s, e) => UpdateImageScale();
                SizeChanged += (s, e) => UpdateImageScale();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"画像の読み込みに失敗しました: {ex.Message}", "エラー",
                               MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void UpdateImageScale()
        {
            if (_originalImage == null) return;

            var canvasWidth = ImageCanvas.ActualWidth;
            var canvasHeight = ImageCanvas.ActualHeight;

            if (canvasWidth == 0 || canvasHeight == 0) return;

            var imageWidth = _originalImage.PixelWidth;
            var imageHeight = _originalImage.PixelHeight;

            // 画像をキャンバスに合わせてスケーリング
            var scaleX = canvasWidth / imageWidth;
            var scaleY = canvasHeight / imageHeight;
            var scale = Math.Min(scaleX, scaleY);

            var scaledWidth = imageWidth * scale;
            var scaledHeight = imageHeight * scale;

            _imageScaleX = scale;
            _imageScaleY = scale;
            _imageOffset = new Point(
                (canvasWidth - scaledWidth) / 2,
                (canvasHeight - scaledHeight) / 2
            );

            // 画像の位置とサイズを更新
            Canvas.SetLeft(SourceImage, _imageOffset.X);
            Canvas.SetTop(SourceImage, _imageOffset.Y);
            SourceImage.Width = scaledWidth;
            SourceImage.Height = scaledHeight;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _startPoint = e.GetPosition(ImageCanvas);
            ImageCanvas.CaptureMouse();

            SelectionRectangle.Visibility = Visibility.Visible;
            DarkOverlay.Visibility = Visibility.Visible;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;

            var currentPoint = e.GetPosition(ImageCanvas);
            var rect = new Rect(_startPoint, currentPoint);

            // 選択範囲を画像の範囲内に制限
            var imageRect = new Rect(_imageOffset.X, _imageOffset.Y,
                                   SourceImage.Width, SourceImage.Height);
            rect.Intersect(imageRect);

            // 選択矩形を更新
            Canvas.SetLeft(SelectionRectangle, rect.X);
            Canvas.SetTop(SelectionRectangle, rect.Y);
            SelectionRectangle.Width = rect.Width;
            SelectionRectangle.Height = rect.Height;

            // プレビューを更新
            UpdatePreview(rect);
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            ImageCanvas.ReleaseMouseCapture();
        }

        private void UpdatePreview(Rect selectionRect)
        {
            if (selectionRect.Width <= 0 || selectionRect.Height <= 0) return;

            try
            {
                // 選択範囲を元画像の座標系に変換
                var imageX = (selectionRect.X - _imageOffset.X) / _imageScaleX;
                var imageY = (selectionRect.Y - _imageOffset.Y) / _imageScaleY;
                var imageWidth = selectionRect.Width / _imageScaleX;
                var imageHeight = selectionRect.Height / _imageScaleY;

                // 範囲をクリップ
                imageX = Math.Max(0, imageX);
                imageY = Math.Max(0, imageY);
                imageWidth = Math.Min(_originalImage.PixelWidth - imageX, imageWidth);
                imageHeight = Math.Min(_originalImage.PixelHeight - imageY, imageHeight);

                if (imageWidth > 0 && imageHeight > 0)
                {
                    var cropRect = new Int32Rect(
                        (int)imageX, (int)imageY,
                        (int)imageWidth, (int)imageHeight
                    );

                    var croppedImage = new CroppedBitmap(_originalImage, cropRect);
                    PreviewImage.Source = croppedImage;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"プレビュー更新エラー: {ex.Message}");
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            SelectionRectangle.Visibility = Visibility.Collapsed;
            DarkOverlay.Visibility = Visibility.Collapsed;
            PreviewImage.Source = null;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (SelectionRectangle.Visibility == Visibility.Collapsed)
            {
                MessageBox.Show("トリミング範囲を選択してください。", "確認",
                               MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // 最終的なトリミング画像を作成
                var selectionRect = new Rect(
                    Canvas.GetLeft(SelectionRectangle),
                    Canvas.GetTop(SelectionRectangle),
                    SelectionRectangle.Width,
                    SelectionRectangle.Height
                );

                var imageX = (selectionRect.X - _imageOffset.X) / _imageScaleX;
                var imageY = (selectionRect.Y - _imageOffset.Y) / _imageScaleY;
                var imageWidth = selectionRect.Width / _imageScaleX;
                var imageHeight = selectionRect.Height / _imageScaleY;

                // 範囲をクリップ
                imageX = Math.Max(0, imageX);
                imageY = Math.Max(0, imageY);
                imageWidth = Math.Min(_originalImage.PixelWidth - imageX, imageWidth);
                imageHeight = Math.Min(_originalImage.PixelHeight - imageY, imageHeight);

                var cropRect = new Int32Rect(
                    (int)imageX, (int)imageY,
                    (int)imageWidth, (int)imageHeight
                );

                CroppedImage = new CroppedBitmap(_originalImage, cropRect);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"トリミングに失敗しました: {ex.Message}", "エラー",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}


namespace Mantensei_Database.Services
{
    public static class ImageService
    {
        private static readonly string ImageDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppConfig.AUTHOR_NAME, AppConfig.APP_NAME, AppConfig.Images_DIRECTORY, AppConfig.PROFILES_DIRECTORY
        );

        static ImageService()
        {
            // 画像保存用ディレクトリを作成
            if (!Directory.Exists(ImageDirectory))
            {
                Directory.CreateDirectory(ImageDirectory);
            }
        }

        /// <summary>
        /// プロファイルIDに対応する画像ファイルパスを取得
        /// </summary>
        public static string GetImagePath(int profileId)
        {
            return Path.Combine(ImageDirectory, $"{profileId}.png");
        }

        /// <summary>
        /// プロファイルIDに対応する画像が存在するかチェック
        /// </summary>
        public static bool HasImage(int profileId)
        {
            return File.Exists(GetImagePath(profileId));
        }


        public static WriteableBitmap GetDefaultImage()
        {
            var bitmap = new WriteableBitmap(1, 1, 1, 1, PixelFormats.Bgr32, null);
            var identifierColor = Colors.Black;
            var colorBytes = new byte[] { identifierColor.B, identifierColor.G, identifierColor.R, identifierColor.A };

            bitmap.WritePixels(new Int32Rect(0, 0, 1, 1), colorBytes, 4, 0);
            return bitmap;
        }

        public static bool IsDefaultImage(WriteableBitmap bitmap)
        {
            if (bitmap?.PixelWidth != 1 || bitmap.PixelHeight != 1) return false;

            // 比較用のデフォルト画像を作成
            var defaultBitmap = GetDefaultImage();

            // 両方のピクセル値を取得して比較
            var targetPixel = new byte[4];
            var defaultPixel = new byte[4];

            bitmap.CopyPixels(targetPixel, 4, 0);
            defaultBitmap.CopyPixels(defaultPixel, 4, 0);

            return BitConverter.ToInt32(targetPixel, 0) == BitConverter.ToInt32(defaultPixel, 0);
        }

        public static BitmapImage ConvertToBitmapImage(ImageSource imageSource)
        {
            if (imageSource is BitmapImage bitmapImage)
            {
                return bitmapImage;
            }

            if (imageSource is BitmapSource bitmapSource)
            {
                // BitmapSourceをBitmapImageに変換
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    stream.Position = 0;

                    var newBitmapImage = new BitmapImage();
                    newBitmapImage.BeginInit();
                    newBitmapImage.StreamSource = stream;
                    newBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    newBitmapImage.EndInit();
                    newBitmapImage.Freeze();

                    return newBitmapImage;
                }
            }

            return null;
        }

        public static void SaveImage(int profileId, ImageSource imageSource)
        {
            SaveImage(profileId, ConvertToBitmapImage(imageSource));
        }

        /// <summary>
        /// 画像をプロファイルIDで保存
        /// </summary>
        public static void SaveImage(int profileId, BitmapSource image)
        {
            if (image == null)
                return;

            var filePath = GetImagePath(profileId);

            try
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));

                using var stream = new FileStream(filePath, FileMode.Create);
                encoder.Save(stream);
            }
            catch (Exception ex)
            {
                throw new IOException($"画像の保存に失敗しました: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// プロファイルIDに対応する画像を読み込み
        /// </summary>
        public static BitmapSource LoadImage(int profileId)
        {
            var filePath = GetImagePath(profileId);

            if (!File.Exists(filePath))
                return null;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"画像の読み込みに失敗: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// プロファイルIDに対応する画像を削除
        /// </summary>
        public static void DeleteImage(int profileId)
        {
            var filePath = GetImagePath(profileId);

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"画像の削除に失敗: {ex.Message}");
                }
            }
        }
    }
}