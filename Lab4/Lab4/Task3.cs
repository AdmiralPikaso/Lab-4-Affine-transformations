using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace Lab4
{
    public sealed partial class MainForm
    {
        private enum Task3Mode
        {
            None,
            EdgeIntersection,
            PointInPolygon,
            PointVsEdge
        }

        private struct T3Edge
        {
            public PointF A;
            public PointF B;

            public T3Edge(PointF a, PointF b)
            {
                A = a;
                B = b;
            }
        }

        private const float EdgePickTolerance = 7f;

        private Panel pnlTask3;
        private Label lblT3Status;
        private Label lblT3Hint;

        private Task3Mode task3Mode = Task3Mode.None;

        private readonly List<PointF> t3SecondEdgePoints = new List<PointF>();
        private T3Edge t3CompletedSecond;
        private bool t3HasCompletedSecond;
        private T3Edge t3FirstEdge;
        private bool t3HasFirstEdge;

        private PointF t3Mouse;
        private PointF t3TestPoint;
        private bool t3HasTestPoint;
        private T3Edge t3SelectedEdge;
        private bool t3HasSelectedEdge;

        private PointF t3Intersection;
        private bool t3HasIntersection;
        private bool t3IntersectionOnSegments;

        private void InitializeTask3()
        {
            BuildTask3UI();
            pbCanvas.MouseMove += T3_MouseMove;
            pbCanvas.MouseDown += T3_MouseDown;
            pbCanvas.Paint += T3_Paint;
        }

        private void BuildTask3UI()
        {
            ClientSize = new Size(1512, 760);
            MinimumSize = new Size(1528, 750);

            pnlTask3 = new Panel
            {
                Location = new Point(1212, 48),
                Size = new Size(280, 700),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(pnlTask3);

            pnlTask3.Controls.Add(new Label
            {
                Text = "Задание 3: точка и рёбра",
                Location = new Point(8, 8),
                Size = new Size(258, 18)
            });

            pnlTask3.Controls.Add(MakeButton("Пересечение двух рёбер", 8, 32, 258, 30,
                (s, e) => T3_SetMode(Task3Mode.EdgeIntersection)));
            pnlTask3.Controls.Add(MakeButton("Точка в полигоне", 8, 68, 258, 30,
                (s, e) => T3_SetMode(Task3Mode.PointInPolygon)));
            pnlTask3.Controls.Add(MakeButton("Точка относительно ребра", 8, 104, 258, 30,
                (s, e) => T3_SetMode(Task3Mode.PointVsEdge)));
            pnlTask3.Controls.Add(MakeButton("Сбросить проверку", 8, 140, 258, 26,
                (s, e) => T3_Reset()));

            lblT3Hint = new Label
            {
                Location = new Point(8, 174),
                Size = new Size(258, 130),
                ForeColor = Color.DimGray
            };
            pnlTask3.Controls.Add(lblT3Hint);

            Font statusFont = matrixFont ?? new Font(FontFamily.GenericMonospace, 8f);
            lblT3Status = new Label
            {
                Location = new Point(8, 312),
                Size = new Size(258, 378),
                BorderStyle = BorderStyle.FixedSingle,
                Font = statusFont,
                Text = "Проверка не запущена."
            };
            pnlTask3.Controls.Add(lblT3Status);

            T3_UpdateHint();
        }

        private void T3_SetMode(Task3Mode mode)
        {
            task3Mode = mode;
            T3_ClearState();
            T3_UpdateHint();
            T3_UpdateStatus("Режим включён: " + T3_ModeName(mode) + ".");
            pbCanvas.Invalidate();
        }

        private void T3_Reset()
        {
            task3Mode = Task3Mode.None;
            T3_ClearState();
            T3_UpdateHint();
            T3_UpdateStatus("Проверка сброшена.");
            pbCanvas.Invalidate();
        }

        private void T3_ClearState()
        {
            t3SecondEdgePoints.Clear();
            t3HasCompletedSecond = false;
            t3HasFirstEdge = false;
            t3HasTestPoint = false;
            t3HasSelectedEdge = false;
            t3HasIntersection = false;
        }

        private static string T3_ModeName(Task3Mode mode)
        {
            switch (mode)
            {
                case Task3Mode.EdgeIntersection: return "пересечение двух рёбер";
                case Task3Mode.PointInPolygon: return "точка в полигоне";
                case Task3Mode.PointVsEdge: return "точка относительно ребра";
                default: return "нет";
            }
        }

        private void T3_UpdateHint()
        {
            string hint;
            switch (task3Mode)
            {
                case Task3Mode.EdgeIntersection:
                    hint = t3HasFirstEdge
                        ? "ЛКМ — начало второго ребра.\nВедите мышь: пересечение\nпересчитывается динамически.\nЛКМ — конец второго ребра.\nПКМ — выбрать другое первое ребро."
                        : "Кликните ЛКМ рядом с ребром\nлюбого полигона, чтобы выбрать\nпервое ребро.\nЗатем нарисуйте второе ребро.";
                    break;
                case Task3Mode.PointInPolygon:
                    hint = "Ведите мышь: точка под курсором\nпроверяется на принадлежность всем\nполигонам (выпуклым и невыпуклым).\nЛКМ — зафиксировать точку.\nНичего не нужно перерисовывать.";
                    break;
                case Task3Mode.PointVsEdge:
                    hint = "Кликните ЛКМ рядом с ребром,\nчтобы выбрать его.\nЗатем ведите мышь: положение\nточки (слева/справа) вычисляется\nотносительно выбранного ребра.";
                    break;
                default:
                    hint = "Выберите один из режимов проверки.\nПроверки выполняются многократно\nбез очистки сцены и повторного\nвыбора инструмента.";
                    break;
            }
            lblT3Hint.Text = hint;
        }

        private void T3_UpdateStatus(string text)
        {
            lblT3Status.Text = text;
        }

        private void T3_MouseMove(object sender, MouseEventArgs e)
        {
            if (task3Mode == Task3Mode.None)
                return;

            t3Mouse = new PointF(e.X, e.Y);

            switch (task3Mode)
            {
                case Task3Mode.EdgeIntersection:
                    T3_UpdateIntersectionPreview();
                    break;
                case Task3Mode.PointInPolygon:
                    t3TestPoint = t3Mouse;
                    t3HasTestPoint = true;
                    T3_UpdateStatus(T3_DescribePointInPolygon(t3TestPoint));
                    break;
                case Task3Mode.PointVsEdge:
                    if (!t3HasSelectedEdge)
                        t3HasSelectedEdge = T3_FindNearestEdge(t3Mouse, out t3SelectedEdge);
                    t3TestPoint = t3Mouse;
                    t3HasTestPoint = true;
                    if (t3HasSelectedEdge)
                        T3_UpdateStatus(T3_DescribePointVsEdge(t3TestPoint));
                    break;
            }

            pbCanvas.Invalidate();
        }

        private void T3_MouseDown(object sender, MouseEventArgs e)
        {
            if (task3Mode == Task3Mode.None)
                return;

            PointF p = new PointF(e.X, e.Y);

            if (e.Button == MouseButtons.Right)
            {
                if (task3Mode == Task3Mode.EdgeIntersection)
                {
                    t3HasFirstEdge = false;
                    t3SecondEdgePoints.Clear();
                    t3HasCompletedSecond = false;
                    t3HasIntersection = false;
                    T3_UpdateHint();
                    T3_UpdateStatus("Первое ребро сброшено. Выберите ребро заново.");
                }
                pbCanvas.Invalidate();
                return;
            }

            if (e.Button != MouseButtons.Left)
                return;

            switch (task3Mode)
            {
                case Task3Mode.EdgeIntersection:
                    T3_HandleIntersectionClick(p);
                    break;
                case Task3Mode.PointInPolygon:
                    t3TestPoint = p;
                    t3HasTestPoint = true;
                    T3_UpdateStatus(T3_DescribePointInPolygon(p));
                    break;
                case Task3Mode.PointVsEdge:
                    T3Edge edge;
                    if (T3_FindNearestEdge(p, out edge))
                    {
                        t3SelectedEdge = edge;
                        t3HasSelectedEdge = true;
                        t3TestPoint = p;
                        t3HasTestPoint = true;
                        T3_UpdateStatus(T3_DescribePointVsEdge(p));
                    }
                    else
                    {
                        T3_UpdateStatus("Рядом с курсором нет ребра. Кликните ближе к ребру.");
                    }
                    break;
            }

            pbCanvas.Invalidate();
        }

        private void T3_HandleIntersectionClick(PointF p)
        {
            if (!t3HasFirstEdge)
            {
                T3Edge edge;
                if (T3_FindNearestEdge(p, out edge))
                {
                    t3FirstEdge = edge;
                    t3HasFirstEdge = true;
                    t3SecondEdgePoints.Clear();
                    t3HasCompletedSecond = false;
                    t3HasIntersection = false;
                    T3_UpdateHint();
                    T3_UpdateStatus("Первое ребро выбрано. ЛКМ — начало второго ребра.");
                }
                else
                {
                    T3_UpdateStatus("Не найдено ребро рядом с курсором. Кликните ближе к ребру полигона.");
                }
                return;
            }

            t3SecondEdgePoints.Add(p);

            if (t3SecondEdgePoints.Count >= 2)
            {
                t3CompletedSecond = new T3Edge(t3SecondEdgePoints[0], t3SecondEdgePoints[1]);
                t3HasCompletedSecond = true;
                t3SecondEdgePoints.Clear();
                T3_UpdateStatus(T3_ComputeIntersection(t3FirstEdge, t3CompletedSecond, false));
                T3_UpdateHint();
            }
            else
            {
                T3_UpdateIntersectionPreview();
            }
        }

        private void T3_UpdateIntersectionPreview()
        {
            if (!t3HasFirstEdge)
            {
                t3HasIntersection = false;
                return;
            }

            if (t3SecondEdgePoints.Count == 0)
            {
                t3HasIntersection = false;
                T3_UpdateStatus("Первое ребро выбрано. ЛКМ — начало второго ребра.");
                return;
            }

            T3Edge preview = new T3Edge(t3SecondEdgePoints[0], t3Mouse);
            T3_UpdateStatus(T3_ComputeIntersection(t3FirstEdge, preview, true));
        }

        private string T3_ComputeIntersection(T3Edge first, T3Edge second, bool preview)
        {
            PointF res;
            bool onFirst, onSecond;

            if (!T3_LineIntersect(first.A, first.B, second.A, second.B, out res, out onFirst, out onSecond))
            {
                t3HasIntersection = false;
                return (preview ? "Предпросмотр: " : "") + "рёбра параллельны — точки пересечения нет.";
            }

            t3HasIntersection = true;
            t3Intersection = res;
            t3IntersectionOnSegments = onFirst && onSecond;

            string head = preview ? "Предпросмотр: " : "";

            if (t3IntersectionOnSegments)
                return string.Format(CultureInfo.InvariantCulture,
                    "{0}рёбра пересекаются в точке ({1:0.#}, {2:0.#}).", head, res.X, res.Y);

            return string.Format(CultureInfo.InvariantCulture,
                "{0}прямые пересекаются в ({1:0.#}, {2:0.#}),\nно точка лежит вне одного из отрезков.",
                head, res.X, res.Y);
        }

        private string T3_DescribePointInPolygon(PointF p)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(CultureInfo.InvariantCulture, "Точка ({0:0.#}, {1:0.#}):\n", p.X, p.Y);

            int found = 0;
            for (int i = 0; i < polygons.Count; i++)
            {
                List<PointF> poly = polygons[i];
                if (poly.Count < 3)
                    continue;

                if (PointInPolygon(poly, p))
                {
                    found++;
                    sb.AppendFormat("  внутри полигона {0} ({1})\n", i + 1,
                        IsConvex(poly) ? "выпуклый" : "невыпуклый");
                }
            }

            if (found == 0)
                sb.Append("  вне всех полигонов.");

            return sb.ToString();
        }

        private string T3_DescribePointVsEdge(PointF p)
        {
            int side = ClassifyPointEdge(t3SelectedEdge.A, t3SelectedEdge.B, p);
            double cross = Cross(t3SelectedEdge.A, t3SelectedEdge.B, p);

            string where = side > 0 ? "СПРАВА" : side < 0 ? "СЛЕВА" : "на прямой";
            return string.Format(CultureInfo.InvariantCulture,
                "Точка ({0:0.#}, {1:0.#})\nлежит {2} относительно ребра.\n\nВекторное произведение =\n{3:0.###}",
                p.X, p.Y, where, cross);
        }

        private void T3_Paint(object sender, PaintEventArgs e)
        {
            if (task3Mode == Task3Mode.None)
                return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            switch (task3Mode)
            {
                case Task3Mode.EdgeIntersection:
                    T3_PaintIntersection(g);
                    break;
                case Task3Mode.PointInPolygon:
                    T3_PaintPointInPolygon(g);
                    break;
                case Task3Mode.PointVsEdge:
                    T3_PaintPointVsEdge(g);
                    break;
            }
        }

        private void T3_PaintIntersection(Graphics g)
        {
            if (t3HasFirstEdge)
                using (Pen pen = new Pen(Color.OrangeRed, 3f))
                    g.DrawLine(pen, t3FirstEdge.A, t3FirstEdge.B);

            using (Pen pen = new Pen(Color.Purple, 2f))
            {
                if (t3HasCompletedSecond)
                    g.DrawLine(pen, t3CompletedSecond.A, t3CompletedSecond.B);
                if (t3SecondEdgePoints.Count == 1)
                    g.DrawLine(pen, t3SecondEdgePoints[0], t3Mouse);
            }

            if (t3HasIntersection)
            {
                Color c = t3IntersectionOnSegments ? Color.Green : Color.Gray;
                using (SolidBrush brush = new SolidBrush(c))
                    g.FillEllipse(brush, t3Intersection.X - 5f, t3Intersection.Y - 5f, 10f, 10f);
                using (Pen pen = new Pen(c, 1.5f))
                    g.DrawEllipse(pen, t3Intersection.X - 8f, t3Intersection.Y - 8f, 16f, 16f);
            }
        }

        private void T3_PaintPointInPolygon(Graphics g)
        {
            if (!t3HasTestPoint)
                return;

            using (Pen pen = new Pen(Color.Green, 2.5f))
            {
                for (int i = 0; i < polygons.Count; i++)
                {
                    List<PointF> poly = polygons[i];
                    if (poly.Count < 3 || !PointInPolygon(poly, t3TestPoint))
                        continue;

                    for (int j = 0; j < poly.Count; j++)
                        g.DrawLine(pen, poly[j], poly[(j + 1) % poly.Count]);
                }
            }

            bool insideAny = false;
            for (int i = 0; i < polygons.Count && !insideAny; i++)
                if (polygons[i].Count >= 3 && PointInPolygon(polygons[i], t3TestPoint))
                    insideAny = true;

            using (SolidBrush brush = new SolidBrush(insideAny ? Color.Green : Color.Red))
                g.FillEllipse(brush, t3TestPoint.X - 5f, t3TestPoint.Y - 5f, 10f, 10f);
        }

        private void T3_PaintPointVsEdge(Graphics g)
        {
            if (!t3HasSelectedEdge)
                return;

            using (Pen pen = new Pen(Color.OrangeRed, 3f))
                g.DrawLine(pen, t3SelectedEdge.A, t3SelectedEdge.B);

            if (!t3HasTestPoint)
                return;

            int side = ClassifyPointEdge(t3SelectedEdge.A, t3SelectedEdge.B, t3TestPoint);
            Color c = side > 0 ? Color.DarkOrange : side < 0 ? Color.RoyalBlue : Color.Gray;

            using (SolidBrush brush = new SolidBrush(c))
                g.FillEllipse(brush, t3TestPoint.X - 5f, t3TestPoint.Y - 5f, 10f, 10f);
        }

        private bool T3_FindNearestEdge(PointF p, out T3Edge best)
        {
            best = default(T3Edge);
            float bestDistance = EdgePickTolerance;
            bool found = false;

            for (int i = 0; i < polygons.Count; i++)
            {
                List<PointF> poly = polygons[i];
                for (int j = 0; j + 1 < poly.Count; j++)
                {
                    float d = DistanceToSegment(p, poly[j], poly[j + 1]);
                    if (d <= bestDistance)
                    {
                        bestDistance = d;
                        best = new T3Edge(poly[j], poly[j + 1]);
                        found = true;
                    }
                }
            }

            return found;
        }

        private static bool T3_LineIntersect(PointF p1, PointF p2, PointF p3, PointF p4,
            out PointF result, out bool onFirst, out bool onSecond)
        {
            onFirst = false;
            onSecond = false;
            result = default(PointF);

            double dx1 = p2.X - p1.X, dy1 = p2.Y - p1.Y;
            double dx2 = p4.X - p3.X, dy2 = p4.Y - p3.Y;
            double denom = dx1 * dy2 - dy1 * dx2;

            if (Math.Abs(denom) < 1e-9)
                return false;

            double t = ((p3.X - p1.X) * dy2 - (p3.Y - p1.Y) * dx2) / denom;
            double u = ((p3.X - p1.X) * dy1 - (p3.Y - p1.Y) * dx1) / denom;

            result = new PointF((float)(p1.X + t * dx1), (float)(p1.Y + t * dy1));
            onFirst = t >= -1e-9 && t <= 1.0 + 1e-9;
            onSecond = u >= -1e-9 && u <= 1.0 + 1e-9;
            return true;
        }

        private static double Cross(PointF a, PointF b, PointF p) =>
            (b.X - a.X) * (p.Y - a.Y) - (b.Y - a.Y) * (p.X - a.X);

        private static int ClassifyPointEdge(PointF a, PointF b, PointF p)
        {
            double cross = Cross(a, b, p);
            if (Math.Abs(cross) < 1e-6)
                return 0;
            return cross > 0 ? 1 : -1;
        }

        private static bool PointInPolygon(List<PointF> poly, PointF p)
        {
            bool inside = false;
            int n = poly.Count;

            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                PointF pi = poly[i];
                PointF pj = poly[j];

                if (((pi.Y > p.Y) != (pj.Y > p.Y)) &&
                    (p.X < (pj.X - pi.X) * (p.Y - pi.Y) / (pj.Y - pi.Y) + pi.X))
                    inside = !inside;
            }

            return inside;
        }

        private static bool IsConvex(List<PointF> poly)
        {
            int n = poly.Count;
            if (n < 3)
                return false;

            int sign = 0;
            for (int i = 0; i < n; i++)
            {
                PointF a = poly[i];
                PointF b = poly[(i + 1) % n];
                PointF c = poly[(i + 2) % n];

                double cross = (b.X - a.X) * (c.Y - b.Y) - (b.Y - a.Y) * (c.X - b.X);
                if (Math.Abs(cross) < 1e-9)
                    continue;

                int s = cross > 0 ? 1 : -1;
                if (sign == 0)
                    sign = s;
                else if (sign != s)
                    return false;
            }

            return sign != 0;
        }

        private static float DistanceToSegment(PointF p, PointF a, PointF b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            double len2 = dx * dx + dy * dy;

            if (len2 < 1e-9)
                return Distance(p, a);

            double t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / len2;
            t = Math.Max(0.0, Math.Min(1.0, t));

            double px = a.X + t * dx;
            double py = a.Y + t * dy;
            return (float)Math.Sqrt((p.X - px) * (p.X - px) + (p.Y - py) * (p.Y - py));
        }

        private static float Distance(PointF a, PointF b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
