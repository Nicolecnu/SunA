using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CsGL.OpenGL;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using Model3D;
using SunAnalysis;

namespace CsGL_Base
{
    public class OpenGLBase : OpenGLControl
    {
        #region 变量
        Timer Timer_GLupdata = new Timer();  //窗口重绘计时器
        //private Cube cube = new Cube();
        public H3DModel model = new H3DModel(); //3维模型
        //旋转，平移，缩放
        Point MousePoint = new Point(0, 0);
        public Vertex3 Translate3D = new Vertex3(0, 0, 0);
        public double step_x = 0;
        public double step_y = 0;
        public float m_xRotation = 0;
        public float m_yRotation = 0;
        public float Scaling = 1;
        //是否漫游
        public bool mamyou = false;
        private Vertex3 preEye = new Vertex3(0, 0.8, 0);  //前一帧的眼睛
        private Vertex3 preCenter = new Vertex3(0, 0, 0);  //前一帧的中心
        #endregion

        public OpenGLBase()
        {
            this.Timer_GLupdata.Tick += new EventHandler(Timer_GLupdata_Tick);
            this.Timer_GLupdata.Interval = 90;
            this.Timer_GLupdata.Start();
            this.KeyDown += new KeyEventHandler(OpenGL_KeyDown);
        }

        #region 事件
        private void Timer_GLupdata_Tick(object sender, EventArgs e)
        {
            this.Invalidate();
        }
   
        //键盘事件
        public void OpenGL_KeyDown(object sender, KeyEventArgs e)
        {
            Keys key = e.KeyData;
            switch (key)
            {
                case (Keys.B):
                    m_xRotation += 5f;
                    //m_yRotation = 0;
                    break;//后旋转
                case (Keys.F):
                    m_xRotation -= 5f;
                    //m_yRotation = 0;
                    break;//前旋转
                case (Keys.P):
                    m_yRotation += 5f;//右旋转
                    //m_xRotation = 0;
                    break;
                case (Keys.Q):
                    m_yRotation -= 5f;//左旋转
                    //m_xRotation = 0;
                    break;
                case (Keys.U): step_y += 0.01f; break;//上移
                case (Keys.D): step_y -= 0.01f; break;//下移
                case (Keys.R): step_x += 0.01f; break;//右移
                case (Keys.L): step_x -= 0.01f; break;//左移
                case (Keys.I): Scaling -= 0.1f; break;//缩小
                case (Keys.O): Scaling += 0.1f; break;//放大
                case (Keys.S):InitGeometry();break;
            }
        }
        //场景重置
        protected override void OnSizeChanged(EventArgs e)
        {
            GL.glViewport(0, 0, this.Bounds.Width, this.Bounds.Height);
            GL.glMatrixMode(GL.GL_PROJECTION);//选择投影矩阵
            GL.glLoadIdentity();//重设投影矩阵
            GL.gluPerspective(40.0, ((double)(this.Width) / (double)(this.Height)), 1.0, 1000.0);//调整视口大小
            GL.gluLookAt(0, 0, 2.5, 0, 0, 0, 0, 1, 0);
            GL.glMatrixMode(GL.GL_MODELVIEW);//选择模型观察矩阵
        }
        //初始化
        protected override void InitGLContext()
        {
            base.InitGLContext();
            //GL.glEnable(GL.GL_TEXTURE_2D);
            GL.glMatrixMode(GL.GL_PROJECTION);//选择投影矩阵
            GL.glLoadIdentity();//重设投影矩阵
            GL.gluPerspective(40.0, ((double)(this.Width) / (double)(this.Height)), 0.5, 1000.0);//调整视口大小
            GL.gluLookAt(0, 0, 2.3, 0, 0, 0, 0, 1, 0);
            GL.glMatrixMode(GL.GL_MODELVIEW);//选择模型观察矩阵
            GL.glLoadIdentity();//重置模型观察矩阵
            GL.glShadeModel(GL.GL_SMOOTH);						// 启用阴影平滑
            GL.glClearColor(1.0f, 1.0f, 1.0f, 0.5f);				// Black Background
            GL.glClearDepth(1.0f);									// Depth Buffer Setup
            GL.glEnable(GL.GL_DEPTH_TEST);							// Enables Depth Testing
            GL.glDepthFunc(GL.GL_LEQUAL);								// The Type Of Depth Testing To Do
            GL.glHint(GL.GL_PERSPECTIVE_CORRECTION_HINT, GL.GL_NICEST);	// Really Nice Perspective Calculations  
            GL.glEnable(GL.GL_TEXTURE_2D);
                     
        }
        //画
        public override void glDraw()
        {
            LTRS();
            model.DrawModel();
            GL.glFlush();
        }
        //？？
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
        #endregion

        #region 方法
        public void AddModel(string modelname)
        {
            model = H3DModel.FromFile(modelname);
            if (!model.LoadTextrue())								// Jump To Texture Loading Routine ( NEW )
            {
                MessageBox.Show("纹理读取失败！");								// If Texture Didn't Load Return FALSE
            }
        }
        private void InitGeometry()
        {
            m_xRotation = 0.0f;
            m_yRotation = 0.0f;
            Translate3D.x = 0;
            Translate3D.y = 0;
            Translate3D.z = 0;
            Scaling = 1;
           
            GL.glMatrixMode(GL.GL_PROJECTION);//选择投影矩阵
            GL.glLoadIdentity();//重设投影矩阵
            GL.gluPerspective(40.0, ((double)(this.Width) / (double)(this.Height)), 0.01, 1000.0);//调整视口大小
            GL.gluLookAt(0, 0, 2.5, 0, 0, 0, 0, 1, 0);
            GL.glMatrixMode(GL.GL_MODELVIEW);//选择模型观察矩阵
            GL.glLoadIdentity();//重置模型观察矩阵
        }
        private void LTRS()
        {
            GL.glClear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);    //清除屏幕及深度缓存
            GL.glLoadIdentity();                                            //重置当前模型视图矩阵
            Translate3D.x = step_x;
            Translate3D.y = step_y;
            GL.glTranslatef((float)Translate3D.x, (float)Translate3D.y, (float)Translate3D.z);
            GL.glRotatef(m_xRotation, 1.0f, 0.0f, 0.0f);
            GL.glRotatef(m_yRotation, 0.0f, 1.0f, 0.0f);
            GL.glScalef(Scaling, Scaling, Scaling);
        }  
        #endregion
    }

    
}