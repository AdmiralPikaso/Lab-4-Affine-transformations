using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Lab4
{
    
    public sealed partial class MainForm
    {
       
       
        private readonly List<List<PointF>> polygons = new List<List<PointF>>();

      
        private readonly List<PointF> currentPolygon = new List<PointF>();

        
        private void InitializeTask1()
        {
            
            pbCanvas.ContextMenuStrip = null;

            
            btnClear.Click += BtnClear_Click;
            pbCanvas.MouseDown += PbCanvas_MouseDown;
            pbCanvas.Paint += PbCanvas_Paint;
        }

       
        private void DrawPolygon(Graphics g, List<PointF> poly, Pen pen, SolidBrush brush)
        {
            int count = poly.Count;
            if (count <= 0)
                return;

            if (count == 1)
            {
                
                float radius = 3f;
                PointF p = poly[0];
                g.FillEllipse(brush, p.X - radius, p.Y - radius, radius * 2, radius * 2);
                return;
            }

           
            for (int i = 0; i < count - 1; i++)
            {
                g.DrawLine(pen, poly[i], poly[i + 1]);
            }
        }

        
        private void PbCanvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            
            g.SmoothingMode = SmoothingMode.AntiAlias;

            
            using (Pen penFinished = new Pen(Color.Black, 2f))
            using (SolidBrush brushFinished = new SolidBrush(Color.Black))
            {
                for (int i = 0; i < polygons.Count; i++)
                {
                    DrawPolygon(g, polygons[i], penFinished, brushFinished);
                }
            }

           
            if (currentPolygon.Count > 0)
            {
                using (Pen penCurrent = new Pen(Color.Blue, 1.5f))
                using (SolidBrush brushCurrent = new SolidBrush(Color.Blue))
                {
                    DrawPolygon(g, currentPolygon, penCurrent, brushCurrent);
                }
            }
        }

        
        private void PbCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (task3Mode != Task3Mode.None)
                return;

            if (e.Button == MouseButtons.Left)
            {
                
                currentPolygon.Add(new PointF(e.X, e.Y));
                pbCanvas.Invalidate();
            }
            else if (e.Button == MouseButtons.Right)
            {
                
                if (currentPolygon.Count > 0)
                {
                  
                    polygons.Add(new List<PointF>(currentPolygon));
                    currentPolygon.Clear();
                    pbCanvas.Invalidate();
                }
              
            }
        }

        
        private void BtnClear_Click(object sender, EventArgs e)
        {
            
            polygons.Clear();

            
            currentPolygon.Clear();

            
            pbCanvas.Invalidate();
        }
    }
}
