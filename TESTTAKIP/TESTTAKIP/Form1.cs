using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        }
        int saniye = 150; // Toplam süre (saniye cinsinden)
        int totalTimeMs;
        int elapsedTime = 0;
        int sorusayisi=12;
        private void frmtest_Load(object sender, EventArgs e)
        {
            totalTimeMs = saniye * 1000; // Toplam süreyi milisaniyeye çeviriyoruz
            // ProgressBar ayarları
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
    }
    
}
