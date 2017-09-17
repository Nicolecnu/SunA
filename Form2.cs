using System;
using System.Windows.Forms;
using CsGL_Base;
using CsGL.OpenGL;
using System.Collections.Generic;

namespace SunAnalysis
{
    public partial class Form2 : Form
    {
        OpenGLBase gl = null;
        double posX, posY, posZ;
        public Form2(OpenGLBase _gl)
        {
            InitializeComponent();
            gl = _gl;
        }
        //获取观测点
        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("请在模型中选择待测点！");
            this.WindowState = FormWindowState.Minimized;
            gl.MouseClick += new MouseEventHandler(glScreen2World);
        }
        //遮挡分析
        private void button2_Click(object sender, EventArgs e)
        {
            double Lon = Convert.ToDouble(lon.Text);
            double Lat = Convert.ToDouble(lat.Text);
            int Y = Convert.ToInt32(TimeY.Text);
            int M = Convert.ToInt32(TimeM.Text);
            int D = Convert.ToInt32(TimeD.Text);
            int H = Convert.ToInt32(textBox1.Text);
            int Min = Convert.ToInt32(textBox2.Text);
            //判断所选点是哪个面并返回面中心
            Vertex3 Obp = new Vertex3(posX, posZ, posY);
            //Vertex3 CenterTri= CalculateTools.PointinWhichTri(Obp, gl.model.model);
            //MessageBox.Show(Convert.ToString(CenterTri.x) + "+" + Convert.ToString(CenterTri.y) + "+" + Convert.ToString(CenterTri.z));
            //单点遮挡判断
            double []SUN= CountSun.CalculateSun(Y, M, D, H, Min, Lon, Lat);//太阳因子，分别为高度、方位、赤角、时角
            Vector3 Dn =CalculateTools.CalculateDirectionVector(SUN[2], SUN[3]);
            //Vector3 Dn = CalculateTools.CalculateDirectionVector(Obp.z,SUN[0],SUN[1]);//太阳单位方向向量
            //Dn.x += Obp.x;
            //Dn.y -= Obp.y;
            //Dn.z += Obp.z;
            bool shadow = CalculateTools.SinglePointShelter(Obp, gl.model.model, Dn);

            this.WindowState = FormWindowState.Minimized;
            if (shadow)
            {
                MessageBox.Show("该点被遮挡！");
            }
            else
            {
                MessageBox.Show("该点被照射！");
            }
            this.WindowState = FormWindowState.Normal;
            if (listBox1.Items.Count > 0)
            {
                listBox1.Items.Clear();
            }
            listBox1.Items.Add("太阳高度角：" + SUN[0]);
            listBox1.Items.Add("太阳方位角：" + SUN[1]);
        }
        //全天日照时间
        private void button3_Click(object sender, EventArgs e)
        {
            double Lon = Convert.ToDouble(lon.Text);
            double Lat = Convert.ToDouble(lat.Text);
            int Y = Convert.ToInt32(TimeY.Text);
            int M = Convert.ToInt32(TimeM.Text);
            int D = Convert.ToInt32(TimeD.Text);
            Vertex3 Obp = new Vertex3(posX, posZ, posY);
            //获取日出、日落时间
            double[] SUN = CountSun.SunRaiseSetTime(Y, M, D, Lon, Lat);
            if (listBox1.Items.Count > 0)
            {
                listBox1.Items.Clear();
            }
            listBox1.Items.Add("日出时间：" + (int)SUN[0] + ":" +(int) ((SUN[0] - (int)SUN[0]) * 60));
            listBox1.Items.Add("日落时间：" + (int)SUN[1] + ":" +(int) ((SUN[1] - (int)SUN[1]) * 60));
           
            //计算日照时间
            List<string> ShadowTime = CalculateTools.SinglePointTime(Obp, gl.model.model, Y, M, D, Lon, Lat);
            for (int i = 0; i < ShadowTime.Count - 1; i++)
            {
                listBox1.Items.Add("被遮挡时间：" + ShadowTime[i]);
            }
            listBox1.Items.Add("全天总日照时间：" + Convert.ToString(Math.Round((SUN[1] - SUN[0] -ShadowTime.Count*10.0/60.0), 2)) + "小时");
            //listBox1.Items.Add("全天总日照时间：" + ShadowTime[ShadowTime.Count - 1] + "小时");

        }


        //屏幕坐标转换为gl坐标，gl坐标再到模型坐标
        unsafe public void glScreen2World(object sender, MouseEventArgs e)
        {
            int[] viewport = new int[4];
            float[] m_srtMatrix = new float[16];
            double[] modelview = new double[16];
            double[] projection = new double[16];
            float winX, winY, winZ;
            
            GL.glPushMatrix();//获取转换矩阵
            GL.glGetIntegerv(GL.GL_VIEWPORT, viewport); // 得到的是最后一个设置视口的参数  
            GL.glGetDoublev(GL.GL_MODELVIEW_MATRIX, modelview);
            GL.glGetDoublev(GL.GL_PROJECTION_MATRIX, projection);//得到投影矩阵
            GL.glPopMatrix();

            winX = e.X;
            winY = gl.ClientRectangle.Height - e.Y;
            //获取象元的颜色深度值作为z值
            GL.glReadPixels((int)winX, (int)winY, 1, 1, GL.GL_DEPTH_COMPONENT, GL.GL_FLOAT, &winZ);
            GL.gluUnProject(winX, winY, winZ, modelview, projection, viewport, out posX, out posY, out posZ);
            //将gl坐标转换为真实坐标
            posX = posX / gl.model.scale;
            posY = posY / gl.model.scale;
            posZ = posZ / gl.model.scale;
            //因为gl坐标系中的y、z轴与真实坐标相反，所以将gl的z坐标赋给真实的y，gl的y赋给真实的z
            Vertex3 Obp = new Vertex3(posX,posZ,posY);
            MessageBox.Show("坐标为：("+Convert.ToString(Obp.x) + "," + Convert.ToString(Obp.y) + ")"+"\n"+"高度为：" + Convert.ToString(Obp.z));
            MessageBox.Show("选点完成，可进行日照分析！");
            this.WindowState = FormWindowState.Normal;
            //判断所选点是哪个面并返回面中心
            //MessageBox.Show(Convert.ToString(winX) + "+" + Convert.ToString(winY) + "+" + Convert.ToString(winZ));
            //MessageBox.Show(Convert.ToString(posX) + "+" + Convert.ToString(posY) + "+" + Convert.ToString(posZ));
            //Vertex3 CenterTri = CalculateTools.PointinWhichTri(Obp, gl.model.model);
            //MessageBox.Show(Convert.ToString(CenterTri.x) + "+" + Convert.ToString(CenterTri.y) + "+" + Convert.ToString(CenterTri.z));
            //日照时间判断
            //double t = CalculateTools.SinglePointTime(Obp, gl.model.model, Y, M, D, Lon, Lat);
            
            
            //SunTime.Text = Convert.ToString(t);
        }


    }
}
