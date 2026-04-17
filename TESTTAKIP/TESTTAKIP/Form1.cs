using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TESTTAKIP
{
    public partial class frmtest : Form
    {
        public frmtest()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct IconInfo
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern IntPtr CreateIconIndirect(ref IconInfo icon);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

        int saniye = 150; // Toplam süre (saniye cinsinden)
        int totalTimeMs;
        int elapsedTime = 0;
        int sorusayisi=12;
        //canvas çizim değişkenleri
        bool cizim = false;
        Point oncekiNokta;

        Bitmap canvas;
        Graphics g;

        bool silgiModu = false;
        Pen kalem = new Pen(Color.Black, 2);
        Pen silgi = new Pen(Color.White, 10);

        int silgiBoyutu = 10;
        Bitmap silgiResim;

        enum AracModu
        {
            Kalem,
            Silgi,
            Yazi
        }

        AracModu aktifArac = AracModu.Kalem;

        Point yaziKonum;
        string yazilanMetin = "";
        Font yaziFont = new Font("Arial", 16);
        Brush yaziRenk = Brushes.Black;

        List<string> resimYollari = new List<string>();
        int aktifIndex = 0;

        string kayitKlasoru = "";

        Stack<Bitmap> undoStack = new Stack<Bitmap>();
        int eskiSplitter = 0;
        void DurumKaydet()
        {
            undoStack.Push(new Bitmap(canvas));
        }
        private Cursor CursorOlustur(Bitmap bmp, int hotspotX, int hotspotY)
        {
            IntPtr hIcon = bmp.GetHicon();

            IconInfo tmp = new IconInfo();
            GetIconInfo(hIcon, ref tmp);

            tmp.xHotspot = hotspotX;
            tmp.yHotspot = hotspotY;
            tmp.fIcon = false; // cursor olduğunu belirt

            IntPtr ptr = CreateIconIndirect(ref tmp);
            return new Cursor(ptr);
        }
        private void frmtest_Load(object sender, EventArgs e)
        {


            pictureBox1.Focus();
            this.KeyPreview = true;
            this.KeyPress += frmtest_KeyPress;
            
            silgiResim = new Bitmap(btnsilgi.Image);
            silgi = new Pen(Color.White, silgiBoyutu);
            silgi.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            silgi.EndCap = System.Drawing.Drawing2D.LineCap.Round;
            //canvas çizim

            canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(canvas);
            g.Clear(Color.White);

            pictureBox1.Image = canvas;


            // ProgressBar ayarları
            totalTimeMs = saniye * 1000; // Toplam süreyi milisaniyeye çeviriyoruz
            progressBar1.Minimum = 0;
            progressBar1.Maximum = totalTimeMs; // toplam süreyi max yapıyoruz
            progressBar1.Value = 0;
            progressBar1.Style = ProgressBarStyle.Continuous;

            timer1.Interval = 50; // akıcı animasyon
            timer1.Start();
            dataGridView1.RowTemplate.Height = 50;


            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.Columns.Clear();

            dataGridView1.Columns.Add("Soru", "Soru");
            dataGridView1.Columns.Add("A", "A");
            dataGridView1.Columns.Add("B", "B");
            dataGridView1.Columns.Add("C", "C");
            dataGridView1.Columns.Add("D", "D");

            for (int i = 1; i <= sorusayisi; i++)
            {
                dataGridView1.Rows.Add("Soru " + i, false, false, false, false);
            }

            // sadece ilk soru açık
            for (int i = 1; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].Visible = false;
            }
        }

        private void frmtest_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (aktifArac != AracModu.Yazi)
                return;

            if (e.KeyChar == (char)Keys.Back)
            {
                if (yazilanMetin.Length > 0)
                {
                    // Önce son karakterin genişliğini ölç
                    string sonKarakter = yazilanMetin.Substring(yazilanMetin.Length - 1, 1);
                    SizeF size = g.MeasureString(sonKarakter, yaziFont);

                    // Son karakterin bulunduğu alanı beyaza boya
                    g.FillRectangle(Brushes.White, yaziKonum.X - size.Width, yaziKonum.Y, size.Width, size.Height);

                    // Cursoru sola kaydır
                    yaziKonum.X -= (int)size.Width;

                    // Yazıdan son karakteri çıkar
                    yazilanMetin = yazilanMetin.Substring(0, yazilanMetin.Length - 1);

                    pictureBox1.Invalidate();
                }
            }
            else if (e.KeyChar == (char)Keys.Enter)
            {
                DurumKaydet(); // 🔥 BURAYA EKLİYORSUN

                // Enter basınca yazıyı kalıcı çiz
                yazilanMetin = "";
            }
            else
            {
                // Yeni harfi canvas'a çiz
                g.DrawString(e.KeyChar.ToString(), yaziFont, yaziRenk, yaziKonum);

                // Cursoru sağa kaydır
                SizeF size = g.MeasureString(e.KeyChar.ToString(), yaziFont);
                yaziKonum.X += (int)size.Width;

                // Yazıyı geçici olarak sakla (Backspace için)
                yazilanMetin += e.KeyChar;

                pictureBox1.Invalidate();
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            elapsedTime += timer1.Interval;

            if (elapsedTime <= totalTimeMs)
            {
                progressBar1.Value = Math.Min(elapsedTime, totalTimeMs);

                // Renk geçişi
                double oran = (double)elapsedTime / totalTimeMs;

                if (oran <= 0.5) // ilk yarı → yeşil
                {
                    progressBar1.ForeColor = Color.Green;
                }
                else if (oran <= 0.8) // ikinci kısım → sarı
                {
                    progressBar1.ForeColor = Color.Yellow;
                }
                else // son kısım → kırmızı
                {
                    progressBar1.ForeColor = Color.Red;
                }

                // Kalan süre label
                int kalanMs = totalTimeMs - elapsedTime;
                lblsure.Text = TimeSpan.FromMilliseconds(kalanMs).ToString(@"mm\:ss");
            }
            else
            {
                timer1.Stop();
                progressBar1.Value = totalTimeMs;
                lblsure.Text = "00:00";
                MessageBox.Show("Süre doldu!");
            }
        }

        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex > 0)
            {
                e.PaintBackground(e.CellBounds, true);

                bool secili = false;
                if (e.Value != null)
                    secili = (bool)e.Value;

                Rectangle rect = new Rectangle(
                    e.CellBounds.X + 15,
                    e.CellBounds.Y + 10,
                    18,
                    18);

                e.Graphics.DrawEllipse(Pens.Black, rect);

                if (secili)
                {
                    e.Graphics.FillEllipse(Brushes.Black, rect);
                }

                e.Handled = true;
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex == 0)
                return;

            // Aynı satırda diğer şıkları temizle
            for (int i = 1; i <= 4; i++)
            {
                dataGridView1.Rows[e.RowIndex].Cells[i].Value = false;
            }

            // Seçilen şıkkı işaretle
            dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = true;

            // Sonraki soruyu aç
            int nextRow = e.RowIndex + 1;

            if (nextRow < dataGridView1.Rows.Count)
            {
                dataGridView1.Rows[nextRow].Visible = true;
            }
        }

        
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (aktifArac == AracModu.Yazi)
            {
                yaziKonum = e.Location;
                yazilanMetin = "";
                this.Focus();
                return;
            }

            DurumKaydet(); // 🔥 BURASI

            cizim = true;
            oncekiNokta = e.Location;
        }
       

       
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!cizim) return;

            if (aktifArac == AracModu.Silgi)
            {
                CizgiBoyuncaSil(oncekiNokta, e.Location);
                oncekiNokta = e.Location;
            }
            else if (aktifArac == AracModu.Kalem)
            {
                g.DrawLine(kalem, oncekiNokta, e.Location);
                oncekiNokta = e.Location;
            }

            pictureBox1.Invalidate();
        }

        private void CursorGuncelle()
        {
            Bitmap bmp;

            if (aktifArac == AracModu.Silgi)
                bmp = new Bitmap(btnsilgi.Image, new Size(32, 32));
            else if (aktifArac == AracModu.Yazi)
                bmp = new Bitmap(btnYazi.Image, new Size(32, 32));
            else
                bmp = new Bitmap(btnkalem.Image, new Size(32, 32));

            pictureBox1.Cursor = CursorOlustur(bmp, 2, bmp.Height - 2);
        }
        private void CizgiBoyuncaSil(Point p1, Point p2)
        {
            int dx = p2.X - p1.X;
            int dy = p2.Y - p1.Y;

            int adim = Math.Max(Math.Abs(dx), Math.Abs(dy));
            if (adim == 0)
            {
                GercekSil(p1.X, p1.Y);
                return;
            }

            float xArtis = dx / (float)adim;
            float yArtis = dy / (float)adim;

            float x = p1.X;
            float y = p1.Y;

            for (int i = 0; i <= adim; i++)
            {
                GercekSil((int)x, (int)y);
                x += xArtis;
                y += yArtis;
            }
        }
        void GeriAl()
        {
            if (undoStack.Count > 0)
            {
                if (canvas != null)
                    canvas.Dispose();

                canvas = undoStack.Pop();
                g = Graphics.FromImage(canvas);

                pictureBox1.Image = canvas;
                pictureBox1.Invalidate();
            }
        }


        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (aktifArac == AracModu.Yazi && !string.IsNullOrEmpty(yazilanMetin))
            {
                e.Graphics.DrawString(yazilanMetin, yaziFont, Brushes.Gray, yaziKonum);
            }
        }
        private void GercekSil(int mouseX, int mouseY)
        {
            int w = silgiResim.Width;
            int h = silgiResim.Height;

            int startX = mouseX - w / 2;
            int startY = mouseY - h / 2;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    Color c = silgiResim.GetPixel(x, y);

                    if (c.A > 50)
                    {
                        int hedefX = startX + x;
                        int hedefY = startY + y;

                        if (hedefX >= 0 && hedefY >= 0 &&
                            hedefX < canvas.Width && hedefY < canvas.Height)
                        {
                            canvas.SetPixel(hedefX, hedefY, Color.White);
                        }
                    }
                }
            }
        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            cizim = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            aktifArac = AracModu.Kalem;
            CursorGuncelle();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            aktifArac = AracModu.Silgi;
            CursorGuncelle();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DurumKaydet(); // 🔥

            g.Clear(Color.White);
            pictureBox1.Invalidate();
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {

            CursorGuncelle();
        }

        private void btnYazi_Click(object sender, EventArgs e)
        {
            aktifArac = AracModu.Yazi;
            pictureBox1.Focus();
            CursorGuncelle();
        }

        private void btndevam_Click(object sender, EventArgs e)
        {
           
            
        }

        private void btndurdur_Click(object sender, EventArgs e)
        {
           
        }
        private void ResmiGoster(string yol)
        {
            string dosyaAdi = Path.GetFileNameWithoutExtension(yol) + ".png";
            string kayitYolu = Path.Combine(kayitKlasoru, dosyaAdi);

            Bitmap yuklenen;

            string okunacakYol = File.Exists(kayitYolu) ? kayitYolu : yol;

            using (var temp = new Bitmap(okunacakYol))
            {
                yuklenen = new Bitmap(temp);
            }

            g.Clear(Color.White);
            g.DrawImage(yuklenen, 0, 0, canvas.Width, canvas.Height);

            pictureBox1.Invalidate();
            SoruGuncelle();
        }
        private void btnbaslat_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                // 1,2,3... diye sıralı dosyaları al
                resimYollari = Directory.GetFiles(fbd.SelectedPath, "*.jpg")
                                        .Concat(Directory.GetFiles(fbd.SelectedPath, "*.png"))
                                        .OrderBy(x => Path.GetFileNameWithoutExtension(x))
                                        .ToList();

                aktifIndex = 0;

                if (resimYollari.Count > 0)
                {
                    ResmiGoster(resimYollari[aktifIndex]);
                }
            }

            kayitKlasoru = Path.Combine(fbd.SelectedPath, "Cizimler");

            if (!Directory.Exists(kayitKlasoru))
                Directory.CreateDirectory(kayitKlasoru);

            if (btnbaslat.Visible == true && btnileri.Visible == false)
            {
                btnbaslat.Visible= false;
                btnileri.Visible= true;
                timer1.Start();
                btndevam.Visible = false;
                btndurdur.Visible = true;
            }
        }
        void sonrakiresim()
        {
            if (resimYollari.Count == 0)
                return;

            try
            {
                string dosyaAdi = Path.GetFileNameWithoutExtension(resimYollari[aktifIndex]) + ".png";
                string kayitYolu = Path.Combine(kayitKlasoru, dosyaAdi);

                using (Bitmap kaydedilecek = new Bitmap(canvas))
                {
                    kaydedilecek.Save(kayitYolu, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kaydetme hatası: " + ex.Message);
            }

            aktifIndex++;

            if (aktifIndex >= resimYollari.Count)
                aktifIndex = 0;

            ResmiGoster(resimYollari[aktifIndex]);
        }

        void SoruGuncelle()
        {
            lblsoru.Text = "Soru " + (aktifIndex + 1);
        }
        private void btnbitir_Click(object sender, EventArgs e)
        {
            DialogResult result1=MessageBox.Show("Test bitirilip, program kapatılacak. Onaylıyor musunuz?", "Bilgi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if(result1==DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            sonrakiresim();
        }

        private void btngeri_Click(object sender, EventArgs e)
        {
            if (resimYollari.Count == 0)
                return;

            try
            {
                string dosyaAdi = Path.GetFileNameWithoutExtension(resimYollari[aktifIndex]) + ".png";
                string kayitYolu = Path.Combine(kayitKlasoru, dosyaAdi);

                using (Bitmap kaydedilecek = new Bitmap(canvas))
                {
                    kaydedilecek.Save(kayitYolu, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Resim kaydedilemedi: " + ex.Message);
            }

            aktifIndex--;

            if (aktifIndex < 0)
                aktifIndex = resimYollari.Count - 1;

            ResmiGoster(resimYollari[aktifIndex]);
        }

        private void frmtest_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (resimYollari.Count > 0)
            {
                string dosyaAdi = Path.GetFileNameWithoutExtension(resimYollari[aktifIndex]) + ".png";
                string kayitYolu = Path.Combine(kayitKlasoru, dosyaAdi);

                using (Bitmap kaydedilecek = new Bitmap(canvas))
                {
                    if (File.Exists(kayitYolu))
                        

                    kaydedilecek.Save(kayitYolu, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
        }

        private void frmtest_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z)
            {
                GeriAl();
            }
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            GeriAl();
        }

        private void btnYazi_Click_1(object sender, EventArgs e)
        {
            aktifArac = AracModu.Yazi;
            pictureBox1.Focus();
            CursorGuncelle();
        }

        private void btnUndo_Click_1(object sender, EventArgs e)
        {
            GeriAl();
        }

        private void btndevam_Click_1(object sender, EventArgs e)
        {
            if (btndevam.Visible == true && btndurdur.Visible == false)
            {
                btndevam.Visible = false;
                btndurdur.Visible = true;
                timer1.Start();
            }

        }

        private void btndurdur_Click_1(object sender, EventArgs e)
        {
            if (btndurdur.Visible == true && btndevam.Visible == false)
            {
                btndurdur.Visible = false;
                btndevam.Visible = true;
                timer1.Stop();
            }
        }

        private void chkgizle_CheckedChanged(object sender, EventArgs e)
        {
            splaltana.Panel2Collapsed = chkgizle.Checked;
        }
    }
    
}
