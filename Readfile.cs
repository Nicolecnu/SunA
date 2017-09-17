using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using CsGL.OpenGL;
using System.Linq;
using System.Reflection;

namespace Model3D
{
    internal class FileHead
    {
        //基本块
        public static UInt32 PRIMARY { get { return 0x4D4D; } set { } }

        //主块
        public static UInt32 OBJECTINFO { get { return 0x3D3D; } set { } }				// 网格对象的版本号
        public static UInt32 VERSION { get { return 0x0002; } set { } }			// .3ds文件的版本
        public static UInt32 EDITKEYFRAME { get { return 0xB000; } set { } }			// 所有关键帧信息的头部

        //  对象的次级定义(包括对象的材质和对象）
        public static UInt32 MATERIAL { get { return 0xAFFF; } set { } }		// 保存纹理信息
        public static UInt32 OBJECT { get { return 0x4000; } set { } }		// 保存对象的面、顶点等信息

        //  材质的次级定义
        public static UInt32 MATNAME { get { return 0xA000; } set { } }			// 保存材质名称
        public static UInt32 MATDIFFUSE { get { return 0xA020; } set { } }			// 对象/材质的颜色
        public static UInt32 MATMAP { get { return 0xA200; } set { } }			// 新材质的头部
        public static UInt32 MATMAPFILE { get { return 0xA300; } set { } }			// 保存纹理的文件名

        public static UInt32 OBJECT_MESH { get { return 0x4100; } set { } }			// 新的网格对象

        //  OBJECT_MESH的次级定义
        public static UInt32 OBJECT_VERTICES { get { return 0x4110; } set { } }		// 对象顶点
        public static UInt32 OBJECT_FACES { get { return 0x4120; } set { } }	// 对象的面
        public static UInt32 OBJECT_MATERIAL { get { return 0x4130; } set { } }		// 对象的材质
        public static UInt32 OBJECT_UV { get { return 0x4140; } set { } }	// 对象的UV纹理坐标

        //转换字符
        public static int byte2int(byte[] buffer) { return BitConverter.ToInt32(buffer, 0); }
        public static float byte2float(byte[] buffer) { return BitConverter.ToSingle(buffer, 0); }
    }

    #region 数据结构
    // 定义3D点的类，用于保存模型中的顶点
    public class CVector3
    {
        public float x, y, z;
    }
    // 定义2D点类，用于保存模型的UV纹理坐标
    public class CVector2
    {
        public float x, y;
    }
    // 面的结构定义
    public class tFace
    {
        public int[] vertIndex = new int[3];     //顶点坐标
        //public int[] coordIndex = new int[3];    //纹理坐标索引

    }
    // 材质信息结构体
    public class tMaterialInfo
    {
        public String strName = "";            //纹理名称
        public String strFile = "";            //如果存在纹理映射，则表示纹理文件名称
        public int[] color = new int[3]; //对象的RGB颜色
        public int texureId;           //纹理ID
        public float uTile;              //u重复
        public float vTile;              //v重复
        public float uOffset;            //u纹理偏移
        public float vOffset;            //v纹理偏移
    }
    //对象信息结构体
    public class t3DObject
    {
        public int numOfVerts;     // 模型中顶点的数目
        public int numOfFaces;     // 模型中面的数目
        public int numTexVertex;   // 模型中纹理坐标的数目
        public int materialID;     // 纹理ID
        public bool bHasTexture;   // 是否具有纹理映射
        public String strName;     // 对象的名称
        public CVector3[] pVerts;    // 对象的顶点
        public CVector3[] pNormals;  // 对象的法向量
        public CVector2[] pTexVerts; // 纹理UV坐标
        public tFace[] pFaces;       // 对象的面信息
    }
    //模型信息结构体
    public class t3DModel
    {
        public int numOfObjects;       // 模型中对象的数目
        public int numOfMaterials;     // 模型中材质的数目
        public List<tMaterialInfo> pMaterials = new List<tMaterialInfo>();   // 材质链表信息
        public List<t3DObject> pObject = new List<t3DObject>();              // 模型中对象链表信息
    }
    //
    public class tIndices
    {
        public UInt16 a, b, c, bVisible;
    }
    // 保存块信息的结构
    public class tChunk
    {
        public UInt32 ID;          //块的ID
        public UInt32 length;      //块的长度
        public UInt32 bytesRead;   //需要读的块数据的字节数
    }
    #endregion

    //读取3ds文件
    public class CLoad3DS
    {
        string base_dir;
        private tChunk m_CurrentChunk = new tChunk();
        private tChunk m_TempChunk = new tChunk();
        private FileStream m_FilePointer;
        public bool Import3DS(t3DModel pModel, String strFileName)  // 装入3ds文件到模型结构中
        {
            if (pModel == null)
                return false;
            pModel.numOfMaterials = 0;
            pModel.numOfObjects = 0;
            base_dir = new FileInfo(strFileName).Directory + "/";  //DircectoryName是文件的路径 
            try
            {
                this.m_FilePointer = new FileStream(strFileName, FileMode.Open);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }
            // 当文件打开之后，首先应该将文件最开始的数据块读出以判断是否是一个3ds文件
            // 如果是3ds文件的话，第一个块ID应该是PRIMARY

            // 将文件的第一块读出并判断是否是3ds文件
            ReadChunk(this.m_CurrentChunk); //读出块的id和块的size
            // 确保是3ds文件
            if (m_CurrentChunk.ID != FileHead.PRIMARY)
            {
                Debug.WriteLine("Unable to load PRIMARY chuck from file: " + strFileName);
                return false;
            }
            // 现在开始读入数据，ProcessNextChunk()是一个递归函数

            // 通过调用下面的递归函数，将对象读出
            ProcessNextChunk(pModel, m_CurrentChunk);

            // 在读完整个3ds文件之后，计算顶点的法线
            ComputeNormals(pModel);

            m_FilePointer.Close();

            return true;
        }
        //读出3ds文件的主要部分
        void ProcessNextChunk(t3DModel pModel, tChunk pPreviousChunk)
        {
            t3DObject newObject = new t3DObject();
            int version = 0;

            m_CurrentChunk = new tChunk();

            //  下面每读一个新块，都要判断一下块的ID，如果该块是需要的读入的，则继续进行
            //  如果是不需要读入的块，则略过

            // 继续读入子块，直到达到预定的长度
            while (pPreviousChunk.bytesRead < pPreviousChunk.length)
            {
                //读入下一个块
                ReadChunk(m_CurrentChunk);

                //判断ID号
                if (m_CurrentChunk.ID == FileHead.VERSION)
                {
                    m_CurrentChunk.bytesRead += fread(ref version, m_CurrentChunk.length - m_CurrentChunk.bytesRead, m_FilePointer);

                    // 如果文件版本号大于3，给出一个警告信息
                    if (version > 3)
                        Debug.WriteLine("Warning:  This 3DS file is over version 3 so it may load incorrectly");
                }
                else if (m_CurrentChunk.ID == FileHead.OBJECTINFO)
                {
                    //读入下一个块
                    ReadChunk(m_TempChunk);

                    //获得网络的版本号
                    m_TempChunk.bytesRead += fread(ref version, m_TempChunk.length - m_TempChunk.bytesRead, m_FilePointer);

                    //增加读入的字节数
                    m_CurrentChunk.bytesRead += m_TempChunk.bytesRead;

                    //进入下一个块
                    ProcessNextChunk(pModel, m_CurrentChunk);
                }
                else if (m_CurrentChunk.ID == FileHead.MATERIAL)//材质信息
                {
                    //材质的数目递增
                    pModel.numOfMaterials++;
                    //在纹理链表中添加一个空白纹理结构
                    pModel.pMaterials.Add(new tMaterialInfo());
                    //进入材质装入函数
                    ProcessNextMaterialChunk(pModel, m_CurrentChunk);
                }
                else if (m_CurrentChunk.ID == FileHead.OBJECT)//对象的名称
                {
                    //对象数目递增
                    pModel.numOfObjects++;

                    //添加一个新的tObject节点到对象的链表中
                    pModel.pObject.Add(new t3DObject());

                    //获得并保存对象的名称，然后增加读入的字节数
                    m_CurrentChunk.bytesRead += getStr(ref pModel.pObject[pModel.numOfObjects - 1].strName);

                    //进入其余对象信息的读入
                    ProcessNextObjectChunk(pModel, pModel.pObject[pModel.numOfObjects - 1], m_CurrentChunk);
                }
                else
                {
                    // 跳过关键帧块的读入，增加需要读入的字节数 EDITKEYFRAME
                    // 跳过所有忽略的块的内容的读入，增加需要读入的字节数
                    while (m_CurrentChunk.bytesRead != m_CurrentChunk.length)
                    {
                        int[] b = new int[1];
                        m_CurrentChunk.bytesRead += fread(ref b, 1, m_FilePointer);
                    }

                }
                //添加从最后块中读入的字节数
                pPreviousChunk.bytesRead += m_CurrentChunk.bytesRead;

            }
            //当前快设置为前面的块
            m_CurrentChunk = pPreviousChunk;
        }
        //处理所有的文件中的对象信息
        void ProcessNextObjectChunk(t3DModel pModel, t3DObject pObject, tChunk pPreviousChunk)
        {
            m_CurrentChunk = new tChunk();

            //继续读入块的内容直至本子块结束
            while (pPreviousChunk.bytesRead < pPreviousChunk.length)
            {
                ReadChunk(m_CurrentChunk);

                if (m_CurrentChunk.ID == FileHead.OBJECT_MESH)//正读入的是一个新块
                {
                    //使用递归函数调用，处理该新块
                    ProcessNextObjectChunk(pModel, pObject, m_CurrentChunk);

                }
                else if (m_CurrentChunk.ID == FileHead.OBJECT_VERTICES)//读入的是对象顶点
                {
                    ReadVertices(pObject, m_CurrentChunk);
                }
                else if (m_CurrentChunk.ID == FileHead.OBJECT_FACES)//读入的是对象的面
                {
                    ReadVertexIndices(pObject, m_CurrentChunk);
                }
                else if (m_CurrentChunk.ID == FileHead.OBJECT_MATERIAL)//读入的是对象的材质名称
                {
                    //该块保存了对象材质的名称，可能是一个颜色，也可能是一个纹理映射。
                    //同时在该块中也保存了纹理对象所赋予的面

                    //下面读入对象的材质名称
                    ReadObjectMaterial(pModel, pObject, m_CurrentChunk);
                }
                else if (m_CurrentChunk.ID == FileHead.OBJECT_UV)//读入对象的UV纹理坐标
                {
                    ReadUVCoordinates(pObject, m_CurrentChunk);
                }
                else
                {
                    //掠过不需要读入的块
                    while (m_CurrentChunk.bytesRead != m_CurrentChunk.length)
                    {
                        int[] b = new int[1];
                        m_CurrentChunk.bytesRead += fread(ref b, 1, m_FilePointer);
                    }
                }

                //添加从最后块中读入的字节数
                pPreviousChunk.bytesRead += m_CurrentChunk.bytesRead;

            }
            //当前快设置为前面的块
            m_CurrentChunk = pPreviousChunk;
        }
        //处理所有的材质信息
        void ProcessNextMaterialChunk(t3DModel pModel, tChunk pPreviousChunk)
        {
            //给当前块分配存储空间
            m_CurrentChunk = new tChunk();

            //继续读入这些块，直到该子块结束
            while (pPreviousChunk.bytesRead < pPreviousChunk.length)
            {
                //读入下一块
                ReadChunk(m_CurrentChunk);

                //判断读入的是什么块
                if (m_CurrentChunk.ID == FileHead.MATNAME)//材质的名称
                {
                    //读入材质的名称
                    m_CurrentChunk.bytesRead += fread(ref pModel.pMaterials[pModel.numOfMaterials - 1].strName, m_CurrentChunk.length - m_CurrentChunk.bytesRead, m_FilePointer);
                }
                else if (m_CurrentChunk.ID == FileHead.MATDIFFUSE)//对象的RGB颜色
                {
                    ReadColorChunk(pModel.pMaterials[pModel.numOfMaterials - 1], m_CurrentChunk);
                }
                else if (m_CurrentChunk.ID == FileHead.MATMAP)//纹理信息头部
                {
                    //进入下一个材质块信息
                    ProcessNextMaterialChunk(pModel, m_CurrentChunk);
                }
                else if (m_CurrentChunk.ID == FileHead.MATMAPFILE)
                {
                    //读入材质文件名称
                    pModel.pMaterials[pModel.numOfMaterials - 1].strFile = base_dir;
                    m_CurrentChunk.bytesRead += fread(ref pModel.pMaterials[pModel.numOfMaterials - 1].strFile, m_CurrentChunk.length - m_CurrentChunk.bytesRead, m_FilePointer);
                }
                else
                {
                    //掠过不需要读入的块
                    while (m_CurrentChunk.bytesRead != m_CurrentChunk.length)
                    {
                        int[] b = new int[1];
                        m_CurrentChunk.bytesRead += fread(ref b, 1, m_FilePointer);
                    }
                }
                //添加从最后块中读入的字节数
                pPreviousChunk.bytesRead += m_CurrentChunk.bytesRead;
            }
            //当前快设置为前面的块
            m_CurrentChunk = pPreviousChunk;
        }
        //读下一个块
        private void ReadChunk(tChunk pChunk)
        {
            //pChunk.bytesRead = fread(ref pChunk.ID, 2, this.m_FilePointer);

            Byte[] id = new Byte[2];
            Byte[] length = new Byte[4];
            pChunk.bytesRead = (UInt32)this.m_FilePointer.Read(id, 0, 2);
            pChunk.bytesRead += (UInt32)this.m_FilePointer.Read(length, 0, 4);
            pChunk.ID = (UInt32)(id[1] * 256 + id[0]);
            pChunk.length = (UInt32)(((length[3] * 256 + length[2]) * 256 + length[1]) * 256 + length[0]);

        }
        //读入RGB颜色
        void ReadColorChunk(tMaterialInfo pMaterial, tChunk pChunk)
        {
            //读入颜色块信息
            ReadChunk(m_TempChunk);

            //读入RGB颜色
            m_TempChunk.bytesRead += fread(ref pMaterial.color, m_TempChunk.length - m_TempChunk.bytesRead, m_FilePointer);

            //增加读入的字节数
            pChunk.bytesRead += m_TempChunk.bytesRead;
        }
        //读入顶点索引
        void ReadVertexIndices(t3DObject pObject, tChunk pPreviousChunk)
        {
            int index = 0;
            //读入该对象中面的数目
            pPreviousChunk.bytesRead += fread(ref pObject.numOfFaces, 2, m_FilePointer);

            //分配所有的储存空间，并初始化结构
            pObject.pFaces = new tFace[pObject.numOfFaces];

            //遍历对象中所有的面
            for (int i = 0; i < pObject.numOfFaces; i++)
            {
                pObject.pFaces[i] = new tFace();
                for (int j = 0; j < 4; j++)
                {
                    //读入当前对象的第一个点
                    pPreviousChunk.bytesRead += fread(ref index, 2, m_FilePointer);

                    if (j < 3)
                    {
                        pObject.pFaces[i].vertIndex[j] = index;
                    }
                }
            }
        }
        //读入对象的UV坐标
        void ReadUVCoordinates(t3DObject pObject, tChunk pPreviousChunk)
        {
            //为了读入对象的UV坐标，首先需要读入数量，再读入具体的数据

            //读入UV坐标的数量
            pPreviousChunk.bytesRead += fread(ref pObject.numTexVertex, 2, m_FilePointer);

            //初始化保存UV坐标的数组
            pObject.pTexVerts = new CVector2[pObject.numTexVertex];

            //读入纹理坐标
            pPreviousChunk.bytesRead += fread(ref pObject.pTexVerts, pPreviousChunk.length - pPreviousChunk.bytesRead, m_FilePointer);
        }
        //读入对象的顶点
        void ReadVertices(t3DObject pObject, tChunk pPreviousChunk)
        {
            //在读入实际的顶点之前，首先必须确定需要读入多少个顶点。

            //读入顶点的数目
            pPreviousChunk.bytesRead += fread(ref pObject.numOfVerts, 2, m_FilePointer);

            //分配顶点的储存空间，然后初始化结构体
            pObject.pVerts = new CVector3[pObject.numOfVerts];

            //读入顶点序列
            pPreviousChunk.bytesRead += fread(ref pObject.pVerts, pPreviousChunk.length - pPreviousChunk.bytesRead, m_FilePointer);

            //因为3DMax的模型Z轴是指向上的，将y轴和z轴翻转——y轴和z轴交换，再把z轴反向

            //遍历所有的顶点
            //for (int i = 0; i < pObject.numOfVerts; i++)
            //{
            //    float fTempY = pObject.pVerts[i].y;
            //    pObject.pVerts[i].y = pObject.pVerts[i].z;
            //    pObject.pVerts[i].z = -1 * fTempY;
            //}
        }
        //读入对象的材质名称
        void ReadObjectMaterial(t3DModel pModel, t3DObject pObject, tChunk pPreviousChunk)
        {
            String strMaterial = "";            //用来保存对象的材质名称
            int[] buffer = new int[50000];    //用来读入不需要的数据

            //读入赋予当前对象的材质名称
            pPreviousChunk.bytesRead += getStr(ref strMaterial);

            //遍历所有的纹理
            for (int i = 0; i < pModel.numOfMaterials; i++)
            {
                //如果读入的纹理与当前纹理名称匹配

                if (strMaterial.Equals(pModel.pMaterials[i].strName))
                {
                    //设置材质ID
                    pObject.materialID = i;
                    //判断是否是纹理映射，如果strFile是一个长度大于1的字符串，则是纹理
                    if (pModel.pMaterials[i].strFile.Length > 0)
                    {
                        //设置对象的纹理映射标志
                        pObject.bHasTexture = true;
                    }
                    break;
                }
                else
                {
                    //如果该对象没有材质，则设置ID为-1
                    pObject.materialID = -1;
                }
            }
            pPreviousChunk.bytesRead += fread(ref buffer, pPreviousChunk.length - pPreviousChunk.bytesRead, m_FilePointer);
        }
        #region 顶点法向量的计算
        //下面的这些函数主要用来计算顶点的法向量，顶点的法向量主要用来计算光照
        //计算对象的法向量
        private void ComputeNormals(t3DModel pModel)
        {
            CVector3 vVector1, vVector2, vNormal;
            CVector3[] vPoly;

            vPoly = new CVector3[3];
            //如果模型中没有对象，则返回
            if (pModel.numOfObjects <= 0)
                return;

            //遍历模型中所有的对象
            for (int index = 0; index < pModel.numOfObjects; index++)
            {
                //获得当前对象
                t3DObject pObject = pModel.pObject[index];

                //分配需要的空间
                CVector3[] pNormals = new CVector3[pObject.numOfFaces];
                CVector3[] pTempNormals = new CVector3[pObject.numOfFaces];
                pObject.pNormals = new CVector3[pObject.numOfVerts];

                //遍历对象所有面
                for (int i = 0; i < pObject.numOfFaces; i++)
                {
                    vPoly[0] = pObject.pVerts[pObject.pFaces[i].vertIndex[0]];
                    vPoly[1] = pObject.pVerts[pObject.pFaces[i].vertIndex[1]];
                    vPoly[2] = pObject.pVerts[pObject.pFaces[i].vertIndex[2]];

                    //计算面的法向量
                    vVector1 = Vector(vPoly[0], vPoly[2]);
                    vVector2 = Vector(vPoly[2], vPoly[1]);

                    vNormal = Cross(vVector1, vVector2);
                    pTempNormals[i] = vNormal;
                    vNormal = Normalize(vNormal);
                    pNormals[i] = vNormal;
                }

                //下面求顶点的法向量:顶点法向量是以该点为顶点的所有三角形法向量之和
                CVector3 vSum = new CVector3();
                vSum.x = 0; vSum.y = 0; vSum.z = 0;
                int shared = 0;

                //遍历所有的顶点
                for (int i = 0; i < pObject.numOfVerts; i++)
                {
                    for (int j = 0; j < pObject.numOfFaces; j++)
                    {
                        if (pObject.pFaces[j].vertIndex[0] == i ||
                            pObject.pFaces[j].vertIndex[1] == i ||
                            pObject.pFaces[j].vertIndex[2] == i)
                        {
                            vSum = AddVector(vSum, pTempNormals[j]);
                            shared++;
                        }
                    }
                    pObject.pNormals[i] = DivideVectorByScaler(vSum, (float)(-1 * shared));

                    //规范化最后的顶点法向量
                    pObject.pNormals[i] = Normalize(pObject.pNormals[i]);

                    vSum.x = 0; vSum.y = 0; vSum.z = 0;
                    shared = 0;
                }
            }
        }
        //求两点决定的矢量
        CVector3 Vector(CVector3 p1, CVector3 p2)
        {
            CVector3 v = new CVector3();
            v.x = p1.x - p2.x;
            v.y = p1.y - p2.y;
            v.z = p1.z - p2.z;
            return v;
        }
        //返回两个矢量的和
        CVector3 AddVector(CVector3 p1, CVector3 p2)
        {
            CVector3 v = new CVector3();
            v.x = p1.x + p2.x;
            v.y = p1.y + p2.y;
            v.z = p1.z + p2.z;
            return v;
        }
        //返回矢量的缩放
        CVector3 DivideVectorByScaler(CVector3 v, float Scaler)
        {
            CVector3 vr = new CVector3();
            vr.x = v.x / Scaler;
            vr.y = v.y / Scaler;
            vr.z = v.z / Scaler;
            return vr;
        }
        //返回两个矢量的叉积
        CVector3 Cross(CVector3 p1, CVector3 p2)
        {
            CVector3 c = new CVector3();
            c.x = ((p1.y * p2.z) - (p1.z * p2.y));
            c.y = ((p1.z * p2.x) - (p1.x * p2.z));
            c.z = ((p1.x * p2.y) - (p1.y * p2.x));
            return c;
        }
        //规范化矢量
        CVector3 Normalize(CVector3 v)
        {
            CVector3 n = new CVector3();
            double mag = Mag(v);
            n.x = v.x / (float)mag;
            n.y = v.y / (float)mag;
            n.z = v.z / (float)mag;
            return n;
        }
        //矢量的模
        double Mag(CVector3 v)
        {
            return Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
        }
        #endregion

        #region 读取并转换二进制数据
        //读出一个字符串
        uint getStr(ref String str)
        {
            str = "";
            char c = (char)m_FilePointer.ReadByte();
            while (c != 0)
            {
                str += c;
                c = (char)m_FilePointer.ReadByte();
            }

            return (uint)(str.Length + 1);
        }
        //读出byte数组
        public static uint fread(ref int[] buffer, uint length, FileStream f)
        {
            for (uint i = 0; i < length; i++)
            {
                try
                {
                    buffer[i] = f.ReadByte();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(f.Name + " 读取出错");
                    Debug.WriteLine(ex.ToString());
                    return i;
                }
            }
            return length;
        }
        //读出2个字节或4个字节的int
        public static uint fread(ref int buffer, uint length, FileStream f)
        {
            if (length == 2)
            {
                Byte[] buf = new Byte[2];
                uint len = (UInt32)f.Read(buf, 0, 2);
                buffer = (buf[1] * 256 + buf[0]);
                return len;
            }
            else if (length == 4)
            {
                Byte[] buf = new Byte[4];
                uint len = (UInt32)f.Read(buf, 0, 4);
                buffer = (((buf[3] * 256 + buf[2]) * 256 + buf[1]) * 256 + buf[0]);
                return len;
            }
            return 0;
        }
        //读出CVector3数组
        public static uint fread(ref CVector3[] buffer, uint length, FileStream f)
        {
            uint l = 0;
            try
            {
                for (uint i = 0; i < length / 12; i++)
                {
                    buffer[i] = new CVector3();
                    Byte[] bts = new Byte[4];
                    l += (uint)f.Read(bts, 0, 4);
                    buffer[i].x = FileHead.byte2float(bts);
                    l += (uint)f.Read(bts, 0, 4);
                    buffer[i].y = FileHead.byte2float(bts);
                    l += (uint)f.Read(bts, 0, 4);
                    buffer[i].z = FileHead.byte2float(bts);
                }
                return l;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(f.Name + " 读取出错");
                Debug.WriteLine(ex.ToString());
                return l;
            }
        }
        //读出CVector数组
        public static uint fread(ref CVector2[] buffer, uint length, FileStream f)
        {
            uint l = 0;
            try
            {
                for (uint i = 0; i < length / 8; i++)
                {
                    buffer[i] = new CVector2();
                    Byte[] bts = new Byte[4];
                    l += (uint)f.Read(bts, 0, 4);
                    buffer[i].x = FileHead.byte2float(bts);
                    l += (uint)f.Read(bts, 0, 4);
                    buffer[i].y = FileHead.byte2float(bts);
                }
                return l;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(f.Name + " 读取出错");
                Debug.WriteLine(ex.ToString());
                return l;
            }
        }
        //读出字符串
        public static uint fread(ref String buffer, uint length, FileStream f)
        {
            uint l = 0;
            //buffer = "";
            try
            {
                for (int i = 0; i < length; i++)
                {
                    Byte[] b = new Byte[1];
                    l += (uint)f.Read(b, 0, 1);
                    if (i != length - 1)
                        buffer += (char)(b[0]);
                }

                return l;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(f.Name + " 读取出错");
                Debug.WriteLine(ex.ToString());
                return l;
            }
        }
        #endregion
    }
    
    //画3d模型
    public class H3DModel
    {
        public const int CHANGE = 1;
        public const int IGNORE = 2;
        public const int ADD = 3;
        public t3DModel model = null;//在GL中的模型（即经过归一化处理的模型）
        //public t3DModel tmodel = null;//真实的模型
        public uint[] g_Texture;
        public CVector3 boxMin, boxMax;
        public double cx, cy, cz, w, h, d;
        public double scale;
        //public string[] floors;
        public int Transparency = 255;
        public int Traparent = 1;

        public H3DModel()
        {
            //this.tmodel = new t3DModel();
            this.model = new t3DModel();
            
        }

    
        public static H3DModel FromFile(string fileName)    //从文件中加载3D模型
        {
            H3DModel h3d = new H3DModel();
            CLoad3DS load = new CLoad3DS();
            load.Import3DS(h3d.model, fileName);
            //h3d.model.numOfMaterials = h3d.tmodel.numOfMaterials;
            //h3d.model.numOfObjects = h3d.tmodel.numOfObjects;
            //for(int i = 0; i < h3d.tmodel.numOfMaterials;i++)
            //{
            //    h3d.model.pMaterials.Add( h3d.tmodel.pMaterials[i]);
            //}
            //for (int i = 0; i < h3d.tmodel.numOfObjects; i++)
            //{
            //    h3d.model.pObject.Add(h3d.tmodel.pObject[i]);
            //}
            //if (!h3d.LoadTextrue())
            //return null;
            h3d.LoadBox();
            return h3d;
        }
        public t3DModel gemodelData()                      //得到3D模型数据
        {
            return this.model;
        }
        public bool LoadTextrue()
        {
            this.g_Texture = new uint[model.numOfMaterials];
            for (int i = 0; i < model.numOfMaterials; i++)
            {
                if (model.pMaterials[i].strFile.Length > 0)
                    if (!LoadGLTextures(ref this.g_Texture, model.pMaterials[i].strFile, i))
                        return false;
                model.pMaterials[i].texureId = i;
            }
            return true;
        }
        static double OutputAbs(double f)
        {
            if (f < 0) return -f;
            return f;
        }
        static double OutputMax(double a, double b)
        {
            if (b > a) return b;
            return a;
        }

        //模型单位归一化
        protected void LoadBox()
        {
            boxMax = new CVector3();
            boxMin = new CVector3();
            boxMax.x = float.MinValue;
            boxMax.y = float.MinValue;
            boxMax.z = float.MinValue;
            boxMin.x = float.MaxValue;
            boxMin.y = float.MaxValue;
            boxMin.z = float.MaxValue;
            
            //获取模型的边界值
            for (int i = 0; i < model.numOfObjects; i++)
            {
                t3DObject pObject = model.pObject[i];
                for (int j = 0; j < pObject.numOfVerts; j++)
                {
                    float x = pObject.pVerts[j].x/1000;
                    float y = pObject.pVerts[j].y/1000;
                    float z = pObject.pVerts[j].z/1000;
                    if (boxMin.x > x)
                        boxMin.x = x;
                    if (boxMin.y > y)
                        boxMin.y = y;
                    if (boxMin.z > z)
                        boxMin.z = z;
                    if (boxMax.x < x)
                        boxMax.x = x;
                    if (boxMax.y < y)
                        boxMax.y = y;
                    if (boxMax.z < z)
                        boxMax.z = z;
                }
            }
            //计算模型的宽度、高度和深度
            w = (boxMax.x - boxMin.x); // OutputAbs(boxMax.x) + OutputAbs(boxMin.x);
            h = (boxMax.y - boxMin.y); // OutputAbs(boxMax.y) + OutputAbs(boxMin.y);
            d = (boxMax.z - boxMin.z); // OutputAbs(boxMax.z) + OutputAbs(boxMin.z);
            //计算模型的中心
            cx = (boxMax.x + boxMin.x) / 2.0;
            cy = (boxMax.y + boxMin.y) / 2.0;
            cz = (boxMax.z + boxMin.z) / 2.0;
            //计算归一化所需的缩放比例
            scale = OutputMax(OutputMax(w, h), d);
            scale = 4.0f / scale;//根据模型实际来决定
            //导入模型的单位为mm，将单位转换为m
            for (int i = 0; i < model.numOfObjects; i++)
            {
                t3DObject pObject = model.pObject[i];
                for (int j = 0; j < pObject.numOfVerts; j++)
                {
                    pObject.pVerts[j].x /= 1000;
                    pObject.pVerts[j].y /= 1000;
                    pObject.pVerts[j].z /= 1000;
                    //pObject.pVerts[j].x -= (float)cx;
                    //pObject.pVerts[j].y -= (float)cy;
                    //pObject.pVerts[j].z -= (float)cz;
                }
            }     
        }

        //创建贴图
        public bool LoadGLTextures(ref uint[] textureArray, String strFileName, int textureID)									// Load Bitmaps And Convert To Textures
        {
            bool Status = false;									// Status Indicator

            Bitmap[] TextureImage = new Bitmap[1];					// Create Storage Space For The Texture

            // memset(TextureImage,0,sizeof(void *)*1);           	// Set The Pointer To NULL

            // Load The Bitmap, Check For Errors, If Bitmap's Not Found Quit
            if ((TextureImage[0] = new Bitmap(strFileName)) != null)
            {
                Status = true;									// Set The Status To TRUE

                uint[] tArray = new uint[1];
                GL.glGenTextures(1, tArray);					// Create The Texture
                textureArray[textureID] = tArray[0];

                TextureImage[0].RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, TextureImage[0].Width, TextureImage[0].Height);
                System.Drawing.Imaging.BitmapData bitmapdata = TextureImage[0].LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                // Typical Texture Generation Using Data From The Bitmap
                GL.glBindTexture(GL.GL_TEXTURE_2D, tArray[0]);
                GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, (int)GL.GL_RGB, TextureImage[0].Width, TextureImage[0].Height, 0, GL.GL_BGR_EXT, GL.GL_UNSIGNED_BYTE, bitmapdata.Scan0);
                GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, GL.GL_LINEAR);
                GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, GL.GL_LINEAR);
                TextureImage[0].UnlockBits(bitmapdata);
                TextureImage[0].Dispose();
            }

            return Status;										// Return The Status
        }

        protected bool CreateTexture(ref uint[] textureArray, String strFileName, int textureID)
        {
            Bitmap image = null;
            try
            {
                image = new Bitmap(strFileName);
            }
            catch (ArgumentException)
            {
                Debug.WriteLine("Could not load " + strFileName + " .");
                return false;
            }
            if (image != null)
            {
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                BitmapData bitmapdata;
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                bitmapdata = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                uint[] tArray = new uint[1];
                GL.glGenTextures(1, tArray);
                textureArray[textureID] = tArray[0];

                GL.glPixelStorei(GL.GL_UNPACK_ALIGNMENT, 1);

                GL.glBindTexture(GL.GL_TEXTURE_2D, textureArray[textureID]);
                GL.gluBuild2DMipmaps(GL.GL_TEXTURE_2D, 3, image.Width, image.Height, GL.GL_RGB, GL.GL_UNSIGNED_BYTE, bitmapdata.Scan0);
                //GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, (int)GL.GL_RGB, image.Width, image.Height, 0, GL.GL_BGR_EXT, GL.GL_UNSIGNED_BYTE, bitmapdata.Scan0);
                GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, GL.GL_LINEAR_MIPMAP_NEAREST);
                GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, GL.GL_LINEAR_MIPMAP_LINEAR);
                image.UnlockBits(bitmapdata);
                image.Dispose();
                return true;
            }
            return false;
        }

        public void DrawModel()                             //画出模型
        {
            GL.glPushMatrix();
            GL.glRotatef(270.0f, 1.0f, 0.0f, 0.0f);

            GL.glEnable(GL.GL_BLEND);
            GL.glBlendFunc(GL.GL_SRC_ALPHA, GL.GL_ONE_MINUS_SRC_ALPHA);
            GL.glDepthMask((byte)Traparent);
            //画太阳

            //画读入的模型
            #region 
            for (int i = 0; i < this.model.numOfObjects; i++)
            {
                if (this.model.pObject.Count <= 0) break;

                t3DObject pObject = this.model.pObject[i];
                // if (!floors.Contains(pObject.strName.Substring(1, 1))) continue;

                if (pObject.bHasTexture)
                {
                    GL.glEnable(GL.GL_TEXTURE_2D);

                    GL.glColor4ub(255, 255, 255, (byte)Transparency);
                    GL.glBindTexture(GL.GL_TEXTURE_2D, this.g_Texture[pObject.materialID]);

                }
                else
                {
                    GL.glDisable(GL.GL_TEXTURE_2D);
                    GL.glColor3ub(255, 255, 255);
                }
                GL.glBegin(GL.GL_TRIANGLES);
                for (int j = 0; j < pObject.numOfFaces; j++)
                {
                    for (int whichVertex = 0; whichVertex < 3; whichVertex++)
                    {
                        int index = pObject.pFaces[j].vertIndex[whichVertex];

                        GL.glNormal3f(pObject.pNormals[index].x, pObject.pNormals[index].y, pObject.pNormals[index].z);

                        if (pObject.bHasTexture)
                        {
                            if (pObject.pTexVerts != null)
                            {
                                GL.glTexCoord2f(pObject.pTexVerts[index].x, pObject.pTexVerts[index].y);
                            }
                        }
                        else
                        {

                            if (this.model.pMaterials.Count != 0 && pObject.materialID >= 0)
                            {
                                int[] color = this.model.pMaterials[pObject.materialID].color;
                                GL.glColor3ub((byte)color[0], (byte)color[1], (byte)color[2]);
                            }
                        }
                        GL.glVertex3f(pObject.pVerts[index].x*(float)scale, pObject.pVerts[index].y*(float)scale, pObject.pVerts[index].z*(float)scale);

                    }
                }
                GL.glEnd();
                GL.glDisable(GL.GL_TEXTURE_2D);
            }
            GL.glDisable(GL.GL_BLEND);
            GL.glPopMatrix();
            #endregion
        }
        //public void DrawBorder()                            //画出边框
        //{
        //    if (this.boxMax.x != float.MinValue && this.boxMin.x != float.MaxValue)
        //    {
        //        GL.glColor3ub((byte)255, (byte)255, (byte)255);
        //        float[] v = new float[6];
        //        v[0] = boxMin.x;
        //        v[1] = boxMin.y;
        //        v[2] = boxMin.z;
        //        v[3] = boxMax.x;
        //        v[4] = boxMax.y;
        //        v[5] = boxMax.z;

        //        GL.glBegin(GL.GL_LINE_LOOP);
        //        {
        //            GL.glVertex3f(v[0], v[1], v[2]);
        //            GL.glVertex3f(v[0], v[4], v[2]);
        //            GL.glVertex3f(v[3], v[4], v[2]);
        //            GL.glVertex3f(v[3], v[1], v[2]);
        //        }
        //        GL.glEnd();
        //        GL.glBegin(GL.GL_LINE_LOOP);
        //        {
        //            GL.glVertex3f(v[0], v[1], v[5]);
        //            GL.glVertex3f(v[0], v[4], v[5]);
        //            GL.glVertex3f(v[3], v[4], v[5]);
        //            GL.glVertex3f(v[3], v[1], v[5]);
        //        }
        //        GL.glEnd();
        //        GL.glBegin(GL.GL_LINES);
        //        {
        //            GL.glVertex3f(v[0], v[1], v[2]);
        //            GL.glVertex3f(v[0], v[1], v[5]);
        //            GL.glVertex3f(v[0], v[4], v[2]);
        //            GL.glVertex3f(v[0], v[4], v[5]);
        //            GL.glVertex3f(v[3], v[4], v[2]);
        //            GL.glVertex3f(v[3], v[4], v[5]);
        //            GL.glVertex3f(v[3], v[1], v[2]);
        //            GL.glVertex3f(v[3], v[1], v[5]);
        //        }
        //        GL.glEnd();
        //    }
        //    else
        //    {
        //        Debug.WriteLine("No Objects");
        //    }

        //}
        //public CVector3[] getOriginalBorder()               //得到模型边框的8个点
        //{
        //    CVector3[] vs = new CVector3[8];
        //    float[] v = new float[6];
        //    v[0] = boxMin.x;
        //    v[1] = boxMin.y;
        //    v[2] = boxMin.z;
        //    v[3] = boxMax.x;
        //    v[4] = boxMax.y;
        //    v[5] = boxMax.z;
        //    for (int i = 0; i < 8; i++)
        //        vs[i] = new CVector3();
        //    vs[0].x = v[0]; vs[0].y = v[1]; vs[0].z = v[2];
        //    vs[1].x = v[0]; vs[1].y = v[4]; vs[1].z = v[2];
        //    vs[2].x = v[3]; vs[2].y = v[4]; vs[2].z = v[2];
        //    vs[3].x = v[3]; vs[3].y = v[1]; vs[3].z = v[2];
        //    vs[4].x = v[0]; vs[4].y = v[1]; vs[4].z = v[5];
        //    vs[5].x = v[0]; vs[5].y = v[4]; vs[5].z = v[5];
        //    vs[6].x = v[3]; vs[6].y = v[4]; vs[6].z = v[5];
        //    vs[7].x = v[3]; vs[7].y = v[1]; vs[7].z = v[5];
        //    return vs;
        //}

    }
}
