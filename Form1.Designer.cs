namespace Motor3D_Educativo_P2
{
    partial class Form1
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

        #region Código generado por el Diseñador de Windows Forms

        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            // 1. AQUÍ ESTÁ EL CAMBIO: Usamos PictureBox en vez de GLControl
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.lblRotX = new System.Windows.Forms.Label();
            this.trackRotX = new System.Windows.Forms.TrackBar();
            this.lblRotY = new System.Windows.Forms.Label();
            this.trackRotY = new System.Windows.Forms.TrackBar();
            this.btnCambiar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackRotX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackRotY)).BeginInit();
            this.SuspendLayout();

            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pictureBox1); // Agregamos el PictureBox
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.flowLayoutPanel1);
            this.splitContainer1.Size = new System.Drawing.Size(800, 450);
            this.splitContainer1.SplitterDistance = 560;
            this.splitContainer1.TabIndex = 0;

            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Black; // Fondo negro
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(560, 450);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // IMPORTANTE: Conectar el evento Paint
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            this.pictureBox1.Resize += new System.EventHandler(this.pictureBox1_Resize);

            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.lblRotX);
            this.flowLayoutPanel1.Controls.Add(this.trackRotX);
            this.flowLayoutPanel1.Controls.Add(this.lblRotY);
            this.flowLayoutPanel1.Controls.Add(this.trackRotY);
            this.flowLayoutPanel1.Controls.Add(this.btnCambiar);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(10);
            this.flowLayoutPanel1.Size = new System.Drawing.Size(236, 450);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // lblRotX
            // 
            this.lblRotX.AutoSize = true;
            this.lblRotX.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRotX.Location = new System.Drawing.Point(13, 10);
            this.lblRotX.Name = "lblRotX";
            this.lblRotX.Size = new System.Drawing.Size(89, 17);
            this.lblRotX.TabIndex = 0;
            this.lblRotX.Text = "Rotación X";
            // 
            // trackRotX
            // 
            this.trackRotX.Location = new System.Drawing.Point(13, 30);
            this.trackRotX.Maximum = 360;
            this.trackRotX.Name = "trackRotX";
            this.trackRotX.Size = new System.Drawing.Size(200, 45);
            this.trackRotX.TabIndex = 1;
            // 
            // lblRotY
            // 
            this.lblRotY.AutoSize = true;
            this.lblRotY.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRotY.Location = new System.Drawing.Point(13, 78);
            this.lblRotY.Name = "lblRotY";
            this.lblRotY.Size = new System.Drawing.Size(89, 17);
            this.lblRotY.TabIndex = 2;
            this.lblRotY.Text = "Rotación Y";
            // 
            // trackRotY
            // 
            this.trackRotY.Location = new System.Drawing.Point(13, 98);
            this.trackRotY.Maximum = 360;
            this.trackRotY.Name = "trackRotY";
            this.trackRotY.Size = new System.Drawing.Size(200, 45);
            this.trackRotY.TabIndex = 3;
            // 
            // btnCambiar
            // 
            this.btnCambiar.Location = new System.Drawing.Point(13, 149);
            this.btnCambiar.Name = "btnCambiar";
            this.btnCambiar.Size = new System.Drawing.Size(200, 40);
            this.btnCambiar.TabIndex = 4;
            this.btnCambiar.Text = "Cambiar Figura";
            this.btnCambiar.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form1";
            this.Text = "Motor 3D - GDI+ (Dev B)";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackRotX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackRotY)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        // Declaramos el PictureBox
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label lblRotX;
        private System.Windows.Forms.TrackBar trackRotX;
        private System.Windows.Forms.Label lblRotY;
        private System.Windows.Forms.TrackBar trackRotY;
        private System.Windows.Forms.Button btnCambiar;
    }
}