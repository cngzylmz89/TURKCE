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
    }
    
}
