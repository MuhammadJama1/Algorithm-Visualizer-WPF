using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AlgorithmVisualizer
{
    public partial class MainWindow : Window
    {
        // Sıralanacak verileri tutan ana dizi
        private int[] array = Array.Empty<int>();

        // Rastgele sayılar üretmek için kullanılacak nesne
        private Random random = new Random();

        // Algoritmanın duraklatılıp duraklatılmadığını kontrol eden bayrak (state)
        private bool isPaused = false;

        public MainWindow()
        {
            InitializeComponent();

            // Pencere boyutu değiştiğinde diziyi yeni boyuta göre tekrar çiz (Responsive UI)
            this.SizeChanged += (s, e) => DrawArray();

            // Uygulama açıldığında varsayılan bir dizi oluştur ve ekrana çiz
            GenerateRandomArray();
        }

        // Slider'dan gelen değere göre animasyon gecikme süresini (ms) hesaplar
        // Hız arttıkça gecikme süresi azalır.
        private int GetDelay()
        {
            return 101 - (int)SpeedSlider.Value;
        }

        // "Yeni Dizi" butonuna tıklandığında çalışır
        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            GenerateRandomArray();
            DrawArray();
        }

        // --- BUBBLE SORT ALGORİTMASI ---

        // "Kabarcık (Bubble)" butonuna tıklandığında çalışır
        private async void BtnSort_Click(object sender, RoutedEventArgs e)
        {
            // İşlem sırasında çakışmaları önlemek için diğer butonları devre dışı bırak
            BtnPause.IsEnabled = true;
            BtnGenerate.IsEnabled = false;
            BtnSort.IsEnabled = false;
            BtnQuickSort.IsEnabled = false;

            // Algoritmayı asenkron olarak başlat (UI thread'i bloklamamak için)
            await BubbleSort();

            // İşlem bitince butonları tekrar aktifleştir
            BtnGenerate.IsEnabled = true;
            BtnSort.IsEnabled = true;
            BtnQuickSort.IsEnabled = true;
            BtnPause.IsEnabled = false;
        }

        // Bubble Sort (Kabarcık Sıralaması) mantığı: O(n^2) karmaşıklığına sahiptir.
        private async System.Threading.Tasks.Task BubbleSort()
        {
            int n = array.Length;
            bool swapped;

            for (int i = 0; i < n - 1; i++)
            {
                swapped = false;

                for (int j = 0; j < n - i - 1; j++)
                {
                    // Karşılaştırılan iki elemanı vurgulayarak çiz
                    DrawArray(j, j + 1);

                    // Animasyon efekti için bekle ve kullanıcının duraklatma isteğini kontrol et
                    await System.Threading.Tasks.Task.Delay(GetDelay());
                    await CheckPause();

                    // Soldaki eleman sağdakinden büyükse yer değiştir (Swap)
                    if (array[j] > array[j + 1])
                    {
                        int temp = array[j];
                        array[j] = array[j + 1];
                        array[j + 1] = temp;
                        swapped = true;

                        // Yer değiştirdikten sonra yeni durumu ekrana çiz
                        DrawArray(j, j + 1);
                        await System.Threading.Tasks.Task.Delay(GetDelay());
                        await CheckPause();
                    }
                }
                // Eğer iç döngüde hiç yer değiştirme olmadıysa dizi zaten sıralıdır, döngüyü kır (Optimizasyon)
                if (!swapped)
                    break;
            }

            // Sıralama başarıyla bittiğinde tüm diziyi yeşil renkle çiz
            DrawFinished();
        }

        // --- ÇİZİM VE GÖRSELLEŞTİRME METOTLARI ---

        // Sıralama tamamlandığında çağrılır, tüm elemanları başarı rengine (Yeşil) boyar
        private void DrawFinished()
        {
            MainCanvas.Children.Clear();
            double canvasWidth = MainCanvas.ActualWidth;
            double rectWidth = canvasWidth / array.Length;

            for (int i = 0; i < array.Length; i++)
            {
                Rectangle rect = new Rectangle
                {
                    Width = Math.Max(1, rectWidth - 2), // Sütunlar arasına hafif boşluk bırak
                    Height = array[i],
                    Fill = Brushes.MediumSpringGreen,
                    RadiusX = 3, // Köşeleri hafif yuvarlatarak modern bir görünüm sağla
                    RadiusY = 3
                };

                Canvas.SetLeft(rect, i * rectWidth);
                Canvas.SetBottom(rect, 0); // Sütunları tuvalin altından hizala
                MainCanvas.Children.Add(rect);
            }
        }

        // Boyut Slider'ındaki değere göre rastgele sayılardan oluşan yeni bir dizi üretir
        private void GenerateRandomArray()
        {
            int currentSize = (int)SizeSlider.Value;
            array = new int[currentSize];
            for (int i = 0; i < currentSize; i++)
            {
                // Ekran yüksekliğine uygun olması için 10 ile 400 arasında değerler üretilir
                array[i] = random.Next(10, 400);
            }
        }

        // Diziyi Canvas üzerine dikdörtgenler (sütunlar) şeklinde çizer.
        // highlight1 ve highlight2 parametreleri, o an karşılaştırılan elemanları kırmızı yapmak içindir.
        private void DrawArray(int highlight1 = -1, int highlight2 = -1)
        {
            MainCanvas.Children.Clear();

            double canvasWidth = MainCanvas.ActualWidth;
            if (canvasWidth == 0) canvasWidth = 800; // Başlangıç genişliği koruması

            double rectWidth = canvasWidth / array.Length;

            for (int i = 0; i < array.Length; i++)
            {
                // Varsayılan sütun rengi
                Brush columnColor = Brushes.SkyBlue;

                // Eğer eleman şu an işleniyorsa rengini değiştir (Görsel geribildirim)
                if (i == highlight1 || i == highlight2)
                {
                    columnColor = Brushes.OrangeRed;
                }

                Rectangle rect = new Rectangle
                {
                    Width = Math.Max(1, rectWidth - 2),
                    Height = array[i],
                    Fill = columnColor,
                    RadiusX = 3,
                    RadiusY = 3
                };

                Canvas.SetLeft(rect, i * rectWidth);
                Canvas.SetBottom(rect, 0);
                MainCanvas.Children.Add(rect);
            }
        }

        // --- QUICK SORT ALGORİTMASI ---

        // "Hızlı (Quick)" butonuna tıklandığında çalışır
        private async void BtnQuickSort_Click(object sender, RoutedEventArgs e)
        {
            BtnPause.IsEnabled = true;
            BtnGenerate.IsEnabled = false;
            BtnSort.IsEnabled = false;
            BtnQuickSort.IsEnabled = false;

            // Böl ve Fethet (Divide and Conquer) mantığı ile çalışan algoritmayı başlat
            await PerformQuickSort(0, array.Length - 1);

            DrawFinished();

            BtnGenerate.IsEnabled = true;
            BtnSort.IsEnabled = true;
            BtnQuickSort.IsEnabled = true;
            BtnPause.IsEnabled = false;
        }

        // Quick Sort özyineli (recursive) fonksiyonu
        private async System.Threading.Tasks.Task PerformQuickSort(int low, int high)
        {
            if (low < high)
            {
                // Diziyi pivot elemanına göre ikiye böl ve pivotun doğru indeksini al
                int pivotIndex = await Partition(low, high);

                // Pivotun solundaki alt diziyi sırala
                await PerformQuickSort(low, pivotIndex - 1);

                // Pivotun sağındaki alt diziyi sırala
                await PerformQuickSort(pivotIndex + 1, high);
            }
        }

        // Quick Sort için Bölümleme (Partition) metodu
        private async System.Threading.Tasks.Task<int> Partition(int low, int high)
        {
            // Genellikle son eleman pivot olarak seçilir
            int pivot = array[high];
            int i = (low - 1);

            for (int j = low; j < high; j++)
            {
                // İşlenen elemanları ekranda göster
                DrawArray(j, high);
                await System.Threading.Tasks.Task.Delay(GetDelay());
                await CheckPause();

                // Eğer mevcut eleman pivottan küçükse, sol tarafa at
                if (array[j] < pivot)
                {
                    i++;
                    int temp = array[i];
                    array[i] = array[j];
                    array[j] = temp;

                    // Yer değişimini ekranda göster
                    DrawArray(i, j);
                    await System.Threading.Tasks.Task.Delay(GetDelay());
                    await CheckPause();
                }
            }

            // Pivot elemanını doğru konumuna (ortaya) yerleştir
            int temp1 = array[i + 1];
            array[i + 1] = array[high];
            array[high] = temp1;

            DrawArray(i + 1, high);
            await System.Threading.Tasks.Task.Delay(GetDelay());

            // Pivotun yeni indeksini döndür
            return i + 1;
        }

        // --- DURAKLATMA VE UI KONTROLLERİ ---

        // Duraklat / Devam Et butonunun mantığını yönetir
        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            isPaused = !isPaused;

            if (isPaused)
            {
                BtnPause.Content = "Devam Et";
                BtnPause.Background = Brushes.Orange; // Kullanıcıyı uyarmak için turuncu renk
            }
            else
            {
                BtnPause.Content = "Duraklat";
                BtnPause.Background = (Brush)new BrushConverter().ConvertFrom("#DC3545"); // Kırmızı renge dön
            }
        }

        // Algoritmaların içine yerleştirilen ve uygulamanın duraklatılıp duraklatılmadığını sürekli kontrol eden metot
        private async System.Threading.Tasks.Task CheckPause()
        {
            // Eğer isPaused true ise, algoritma bu döngü içinde hapsolur ve bekler
            while (isPaused)
            {
                await System.Threading.Tasks.Task.Delay(50); // İşlemciyi yormamak için kısa gecikmelerle kontrol et
            }
        }

        // Hız Slider'ı kaydırıldığında ekrandaki metni anında günceller
        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SpeedValueText != null)
            {
                SpeedValueText.Text = ((int)SpeedSlider.Value).ToString();
            }
        }

        // Boyut Slider'ı kaydırıldığında ekrandaki metni anında günceller
        private void SizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SizeValueText != null)
            {
                SizeValueText.Text = ((int)SizeSlider.Value).ToString();
            }
        }
    }
}