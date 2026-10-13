namespace Lab4
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.btnClear = new System.Windows.Forms.Button();
            this.pbCanvas = new Lab4.DoubleBufferedPictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbCanvas)).BeginInit();
            this.SuspendLayout();
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(12, 12);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(140, 30);
            this.btnClear.TabIndex = 0;
            this.btnClear.Text = "Очистка сцены";
            this.btnClear.UseVisualStyleBackColor = true;
            // 
            // pbCanvas
            // 
            this.pbCanvas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbCanvas.BackColor = System.Drawing.Color.White;
            this.pbCanvas.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbCanvas.Location = new System.Drawing.Point(12, 48);
            this.pbCanvas.Name = "pbCanvas";
            this.pbCanvas.Size = new System.Drawing.Size(900, 650);
            this.pbCanvas.TabIndex = 1;
            this.pbCanvas.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(924, 710);
            this.Controls.Add(this.pbCanvas);
            this.Controls.Add(this.btnClear);
            this.Name = "MainForm";
            this.Text = "ЛР4 - Аффинные преобразования на плоскости";
            ((System.ComponentModel.ISupportInitialize)(this.pbCanvas)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button btnClear;
        private DoubleBufferedPictureBox pbCanvas;
    }
}
