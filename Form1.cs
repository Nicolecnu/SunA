using System;
using System.Windows.Forms;
using Model3D;
using CsGL_Base;
using CsGL.OpenGL;

namespace SunAnalysis
{
    public partial class 日照分析工具 : Form
    {
        Form2 form2 = null;
        public OpenGLBase gl = new OpenGLBase();
        public 日照分析工具()
        {
            InitializeComponent();
            gl.Parent = this;
            gl.Dock = DockStyle.Fill;
            //gl.KeyPress += new KeyPressEventHandler(gl.OpenGL_KeyDown);
            gl.KeyDown+= new KeyEventHandler(gl.OpenGL_KeyDown);
            //gl.MouseDown += new MouseEventHandler(gl.glOnMouseDown);
            //gl.MouseMove += new MouseEventHandler(gl.glOnMouseMove);
            //gl.MouseWheel += new MouseEventHandler(gl.glOnMouseWheel);
            //gl.MouseClick += new MouseEventHandler(gl.glScreen2World);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        private void ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sender == 文件ToolStripMenuItem) OpenFile();
            else if (sender == 日照分析ToolStripMenuItem) SunTime();
            else if (sender == 帮助ToolStripMenuItem) Help();
        }

        private void OpenFile()
        {
            //打开文件的函数！！！！
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "3ds 文件|*.3ds";
            //？？
            openFileDialog.RestoreDirectory = false;
            //?
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;
            //??
            if (openFileDialog.ShowDialog() != DialogResult.OK) return;
            //H3DModel model = new H3DModel(); //3维模型
            gl.AddModel(openFileDialog.FileName);

        }

        private void SunTime()
        {
            form2 = new Form2(gl);
            form2.Show();
        }
       
        private  void Help()
        {
            Form3 form3 = new Form3();
            form3.Show();
        }
    }
}
