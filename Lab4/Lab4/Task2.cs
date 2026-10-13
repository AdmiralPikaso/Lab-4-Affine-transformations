using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace Lab4
{
    public sealed class Mat3
    {
        public readonly double[][] M;

        public Mat3(double[][] m = null)
        {
            if (m == null)
            {
                M = new[]
                {
                    new[] { 1.0, 0.0, 0.0 },
                    new[] { 0.0, 1.0, 0.0 },
                    new[] { 0.0, 0.0, 1.0 }
                };
            }
            else
            {
                M = new double[3][];
                for (int i = 0; i < 3; i++)
                    M[i] = new[] { m[i][0], m[i][1], m[i][2] };
            }
        }

        public static Mat3 operator *(Mat3 a, Mat3 b)
        {
            double[][] r = new double[3][];
            for (int i = 0; i < 3; i++)
            {
                r[i] = new double[3];
                for (int j = 0; j < 3; j++)
                    r[i][j] = a.M[i][0] * b.M[0][j] + a.M[i][1] * b.M[1][j] + a.M[i][2] * b.M[2][j];
            }
            return new Mat3(r);
        }

        public PointF Apply(PointF p)
        {
            double x = M[0][0] * p.X + M[0][1] * p.Y + M[0][2];
            double y = M[1][0] * p.X + M[1][1] * p.Y + M[1][2];
            return new PointF((float)x, (float)y);
        }
    }

    public static class Affine
    {
        public static Mat3 Identity() => new Mat3();

        public static Mat3 Translation(double dx, double dy) =>
            new Mat3(new[]
            {
                new[] { 1.0, 0.0, dx },
                new[] { 0.0, 1.0, dy },
                new[] { 0.0, 0.0, 1.0 }
            });

        public static Mat3 Rotation(double angle)
        {
            double c = Math.Cos(angle), s = Math.Sin(angle);
            return new Mat3(new[]
            {
                new[] { c, -s, 0.0 },
                new[] { s, c, 0.0 },
                new[] { 0.0, 0.0, 1.0 }
            });
        }

        public static Mat3 Scaling(double sx, double sy) =>
            new Mat3(new[]
            {
                new[] { sx, 0.0, 0.0 },
                new[] { 0.0, sy, 0.0 },
                new[] { 0.0, 0.0, 1.0 }
            });

        public static Mat3 Translate(double dx, double dy) => Translation(dx, dy);

        public static Mat3 RotateAround(PointF p, double angle) =>
            Translation(p.X, p.Y) * Rotation(angle) * Translation(-p.X, -p.Y);

        public static Mat3 ScaleAround(PointF p, double sx, double sy) =>
            Translation(p.X, p.Y) * Scaling(sx, sy) * Translation(-p.X, -p.Y);

        public static PointF PolygonCenter(List<PointF> poly)
        {
            int n = poly.Count;
            if (n == 0)
                throw new InvalidOperationException("пустой полигон: центра нет");
            double sx = 0.0, sy = 0.0;
            foreach (PointF v in poly) { sx += v.X; sy += v.Y; }
            return new PointF((float)(sx / n), (float)(sy / n));
        }

        public static Mat3 RotateAroundCenter(List<PointF> poly, double angle) =>
            RotateAround(PolygonCenter(poly), angle);

        public static Mat3 ScaleAroundCenter(List<PointF> poly, double sx, double sy) =>
            ScaleAround(PolygonCenter(poly), sx, sy);

        public static List<PointF> Transformed(List<PointF> poly, Mat3 m)
        {
            List<PointF> result = new List<PointF>(poly.Count);
            foreach (PointF v in poly)
                result.Add(m.Apply(v));
            return result;
        }

        public static void ApplyTransform(List<PointF> poly, Mat3 m)
        {
            for (int i = 0; i < poly.Count; i++)
                poly[i] = m.Apply(poly[i]);
        }
    }

    public sealed partial class MainForm
    {
        private ListBox lstPolygons;
        private TextBox txtDx, txtDy, txtAngle, txtScaleX, txtScaleY, txtPivotX, txtPivotY;
        private Panel pnlTask2;
        private Label lblMatrix, lblStatus;
        private Font matrixFont;
        private int selectedPolygonIndex = -1;

        private void MovePolygon() => Run(() =>
            Apply(Selected(), Affine.Translate(Num(txtDx), Num(txtDy)), "Смещение"));

        private void RotateAroundPoint() => Run(() =>
            Apply(Selected(), Affine.RotateAround(new PointF((float)Num(txtPivotX), (float)Num(txtPivotY)), Num(txtAngle) * Math.PI / 180.0), "Поворот вокруг точки"));

        private void RotateAroundCenter() => Run(() =>
        {
            List<PointF> poly = Selected();
            Apply(poly, Affine.RotateAroundCenter(poly, Num(txtAngle) * Math.PI / 180.0), "Поворот вокруг центра");
        });

        private void ScaleAroundPoint() => Run(() =>
            Apply(Selected(), Affine.ScaleAround(new PointF((float)Num(txtPivotX), (float)Num(txtPivotY)), Num(txtScaleX), Num(txtScaleY)), "Масштабирование относительно точки"));

        private void ScaleAroundCenter() => Run(() =>
        {
            List<PointF> poly = Selected();
            Apply(poly, Affine.ScaleAroundCenter(poly, Num(txtScaleX), Num(txtScaleY)), "Масштабирование относительно центра");
        });

        private void Apply(List<PointF> poly, Mat3 m, string name)
        {
            Affine.ApplyTransform(poly, m);
            ShowMatrix(m);
            lblStatus.Text = name + " — применено.";
            T3_OnPolygonsChanged(); // выбранные в Task3 рёбра следуют за полигоном
            pbCanvas.Invalidate();
        }

        private void ShowMatrix(Mat3 m)
        {
            lblMatrix.Text = string.Format(CultureInfo.InvariantCulture,
                "Матрица:\n| {0,7:0.###} {1,7:0.###} {2,7:0.###} |\n| {3,7:0.###} {4,7:0.###} {5,7:0.###} |\n| {6,7:0.###} {7,7:0.###} {8,7:0.###} |",
                m.M[0][0], m.M[0][1], m.M[0][2],
                m.M[1][0], m.M[1][1], m.M[1][2],
                m.M[2][0], m.M[2][1], m.M[2][2]);
        }

        private List<PointF> Selected()
        {
            if (selectedPolygonIndex < 0 || selectedPolygonIndex >= polygons.Count)
                throw new InvalidOperationException("Сначала выберите объект в списке.");
            return polygons[selectedPolygonIndex];
        }

        private double Num(TextBox tb)
        {
            double v;
            if (!TryNum(tb, out v))
                throw new FormatException("Некорректное число: \"" + tb.Text + "\"");
            return v;
        }

        private static bool TryNum(TextBox tb, out double value)
        {
            value = 0.0;
            return tb != null &&
                double.TryParse(tb.Text.Replace(',', '.').Trim(),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out value) &&
                !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private void Run(Action action)
        {
            try { action(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "ЛР4"); }
        }

        private void InitializeTask2()
        {
            BuildUI();
            pbCanvas.MouseUp += (s, e) => RefreshList();
            btnClear.Click += (s, e) => RefreshList();
            pbCanvas.Paint += PaintTask2;
            Disposed += (s, e) => { if (matrixFont != null) matrixFont.Dispose(); };
            RefreshList();
        }

        private void BuildUI()
        {
            ClientSize = new Size(1220, 760);
            MinimumSize = new Size(1236, 750);
            pbCanvas.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;

            pnlTask2 = new Panel
            {
                Location = new Point(924, 48),
                Size = new Size(280, 700),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(pnlTask2);

            pnlTask2.Controls.Add(new Label { Text = "Полигоны (выберите объект):", Location = new Point(8, 8), Size = new Size(258, 18) });

            lstPolygons = new ListBox { Location = new Point(8, 28), Size = new Size(258, 110), IntegralHeight = false };
            lstPolygons.SelectedIndexChanged += (s, e) => { selectedPolygonIndex = lstPolygons.SelectedIndex; pbCanvas.Invalidate(); };
            pnlTask2.Controls.Add(lstPolygons);
            pnlTask2.Controls.Add(MakeButton("Обновить список", 8, 142, 258, 26, (s, e) => RefreshList()));

            GroupBox move = MakeGroup("Смещение", 176);
            txtDx = MakeField(move, "dx:", 10, 26, "50");
            txtDy = MakeField(move, "dy:", 10, 52, "0");
            move.Controls.Add(MakeButton("Применить", 150, 23, 98, 52, (s, e) => MovePolygon()));
            pnlTask2.Controls.Add(move);

            GroupBox point = MakeGroup("Заданная точка (x, y)", 270);
            point.Size = new Size(258, 60);
            txtPivotX = MakeField(point, "x:", 10, 26, "450", 22, 96);
            txtPivotY = MakeField(point, "y:", 130, 26, "325", 22, 96);
            txtPivotX.TextChanged += (s, e) => pbCanvas.Invalidate();
            txtPivotY.TextChanged += (s, e) => pbCanvas.Invalidate();
            pnlTask2.Controls.Add(point);

            GroupBox rot = MakeGroup("Поворот (градусы)", 338);
            txtAngle = MakeField(rot, "Угол:", 10, 24, "90", 40, 90);
            rot.Controls.Add(MakeButton("Вокруг точки", 10, 50, 118, 26, (s, e) => RotateAroundPoint()));
            rot.Controls.Add(MakeButton("Вокруг центра", 132, 50, 116, 26, (s, e) => RotateAroundCenter()));
            pnlTask2.Controls.Add(rot);

            GroupBox scale = MakeGroup("Масштабирование", 432);
            scale.Size = new Size(258, 112);
            txtScaleX = MakeField(scale, "sx:", 10, 22, "2", 30, 80);
            txtScaleY = MakeField(scale, "sy:", 10, 48, "2", 30, 80);
            scale.Controls.Add(MakeButton("Отн. точки", 10, 78, 118, 26, (s, e) => ScaleAroundPoint()));
            scale.Controls.Add(MakeButton("Отн. центра", 132, 78, 116, 26, (s, e) => ScaleAroundCenter()));
            pnlTask2.Controls.Add(scale);

            matrixFont = new Font(FontFamily.GenericMonospace, 8f);
            lblMatrix = new Label
            {
                Location = new Point(8, 552),
                Size = new Size(258, 68),
                BorderStyle = BorderStyle.FixedSingle,
                Font = matrixFont,
                Text = "Матрица: -"
            };
            pnlTask2.Controls.Add(lblMatrix);

            lblStatus = new Label
            {
                Location = new Point(8, 630),
                Size = new Size(258, 60),
                ForeColor = Color.DimGray,
                Text = "ЛКМ — вершина, ПКМ — замкнуть."
            };
            pnlTask2.Controls.Add(lblStatus);
        }

        private static GroupBox MakeGroup(string text, int y) =>
            new GroupBox { Text = text, Location = new Point(8, y), Size = new Size(258, 86) };

        private static Button MakeButton(string text, int x, int y, int w, int h, EventHandler onClick)
        {
            Button b = new Button { Text = text, Location = new Point(x, y), Size = new Size(w, h) };
            b.Click += onClick;
            return b;
        }

        private static TextBox MakeField(Control parent, string label, int x, int y, string def,
            int labelWidth = 56, int width = 70)
        {
            parent.Controls.Add(new Label
            {
                Text = label,
                Location = new Point(x, y),
                Size = new Size(labelWidth, 20),
                TextAlign = ContentAlignment.MiddleLeft
            });
            TextBox tb = new TextBox { Location = new Point(x + labelWidth, y), Size = new Size(width, 20), Text = def };
            parent.Controls.Add(tb);
            return tb;
        }

        private void RefreshList()
        {
            if (lstPolygons.Items.Count == polygons.Count)
                return;

            int prev = selectedPolygonIndex;
            lstPolygons.BeginUpdate();
            lstPolygons.Items.Clear();
            for (int i = 0; i < polygons.Count; i++)
            {
                int n = polygons[i].Count;
                lstPolygons.Items.Add((n == 1 ? "точка" : n == 2 ? "ребро" : "полигон") + " " + (i + 1) + " (" + n + " верш.)");
            }
            lstPolygons.EndUpdate();

            if (prev >= 0 && prev < polygons.Count)
                lstPolygons.SelectedIndex = prev;
            else if (polygons.Count > 0)
                lstPolygons.SelectedIndex = polygons.Count - 1;
            else
                selectedPolygonIndex = -1;
        }

        private void PaintTask2(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            if (selectedPolygonIndex >= 0 && selectedPolygonIndex < polygons.Count)
            {
                List<PointF> poly = polygons[selectedPolygonIndex];
                using (Pen pen = new Pen(Color.Red, 3f))
                using (SolidBrush brush = new SolidBrush(Color.Red))
                {
                    if (poly.Count == 1)
                        e.Graphics.FillEllipse(brush, poly[0].X - 4f, poly[0].Y - 4f, 8f, 8f);
                    else
                        DrawPolygon(e.Graphics, poly, pen, brush); // с замыкающим ребром
                }
            }

            double px, py;
            if (TryNum(txtPivotX, out px) && TryNum(txtPivotY, out py))
                using (Pen pen = new Pen(Color.Green, 1.5f))
                {
                    e.Graphics.DrawLine(pen, (float)px - 8f, (float)py, (float)px + 8f, (float)py);
                    e.Graphics.DrawLine(pen, (float)px, (float)py - 8f, (float)px, (float)py + 8f);
                }
        }
    }
}
