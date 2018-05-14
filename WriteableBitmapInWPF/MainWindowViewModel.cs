using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WriteableBitmapInWPF
{
    public class MainWindowViewModel : NotificationObject
    {
        /// <summary>
        /// 图片1序号（起）
        /// </summary>
        private static readonly int INDEX_MIN = 0;
        /// <summary>
        ///  图片11序号（终）
        /// </summary>
        private static readonly int INDEX_MAX = 11;
        private int bitmapIndex = INDEX_MIN;
        private int writableBitmapIndex = INDEX_MIN;
        /// <summary>
        /// UI绑定的资源对象
        /// </summary>
        private WriteableBitmap writableBitmap;
        public WriteableBitmap WritableBitmap
        {
            get
            {
                return writableBitmap;
            }

            set
            {
                writableBitmap = value;
                this.RaisePropertyChanged("WritableBitmap");
            }
        }
        /// <summary>
        /// 循环加载图片
        /// </summary>
        public void runimg()
        {
            int i = 0;
            new Task(async ()=> {
                while (true)
                {
                    if (i == 11)
                    {
                        i = 0;

                    }
                    using (Bitmap bmp =new Bitmap(GetUri(i)))   // Bitmap.FromFile(GetUri(i)))
                    {
                        //其他格式的图片转32rgb
                        using (Bitmap bitmap32 = new Bitmap(bmp.Width, bmp.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
                        {
                            using (Graphics g = Graphics.FromImage(bitmap32))
                            {
                                g.DrawImageUnscaled(bmp, 0, 0);
                            }
                            Console.WriteLine(bitmap32.PixelFormat);
                            bool issc = false;
                            string str = string.Empty;
                            //转化方式需要用GetRgb32_From_Bitmap ，Convertbmp这种方式报错内存异常
                            //UpdateWritableBitmap(Convertbmp(bitmap32));
                            //更新资源对象
                            UpdateWritableBitmap(GetRgb32_From_Bitmap(bitmap32, ref issc, ref str));
                        }
                    }
                    i++;
                    //休眠10ms
                    await Task.Delay(10);
                }
            }).Start();
        }
        /// <summary>
        /// 帧写入
        /// </summary>
        /// <param name="frame"></param>
        private void UpdateWritableBitmap(BitmapFrame frame)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                writableBitmap.Lock();
                frame.CopyPixels(new System.Windows.Int32Rect(0, 0, 3072, 2048), writableBitmap.BackBuffer, (int)writableBitmap.BackBufferStride * (int)writableBitmap.Height, (int)writableBitmap.BackBufferStride);
                writableBitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, 3072, 2048));
                writableBitmap.Unlock();
            }, System.Windows.Threading.DispatcherPriority.Background);
        }
        /// <summary>
        /// 数组写入
        /// </summary>
        /// <param name="byt"></param>
        private void UpdateWritableBitmap(byte[] byt)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                writableBitmap.Lock();
                Marshal.Copy(byt, 0, writableBitmap.BackBuffer, byt.Length);
                writableBitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, 3072, 2048));
                writableBitmap.Unlock();
            }, System.Windows.Threading.DispatcherPriority.Background);
        }
        /// <summary>
        /// 将Image转换为byte[]
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public byte[] ConvertImage(Image image)
        {
            FileStream fs = new FileStream("imagetemp", FileMode.Create, FileAccess.Write, FileShare.None);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, (object)image);
            fs.Close();
            fs = new FileStream("imagetemp", FileMode.Open, FileAccess.Read, FileShare.None);
            byte[] bytes = new byte[fs.Length];
            fs.Read(bytes, 0, (int)fs.Length);
            fs.Close();
            return bytes;
        }
        /// <summary>
        /// 将bmp转换为byte[]
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public byte[] Convertbmp(Bitmap image)
        {
            byte[] bytes;
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                bytes = ms.GetBuffer(); 
                ms.Close();
            }
            return bytes;
        }
        /// <summary>  
        /// Bitmap转换层RGB32  
        /// </summary>  
        /// <param name="Source">Bitmap图片</param>  
        /// <returns></returns>  
        public byte[] GetRgb32_From_Bitmap(Bitmap Source, ref bool bError, ref string errorMsg)
        {
            bError = false;

            int lPicWidth = Source.Width;
            int lPicHeight = Source.Height;

            Rectangle rect = new Rectangle(0, 0, lPicWidth, lPicHeight);
            System.Drawing.Imaging.BitmapData bmpData = Source.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, Source.PixelFormat);
            IntPtr iPtr = bmpData.Scan0;

            int picSize = lPicWidth * lPicHeight * 4;

            byte[] pRrgaByte = new byte[picSize];

            System.Runtime.InteropServices.Marshal.Copy(iPtr, pRrgaByte, 0, picSize);

            Source.UnlockBits(bmpData);

            int iPoint = 0;
            int A = 0;

            try
            {
                bError = true;
                errorMsg = "BMP数据转换成功";

                return pRrgaByte;
            }
            catch (Exception exp)
            {
                pRrgaByte = null;

                bError = false;
                errorMsg = exp.ToString();
                //throw;  
            }

            return null;
        }
        /// <summary>
        /// 图片路径
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private string GetUri(int index)
        {
            var directory = @"……"; //测试图片路径  鄙人用的12张6M/pc 3072*2048  bmp 8位黑白图
            var extention = ".bmp";//图片格式

            var fileName = bitmapIndex.ToString();
            bitmapIndex = (INDEX_MAX == bitmapIndex) ? (INDEX_MIN) : (bitmapIndex + 1);

            return directory + fileName + extention;
        }
        /// <summary>
        /// 构造
        /// </summary>
        public MainWindowViewModel()
        {
            //8位
            //WritableBitmap = new WriteableBitmap(3072, 2048, 96, 96, System.Windows.Media.PixelFormats.Indexed8,BitmapPalettes.Gray256);
            //初始化一次
            //32rbg
            WritableBitmap = new WriteableBitmap(3072, 2048, 96,96, System.Windows.Media.PixelFormats.Bgr32, null);
            runimg();
        }

    }
}
