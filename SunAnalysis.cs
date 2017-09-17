using System;
using System.Collections.Generic;
using Model3D;
namespace SunAnalysis

{
    #region 构造函数形式的计算太阳因子
    //计算太阳因子（太阳高度角、太阳方位角）
    /*public class CountSun
    {
        public double Lon, Lat;//计算太阳因子时需要的经度、纬度
        public int Y, M, D, H, Min;//年、月、日、时、分
        public double Ea, Aa;//太阳高度角、方位角
        public int[] Month = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        public CountSun(double lon, double lat, int y, int m, int d, int h, int min)
        {
            Lon = lon;
            Lat = lat;
            Y = y;
            M = m;
            D = d;
            H = h;
            Min = min;
        }
        //计算真太阳时，参数为全年第几天
        public double  TrueSunTime(int n)
        {
            float tb = H + Min / 60;//当前时间
            //求修正误差E
            double Q = 280 + 0.9856 * n;
            double E = 9.5 * Math.Sin(2 * Q*Math.PI/180) - 7.7 * Math.Sin((Q + 78)*Math.PI/180);
            //double E = 0.0172 + 0.4281 * Math.Cos(Q) - 7.3515 * Math.Sin(Q) - 3.3495 * Math.Cos(2 * Q) - 9.3619 * Math.Sin(2 * Q);
            //计算真太阳时
            double TrueST = tb - 4 * (120 - Lon) / 60 + E/60;
            return TrueST;
        }
        //计算当天为全年的第几天
        public int CountDay()
        {
                int n = 0;
                for (int i = 0; i < M-1; i++)
                {
                    n += Month[i];
                }
                n = n + D-1;
                if ((Y % 100 != 0 && Y % 4 == 0 || Y % 400 == 0) && M > 2)
                {
                    n++;
                }
            return n;
        }
        //计算太阳高度角和方位角，参数为全年第几天和真太阳时
        public void Count(int n,double truest)
        {
            double Ha = (truest - 12) * 15*Math.PI/180;//时角(弧度)
            double Dec = 23.45 * Math.Sin(Math.PI*(360 * (n-80) / 370)/180)*Math.PI/180;  //赤纬(弧度）
            double LatRad = Lat * Math.PI / 180;
            Ea = Math.Asin(Math.Sin(LatRad) * Math.Sin(Dec) + Math.Cos(LatRad) * Math.Cos(Dec) * Math.Cos(Ha));
            Aa = Math.Acos((Math.Sin(Dec) -Math.Sin(Ha)*Math.Sin(LatRad))/(Math.Cos(Ha)*Math.Cos(LatRad)));
            //Aa = Math.Asin(Math.Cos(Dec) * Math.Sin(Ha) / Math.Cos(Ea));
            Ea = Ea * 180 / Math.PI;
            Aa=Aa * 180 / Math.PI;
            if (Ha < 0)
            {
                Aa =Aa;
            }
            else
            {
                Aa = 360 - Aa;
            }
        }
        //计算日出、日落时间
    }*/
    #endregion 
    //计算太阳因子（太阳高度角、太阳方位角）
    public class CountSun
    {
        //计算当天为全年的第几天
        public static int CountDay(int Y, int M, int D)
        {
            int[] Month = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            int n = 0;
            for (int i = 0; i < M - 1; i++)
            {
                n += Month[i];
            }
            n = n + D - 1;
            if ((Y % 100 != 0 && Y % 4 == 0 || Y % 400 == 0) && M > 2)
            {
                n++;
            }
            return n;
        }
        //计算真太阳时，参数为年、月、日、时、分经度
        public static double TrueSunTime(int Y, int M, int D, int H, int Min, double Lon)
        {
            int n = CountDay(Y, M, D);
            float tb = H + Min / 60;//当前时间
            double n0 = 79.6764 + 0.2422 * (Y - 1985) - (int)((Y - 1985) / 4);
            double Q = 2 * Math.PI * (n - n0) / 365.2422;
            //求修正误差E
            //double Q = 280 + 0.9856 * n;
            //double E = 9.5 * Math.Sin(2 * Q * Math.PI / 180) - 7.7 * Math.Sin((Q + 78) * Math.PI / 180);
            double E = 0.0028 - 7.0924 * Math.Cos(Q) - 1.9875 * Math.Sin(Q) - 0.6882 * Math.Cos(2 * Q) + 9.9059 * Math.Sin(2 * Q);
            //计算真太阳时
            double TrueST = tb - 4 * (120 - Lon) / 60 + E / 60;
            return TrueST;
        }
        //计算太阳高度角和方位角，参数为年、月、日、时、分、经度、纬度
        public static double[] CalculateSun(int Y, int M, int D, int H, int Min, double Lon, double Lat)
        {
            int n = CountDay(Y, M, D);
            double truest = TrueSunTime(Y, M, D, H, Min, Lon);
            double n0 = 79.6764 + 0.2422 * (Y - 1985) - (int)((Y - 1985) / 4);
            double Q = 2 * Math.PI * (n - n0) / 365.2422;
            double Ha = (truest - 12) * 15 * Math.PI / 180;//时角(弧度)
            double Dec = 0.3723 + 23.2567 * Math.Sin(Q) + 0.1149 * Math.Sin(2 * Q) - 0.1712 * Math.Sin(3 * Q)
                         - 0.758 * Math.Cos(Q) + 0.3656 * Math.Cos(2 * Q) + 0.0201 * Math.Cos(3 * Q);
            Dec = Dec * Math.PI / 180;
            //double Dec = 23.45 * Math.Sin(Math.PI * (360.0 * (n - 80) / 370.0) / 180) * Math.PI / 180;  //赤纬(弧度）
            double LatRad = Lat * Math.PI / 180;
            double Ea = Math.Asin(Math.Sin(LatRad) * Math.Sin(Dec) + Math.Cos(LatRad) * Math.Cos(Dec) * Math.Cos(Ha));
            double t1 = (Math.Sin(Ea) * Math.Sin(LatRad) - Math.Sin(Dec)) / (Math.Cos(Ea) * Math.Cos(LatRad));
            double Aa = Math.Acos((Math.Sin(Ea) * Math.Sin(LatRad) - Math.Sin(Dec)) / (Math.Cos(Ea) * Math.Cos(LatRad)));
            //double Aa = Math.Asin(Math.Cos(Dec) * Math.Sin(Ha) / Math.Cos(Ea));
            Ea = Ea * 180 / Math.PI;
            Aa = Aa * 180 / Math.PI;
            if (Ha >= 0)
            {
                Aa = 360 - Aa;
            }
            double[] Sun = { Ea, Aa, Dec, Ha };
            return Sun;
        }
        //计算日出、日落时间
        public static double[] SunRaiseSetTime(int Y, int M, int D, double Lon, double Lat)
        {
            int n = CountDay(Y, M, D);
            double Dec = 23.45 * Math.Sin(Math.PI * (360 * (n - 80) / 370) / 180) * Math.PI / 180;  //赤纬(弧度）
            double Ha = Math.Acos(-Math.Tan(Lat) * Math.Tan(Dec));//计算时角
            Ha = Ha * 180 / Math.PI;//弧度转角度
            double Tr, Ts;//日出日落时间
            //求修正误差E
            double Q = 280 + 0.9856 * n;
            double E = 9.5 * Math.Sin(2 * Q * Math.PI / 180) - 7.7 * Math.Sin((Q + 78) * Math.PI / 180);
            Ts = Ha / 15 + 12 + 4 * (120 - Lon) / 60 - E / 60;
            Tr = 12 - Ha / 15 + 4 * (120 - Lon) / 60 - E / 60;
            double[] Time = { Tr, Ts };
            return Time;
        }
    }

    //定义点类
    public class Vertex3
    {
        public double x, y, z;
        public Vertex3(double x1, double y1, double z1)
        {
            x = x1;
            y = y1;
            z = z1;
        }
    }

    //定义向量类
    public class Vector3
    {
        public double x, y, z;
        public Vector3(Vertex3 V1, Vertex3 V2)
        {
            x = V2.x - V1.x;
            y = V2.y - V1.y;
            z = V2.z - V1.z;
        }
        public Vector3(Double X, Double Y, Double Z)
        {
            x = X;
            y = Y;
            z = Z;
        }
    }

    //定义三角面类
    public class Triangle
    {
        public Vertex3 p1, p2, p3;
        public Vertex3 center = new Vertex3(0, 0, 0);
        public Vector3 Vn, Dn;
        public bool IsSunnySide = true;
        public Triangle(Vertex3 _p1, Vertex3 _p2, Vertex3 _p3)
        {
            p1 = _p1;
            p2 = _p2;
            p3 = _p3;
            center.x = (p1.x + p2.x + p3.x) / 3;
            center.y = (p1.y + p2.y + p3.y) / 3;
            center.z = (p1.z + p2.z + p3.z) / 3;
        }
        //计算面的法向量
        public void CalculateVectorNorm()
        {
            Vn = CalculateTools.Cross(p1, p2, p3);
        }

        //判断面是阳面还是阴面,参数Dn为太阳的方向向量
        public void SunnySide(Vector3 Dn)
        {
            //Dn =CalculateTools.CalculateDirectionVector(Dec, Ha);
            double Angle = Math.Acos(CalculateTools.DotProduct(Vn, Dn) / (CalculateTools.CalculateNorm(Vn) * CalculateTools.CalculateNorm(Dn)));
            Angle = Angle / Math.PI * 180;
            if (Angle >= 90)
            {
                IsSunnySide = false;
            }
            else
            {
                IsSunnySide = true;
            }
        }
    }

    //定义计算工具
    public class CalculateTools
    {
        //空间向量点积
        public static double DotProduct(Vector3 v1, Vector3 v2)
        {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }
        public static double DotProduct(Vector3 v1, Vertex3 v2)
        {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }

        //计算三点的中心
        public static Vertex3 CenterP(Vertex3 v1, Vertex3 v2, Vertex3 v3)
        {
            Vertex3 Cp = new Vertex3(0, 0, 0);
            Cp.x = (v1.x + v2.x + v3.x) / 3;
            Cp.y = (v1.y + v2.y + v3.y) / 3;
            Cp.z = (v1.z + v2.z + v3.z) / 3;
            return Cp;
        }

        //计算向量叉积--参数为两个向量
        public static Vector3 Cross(Vector3 v1, Vector3 v2)
        {
            double dx = v1.y * v2.z - v1.z * v2.y;
            double dy = v1.z * v2.x - v1.x * v2.z;
            double dz = v1.x * v2.y - v1.y * v2.x;
            Vector3 c = new Vector3(dx, dy, dz);
            return c;
        }

        //叉积--参数为三个点
        public static Vector3 Cross(Vertex3 p1, Vertex3 p2, Vertex3 p3)
        {
            Vector3 v1 = new Vector3(p1, p2);
            Vector3 v2 = new Vector3(p1, p3);
            double dx = v1.y * v2.z - v1.z * v2.y;
            double dy = v1.z * v2.x - v1.x * v2.z;
            double dz = v1.x * v2.y - v1.y * v2.x;
            Vector3 c = new Vector3(dx, dy, dz);
            return c;
        }

        //计算向量的模
        public static double CalculateNorm(Vector3 v)
        {
            double norm = Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
            return norm;
        }

        //根据太阳高度角、方位角计算射线的方向向量
        //Height-待观测点的高度，Ha-高度角，Aa-方位角，返回值为方向向量
        //public static Vector3 CalculateDirectionVector(double Height, double Ea, double Aa)
        //{
        //    double dx, dy, dz;
        //    dx = -Height * (1 / Math.Tan(Ea * Math.PI / 180)) * Math.Sin(Aa * Math.PI / 180);
        //    dy = -Height * (1 / Math.Tan(Ea * Math.PI / 180)) * Math.Cos(Aa * Math.PI / 180);
        //    dz = Height;
        //    Vector3 Dn = new Vector3(dx, dy, dz);
        //    return Dn;
        //}
        //太阳赤纬和时角可以计算是太阳的单位方向向量（太阳为平行光），赤纬和时角是以地心为原点的
        //但是模型系统有自己的原点，将不同原点的量是不可以计算的
        public static Vector3 CalculateDirectionVector(double Dec, double Ha)
        {
            double dx, dy, dz;
            dx = Math.Cos(Ha * Math.PI / 180) * Math.Cos(Dec * Math.PI / 180);
            dy = Math.Sin(Ha * Math.PI / 180) * Math.Sin(Dec * Math.PI / 180);
            dz = Math.Sin(Dec * Math.PI / 180);
            Vector3 Dn = new Vector3(dx, dy, dz);
            return Dn;
        }
        #region 点在面内判断
        //判断两个点是否相等
        public static bool IsSame(Vertex3 v1, Vertex3 v2)
        {
            return v1.x == v2.x && v1.y == v2.y && v1.z == v2.z;
        }

        //判断P、C点是否在线段AB的同侧
        public static bool SameSide(Vertex3 A, Vertex3 B, Vertex3 C, Vertex3 P)
        {
            Vector3 v1 = Cross(A, B, C);
            Vector3 v2 = Cross(A, B, P);
            // v1 and v2 should point to the same direction
            return DotProduct(v1, v2) > 0;
        }

        //判断点是否在三角面上
        public static bool PointinTriangle(Vertex3 A, Vertex3 B, Vertex3 C, Vertex3 P)
        {
            return SameSide(A, B, C, P) && SameSide(B, C, A, P) && SameSide(C, A, B, P);
        }

        //屏幕拾取点与模型中面的映射，即拾取点属于哪个面的判断，并返回该面中心点
        public static Vertex3 PointinWhichTri(Vertex3 ObP, t3DModel model)
        {
            Vertex3 Center = new Vertex3(0, 0, 0);
            int t = 0;
            for (int i = 0; i < model.numOfObjects; i++)
            {
                t3DObject pObject = model.pObject[i];//模型中的体
                for (int j = 0; j < pObject.numOfFaces; j++)//体的面
                {
                    Vertex3[] p = new Vertex3[3];
                    for (int k = 0; k < 3; k++)
                    {
                        int index = pObject.pFaces[j].vertIndex[k];
                        Vertex3 pi = new Vertex3(pObject.pVerts[index].x, pObject.pVerts[index].y, pObject.pVerts[index].z);//每一个三角面顶点坐标
                        p[k] = pi;
                    }

                    Triangle Tri = new Triangle(p[0], p[1], p[2]);
                    if (PointinTriangle(Tri.p1, Tri.p2, Tri.p3, ObP))
                    {
                        Center = CenterP(Tri.p1, Tri.p2, Tri.p3);
                        break;
                        //goto finish;     
                    }

                }
                t++;
            }
            // finish:
            return Center;
        }
        #endregion

        //点是否被三角面遮挡
        //Tn-三角面的法向量，TP-三角面上的一个点，RP-射线的起点就待测点，Rd-射线的方向向量
        public static bool IntersectJudge(Vector3 Tn, Vertex3 TP, Vertex3 RP, Vector3 Rd)
        {
            bool IsIntersectTri;
            //
            double t = (CalculateTools.DotProduct(Tn, TP) - CalculateTools.DotProduct(Tn, RP)) / CalculateTools.DotProduct(Tn, Rd);
            if (t > 0)
            {
                IsIntersectTri = true;
            }
            else
            {
                IsIntersectTri = false;
            }
            return IsIntersectTri;
        }

        //计算单点特定时刻是否被遮挡
        public static bool SinglePointShelter(Vertex3 ObP, t3DModel model, Vector3 Dn)
        {
            List<string > sunside = new List<string>();
            bool Shelter = false;
            for (int i = 1; i < model.numOfObjects; i++)
            {
                t3DObject pObject = model.pObject[i];//模型中的体
                for (int j = 0; j < pObject.numOfFaces; j++)//体的面
                {
                    Vertex3[] p = new Vertex3[3];
                    //获取面的顶点坐标
                    for (int k = 0; k < 3; k++)
                    {
                        int index = pObject.pFaces[j].vertIndex[k];
                        Vertex3 pi = new Vertex3(pObject.pVerts[index].x, pObject.pVerts[index].y, pObject.pVerts[index].z);//每一个三角面顶点坐标
                        p[k] = pi;
                    }
                    Triangle Tri = new Triangle(p[0], p[1], p[2]);
                    
                    //计算三角面的法向量
                    Tri.CalculateVectorNorm();
                    //判断三角面是否是阳面，如果是阳面则做点是否被其遮挡判断
                    Tri.SunnySide(Dn);
                    if (Tri.IsSunnySide)
                    {
                        sunside.Add(i+";"+j);
                        //if (IntersectJudge(Tri.Vn, Tri.p1, ObP, Dn))
                        //{
                        //    Shelter = true;
                        //    break;
                        //}
                    }

                }
                //if (Shelter) break;
            }
            return Shelter;
        }

        //计算单点日照时间
        public static List<string> SinglePointTime(Vertex3 ObP, t3DModel model, int Y, int M, int D, double Lon, double Lat)
        {
            double Tr = CountSun.SunRaiseSetTime(Y, M, D, Lon, Lat)[0];//日出时间
            double Ts = CountSun.SunRaiseSetTime(Y, M, D, Lon, Lat)[1];//日落时间
            double t = 0;//遮挡时间
            List<string> ShadowTime = new List<string>();
            List<Vector3> dn = new List<Vector3>();
            Vector3 Dn = null;
            //每隔10分钟判读一次观测被遮挡情况
            for (double T = Tr; T <= Ts; T += 10.0 / 60.0)
            {
                int H = (int)T;
                int Min = (int)((T - H) * 60);
                H = H + Min / 60;
                Min = Min % 60;
                double[] sun = CountSun.CalculateSun(Y, M, D, H, Min, Lon, Lat);//计算太阳因子
                Dn = CalculateDirectionVector(sun[2], sun[3]);
                dn.Add(Dn);
                //Vector3 Dn = CalculateDirectionVector(ObP.z,sun[0], sun[1]);//计算太阳单位方向向量
                //Dn.x += ObP.x;
                //Dn.y -= ObP.y;
                //Dn.z += ObP.z;
                bool shelter = SinglePointShelter(ObP, model, Dn);//判断是否被遮挡
                if (shelter)
                {
                    t += 10;
                    if (Min < 10)
                    {
                        ShadowTime.Add(H + ":0" + Min+"--"+H+":"+(Min+10));
                    }
                    else
                    {
                        ShadowTime.Add(H + ":" + Min+ "--" + H +":"+ (Min + 10));
                    }
                }

            }
            t = t / 60;//被遮挡时间
            ShadowTime.Add(Convert.ToString(Math.Round((Ts - Tr - t),2)));//日照时间
            return ShadowTime;
        }
        #region 阴影
        ////场景静态遮挡情况
        //public static void SceneShelter(t3DModel model, int Y, int M, int D, int H, int Min, double Lon, double Lat)
        //{
        //    for(int i = 0; i < model.numOfObjects; i++)
        //    {
        //        t3DObject Object= model.pObject[i];
        //        //某一个体中的面的遮挡情况--以构成面的三个顶点为的中心为观测点
        //        for(int j = 0; j < Object.numOfFaces; j++)
        //        {
        //            Vertex3[] p = new Vertex3[3];
        //            //找到三角面的顶点坐标
        //            for (int k = 0; k < 3; k++)
        //            {
        //                int index = Object.pFaces[j].vertIndex[k];
        //                Vertex3 pi = new Vertex3(Object.pVerts[index].x, Object.pVerts[index].y, Object.pVerts[index].z);//每一个三角面顶点坐标
        //                p[k] = pi;
        //            }
        //            Vertex3 cp = CenterP(p[0], p[1], p[2]);
        //            Triangle Tri = new Triangle(p[0], p[1], p[2]);
        //            //Tri.CalculateVectorNorm();//计算三角面的法向量
        //            //Tri.SunnySide(Dn);//判断三角面是否是阳面，如果是阳面则做点是否被其遮挡判断
        //            bool Shelter=SinglePointShelter(cp, model, Y, M, D, H, Min, Lon, Lat);
        //            //如果面是被遮挡了改变显示方式--或者贴图改变或者换颜色？？等把模型读进来了再说
        //            if (Shelter)
        //            {
        //                break;
        //            }
        //        }
        //    }
        //}

        ////场景动态遮挡情况
        //public static void SceneShelterActive(t3DModel model, int Y, int M, int D, int Hs, int Mins,int Hend,int Minend, int interval,double Lon, double Lat)
        //{
        //    double TStart = Hs + Mins / 60;
        //    double TEnd = Hend + Minend / 60;
        //    for(double T = TStart; T < TEnd; T += (interval / 60))
        //    {
        //        int H = (int)T;
        //        int Min = (int)(H - T) * 60;
        //        SceneShelter(model, Y, M, D, H, Min, Lon, Lat);
        //    }
        // }
        //}
        #endregion 
    }
}