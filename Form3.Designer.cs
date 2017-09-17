namespace SunAnalysis
{
    partial class Form3
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox1.Location = new System.Drawing.Point(1, 1);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(450, 311);
            this.textBox1.TabIndex = 0;
            this.textBox1.Text = "文件：选择相应文件并打开\r\n场景漫游操作：\r\n（1）放大：O（2）缩小：I\r\n（3）左移：L（4）右移：R\r\n（5）上移：U（6）下移：D\r\n（7）向前旋转：F（" +
    "8）向后旋转：B\r\n（9）向右旋转：P（10）向左旋转：Q\r\n日照分析：进入参数输入界面。\r\n输入参数，点击拾取模型坐标，在模型中点击选择观测点。\r\n点击遮挡分" +
    "析，进行观测点遮挡判断。\r\n点击日照时间，可在参数页面显示日照时间。";
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(448, 311);
            this.Controls.Add(this.textBox1);
            this.Name = "Form3";
            this.Text = "帮助";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
    }
}