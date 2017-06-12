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
using System.Collections;

namespace Thompson
{
    public partial class MainForm : Form
    {
        NFA nfa;

        //вспомогательная переменная, обозначает текущую перетаскиваемую фигуру
        PictureBox movingPB = null;
        List<PictureBox> pbs = new List<PictureBox>();

        /*координаты смещения относительно левого верхнего угла PictureBox-а, 
            нужны для того, что бы PictureBox переместился именно за ту точку,
            за которую его потянули
        */
        int dx = 0;
        int dy = 0;
        bool[] isfinal;
        bool[] used;
        bool is_builded = false;


        public MainForm()
        {
            InitializeComponent();
        }

        //событие на зажатие кнопки, начала drag'n'drop
        private void pb_MouseDown(object sender, MouseEventArgs e)
        {
            movingPB = sender as PictureBox;
            dx = e.X;
            dy = e.Y;
            movingPB.DoDragDrop(movingPB, DragDropEffects.Move);
        }

        //расчет координат точки на второй вершине (конечной для стрелки)
        private Point getNearestPoint(Point to, Point from, int r)
        {
            double x1 = to.X + r, y1 = to.Y + r;
            double x2 = from.X + r, y2 = from.Y + r;
            double a = Math.Atan2(y1 - y2, x1 - x2);
            double dst = Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1)) - r;
            int x3 = (int)(x1 - dst * Math.Cos(a));
            int y3 = (int)(y1 - dst * Math.Sin(a));

            return new Point(x3, y3);
        }

        //расчет координат текста подписи линии
        private PointF getPointForText(Point to, Point from, int r)
        {
            //математики мало, но для ее понимания пришлось сломать мозг

            double x1 = to.X + r, y1 = to.Y + r;
            double x2 = from.X + r, y2 = from.Y + r;
            double a = Math.Atan2(y2 - y1, x2 - x1);
            double dst = Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1)) - 10;
            dst /= 2;
            float x3 = (float)(x1 + dst * Math.Cos(a));
            float y3 = (float)(y1 + dst * Math.Sin(a));
            x3 += (float)(Math.Sin(a) * r);
            y3 -= (float)(Math.Cos(a) * r);

            return new PointF(x3, y3);
        }

        //расчет стартового угла рисования дуги
        private double getStartAngle(Point to, Point from, int r)
        {
            double x1 = to.X + r, y1 = to.Y + r;
            double x2 = from.X + r, y2 = from.Y + r;
            return Math.Atan2(y2 - y1, x2 - x1);
        }

        private List<List<int>> getNumberConnections()
        {
            int nv = nfa.grid.Count; //количество вершин
            List<List<int>> res = new List<List<int>>();
            for (int i = 0; i < nv; ++i)
            {
                res.Add(new List<int>());
                for (int j = 0; j < nv; ++j)
                {
                    res[i].Add(0);
                }
            }
            for (int i = 0; i < nfa.grid.Count; ++i)
            {
                for (int j = 0; j < nfa.grid[i].Count; ++j)
                {
                    for (int k = 0; k < nfa.grid[i][j].Count; ++k)
                    {
                        res[i][nfa.grid[i][j][k]]++;
                        res[nfa.grid[i][j][k]][i]++;
                    }
                }
            }

            return res;
        }

        //отрисовка/перерисовка линий
        private void drawLines()
        {
            if (!is_builded)
                return;
            Pen pen = new Pen(Color.Black, 4);
            Pen spen = new Pen(Color.Black, 4);
            //конец линии - стрелка
            pen.CustomEndCap = new System.Drawing.Drawing2D.AdjustableArrowCap(3.0f, 4.0f, true);

            List<List<int>> paintLine = getNumberConnections();

            int nv = nfa.grid.Count; //количество вершин
            List<List<int>> paintedLine = new List<List<int>>();
            for (int i = 0; i < nv; ++i)
            {
                paintedLine.Add(new List<int>());
                for (int j = 0; j < nv; ++j)
                {
                    paintedLine[i].Add(0);
                }
            }

            //рисуем на панели
            using (Graphics g = panel1.CreateGraphics())
            {
                //очистка панели
                g.Clear(Color.White);

                for (int i = 0; i < nfa.grid.Count; ++i)
                {
                    int fvertex = i; //номер стартовой вершины
                    PictureBox pb0 = pbs[fvertex];
                    for (int j = 0; j < nfa.grid[i].Count; ++j)
                    {
                        //перебор всех переходов
                        for (int k = 0; k < nfa.grid[i][j].Count; ++k)
                        {
                            int svertex = nfa.grid[i][j][k]; //номер конечной вершины
                            PictureBox pb1 = pbs[svertex];

                            Point a, b;
                            //a - начальная точка
                            //b - конечная точка
                            //учитываются смещения от центра
                            a = new Point(pb0.Left + pb0.Width / 2, pb0.Top + pb0.Height / 2);
                            b = getNearestPoint(pb0.Location, pb1.Location, pb1.Width / 2);


                            //Если это первая линия и количество линий НЕчетно - линия прямая
                            //иначе дуга
                            string letter = nfa.alphabet[j].ToString();

                            if (paintLine[fvertex][svertex] % 2 == 1)
                            {
                                g.DrawLine(pen, a, b);
                                g.DrawString(letter, Font, Brushes.Black, getPointForText(pb0.Location, pb1.Location, pb1.Width / 2));
                            }
                            else
                            {
                                Point from = pb0.Location;
                                Point to = pb1.Location;
                                int r = pb1.Width / 2;
                                double beta = getStartAngle(pb0.Location, pb1.Location, r);
                                double alpha = Math.Acos(3 / Math.Sqrt(9.25));

                                int xst, yst; //координаты начала линии
                                int xen, yen; //координаты конца линии
                                int xis1, yis1, xis2, yis2; //координаты изломов
                                int nArc = paintedLine[fvertex][svertex] % 2; //номер линии. Четный - верх, нечетный - низ

                                int lenlin = 2 * r;

                                if (nArc == 0) //верхняя
                                {
                                    xst = (int)(from.X + r + r * Math.Cos(beta));
                                    yst = (int)(from.Y + r + r * Math.Sin(beta));
                                    xen = (int)(to.X + r + r * Math.Cos(Math.PI + beta));
                                    yen = (int)(to.Y + r + r * Math.Sin(Math.PI + beta));

                                    xis1 = (int)(xst + lenlin * Math.Cos(beta - alpha));
                                    yis1 = (int)(yst + lenlin * Math.Sin(beta - alpha));
                                    xis2 = (int)(xen + lenlin * Math.Cos(Math.PI + beta + alpha));
                                    yis2 = (int)(yen + lenlin * Math.Sin(Math.PI + beta + alpha));

                                    int distance = (int)(Math.Sqrt((from.X - to.X) * (from.X - to.X) + (from.Y - to.Y) * (from.Y - to.Y)));
                                    if (distance < 6 * r)
                                    {
                                        g.DrawLine(spen, xst, yst, xen, yen);
                                        g.DrawString(letter, Font, Brushes.Black, getPointForText(pb0.Location, pb1.Location, pb1.Width / 2));
                                    }
                                    else
                                    {
                                        g.DrawLine(spen, xst, yst, xis1, yis1);
                                        g.DrawLine(spen, xis1, yis1, xis2, yis2);
                                        g.DrawLine(pen, xis2, yis2, xen, yen);
                                        g.DrawString(letter, Font, Brushes.Black, getPointForText(pb0.Location, pb1.Location, pb1.Width / 2));
                                    }
                                }
                                else //Нижняя
                                {
                                    xst = (int)(from.X + r + r * Math.Cos(beta));
                                    yst = (int)(from.Y + r + r * Math.Sin(beta));
                                    xen = (int)(to.X + r + r * Math.Cos(Math.PI + beta));
                                    yen = (int)(to.Y + r + r * Math.Sin(Math.PI + beta));

                                    xis1 = (int)(xst + lenlin * Math.Cos(beta - alpha));
                                    yis1 = (int)(yst + lenlin * Math.Sin(beta - alpha));
                                    xis2 = (int)(xen + lenlin * Math.Cos(Math.PI + beta + alpha));
                                    yis2 = (int)(yen + lenlin * Math.Sin(Math.PI + beta + alpha));

                                    int distance = (int)(Math.Sqrt((from.X - to.X) * (from.X - to.X) + (from.Y - to.Y) * (from.Y - to.Y)));
                                    if (distance < 6 * r)
                                    {
                                        g.DrawLine(spen, xst, yst, xen, yen);
                                        g.DrawString(letter, Font, Brushes.Black, getPointForText(pb0.Location, pb1.Location, pb1.Width / 2));
                                    }
                                    else
                                    {
                                        g.DrawLine(spen, xst, yst, xis1, yis1);
                                        g.DrawLine(spen, xis1, yis1, xis2, yis2);
                                        g.DrawLine(pen, xis2, yis2, xen, yen);
                                        g.DrawString(letter, Font, Brushes.Black, getPointForText(pb0.Location, pb1.Location, pb1.Width / 2));
                                    }
                                }
                            }
                            paintedLine[fvertex][svertex]++;
                            paintedLine[svertex][fvertex]++;
                        }
                    }
                }
                g.Save();
            }
        }


        private bool checkfinal(int v)
        {
            if (used[v])
                return isfinal[v];
            used[v] = true;
            //идем в глубину, пока не найдем конечную вершину или не сможем делать переходы
            if (isfinal[v])
                return true;

            for (int i = 0; i < nfa.grid[v][0].Count; ++i)
            {
                isfinal[v] = isfinal[v] || checkfinal(nfa.grid[v][0][i]);
                if (isfinal[v])
                    return true;
            }
            return isfinal[v];
        }

        //создание PictureBox-ов
        private void createPictureBoxes(int n)
        {
            if (!is_builded)
                return;
            //пометка вершин конечными
            isfinal = new bool[n];
            used = new bool[n];
            isfinal[isfinal.Length - 1] = true;
            /*
                Если вершина по eps переходу соединена с конечной - она конечна.
                Конечной считается последняя
                eps Переходы в последнем столбце
            */
            for (int i = 0; i < n; ++i)
            {
                Array.Clear(used, 0, used.Length);
                checkfinal(i);
            }

            //размеры PictureBox-а
            int w = 50;
            int h = 50;

            //добавление скругления PictureBox-а, но не отрисовка самого круга
            System.Drawing.Drawing2D.GraphicsPath roundpb = new System.Drawing.Drawing2D.GraphicsPath();
            roundpb.AddEllipse(0, 0, w, h);
            Region regionpb = new Region(roundpb);

            for (int i = 0; i < n; ++i)
            {
                var cur = new PictureBox();
                cur.Image = new Bitmap(w, h);
                cur.Width = w;
                cur.Height = h;
                using (Graphics g = Graphics.FromImage(cur.Image))
                {
                    //отрисовка круга и дополнительного круга, если вершина конечна
                    Pen myBorderPen = new Pen(Color.Black, 1.8f);
                    if (isfinal[i])
                        g.DrawEllipse(myBorderPen, 4, 4, w - 8, h - 8);
                    g.DrawEllipse(myBorderPen, 1, 1, w - 2, h - 2);
                    //вписывание названия вершины
                    g.DrawString("S" + i, Font, Brushes.Black, (float)w / 2 - 10, (float)h / 2 - 10);
                }
                //расположение
                cur.Left = 10 + 60 * (i % 5);
                //выводим по пять в ряд
                cur.Top = 10 + 60 * (i / 5);
                cur.MouseDown += pb_MouseDown;
                cur.Region = regionpb;

                pbs.Add(cur);
                panel1.Controls.Add(cur);
            }
        }

        //Разрешаем перенос, если переносится картинка
        private void panel1_DragEnter(object sender, DragEventArgs e)
        {
            var strs = e.Data.GetFormats();
            if (strs.Length > 0 && strs[0].LastIndexOf("PictureBox") != -1)
                e.Effect = DragDropEffects.Move;
        }

        //сам перенос
        private void panel1_DragDrop(object sender, DragEventArgs e)
        {
            var cur = e.Data;
            movingPB.Location = panel1.PointToClient(MousePosition);
            movingPB.Left -= dx;
            movingPB.Top -= dy;
            drawLines();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            drawLines();
        }

        private void button_build_Click(object sender, EventArgs e)
        {
            pbs.ForEach(panel1.Controls.Remove);
            pbs.Clear();
            nfa = new NFA(textbox_RE.Text);
            textbox_PostfixRE.Text = nfa.PostixRegularExpression;
            textbox_InfixRE.Text = nfa.InfixRegularExpression;

            nfa.build(textbox_graph);
            is_builded = true;
            createPictureBoxes(nfa.grid.Count);
            drawLines();
            button_save.Enabled = true;
            button_check.Enabled = true;
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                Bitmap bmp = new Bitmap(panel1.Width, panel1.Height);
                Rectangle rect = panel1.RectangleToScreen(panel1.Bounds);
                Graphics.FromImage(bmp).CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp";
                sfd.Title = "Сохранение графика";
                sfd.ShowDialog();
                if (sfd.FileName != "")
                {
                    string extention = Path.GetExtension(sfd.FileName);
                    if (extention == ".png")
                        bmp.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
                    if (extention == ".jpg")
                        bmp.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
            }
            else
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Text file|*.txt";
                sfd.Title = "Сохранение лога построения графика";
                sfd.ShowDialog();
                if (sfd.FileName != "")
                {
                    using (StreamWriter sw = new StreamWriter(sfd.FileName))
                        sw.Write(textbox_graph.Text);
                }
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            drawLines();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textbox_graph.Font = new Font(FontFamily.GenericMonospace, textbox_graph.Font.Size);
        }

        private void button_check_Click(object sender, EventArgs e)
        {
            CheckForm chkfrm = new CheckForm(nfa);
            chkfrm.ShowDialog();
        }
    }
}
