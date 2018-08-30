# WriteableBitmapInWPF
WriteableBitmap低开销更新WPFImage UI控件

WPF的Image控件绑定资源对象WritableBitmap

1. 原始图片加载到bitmap对象
2. 转化bitmap对象为pixelformat是rgb32的bitmap
3. 再将其转化成rgb32的byte[]
4. Marshal.Copy()将得到的数组赋值入WritableBitmap的writableBitmap.BackBuffer指针指向的内存地址中
5. 最后更新界面UI

##该方式的主要目的是降低了原来imagesource对象绑定之后，释放很慢的问题!

run的时候，记得修改图片路径，我这边用的是12张 3072*2048 6M/pc bmp 8位图片

改进点：WritableBitmap初始化的地方，我这里初始化了rgb32的，如果初始化8位的，在Marshal.Copy()的时候回报异常。如果改进这里就不用第2步转化了

有大神有更好做法请指教，谢谢！

