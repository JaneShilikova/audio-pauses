using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using System.IO;

namespace wav
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            btnCut.Enabled = false;
            btnSave.Enabled = false;
        }

        float[] L;
        float[] R;
        Wave wav = new Wave();

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Audio Files (.wav)|*.wav";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                path = dialog.FileName;
                btnCut.Enabled = true;
                bool flag = wav.readWav(path, out L, out R);
                if (flag)
                {
                    int length = L.Length;
                    double sec = wav.duration();
                    double t = sec / length;
                    chart1.Series[0].Color = Color.Green;
                    chart2.Series[0].Color = Color.Green;
                    progressOpen.Value = 0;
                    progressOpen.Maximum = length;
                    for (int i = 0; i < length; i++)
                    {
                        chart1.Series[0].Points.AddXY(t, L[i]);
                        chart2.Series[0].Points.AddXY(t, R[i]);
                        t += sec / length;
                        progressOpen.Value = i;
                    }
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Audio Files (.wav)|*.wav";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string path = dialog.FileName;
                wav.writeWav(path, new_L, new_R);
            }
        }
        float min_amp = 0.0f;
        double min_t = 0;
        private void txtAmp_Leave(object sender, EventArgs e)
        {
            if (txtAmp.Text != "")
                min_amp = (float)Convert.ToDouble(txtAmp.Text);
        }

        private void txtT_Leave(object sender, EventArgs e)
        {
            if (txtT.Text != "")
                min_t = Convert.ToDouble(txtT.Text);
        }

        private void txtAmp_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '.')
                e.KeyChar = ',';
        }

        private void txtT_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '.')
                e.KeyChar = ',';
        }

        string path = "";
        float[] new_L;
        float[] new_R;
        private void btnCut_Click(object sender, EventArgs e)
        {
            btnSave.Enabled = true;
            int k_L = 0;
            int length = L.Length;
            double sec = wav.duration();
            for (int i = 0, k = 0; i < length; i++, k++)
            {
                if ((L[i] >= min_amp || L[i] <= -min_amp) &&
                    (R[i] >= min_amp || R[i] <= -min_amp))
                {
                    if (k <= (((min_t / length) - (sec / length)) / (sec / length)))
                        for (int temp = 0; temp < k; temp++)
                            k_L++;
                    else
                        for (int temp = 0; temp < (((min_t / length) - (sec / length)) / (sec / length)); temp++)
                            k_L++;
                    k = 0;
                    k_L++;
                }
                else
                    k = 0;
            }
            new_L = new float[k_L];
            new_R = new float[k_L];
            k_L = 0;
            double t = sec / length;
            for (int i = 0, k = 0; i < length; i++, k++)
            {
                if ((L[i] >= min_amp || L[i] <= -min_amp) &&
                    (R[i] >= min_amp || R[i] <= -min_amp))
                {
                    if (k <= (((min_t / length) - t) / t))
                        for (int temp = 0; temp < k; temp++)
                        {
                            new_L[k_L] = L[i - k + temp];
                            new_R[k_L] = R[i - k + temp];
                            k_L++;
                        }
                    else
                    {
                        for (int temp = 0; temp < (((min_t / length) - t) / t); temp++)
                        {
                            new_L[k_L] = L[i - (int)(((min_t / length) - t) / t) + temp];
                            new_R[k_L] = R[i - (int)(((min_t / length) - t) / t) + temp];
                            k_L++;
                        }
                    }
                    k = 0;
                    new_L[k_L] = L[i];
                    new_R[k_L] = R[i];
                    k_L++;
                }
                else
                    k = 0;
            }
            double new_sec = 2 * L.Length / wav.bytesForSamp / wav.channels / wav.sampleRate;
            double new_t = sec / k_L;
            chart3.Series[0].Color = Color.Green;
            chart4.Series[0].Color = Color.Green;
            progressCut.Value = 0;
            progressCut.Maximum = k_L;
            for (int i = 0; i < k_L; i++)
            {
                chart3.Series[0].Points.AddXY(new_t, new_L[i]);
                chart4.Series[0].Points.AddXY(new_t, new_R[i]);
                new_t += new_sec / k_L;
                progressCut.Value = i;
            }
        }
    }
}
