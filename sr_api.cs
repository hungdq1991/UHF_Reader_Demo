using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace SrDemo
{
    // 功率 （设置，获取）
    [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]
    public struct power_t
    {
        public byte loop;				// 开闭环（开环（默认状态）：0x00;闭环：0x01）
        public byte read;				// 读功率（取值范围：5~30 dBm）
        public byte write;				// 写功率（取值范围：5~30 dBm）
    }

    /************************************************************************
    温度×100，转换为十六进制后，负数则取补码
    例子：当前模块温度为 -40℃，-40*100 = -4000 = 0xFO60,
		    则 temp_msb = 0xF0;temp_lsb = 0x60
    ************************************************************************/
    // 模块的温度
    [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]
    public struct temperature_t
    {
        public byte temp_msb;			// 温度值的高8位
        public byte temp_lsb;			// 温度值的低8位
    }


    // 模块外接的GPIO
    [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]
    public struct gpio_t
    {
        public byte gpio;					// bit[0]:gpio1。bit[1]:gpio2。...bit[7]:gpio8
        public byte gpio_level;				// 对应每个gpio的高低电平
    }


    //说明：开启状态：fastid_switch为0x01；关闭状态：fastid_switch为0x00。
    [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]
    public struct fastid_t
    {
        public byte fastid_switch;			// fastid 的开关。
    }

    //说明：开启状态：carrier_switch为0x01；关闭状态：carrier_switch为0x00。
    [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]
    public struct carrier_t
    {
        public byte carrier_switch;			// carrier 的开关。
    }

    [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]
    public struct Boot_t
    {
        public byte bootAuto_switch;			// carrier 的开关。
    }



    /*
    读写器频率区域
    China1  0x01 
    China2  0x02 
    Europe  0x03 
    USA		0x04 
    Korea	0x05 
    Japan	0x06
    */
    // 读写器频率区域
    [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]
    public struct frequency_region_t
    {
        public byte save_setting;			// 保存设置标志.说明：保存设置标志为0时，不保存设置，为1时保存设置。
        public byte region;					// 读写器频率区域
    }


    // 频点
    [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]
    public struct output_frequency_t
    {
        public char frequency_num;			// 频点个数
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = global.OUTPUT_FREQUENCY_NUM)]
        public float[] frequency;	            // 输出频率
    }


    // 设置模块通讯波特率
    [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]
    public struct baud_rate_t
    {
        public byte rate_type;				// 波特率 类型
    }


    // 天线 （设置，获取）
    [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]
    public struct antenna_t
    {
        public byte ants;				// 天线号（bit0：ant1；bit1：ant2；... bit7：ant8）
    }



    /************************************************************************
    说明：设置天线工作时间只适用四端口模块，Data0，Data1为天线1的工作
    时间（单位ms，范围30ms—60000ms），Data2，Data3为天线2的工作时间（单
    位ms，范围30ms—60000ms），Data4，Data5为天线3的工作时间（单位ms，
    范围30ms—60000ms），Data6，Data7为天线4 的工作时间（单位ms，范围
    30ms—60000ms），Data8，Data9为等待时间（单位ms，范围0ms—60000ms），

    如果是使用部分天线，则只需使能需要的天线号就可以了

    例：设置天线1的工作时间为100ms，天线2工作时间150ms，天线3工作
    时间314ms，天线4工作时间30ms，等待时间10000ms。
    命令：BB 1F 0A 00 64 00 96 01 3A 00 1E 27 10 B3 0D 0A
    ************************************************************************/
    // 天线工作时间及等待时间
    [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]
    public struct antenna_time_t
    {
        public byte com_type;			// 命令类型（0x1F,0x9F;0x20,0xA0）
        public byte ant1_msb;			// 天线1的工作时间值的高8位
        public byte ant1_lsb;			// 天线1的工作时间值的低8位
        public byte ant2_msb;			// 天线2的工作时间值的高8位
        public byte ant2_lsb;			// 天线2的工作时间值的低8位
        public byte ant3_msb;			// 天线3的工作时间值的高8位
        public byte ant3_lsb;			// 天线3的工作时间值的低8位
        public byte ant4_msb;			// 天线4的工作时间值的高8位
        public byte ant4_lsb;			// 天线4的工作时间值的低8位
        public byte wait_msb;			// 等待时间值的高8位
        public byte wait_lsb;			// 等待时间值的低8位
    }


    // 标签epc数据
    [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]
    public struct epc_t
    {
        public byte pc_msb;
        public byte pc_lsb;
        public char epc_len;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = global.PACKET_128)]
        public byte[] epc;	    // PACKET_MID

    }


    /************************************************************************
    说明：RSSI以补码的形式表示，共16bit，为实际值×10。如-65.7dBm，则
    RSSI=FD6F = -657;
    ************************************************************************/
    // 查询标签EPC 
    [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]
    public struct query_epc_t
    {
        public byte com_type;	    // 命令类型（0x16,0x96）
        public epc_t epc;

        public byte rssi_msb;
        public byte rssi_lsb;
        public byte ant_id;		    // 天线号
        public int tid_len;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = global.PACKET_128)]
        public byte[] tid;            // PACKET_MID
    }


    /************************************************************************
    说明：AP为标签的访问密码；PC+EPC过滤查询需要，若不过滤，则必须
    全部置零；MB为用户需要查询的数据的bank号；SA为需查询的数据的起始地
    址，单位为字；DL为需查询的数据长度，单位为字
    ************************************************************************/
    // 读取，写入标签数据
    // [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)] 
    [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]
    public struct tags_data_t
    {
        public byte com_type;				// 命令类型（0x19,0x99;0x1A,0x9A）
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = global.PASSWORD_LEN)]
        public byte[] password;	            // 密码,PASSWORD_LEN
        public epc_t epc;
        public byte mem_bank;				// 要读写的标签的内存区
        public byte start_addr_msb;			// 要读写的标签的内存区的开始位置的高8位
        public byte start_addr_lsb;			// 要读写的标签的内存区的开始位置的低8位
        public byte data_len_msb;			// 要读写的数据长度的高8位，单位为字
        public byte data_len_lsb;			// 要读写的数据长度的低8位，单位为字
        public byte ant_id;					// 天线号
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = global.PACKET_MAX)]
        public byte[] data;		            // 要读写的数据, PACKET_MAX
    }


    /************************************************************************
    说明：循环查询标签EPC次数范围为1~0xFFFF，为0时，表示永久查询标
    签EPC 
    例：循环查询标签EPC次数为100次
    命令：BB 17 02 00 64 7D 0D 0A 
    ************************************************************************/
    [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]
    public struct multi_query_epc_t
    {
        public byte com_type;			// 命令类型（0x17,0x97;0x18,0x98）
        public byte query_total_msb;	// 查询次数的高8位
        public byte query_total_lsb;	// 查询次数的低8位
        public byte packet_num;			// 完整的数据包的个数

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = global.RECV_PACKET_NUM)]
        public query_epc_t[] tags_epc;	// 标签的EPC相关信息,RECV_PACKET_NUM
    }




    // 写标签的EPC
    [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]
    public struct tags_epc_t
    {
        public byte com_type;				// 命令类型 WRITE_TAGS_EPC
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = global.PACKET_128)]
        public byte[] epc;		                    // PACKET_128
        public byte epc_len;

        public tags_epc_t(int e_len)
        {
            com_type = 0;
            epc = new byte[e_len];
            epc_len = 0;
        }
    }

    class sr_api
    {
        /***********************************************************************************/
        /**  函数名:   Set_Power                                                           */
        /**  功能:     该函数用于设置模块工作功率                                          */
        /*  loop:  该参数为无符号8位，参数是设置模块的开闭环功能。		    */
        /*  read:  该参数为无符号8位，参数是设置模块读功率。				*/
        /*  write: 该参数为无符号8位，参数是设置模块写功率。				*/
        /*   返回值:   设置成功返回0，失败返回负数                                         */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Set_Power(byte loop, byte read, byte write);


        /***********************************************************************************/
        /**  函数名:   Get_Power                                                           */
        /**  功能:     该函数用于获取模块工作功率                                          */
        /*  loop:  该参数为无符号8位，参数是获取模块的开闭环功能。			*/
        /*  read:  该参数为无符号8位，参数是获取模块读功率。				*/
        /*  write: 该参数为无符号8位，参数是获取模块写功率。				*/
        /*   返回值:   设置成功返回0，失败返回负数                                         */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Get_Power(ref byte loop,ref byte read,ref byte write);

        /***********************************************************************************/
        /**  函数名:   Set_Gpio_level                                                      */
        /**  功能:     该函数用于设置模块GPIO状态                                          */
        /*  gpio:   设置GPIO位，bit0 ->GPIO1 bit1->GPIO2 bit2->GPIO3。			*/
        /*  level:  设置GPIO状态，	0 低电平  1 高电平							*/
        /*   返回值:   设置成功返回0，失败返回负数                                         */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Set_Gpio_level(byte g_data, byte level);

        /***********************************************************************************/
        /**  函数名:   Get_Gpio_level                                                      */
        /**  功能:     该函数用于获取模块GPIO状态                                          */
        /*  gpio:   设置GPIO位，bit0 ->GPIO1 bit1->GPIO2 bit2->GPIO3。			*/
        /*  level:  设置GPIO状态，	0 低电平  1 高电平							*/
        /*   返回值:   设置成功返回0，失败返回负数                                         */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Get_Gpio_level(byte g_data,ref byte level);

        /***********************************************************************************/
        /**  函数名:   Get_hardware_version                                                */
        /**  功能:     该函数用于获取硬件版本信息                                          */
        /*  version:   获取的版本号						*/
        /*   返回值:   设置成功返回0，失败返回负数                                         */
        /***********************************************************************************/
        [DllImport("api_dll.dll")]
        public extern static int Get_hardware_version(byte[] version);

        /***********************************************************************************/
        /**  函数名:   Get_firmware_version                                                */
        /**  功能:     该函数用于获取模块版本信息                                          */
        /*  version:   获取的版本号						*/
        /*   返回值:   设置成功返回0，失败返回负数                                         */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Get_firmware_version(byte[] version);

        /***********************************************************************************/
        /**  函数名:   Set_output_frequency                                                */
        /**  功能:     该函数用于设置模块射频输出频率                                      */
        /*  count:   设置射频输出跳频个数						*/
        /*  data:  设置射频输出跳频频率   	单位为KHz			*/
        /*	data的值个数是根据conut，射频输出频率为实际*1000    */
        /*   返回值:   设置成功返回0，失败返回负数                                         */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Set_output_frequency(byte count, int[] data);

        /***********************************************************************************/
        /**  函数名:   Get_output_frequency                                                */
        /**  功能:     该函数用于获取模块射频输出频率                                      */
        /*  count:   获取射频输出跳频个数						*/
        /*  data:  获取射频输出跳频频率							*/
        /*   返回值:   设置成功返回0，失败返回负数                                         */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Get_output_frequency(ref byte count, int[] data);

        /***********************************************************************************/
        /**  函数名:   Set_Work_Antanne                                                    */
        /**  功能:     该函数用于设置模块工作天线                                          */
        /*  ants:   设置模块工作天线						*/
        /*					ants					*/
        /*  Ant8 Ant7 Ant6 Ant5 Ant4 Ant3 Ant2 Ant1 */
        /*   返回值:   设置成功返回0，失败返回负数                                         */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Set_Work_Antanne(byte ants);


        /***********************************************************************************/
        /**  函数名:   Get_Work_Antanne                                                    */
        /**  功能:     该函数用于获取模块工作天线                                          */
        /*  ants:   获取模块工作天线						*/
        /*   返回值:   设置成功返回0，失败返回负数                                         */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)] //2015-03-10-00
        public extern static int Get_Work_Antanne(ref byte ants);

        /***********************************************************************************/
        /**  函数名:   Set_gen2_param													   */
        /**  功能:     该函数用于设置Q算法参数                                             */
        /*  q_data:   设置Q算法:0 表示固定Q 算法，1 表示动态Q 算法						   */
        /*  startQ 设置：0 至15															   */
        /*  MinQ 设置：0 至15															   */
        /*  MaxQ 设置：0 至15															   */
        /*   返回值:   设置成功返回0，失败返回负数                                         */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Set_gen2_param(byte q_data, byte start_q, byte min_q, byte max_q,byte select_q,byte session_q,byte target_q);

        /***********************************************************************************/
        /**  函数名:   Get_gen2_param													   */
        /**  功能:     该函数用于获取Q算法参数                                             */
        /*  q_data:   设置Q算法:0 表示固定Q 算法，1 表示动态Q 算法						   */
        /*  startQ 设置：0 至15															   */
        /*  MinQ 设置：0 至15															   */
        /*  MaxQ 设置：0 至15															   */
        /*   返回值:   设置成功返回0，失败返回负数                                         */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Get_gen2_param(ref byte q_data, ref byte start_q, ref byte min_q, ref byte max_q,ref byte select_q, ref byte session_q, ref byte target_q);

        /***********************************************************************************/
        /**  函数名:   Set_frequency													   */
        /**  功能:     该函数用于设置读写器频率区域                                        */
        /*  region:   设置读写器频率区域						
			        China1		0x01
			        China2		0x02
			        Europe		0x03
			        USA			0x04
			        Korea		0x05
			        Japan		0x06													   */
        /*   返回值:   设置成功返回0，失败返回负数                                         */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Set_frequency(byte saving,byte region);

        /***********************************************************************************/
        /**  函数名:   Get_frequency													   */
        /**  功能:     该函数用于获取读写器频率区域                                        */
        /*  region:   获取读写器频率区域						
				        China1 0x01
				        China2 0x02
				        Europe 0x03
				        USA 0x04
				        Korea 0x05
				        Japan 0x06														   */
        /*   返回值:   设置成功返回0，失败返回负数                                         */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Get_frequency(ref byte region);

        
        /***********************************************************************************/ 
        /**  函数名:   Set_antenna_carrier												   */
        /**  功能:     该函数用于设置天线载波		                                       */
        /*  carrier:   设置天线载波		*/				
        /*   返回值:   设置成功返回0，失败返回负数                                         */                            
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Set_antenna_carrier(byte carrier);

        /***********************************************************************************/ 
        /**  函数名:   Get_antenna_carrier												   */
        /**  功能:     该函数用于无线载波			                                       */
        /*  carrier:   设置天线载波		*/				
        /*   返回值:   设置成功返回0，失败返回负数                                         */                            
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Get_antenna_carrier(ref byte carrier);


        /***********************************************************************************/ 
        /**  函数名:   Set_rf_link_profile												   */
        /**  功能:     该函数用于设置天线载波		                                       */
        /*  rf_link:   设置rf link profile		*/		
        /*   返回值:   设置成功返回0，失败返回负数                                         */                            
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Set_rf_link_profile(byte rf_link);

        /***********************************************************************************/ 
        /**  函数名:   Get_rf_link_profile												   */
        /**  功能:     该函数用于设置rf link profile                                       */
        /*  rf_link:   获取rf link profile		*/				
        /*   返回值:   获取成功返回0，失败返回负数                                         */                            
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Get_rf_link_profile(ref byte rf_link);

        /***********************************************************************************/ 
        /**  函数名:   Set_register_status												   */
        /**  功能:     该函数用于设置寄存器状态		                                       */
        /*  reg_type:   设置寄存器类型		*/		
        /*  addr:		设置寄存器地址		*/		
        /*  data:		设置寄存器数据		*/		
        /*   返回值:   设置成功返回0，失败返回负数                                         */                            
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Set_register_status(byte reg_type,uint addr,uint data);

        /***********************************************************************************/ 
        /**  函数名:   Get_register_status												   */
        /**  功能:     该函数用于获取寄存器状态		                                       */
        /*  reg_type:   寄存器类型			*/		
        /*  addr:		寄存器地址			*/		
        /*  data:		获取寄存器数据		*/						
        /*   返回值:   获取成功返回0，失败返回负数                                         */                            
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Get_register_status(byte reg_type,uint addr,ref uint data);

        /***********************************************************************************/
        /**  函数名:   Sigle_Query_Tags_Epc												   */
        /**  功能:     该函数用于单次读取标签EPC数据                                       */
        /*  epc:   模块返回标签的EPC数据						*/
        /*  tid:	返回标签的TID数据							*/
        /*  epc_len:   模块返回标签的EPC数据的长度				*/
        /*  tid_len:	返回标签的TID数据的长度					*/
        /*  rssi:   读取标签的信号强度值  实际值*10				*/
        /*  ant_id:   读取到标签的天线号						*/
        /*   返回值:   成功返回0，失败返回负数								               */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Sigle_Query_Tags_Epc(byte[] epc, ref byte epc_len, byte[] tid, ref byte tid_len, ref ushort rssi, ref byte ant_id);

        /***********************************************************************************/
        /**  函数名:   Multi_Query_Tags_Epc												   */
        /**  功能:     该函数用于多次读取标签EPC数据                                       */
        /*  count:   循环读标签次数 00 永久循环   100  循环一百次						   */
        /*  packnum:   返回EPC的总个数						*/
        /*  rev_num:   返回EPC的个数						*/
        /*  epc:   返回EPC的数据							*/
        /*  tid:   返回tid的数据							*/
        /*  rssi:   返回查询标签的信号强度	为实际值×10				*/
        /*  data_from_ant:   返回查询EPC的天线号						*/
        /*   返回值:   设置成功返回0，失败返回负数                                         */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Get_Multi_Query_Tags_Epc_Data(ref multi_query_epc_t multi_epc);

        /***********************************************************************************/
        /**  函数名:   Multi_Query_Tags_Epc												   */
        /**  功能:     该函数用于开始循环读标签											   */
        /**  times:		查询标签的次数	0 表示循环读标签								   */
        /*   返回值:   成功返回0，失败返回负数											   */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Multi_Query_Tags_Epc(UInt32 times);

        /***********************************************************************************/
        /**  函数名:   Stop_Multi_Query_Tags_Epc										   */
        /**  功能:     该函数用于停止循环读标签											   */
        /*   返回值:   成功返回0，失败返回负数											   */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Stop_Multi_Query_Tags_Epc();

        /***********************************************************************************/
        /**  函数名:   read_data														   */
        /**  功能:     该函数用于读标签数据                                                */
        /*  password:   读标签的密码							*/
        /*  filter_bak:   过滤类型								*/
        /*  filter_len:   过滤数据长度							*/
        /*  filter_data:   过滤数据								*/
        /*  memory_bank:   需要查询标签的bank					*/
        /*  start_addr:    需要查询的起始地址					*/
        /*  recv_len:    需要查询的长度							*/
        /*  recv_data:   返回查询标签的数据						*/
        /*  data_from_ant:   返回查询标签的天线号				*/
        /*   返回值:   成功返回0，失败返回负数                                             */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int read_data(uint password, byte filter_bak, ushort filter_len, byte[] filter_data, byte memory_bank, ushort start_addr, ushort recv_len, byte[] recv_data,ref byte data_from_ant);

        /***********************************************************************************/
        /**  函数名:   write_data														   */
        /**  功能:     该函数用于写标签数据												   */
        /*  password:   写标签数据的密码						*/
        /*  filter_bak:   写标签过滤类型						*/
        /*  filter_len:   写标签过滤数据长度					*/
        /*  filter_data:   过滤数据								*/
        /*  memory_bank:   需要写入标签的bank					*/
        /*  start_addr:    需要写入的起始地址					*/
        /*  data_len:   需要写入标签的长度						*/
        /*  write_buffer:   需要写入标签的数据					*/
        /*  data_from_ant:   返回写入标签的天线号				*/
        /*   返回值:   成功返回0，失败返回负数                                             */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int write_data(uint password, byte filter_bak, ushort filter_len, byte[] filter_data, byte memory_bank, ushort start_addr, ushort data_len, byte[] write_buffer, ref byte data_from_ant);

        /***********************************************************************************/
        /**  函数名:   lock_tag															   */
        /**  功能:     该函数用于锁定标签                                                  */
        /*  password:   锁定标签密码							*/
        /*  filter_bak:   锁定标签过滤类型						*/
        /*  filter_len:   锁定标签过滤数据长度					*/
        /*  filter_data:   锁定标签的过滤数据					*/
        /*	mask:	bit0:USER   bit1:TID    bit2:EPC    bit3:access   bit4:kill*/
        /*	action:	*/
        /*  写入口令 永久锁定 描述 */
        /*	0	0		在开放状态或保护状态下可以写入相关存储体。								  */
        /*	0	1		在开放状态或保护状态可以永久写入相关存储体，或者可以永远不锁定相关存储体。*/
        /*	1	0		在保护状态下可以写入相关存储体但在开放状态下不行						  */
        /*	1	1		在任何状态下都不可以写入相关存储体。									  */
        /*  data_from_ant:   返回锁定标签的天线号				*/
        /*   返回值:   成功返回0，失败返回负数                                             */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int lock_tag(uint password, byte filter_bak, ushort filter_len, byte[] filter_data, byte mask, byte action,ref byte data_from_ant);

        /***********************************************************************************/
        /**  函数名:   kill_tag															   */
        /**  功能:     该函数用于杀死标签												   */
        /*  password:   杀死标签的杀死密码						*/
        /*  filter_bak:   锁定标签过滤类型						*/
        /*  filter_len:   锁定标签过滤数据长度					*/
        /*  filter_data:   锁定标签的过滤数据					*/
        /*  data_from_ant:   返回锁定标签的天线号				*/
        /*   返回值:   成功返回0，失败返回负数                                             */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int kill_tag(uint password, byte filter_bak, ushort filter_len, byte[] filter_data, ref byte data_from_ant);

        /***********************************************************************************/
        /**  函数名:   Get_module_temperature											   */
        /**  功能:     该函数用于查询模块当前温度										   */
        /*  temperature:   返回模块的当前温度	温度×100，转换为十六进制后，负数则取补码  */
        /*   返回值:   成功返回0，失败返回负数                                         */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Get_module_temperature(ref ushort temperature);

        /***********************************************************************************/
        /**  函数名:   Set_multi_query_tags_interval								       */
        /**  功能:     该函数设置循环查询标签工作时间及间断时间							   */
        /*  work_time:   设置循环查询标签工作时间 （0000-FFFF）			*/
        /*  interval:   设置循环查询标签间断时间 （0000-FFFF）			*/
        /*   返回值:   成功返回0，失败返回负数                                             */
        /*  说明：只适用于单通道设备。当设置都为0时，表示循环不间断寻卡                    */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Set_multi_query_tags_interval(ushort work_time, ushort interval);

        /***********************************************************************************/
        /**  函数名:   Get_multi_query_tags_interval								       */
        /**  功能:     该函数获取循环查询标签工作时间及间断时间							   */
        /*  work_time:   获取循环查询标签工作时间 （0000-FFFF）			*/
        /*  interval:   获取循环查询标签间断时间 （0000-FFFF）			*/
        /*   返回值:   成功返回0，失败返回负数                                             */
        /*  说明：只适用于单通道设备。Time，单位为ms									   */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Get_multi_query_tags_interval(ref ushort work_time, ref ushort interval);


        /***********************************************************************************/
        /**  函数名:   Set_antenna_worktime_and_waittime								   */
        /**  功能:     该函数设置循环查询标签工作时间及间断时间							   */
        /*  ant1_work_time:   设置天线1工作时间 （单位ms，范围30ms—60000ms）			*/
        /*  ant2_work_time:   设置天线2工作时间 （单位ms，范围30ms—60000ms）			*/
        /*  ant3_work_time:   设置天线3工作时间 （单位ms，范围30ms—60000ms）			*/
        /*  ant4_work_time:   设置天线4工作时间 （单位ms，范围30ms—60000ms）			*/
        /*  wait_time:		  设置天线等待时间 （单位ms，范围0ms—60000ms）			    */
        /*   返回值:   成功返回0，失败返回负数				                               */
        /*  说明：只适用于多通道设备。									                   */
        /***********************************************************************************/ //2015-03-10－00
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Set_antenna_worktime_and_waittime(ushort ant1_work_time, ushort ant2_work_time, ushort ant3_work_time, ushort ant4_work_time, ushort wait_time);

        /***********************************************************************************/
        /**  函数名:   Get_antenna_worktime_and_waittime								   */
        /**  功能:     该函数获取天线工作时间及等待时间									   */
        /*  ant1_work_time:   获取天线1工作时间 （单位ms，范围30ms—60000ms）			*/
        /*  ant2_work_time:   获取天线2工作时间 （单位ms，范围30ms—60000ms）			*/
        /*  ant3_work_time:   获取天线3工作时间 （单位ms，范围30ms—60000ms）			*/
        /*  ant4_work_time:   获取天线4工作时间 （单位ms，范围30ms—60000ms）			*/
        /*  wait_time:		  获取天线等待时间 （单位ms，范围0ms—60000ms）			    */
        /*   返回值:   成功返回0，失败返回负数			                                   */
        /*  说明：只适用于多通道设备。									                   */
        /***********************************************************************************/ //2015-03-10-00
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Get_antenna_worktime_and_waittime(ref ushort ant1_work_time, ref ushort ant2_work_time, ref ushort ant3_work_time, ref ushort ant4_work_time, ref ushort wait_time);

        /***********************************************************************************/
        /**  函数名:   Set_fastid														   */
        /**  功能:     该函数用于设置fastid                                                */
        /*  fastid:   设置fastid， 开启FastID为0x01，关闭FastID为0x00。					   */
        /*   返回值:   成功返回0，失败返回负数											   */
        /*  说明：FastID只对特定品种的标签有效。										   */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Set_fastid(byte fastid);
        //-------------------------------------------------------------------
        /***********************************************************************************/
        /**  函数名:   Set_TagFocus														   */
        /**  功能:     该函数用于设置tagFocus                                              */
        /*  focus:     设置tagFocus， 开启 tagFocus为0x01，关闭tagFocus为0x00。			   */
        /*  返回值:   成功返回0，失败返回负数											   */
        /*  说明：    tagFocus只对特定品种的标签有效。									   */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Set_TagFocus(byte focus); //20150413

        //-------------------------------------------------------------------

        /***********************************************************************************/
        /**  函数名:   Get_fastid														   */
        /**  功能:     该函数用于获取fastid                                                */
        /*  fastid:   获取fastid，开启FastID为0x01，关闭FastID为0x00。					   */
        /*   返回值:   成功返回0，失败返回负数				                               */
        /*  说明：FastID只对特定品种的标签有效。										   */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Get_fastid(ref byte fastid);
        //-----------------------------------------------------------------------------
        /***********************************************************************************/
        /**  函数名:   Get_tagFocus														   */
        /**  功能:     该函数用于获取 tagFocus                                             */
        /*    focus:   获取tagFocus，开启 tagFocus为0x01，关闭tagfocus为0x00。			  */
        /*   返回值:   成功返回0，失败返回负数				                              */
        /*  说明：    tagFocus只对特定品种的标签有效。									*/
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Get_TagFocus(ref byte focus); //20150413
        //-----------------------------------------------------------------------------


        /***********************************************************************************/
        /**  函数名:   Set_module_baud_rate												   */
        /**  功能:     该函数用于设置模块通讯波特率                                        */
        /*  baud_rate:   设置模块通讯波特率，					*/
        /*  Data0=0，对应设置值9600，							*/
        /*	Data0=1，对应设置值19200，							*/
        /*	Data0=2，对应设置值38400，							*/
        /*	Data0=3，对应设置值57600，							*/
        /*	Data0=4，对应设置值115200，							*/
        /*	其他，非法值。										*/
        /*   返回值:  成功返回0，失败返回负数					                           */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Set_module_baud_rate(byte baud_rate);

        /***********************************************************************************/
        /**  函数名:   Set_qtparam														   */
        /**  功能:     该函数用于设置标签QT状态											   */
        /*  password:   设置标签QT的AP密码							*/
        /*  filter_bak:   设置标签QT过滤类型						*/
        /*  filter_len:   设置标签QT过滤数据长度					*/
        /*  filter_data:   设置标签QT的过滤数据						*/
        /*  qt_param:		设置QT标签的状态						*/
        /*  bit0=0 表示无近距离控制，bit0=1 表示启用近距离控制      */
        /*  bit1=0 表示标签启用Private Memory Map，bit1=1 表示标签使用PublicMemory Map*/
        /*  data_from_ant:  返回设置标签QT的天线号					*/
        /*   返回值:   成功返回0，失败返回负数                                             */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Set_qtparam(uint password, byte filter_bak, ushort filter_len, byte[] filter_data, byte qt_param);

        /***********************************************************************************/
        /**  函数名:   Get_qtparam														   */
        /**  功能:     该函数用于获取标签QT状态											   */
        /*  password:   获取标签QT的AP密码							*/
        /*  filter_bak:   获取标签QT过滤类型						*/
        /*  filter_len:   获取标签QT过滤数据长度					*/
        /*  filter_data:  获取标签QT的过滤数据						*/
        /*  qt_param:		返回标签QT的状态						*/
        /*  bit0=0 表示无近距离控制，bit0=1 表示启用近距离控制      */
        /*  bit1=0 表示标签启用Private Memory Map，bit1=1 表示标签使用PublicMemory Map*/
        /*  data_from_ant:   返回获取标签QT的天线号					*/
        /*   返回值:   成功返回0，失败返回负数                                             */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int Get_qtparam(uint password, byte filter_bak, ushort filter_len, byte[] filter_data, ref byte qt_param);

        
        /***********************************************************************************/ 
        /**  函数名:   Qtpek_operating													   */
        /**  功能:     该函数用于获取标签QT状态											   */
        /*  password:   获取标签QT的AP密码							*/
        /*  filter_bak:   获取标签QT过滤类型						*/
        /*  filter_len:   获取标签QT过滤数据长度					*/
        /*  filter_data:  获取标签QT的过滤数据						*/
        /*  qt_param:		返回标签QT的状态						*/
        /*  oper_type  0 表示QT之後無操縱，1 表示QT之後執行Read操縱,  2 標示QT之後執行Write操縱 */
        /*  close_status  bit0=0 表示无近距离控制，bit0=1 表示启用近距离控制      */
        /*  mem_bank:  操縱標籤的bank號								*/
        /*  addr:	操縱標籤的起始地址								*/
        /*  data_len:	操縱標籤的數據長度							*/
        /*  data:	操縱標籤的數據									*/
        /*   返回值:   成功返回0，失败返回负数                                             */                            
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int qtpek_operat(uint password, byte filter_bak, ushort filter_len, byte[] filter_data, byte oper_type,
							                 byte close_status,byte mem_bank,ushort addr,ushort data_len,byte[] data); 


        /***********************************************************************************/
        /**  函数名:   uart_trans_open													   */
        /**  功能:     该函数用于打开串口   											   */
        /*  name:   串口名称(全部路径)							*/
        /*  com_baudrate:   串口波特率							*/
        /*   返回值:   成功返回0，失败返回负数                                             */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int uart_trans_open(byte[] name, int com_baudrate);

        /***********************************************************************************/
        /**  函数名:   uart_trans_close													   */
        /**  功能:     该函数用于关闭串口   											   */
        /*   返回值:   NULL					                                               */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static void uart_trans_close();

        /***********************************************************************************/
        /**  函数名:   uart_trans_send													   */
        /**  功能:     该函数用于串口发送数据   										   */
        /*  send_buffer:   发送的数据							*/
        /*  data_len:   数据长度								*/
        /*   返回值:   成功返回0，失败返回负数                                             */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int uart_trans_send(byte[] send_buffer, uint data_len);

        /***********************************************************************************/
        /**  函数名:   uart_trans_recv													   */
        /**  功能:     该函数用于串口发送数据   										   */
        /*  send_buffer:   发送的数据							*/
        /*  date_len:   接收的数据长度								*/
        /*   返回值:   成功返回0，失败返回负数                                             */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int uart_trans_recv(byte[] recv_buffer, ref uint date_len);

        /***********************************************************************************/
        /**  函数名:   uart_trans_recv													   */
        /**  功能:     该函数用于网口的初始化   										   */
        /*   返回值:   NULL                                             */
        /***********************************************************************************/
        //[DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        //public extern static void net_trans_init();

        /***********************************************************************************/
        /**  函数名:   uart_trans_recv													   */
        /**  功能:     该函数用于网口的初始化   										   */
        /*  strIP:   IP地址							*/
        /*  ipAddr:   发送的数据							*/
        /*  nLocalPort:   本地端口							*/
        /*  nPeerPort:   pc机为服务器，设备为客户端   设置端口					*/
        /*				pc机为客户端，设备为服务器	端口设置为0					*/
        /*  nWorkMode:   发送的数据							*/
        /*   返回值:   成功返回0，失败返回负数                                             */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int net_trans_open(ref char[] strIP,ref char[] ipAddr, uint nLocalPort, uint nPeerPort, byte nWorkMode);

        /***********************************************************************************/
        /**  函数名:   net_trans_close													   */
        /**  功能:     该函数用于网口通讯关闭   										   */
        /*   返回值:   NULL																   */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static void net_trans_close();

        /***********************************************************************************/  //2015-03-10-22
        /**  函数名:   net_trans_send													   */
        /**  功能:     该函数用于网口发送数据   										   */
        /*  send_buffer:   发送数据									*/
        /*  data_len:   数据长度									*/
        /*   返回值:   成功返回0，失败返回负数											   */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int net_trans_send(byte[] send_buffer, uint data_len);

        /***********************************************************************************/
        /**  函数名:   net_trans_send_set												   */
        /**  功能:     该函数用于网口通讯发送设置  										   */
        /*   返回值:   NULL																   */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static void net_trans_send_set();

        /***********************************************************************************/
        /**  函数名:   net_trans_recv													   */
        /**  功能:     该函数用于网口接收数据	  										   */
        /*  send_buffer:   发送数据											*/
        /*  data_len:   接收数据数据长度									*/
        /*   返回值:   成功返回0，失败返回负数											   */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static int net_trans_recv(byte[] recv_buffer, uint[] data_len);

        /***********************************************************************************/
        /**  函数名:   net_trans_recv_set												   */
        /**  功能:     该函数用于网口通讯接收设置  										   */
        /*   返回值:   NULL																   */
        /***********************************************************************************/
        [DllImport("api_dll.dll", CharSet = CharSet.Ansi)]
        public extern static void net_trans_recv_set();


        /***********************************************************************************/
        /**  函数名:   basic_init												           */
        /**  功能:     该函数用于通讯初始化     										   */
        /**  itype:    101   CONNECT_SERIAL   102   CONNECT_NET                            */
        /*   返回值:   NULL																   */
        /***********************************************************************************/
        [DllImport("api_dll.dll")]
        public extern static int basic_init(int itype);

    }
}

