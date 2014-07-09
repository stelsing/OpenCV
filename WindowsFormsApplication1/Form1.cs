using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;


namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private string filename;
        private int count;
        private int count_point;
        private Int32[] massBools;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            trackBar1.Value = 233;
        }

        private void Process_image(string filename, int gray=50)
        {
            pictureBox1.Load(filename);
            Bitmap bmp = new Bitmap(filename);

            //открытие картинки
            Image<Bgr, Byte> My_Image = new Image<Bgr, byte>(bmp);
            //конвертация в ЧБ
            Image<Gray, Byte> gray_image = My_Image.Convert<Gray, Byte>();
            //бинаризация изображения, выставление предела цвета - ползунком
            Image<Gray, Byte> bin = gray_image.ThresholdBinary(new Gray(gray), new Gray(255));
            //выделение границ
            Image<Gray, Byte> cany_image = bin.Canny(new Gray(), new Gray());
            //вывод результатов
            pictureBox1.Image = My_Image.ToBitmap();
            pictureBox2.Image = bin.ToBitmap();

            
            //count = 0;
            count_point = 0;

            //нахождение координат точек контура
            int count = 0;
            Points[] pointses = new Points[cany_image.Bytes.Length];
            int x1 = 0;
            int y1 = 0;
            for (y1 = 0; y1 < cany_image.Rows; y1++)
            {
                for (x1 = 0; x1 < cany_image.Cols; x1++)
                {
                    if (cany_image.Data[y1, x1, 0] == 255)
                    {
                        pointses[count]= new Points(x1,y1);
                        count++;
                    }
                }
            }


            int y=1;
            Bitmap btm1 = new Bitmap(cany_image.ToBitmap());


            int[] res_point = new int[cany_image.Rows*2];
            int count1 = 0;
            for (int i = 0; i < cany_image.Rows*2; i += 2)
            {
                try
                {
                    res_point[count1] = pointses[i].x + (pointses[i + 1].x - pointses[i].x) / 2;
                    count1++;
                }
                catch
                {
                }
            }

            //массив для сохранения позиции средних точек
            int[] array_points_middle = new int[cany_image.Rows];

            //нахождение средней точки, фильтр и отрисовка.
            for (y = 0; y < cany_image.Rows; y++)
            {
                if (y > 0)
                {
                    int prev = res_point[y - 1];
                    int typ = res_point[y];

                    if ((typ > prev - 10) && (typ < prev + 10))
                    {
                        btm1.SetPixel(res_point[y], y, Color.White);
                        array_points_middle[y] = res_point[y];
                    }
                    else
                    {
                        btm1.SetPixel(res_point[y - 1], y, Color.White);
                        res_point[y] = res_point[y - 1];
                        array_points_middle[y] = res_point[y-1];
                    }
                }
                else
                {
                    btm1.SetPixel(res_point[y], y, Color.White);
                }
            }

            //нахождение расстояния от центра до точек

            int[] array_points_length = new int[cany_image.Rows/5];

            try
            {
                int count_points = 0;
                int count2 = 0;
                for (int i = 20; i < cany_image.Rows; i += 5)
                {
                    count_point = array_points_middle[i];
                    while (cany_image.Data[i, count_point, 0] != 255)
                    {
                        //если раскомментировать - расчерчивание изображения до контура
                        //btm1.SetPixel(count_point, i, Color.White);
                        array_points_length[count2]++;
                        if (count_point < cany_image.Cols - 1)
                        {
                            count_point++;
                        }
                        else
                        {
                            return;
                        }
                    }
                    count2++;
                }
            }
            catch 
            {
                
            }

            //нахождение шеи min

            int min = array_points_length[1];
            int pos_min = 0;
            

            for (int i = 1; i < (cany_image.Rows/5)-10; i++)
            {
                if (array_points_length[i] < min)
                {
                    min = array_points_length[i];
                    pos_min = i;
                }
                
            }

            for (int i = array_points_middle[pos_min]; i < (array_points_middle[pos_min] + array_points_length[pos_min]); i++)
            {
                btm1.SetPixel(i, (pos_min * 5) + 20, Color.White);
            }
            //
            //нахожденние max

            int max = array_points_length[1];
            int pos_max = 0;

            for (int i = 1; i < (cany_image.Rows / 5) - 10; i++)
            {
                if (array_points_length[i]  > max)
                {
                    max = array_points_length[i];
                    pos_max = i;
                }

            }

            for (int i = array_points_middle[pos_max]; i < (array_points_middle[pos_max] + array_points_length[pos_max]); i++)
            {
                try
                {
                    btm1.SetPixel(i, (pos_max * 5) + 20, Color.White);
                }
                catch (Exception)
                {

                }
                
            }
            //


            //вывод результата
            pictureBox3.Image = btm1;

            label2.Text = count.ToString();
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            if (filename != null)
            {
                Process_image(filename, trackBar1.Value);
            }
            label1.Text = trackBar1.Value.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog Openfile = new OpenFileDialog();
            if (Openfile.ShowDialog() == DialogResult.OK)
            {
                filename = Openfile.FileName;
                Process_image(filename, trackBar1.Value);
            }
        }
    }

    //класс для сохранения координат точек границы
    public class Points
    {
        public Int32 x;
        public Int32 y;

        public Points(int _x1, int _y1)
        {
            x = _x1;
            y = _y1;
        }
    }
}
