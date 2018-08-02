// Copyright 2016 Nibiru. All rights reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
///  全局变量
/// </summary>
namespace Nvr.Internal
{
    class NvrGlobal
    {
        /// <summary>
        ///  默认的z距离
        /// </summary>
        public static float defaultGazeDistance = 50;

        // 是否初始化陀螺仪接口
        public static bool trackerInited = false;
        // nvr是否初始化
        public static bool nvrStarted = false;
        // 运行环境是否支持dtr
        public static bool supportDtr = false;
        // 加载哪个库
        public static bool useNvrSo = false;
        // 畸变开关
        public static bool distortionEnabled = true;
        // DFT光学参数 {inter_lens_distance,vertical_distance_to_lens_center,screen_to_lens_distance,fov,distortion_coef_x,distortion_coef_y,screen_width,screen_height
        //  border_size_meters,screen_size_x,screen_size_y,inter_lens_distance_v2,screen_to_lens_distance_v2,distortion_coef_v2_k1,distortion_coef_v2_k2,lens_ipd,distortion_mesh_x,distortion_mesh_y}
        public static float[] dftProfileParams = new float[21];

        public static float fovNear = -1;
        public static float fovFar = -1;

        // 渠道标识
        public static string channelCode = "";
        // JAR版本号
        public static int jarVersion = -1;
        // SO版本号
        public static int soVersion = -1;
        // 平台ID
        public static int platformID = -1;
        // 平台性能等级
        public static int platPerformanceLevel = -1;

    }

    public enum PERFORMANCE
    {   // H8&V700=0,RK3288&S900&Intel T3=1,RK3399&Samsung7420&Intel T4 &MTK=2
        LOW = 0,
        NORMAL =1,
        HIGH = 2,
    }

    public enum PLATFORM
    {
        GENERAL = 0x0000,
        RK_3288_CG = 0x0001,
        ACT_S900 = 0x0002,
        SAMSUNG = 0x0003,
        INTEL_T3 = 0x0004,
        INTEL_T4 = 0x0005,
        MTK_X20 = 0x0006,
        QUALCOMM = 0x0007,
        RK_3399 = 0x0008,
        SAMSUNG_8890VR = 0x0009,
    }

    public enum JARVERSION
    {
       // 161228 [增加版本号]
       JAR_161228 = 161228,

    }

    public enum SOVERSION
    {
        // 161228 [增加版本号]
        SO_1228 = 161228,
    }

   public enum GazeTag
    {
        Show = 0,
        Hide = 1,
        Set_Distance = 2,
        Set_Size = 3,
        Set_Color = 4
    }

    public enum GazeSize
    {
        Original = 0,
        Large = 1,
        Medium = 2,
        Small= 3
    }

    public enum TextureQuality
    {
         Simple = 2,
         Good = 0,
         Best= 1
    }
     
}
