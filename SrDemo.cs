using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;             //thread
using System.IO.Ports;              //SerialPort
using System.Text.RegularExpressions;
using System.Net;


namespace SrDemo
{


    public partial class SrDemo : Form
    {
        public delegate void MyInvoke(multi_query_epc_t tags);

        private const int OPER_OK = 0;          // 表示sr api函数是否成功执行
        private int m_connect_type = transfer.CONNECT_SERIAL;
        private bool m_bConnect = false;
        private const int READ_FLAG = 1;
        private const int WRITE_FLAG = 2;

        private volatile List<tag> tags_list = new List<tag>(1000);

        private int tags_count_persecond = 0;


        // 标签显示项
        private const   int listView_label_Num = 0;
        private const   int listView_label_AntID = 1;
        private const   int listView_label_EPC = 2;
        private const   int listView_label_TID = 3;
        private const   int listView_label_PC = 4;
        private const   int listView_label_RSSI = 5;
        private const   int listView_label_Count = 6;
        private const   int listView_label_Last_Time = 7;


        // 网络模块显示项
        private const int listView_net_Num = 0;
        private const int listView_net_MAC = 1;
        private const int listView_net_IP  = 2;
        

        public SrDemo()
        {
            InitializeComponent();

            for (int i = 1; i < 50; i++)
            {
                cbCOM.Items.Add("COM" + i);
            }
            cbCOM.Items.Add("COM254");
            cbCOM.Items.Add("COM255");
            cbCOM.SelectedIndex = 0;
            cbBaute.SelectedIndex = 4;          // 115200
            rbCOM.Checked = true;               // 默认使用串口通信
            // disable
            textBox_connect_ip.Enabled = false;
            textBox_connect_port.Enabled = false;

            //操作 log
            this.listView_oper_log.Columns.Add("Num", 50, HorizontalAlignment.Left);//序号
            this.listView_oper_log.Columns.Add("Time", 150, HorizontalAlignment.Left);//时间
            this.listView_oper_log.Columns.Add("Operation Result", 450, HorizontalAlignment.Left);//执行结果
            this.listView_oper_log.GridLines = true;
            this.listView_oper_log.FullRowSelect = true;
            this.listView_oper_log.MultiSelect = false;

            // region
            comboBox_region.SelectedIndex = 1;          // china2

            // set baud rate
            comboBox_set_baudrate.SelectedIndex = 4;    // 115200

            // ant
    //        checkBox_ant1.Checked = true;               // 默认ant1被选中  //2015-03-10-00


            /*标签数据
            listView_label_Num = 0;
            listView_label_AntID = 1;
            listView_label_EPC = 2;
            listView_label_TID = 3;
            listView_label_PC = 4;
            listView_label_RSSI = 5;
            listView_label_Count = 6;
            listView_label_Last_Time = 7;
             */
            this.listView_label.Columns.Add("Num", 30, HorizontalAlignment.Left);
            this.listView_label.Columns.Add("AntID", 30, HorizontalAlignment.Left);
            this.listView_label.Columns.Add("EPC", 250, HorizontalAlignment.Left);
            this.listView_label.Columns.Add("TID", 80, HorizontalAlignment.Left);
            this.listView_label.Columns.Add("PC", 60, HorizontalAlignment.Left);
            this.listView_label.Columns.Add("RSSI", 60, HorizontalAlignment.Left);
            this.listView_label.Columns.Add("Count", 50, HorizontalAlignment.Left);
            this.listView_label.Columns.Add("Last Time", 130, HorizontalAlignment.Left);
            this.listView_label.GridLines = true;
            this.listView_label.FullRowSelect = true;
            this.listView_label.MultiSelect = false;

            // memory bank
            comboBox_mb.SelectedIndex = 1;      // epc

            // gen_param
            q_data.SelectedIndex = 1;
            start_q.SelectedIndex = 4;
            min_q.SelectedIndex = 0;
            max_q.SelectedIndex = 15;

            // RF LINK PROFILE
            rf_link_profile_index.SelectedIndex = 1;

       

            // filter
            filterbox.SelectedIndex = 0;

            // 网络模块
            this.listView_net.Columns.Add("Num", 30, HorizontalAlignment.Left);
            this.listView_net.Columns.Add("MAC", 200, HorizontalAlignment.Left);
            this.listView_net.Columns.Add("IP", 150, HorizontalAlignment.Left);
            this.listView_net.GridLines = true;
            this.listView_net.FullRowSelect = true;
            this.listView_net.MultiSelect = false;


            disable();


        }

        private string add_zero(string str)
        {
            string str_buf;
            int temp = 20 - (str.Length);
            if (temp != 20)
            {
                for (int i = 0; i < temp; i++)
                {
                    str = str + "\0";
                }
            }
            str_buf = str;
            return str_buf;
        }

        private void UpdataNet(net_device_t param)
        {
            listView_net.Items.Clear();

            for (int index = 0; index < param.device_num; index++ )
            {
                //转换成string
                string str_Num = "";
                string str_MAC = new string(param.net_device[index].device_info.strMAC);
                string str_IP = new string(param.net_device[index].device_info.strIP);

                str_Num = (this.listView_net.Items.Count + 1).ToString();
                ListViewItem item = new ListViewItem(str_Num);

                //label60.Text = (search_ip.Length).ToString();

                
                        item.SubItems.Add(str_MAC);
                        item.SubItems.Add(str_IP);
                        this.listView_net.Items.Add(item);
                        this.listView_net.Items[this.listView_net.Items.Count - 1].EnsureVisible();
                        this.listView_net.Items[this.listView_net.Items.Count - 1].Selected = true;
            }
        }



        private void set_count()
        {
            lock((object)tags_count_persecond)
            {
                ++tags_count_persecond;
            }
            
        }

        private void reset_count()
        {
            lock ((object)tags_count_persecond)
            {
                tags_count_persecond = 0;
            }
        }

        private int get_count()
        {
            return tags_count_persecond;
        }

        // 清空列表
        private void tags_list_init()
        {
            tags_list.Clear();
        }


        // 遍历列表，epc已存在，返回其在列表中的编号。不存在返回-1
        private int tags_list_traverse(string epc)
        {
            for (int index = 0; index < tags_list.Count; index++ )
            {
                if (tags_list[index].epc == epc)
                {
                    return index;
                }
            }

            return -1;
        }

        // 添加对象到列表中
        private void tags_list_add(tag item)
        {
            tags_list.Add(item);
        }


        private string epc_format(byte[] epc, char epc_len)
        {
            string str_epc = "";
            for (int index = 0; index < epc_len; index++)
            {
                str_epc += epc[index].ToString("X2");
                if (index < epc_len - 1)
                {
                    str_epc += "-";
                }
            }
            return str_epc;
        }

        private string tid_format(byte[] tid, int tid_len)
        {
            string str_tid = "";
            for (int index = 0; index < tid_len; index++)
            {
                str_tid += tid[index].ToString("X2");
                if (index < tid_len - 1)
                {
                    str_tid += "-";
                }
            }
            return str_tid;
        }


        private double rssi_calculate(byte rssi_msb, byte rssi_lsb)
        {
            ushort temp_rssi = (ushort)(((ushort)rssi_msb << 8) + (ushort)rssi_lsb);
            double sh_rssi = (double)(short)temp_rssi / 10;

            return sh_rssi;
        }


        private ushort pc_calculate(byte pc_msb, byte pc_lsb)
        {
            ushort temp_pc = (ushort)((((ushort)pc_msb) << 8) + (ushort)pc_lsb);

            return temp_pc;
        }

        // 单次寻标签
        private void single_analyze_data(query_epc_t tag)
        {
            string temp_epc = epc_format(tag.epc.epc, tag.epc.epc_len);
            // 判断当前标签是会否存在列表中
            int offset = 0;
            if (-1 == (offset = tags_list_traverse(temp_epc)))
            {
                tag temp_tag = new tag();

                ++temp_tag.count;

                temp_tag.ant_id = tag.ant_id;
                temp_tag.pc = pc_calculate(tag.epc.pc_msb, tag.epc.pc_lsb);
                temp_tag.rssi = rssi_calculate(tag.rssi_msb, tag.rssi_lsb);
                temp_tag.epc = temp_epc;
                temp_tag.tid = tid_format(tag.tid, tag.tid_len);
                temp_tag.latest_time = string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);

                tags_list_add(temp_tag);
            }
            else        // 标签已存在，增加count 即可
            {
                tags_list[offset].count++;
                tags_list[offset].pc = pc_calculate(tag.epc.pc_msb, tag.epc.pc_lsb);
                tags_list[offset].rssi = rssi_calculate(tag.rssi_msb, tag.rssi_lsb);
                tags_list[offset].ant_id = tag.ant_id;
                tags_list[offset].latest_time = string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
            }
        }
        


        // 循环寻标签
        private void multi_analyze_data(multi_query_epc_t tags)
        {
            for (int index = 0; index < tags.packet_num; index++ )
            {
                if ((tags.tags_epc[index].epc.epc_len > 124) || (tags.tags_epc[index].epc.epc_len <= 0))
                {
                    continue;
                }
                    
                set_count();

                string temp_epc = epc_format(tags.tags_epc[index].epc.epc,tags.tags_epc[index].epc.epc_len);
                string temp_tid = tid_format(tags.tags_epc[index].tid, tags.tags_epc[index].tid_len);
                // 判断当前标签是会否存在列表中
                int offset = 0;
                if (-1 == (offset = tags_list_traverse(temp_epc)))
                {
                    tag temp_tag = new tag();

                    temp_tag.count  = 1;
                    temp_tag.ant_id = tags.tags_epc[index].ant_id;
                    temp_tag.pc     = pc_calculate(tags.tags_epc[index].epc.pc_msb, tags.tags_epc[index].epc.pc_lsb);
                    temp_tag.rssi   = rssi_calculate(tags.tags_epc[index].rssi_msb, tags.tags_epc[index].rssi_lsb);
                    temp_tag.epc    = temp_epc;
                    temp_tag.tid    = temp_tid;
                    temp_tag.latest_time = string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);

                    // 写保护
          //          lock (tags_list)
                    {
                        tags_list_add(temp_tag);
                    }
                    
                }
                else        // 标签已存在，增加count 即可
                {
                    // 写保护
                    {
                        tags_list[offset].count = tags_list[offset].count + 1;
                        tags_list[offset].pc = pc_calculate(tags.tags_epc[index].epc.pc_msb, tags.tags_epc[index].epc.pc_lsb);
                        tags_list[offset].rssi = rssi_calculate(tags.tags_epc[index].rssi_msb, tags.tags_epc[index].rssi_lsb);
                        tags_list[offset].ant_id = tags.tags_epc[index].ant_id;
                        tags_list[offset].latest_time = string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
                    }
                }
            }
        }



        private void UpdataLabel(query_epc_t tag)
        {
            switch (tag.ant_id)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                    single_analyze_data(tag);
                    updata_tags_listview();
                    break;

                default:
                    break;
            }
        }


        private void UpdataLabel(tag tag_item)
        {
            //转换成string

            string str_pc       = tag_item.pc.ToString("X2");
            string str_epc      = tag_item.epc;
            string str_tid      = tag_item.tid;
            string str_read_cnt = tag_item.count.ToString();
            string str_ant_id   = tag_item.ant_id.ToString();
            string str_rssi     = tag_item.rssi.ToString("f1");
            string str_time     = tag_item.latest_time;

            //       AddTagToBuf(tag);

            bool Exist = false;
            //判断标签是否被重复扫描
            foreach (ListViewItem viewitem in this.listView_label.Items)
            {
                if (viewitem.SubItems[listView_label_EPC].Text == str_epc)
                {
                    viewitem.SubItems[listView_label_AntID].Text    = str_ant_id;
                    viewitem.SubItems[listView_label_PC].Text       = str_pc;
                    viewitem.SubItems[listView_label_RSSI].Text     = str_rssi;
                    viewitem.SubItems[listView_label_Count].Text    = str_read_cnt;
                    viewitem.SubItems[listView_label_Last_Time].Text = str_time;
                    Exist = true;
                    break;
                }
            }

            if (!Exist)
            {
                ListViewItem item = new ListViewItem((this.listView_label.Items.Count + 1).ToString());
                item.SubItems.Add(str_ant_id);
                item.SubItems.Add(str_epc);
                item.SubItems.Add(str_tid);
                item.SubItems.Add(str_pc);     
                item.SubItems.Add(str_rssi);
                item.SubItems.Add(str_read_cnt);
                item.SubItems.Add(str_time);
                this.listView_label.Items.Add(item);
                this.listView_label.Items[this.listView_label.Items.Count - 1].EnsureVisible();
                this.listView_label.Items[this.listView_label.Items.Count - 1].Selected = true;

                //            multi_get_tag_count_add();
            }
        }



        private void updata_tags_listview()
        {
            List<tag> temp_tags_list = new List<tag>(1000);

  //          lock (tags_list)
            {
                temp_tags_list = tags_list;
            }
            
            for (int index = 0; index < temp_tags_list.Count; index++ )
            {
                UpdataLabel(temp_tags_list[index]);
            }
        }

        /* * */

         



       


        private void UpdateLog(string strLog)
        {
            string strDateTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
            ListViewItem item = new ListViewItem((this.listView_oper_log.Items.Count + 1).ToString());
            item.SubItems.Add(strDateTime);
            item.SubItems.Add(strLog);
            this.listView_oper_log.Items.Add(item);
            this.listView_oper_log.Items[this.listView_oper_log.Items.Count - 1].EnsureVisible();
            this.listView_oper_log.Items[this.listView_oper_log.Items.Count - 1].Selected = true;
        }


        private void    set_connect_type(int connect_type)
        {
            m_connect_type = connect_type;
        }

        private int get_connect_type()
        {
            return m_connect_type;
        }


        private void    get_version()
        {
            int ret = 0;
            byte[] hard_ware = new byte[global.PACKET_128];
            ret = sr_api.Get_hardware_version(hard_ware); // 获取硬件版本号
            string hard_str;
            if (OPER_OK == ret)
            {
                string hard_ver;
                hard_ver = System.Text.Encoding.Default.GetString(hard_ware);

                label_hardware_version.Text = "Hardware:" + hard_ver; //Hardware:硬件版本:

                hard_str = "Get hardware success.";//";获取硬件版本成功！
            }
            else
            {
                hard_str = "！Get hardware fail.";//Get hardware fail.";获取硬件版本失败
            }
            UpdateLog(hard_str);

            byte[] firm_ware = new byte[global.PACKET_128];
            ret = sr_api.Get_firmware_version(firm_ware); // 获取固件版本号
            string firm_str;
            if (OPER_OK == ret)
            {
                string firm_ver;
                firm_ver = System.Text.Encoding.Default.GetString(firm_ware);

                label_firmware_version.Text = "Firmware " + firm_ver; //Firmware:固件版本:

                firm_str = "Get Firmware success.";//Get Firmware success.";获取固件版本成功!
            }
            else
            {
                firm_str = "Get Firmware fail.";//Get Firmware fail."获取固件版本失败!;
            }
            UpdateLog(firm_str);
        }



        private int cur_pitch = 0;
        private net_device_t net_device = new net_device_t();

        private void get_device_ip()
        {
            device_t param = new device_t();
            param.device_info.strIP = textBox_connect_ip.Text.PadRight(20, '\0').ToCharArray();
            param.net_setting.ipAddr = textBox_connect_ip.Text.PadRight(20, '\0').ToCharArray();

            param.socket_setting.nLocalPort = int.Parse(textBox_connect_port.Text);

            net_device.net_device[cur_pitch] = param;
        //    return param;
        }


        private void disable()
        {
            radioButton_module_is_server.Enabled = false;
            radioButton_module_is_client.Enabled = false;
        }


        private void enable()
        {
            radioButton_module_is_server.Enabled = true;
            radioButton_module_is_client.Enabled = true;
        }



        private void btnConnect_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {
                int connect_type = get_connect_type();
                int ret = 0;

                switch (connect_type)
                {
                    case transfer.CONNECT_SERIAL:
                        ret = sr_api.basic_init(transfer.CONNECT_SERIAL);   //初始化为网络通信模式
                        if (m_bConnect)
                        {
                            try
                            {
                                sr_api.uart_trans_close();
                                UpdateLog("Close Com successful");//Close Com successful");关闭串口成功！
                            }
                            catch
                            {
                                UpdateLog("Close Com fail");//Close Com fail");关闭 串口失败！
                            }
                            
                        }
                        else
                        {
                            uart_open_t _open = new uart_open_t(cbCOM.Text, int.Parse(cbBaute.Text));

                            //MessageBox.Show("COM = " + cbCOM.Text + " 波特率 = " + cbBaute.Text, " 信息提示");

                            ret = transfer.transfer_open(ref _open);
                            
                            /*byte[] com_name = System.Text.Encoding.Default.GetBytes(cbCOM.Text);
                            ret = sr_api.uart_trans_open(com_name, int.Parse(cbBaute.Text));*/

                            if (OPER_OK == ret)
                            {
                                UpdateLog("Open Com successfu");//Open Com successful");打开串口成功！

                                get_version();
                            }
                            else
                            {
                                UpdateLog("Open Com fail");//Open Com fail");打开串口失败！
                            }
                        }
                        break;

                    case transfer.CONNECT_NET:
                        ret = sr_api.basic_init(transfer.CONNECT_NET);   //初始化为网络通信模式

                        if (m_bConnect)
                        {
                            try
                            {
                                sr_api.net_trans_close();
                                UpdateLog("Close net successful");//Close net successful");关闭网口成功！
                            }
                            catch
                            {
                                UpdateLog("Close net fail");//Close net fail");关闭网口失败！
                            }
                          
                        }
                        else
                        {
                          //  get_device_ip();
                            //device_t param = new device_t();
                            // ip

/*
                            char[] strIP = textBox_connect_ip.Text.PadRight(20, '\0').ToCharArray();
                            char[] ipAddr = textBox_connect_ip.Text.PadRight(20, '\0').ToCharArray();
                            byte nWorkMode = 0;
                            uint nPeerPort = 0;
                            // port
                            uint nLocalPort = uint.Parse(textBox_connect_port.Text);

                            // server or client
                            if (radioButton_module_is_client.Checked == true)
                            {   // pc机为服务器，设备为客户端
   //                             param.socket_setting.szPeerName = textBox_server_ip.Text.PadRight(64, '\0').ToCharArray();
                                nPeerPort = uint.Parse(textBox_connect_port.Text);
                                // 设置c2000模块 nWorkMode; 
                                //0：TCP 客户端；1：TCP 服务器；2：UDP1 或自动；
                                //3：UDP2；4：自动，根据具体的产品而定
                                nWorkMode = 0;
                            }
                            else if (radioButton_module_is_server.Checked == true)
                            {   // pc机为客户端，设备为服务器

                                // 设置c2000模块 nWorkMode; 
                                //0：TCP 客户端；1：TCP 服务器；2：UDP1 或自动；
                                //3：UDP2；4：自动，根据具体的产品而定
                                nWorkMode = 1;
                            }

                            ret = sr_api.net_trans_open(ref strIP, ref ipAddr, nLocalPort, nPeerPort, nWorkMode);
*/
                            device_t param = new device_t();
                            // ip
                            param.device_info.strIP = textBox_connect_ip.Text.PadRight(20, '\0').ToCharArray();
                            param.net_setting.ipAddr = textBox_connect_ip.Text.PadRight(20, '\0').ToCharArray();

                            // port
                            param.socket_setting.nLocalPort = int.Parse(textBox_connect_port.Text);

                            // server or client
                            if (radioButton_module_is_client.Checked == true)
                            {   // pc机为服务器，设备为客户端
   //                             param.socket_setting.szPeerName = textBox_server_ip.Text.PadRight(64, '\0').ToCharArray();
                                param.socket_setting.nPeerPort = int.Parse(textBox_connect_port.Text);
                                // 设置c2000模块 nWorkMode; 
                                //0：TCP 客户端；1：TCP 服务器；2：UDP1 或自动；
                                //3：UDP2；4：自动，根据具体的产品而定
                                param.socket_setting.nWorkMode = 0;
                            }
                            else if (radioButton_module_is_server.Checked == true)
                            {   // pc机为客户端，设备为服务器

                                // 设置c2000模块 nWorkMode; 
                                //0：TCP 客户端；1：TCP 服务器；2：UDP1 或自动；
                                //3：UDP2；4：自动，根据具体的产品而定
                                param.socket_setting.nWorkMode = 1;
                            }
                            ret = transfer.transfer_open(ref param);
                            if (OPER_OK == ret)
                            {
                                UpdateLog("Open net successful");//Open net successful");打开网口成功！
                                transfer.transfer_recv_set();

                                get_version();
                            }
                            else
                            {
                                UpdateLog("Open net fail");//Open net fail");打开网口失败！
                            }
                        }
                        break;

                    default:
                        break;
                }

                if (OPER_OK == ret)
                {
                    m_bConnect = !m_bConnect;
                    /*
                    if (btnConnect.Text.Equals("打开"))
                    {
                        if (Read_RegistryKey())//2015-03-12-00
                        {
                            startOK = true;
                            this.button_query.Text = MULTI_START_LABEL;
                            this.button_query.Refresh();//
                            int i = 0;
                                _multi_stop();//2015-03-12-00
                            this.timer_auto.Enabled = true;
                        }
                    }
                    */

                    btnConnect.Text = m_bConnect ? "Close" : "Connect"; //"Close关闭" : 打开"";
                    rbCOM.Enabled = rbCOM.Checked || !m_bConnect;
                    rbNet.Enabled = rbNet.Checked || !m_bConnect;

                }
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }    
        }

        private void rbCOM_Click(object sender, EventArgs e)
        {
            rbNet.Checked = false;
            set_connect_type(transfer.CONNECT_SERIAL);

            //enable
            cbCOM.Enabled = true;
            cbBaute.Enabled = true;

            // disable
            textBox_connect_ip.Enabled = false;
            textBox_connect_port.Enabled = false;
            disable();
        }

        private void rbNet_Click(object sender, EventArgs e)
        {
            rbCOM.Checked = false;
            set_connect_type(transfer.CONNECT_NET);
            //sr_api.basic_init(transfer.CONNECT_NET);   //初始化为网络通信模式

            // enable
            textBox_connect_ip.Enabled = true;
            textBox_connect_port.Enabled = true;
            enable();

            // disable
            cbCOM.Enabled = false;
            cbBaute.Enabled = false;
        }




        private void button_power_get_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {
                int ret = 0;
                byte loop = 0x00;  //默认为开闭环
                byte read = 0x00;
                byte write = 0x00;

                ret = sr_api.Get_Power(ref loop,ref read,ref write);

                if (OPER_OK == ret)//Get Power success.";
                {
                    comboBox_read_power.Text = read.ToString();
                    comboBox_write_power.Text = write.ToString();

                    oper_result = "Set Power success.";//获取功率成功！
                }
                else //Get Power fail.";
                {
                    oper_result = "Set Power fail.";//获取功率失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }
        }

        private void button_power_set_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {
                int ret = 0;
                byte loop = 0x00;  //默认为开闭环
                byte read = byte.Parse(comboBox_read_power.Text);
                byte write = byte.Parse(comboBox_write_power.Text);

                ret = sr_api.Set_Power(loop,read,write); // 设置读写功率

                if (OPER_OK == ret)
                {
                    oper_result = "Set Power success.";//";设置功率成功！
                }
                else
                {
                    oper_result = "Set Power fail.";//";设置功率失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }    
        }

        private void button1_get_temperature_Click(object sender, EventArgs e)
        {

            string oper_result;
            try
            {
                int ret = 0;
                ushort module_temp = 0;
                ret = sr_api.Get_module_temperature(ref module_temp);

                if (OPER_OK == ret)
                {
                    int cur_temp = module_temp;
                    double temper = (double)cur_temp / 100;

                    label_temperature.Text = temper.ToString();
                    oper_result = "Get Temperature success.";//"获取温度成功！;
                }
                else
                {
                    oper_result = "Get Temperature fail.";//";获取温度失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }
        }

        private void button_get_fastid_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {
                int ret = 0;
                byte fastid_switch = 0x0;

                ret = sr_api.Get_fastid(ref fastid_switch); // 获取硬件版本号

                if (OPER_OK == ret)
                {
                    switch(fastid_switch)
                    {
                        case global.FASTID_ON:
                            radioButton_fastid_open.Checked = true;
                            radioButton_fastid_close.Checked = false;
                            break;

                        case global.FASTID_OFF:
                            radioButton_fastid_open.Checked = false;
                            radioButton_fastid_close.Checked = true;
                            break;

                        default:
                            oper_result = "Data Error.";
                            break;
                    }

                    oper_result = "Get  FastID success.";//Get  FastID success.";获取 FastID  状态成功！
                }
                else
                {
                    oper_result = "Get  FastID fail.";//";获取 FastID  状态失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }
        }


        private void button_set_gen2_fastid_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {
                int ret = 0;

                byte fastid_switch = 0x0;

                if (true == radioButton_fastid_open.Checked)
                {
                    fastid_switch = 0x01; // 开启
                }
                else
                {
                    fastid_switch = 0x00; //关闭
                }

                ret = sr_api.Set_fastid(fastid_switch); // 设置FASTID

                if (OPER_OK == ret)
                {
                    oper_result = "Set  FastID success.";//";设置 FastID 成功！
                }
                else
                {
                    oper_result = "Set  FastID fail.";//";设置 FastID 失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }
        }



        private void button_gpio_get_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {
                int ret = 0;

                byte gpio = 0x7;
                byte gpio_level = 0x0;
                ret = sr_api.Get_Gpio_level(gpio, ref gpio_level); // 获取硬件版本号

                if (OPER_OK == ret)
                {
                    bool gpio1_high = false;
                    bool gpio1_low = false;
                    bool gpio2_high = false;
                    bool gpio2_low = false;
                    bool gpio3_high = false;
                    bool gpio3_low = false;

                    if (0 != (gpio & 0x01))    // 选中gpio1
                    {
                        if (0 != (gpio_level & 0x01)) // 高电平
                        {
                            gpio1_high = true;
                            gpio1_low = false;
                        }
                        else            // 低电平
                        {
                            gpio1_high = false;
                            gpio1_low = true;
                        }
                    }

                    if (0 != (gpio & 0x02))    // 选中gpio2
                    {
                        if (0 != (gpio_level & 0x02)) // 高电平
                        {
                            gpio2_high = true;
                            gpio2_low = false;
                        }
                        else            // 低电平
                        {
                            gpio2_high = false;
                            gpio2_low = true;
                        }
                    }

                    if (0 != (gpio & 0x04))    // 选中gpio3
                    {
                        if (0 != (gpio_level & 0x04)) // 高电平
                        {
                            gpio3_high = true;
                            gpio3_low = false;
                        }
                        else            // 低电平
                        {
                            gpio3_high = false;
                            gpio3_low = true;
                        }
                    }

                    radioButton_gpio1_high.Checked = gpio1_high;
                    radioButton_gpio1_low.Checked = gpio1_low;

                    radioButton_gpio2_high.Checked = gpio2_high;
                    radioButton_gpio2_low.Checked = gpio2_low;

                    radioButton_gpio3_high.Checked = gpio3_high;
                    radioButton_gpio3_low.Checked = gpio3_low;

                    oper_result = "Get gpio level success.";//Get gpio level success.";获取 GPIO 状态成功！
                }
                else
                {
                    oper_result = "Get gpio level fail";//Get gpio level fail.";获取 GPIO 状态失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }
        }

        private void button_gpio_set_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {
                int ret = 0;

                byte gpio = 0x7;
                byte gpio_level = 0x0;
                if (true == radioButton_gpio1_high.Checked)    // 设置gpio1为高电平
                {
                    gpio_level |= 0x01;
                }

                if (true == radioButton_gpio2_high.Checked)    // 选中gpio2
                {
                    gpio_level |= 0x02;
                }

                if (true == radioButton_gpio3_high.Checked)    // 选中gpio3
                {
                    gpio_level |= 0x04;
                }

                ret = sr_api.Set_Gpio_level(gpio,gpio_level);

                if (OPER_OK == ret)
                {
                    oper_result = "Set gpio level success.";//";设置 GPIO 状态成功！
                }
                else
                {
                    oper_result = "Set gpio level fail.";//";设置 GPIO 状态失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }
        }

        private void button_get_region_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {
                int ret = 0;
                byte fequency_region = 0x0;

                ret = sr_api.Get_frequency(ref fequency_region); // 设置区域

                if (OPER_OK == ret)
                {
                    switch (fequency_region)
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                            comboBox_region.SelectedIndex = fequency_region - 1;
                            break;
                        default:
                            break;
                    }

                    oper_result = "Get Frequency Region success.";//";获取设置频率范围成功！
                }
                else
                {
                    oper_result = "Get Frequency Region fail.";//Get Frequency Region fail.";获取设置频率范围失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }
        }

        private void button_set_region_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {
                int ret = 0;
                byte fequency_region = 0x0;
                byte save_setting = 0x0;

                switch ((comboBox_region.SelectedIndex + 1))
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                        fequency_region = (byte)(comboBox_region.SelectedIndex + 1);
                        break;
                    default:
                        break;
                }

                if (true == checkBox_region_save.Checked)
                {
                    save_setting = 0x01;
                }
                else
                {
                    save_setting = 0x00;
                }

                ret = sr_api.Set_frequency(save_setting, fequency_region); // 获取频率区域

                if (OPER_OK == ret)
                {
                    oper_result = "Set Frequency Region success.";//Set Frequency Region success.";设置频率范围成功！
                }
                else
                {
                    oper_result = "Set Frequency Region fail.";//Set Frequency Region fail.";设置频率范围失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }
        }

        private void button_create_fp_Click(object sender, EventArgs e)
        {
            float fp_min = float.Parse(textBox_fp_min.Text);
            float fp_max = float.Parse(textBox_fp_max.Text);
            float fp_int = float.Parse(textBox_fp_interval.Text);

            if (fp_int > 0.0001)
            {
                if (fp_min < fp_max)
                {
                    float temp;
                    string fp_str = "";

                    temp = fp_min;
                    while (temp <= fp_max)
                    {
                        fp_str += temp.ToString() + ", ";
                        temp += fp_int;
                    }

                    richTextBox_frequency_point.Text = fp_str;
                }
            }
        }

        private void button_fp_example_Click(object sender, EventArgs e)
        {
            textBox_fp_min.Text = "920.000";
            textBox_fp_max.Text = "925.000";
            textBox_fp_interval.Text = "0.250";

            button_create_fp_Click(sender, e);
        }

        private void button_fp_clear_Click(object sender, EventArgs e)
        {
            richTextBox_frequency_point.Text = "";
        }

        private void button_get_fp_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {
                int ret = 0;
                byte frequency_num = 0;
                int[] test2 = new int[global.OUTPUT_FREQUENCY_NUM];

                float[] fre_value = new float[global.OUTPUT_FREQUENCY_NUM];
                int[] frequency = new int[global.OUTPUT_FREQUENCY_NUM];

                ret = sr_api.Get_output_frequency(ref frequency_num, frequency);

                if (OPER_OK == ret)
                {
                    string fp_str = "";

                    for (int index = 0; index < frequency_num; index++)
                    {
                        fre_value[index] = (float)(frequency[index] / 1000.000);
                        fp_str += fre_value[index].ToString() + ", ";
                    }
                    richTextBox_frequency_point.Text = fp_str;

                    oper_result = "Get  Frequency Point success.";//Get  Frequency Point success.";获取频点成功！
                }
                else
                {
                    oper_result = "Get Frequency Point fail";//Get Frequency Point fail.";获取频点失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }
        }

        private void button_set_fp_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {

                string str_fp = richTextBox_frequency_point.Text;
                byte frequency_num = 0;
                float[] temp_fp = new float[global.OUTPUT_FREQUENCY_NUM];
                str_fp = str_fp.Trim(' ');
                string[] temp_per_fp = str_fp.Split(new char[] { ';', ',' });

                int[] frequency = new int[global.OUTPUT_FREQUENCY_NUM];

                int fp_len = 0;
                for (int index = 0; index < temp_per_fp.Length; index++)
                {
                    if (temp_per_fp[index] == "")
                    {
                        continue;
                    }
                    else
                    {
                        temp_fp[fp_len++] = float.Parse(temp_per_fp[index]);
                    }
                }

                int ret = 0;

                frequency_num = (byte)fp_len;

                ///*    //设置的数据在存放到数组过程中有错误
                for (int index = 0; index < fp_len; index++)
                {                         
                    frequency[index] = (int)(temp_fp[index]*1000+0.5);
                }
                

                ret = sr_api.Set_output_frequency(frequency_num, frequency);

                if (OPER_OK == ret)
                {
                    oper_result = "Set Frequency Point success.";//";设置频点成功！
                }
                else
                {
                    oper_result = "Set Frequency Point fail.";//";设置频点失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }


        }

        private void button_set_baudrate_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {
                int ret = 0;

                byte rate_type = (byte)(comboBox_set_baudrate.SelectedIndex);

                ret = sr_api.Set_module_baud_rate(rate_type); // 设置波特率

                if (OPER_OK == ret)
                {
                    oper_result = "Set Com Baudrate success.";//";设置波特率成功！
                }
                else
                {
                    oper_result = "Set Com Baudrate fail.";//";设置波特率失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }
        }

        private void set_ant_worktime()
        {
            string oper_result;
            try
            {
                 int ret = 0;
                  
                //设置天线工作时间
                ushort ant1_wk = ushort.Parse(textBox_ant1_worktime.Text);
                ushort ant2_wk = ushort.Parse(textBox_ant2_worktime.Text);
                ushort ant3_wk = ushort.Parse(textBox_ant3_worktime.Text);
                ushort ant4_wk = ushort.Parse(textBox_ant4_worktime.Text);
                ushort wait_time = ushort.Parse(textBox_waittime.Text);

                ret = sr_api.Set_antenna_worktime_and_waittime(ant1_wk,ant2_wk,ant3_wk,ant4_wk,wait_time); // 设置天线工作时间和等待时间

                if (OPER_OK == ret)
                {
                    oper_result = "Set Antenna Work Time success.";//";设置天线工作时间成功！
                }
                else
                {
                    oper_result = "Set Antenna Work Time fail.";//";设置天线工作时间失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }
        }

        private void button_set_ant_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {
                // 1 设置天线号
                int ret = 0;

                byte ants = 0x00;
                if (true == checkBox_ant1.Checked)
                {
                    ants |= 0x01;
                }
                if (true == checkBox_ant2.Checked)
                {
                    ants |= 0x02;
                }
                if (true == checkBox_ant3.Checked)
                {
                    ants |= 0x04;
                }
                if (true == checkBox_ant4.Checked)
                {
                    ants |= 0x08;
                }

                ret = sr_api.Set_Work_Antanne(ants);

                if (OPER_OK == ret)
                {
                    oper_result = "Set Antenna success.";//";设置天线成功！

                    // 2 设置天线工作时间
                    set_ant_worktime();
                }
                else
                {
                    oper_result = "Set Antenna fail.";//";设置天线失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }
        }


        private void single_query()
        {
            string oper_result;
            try
            {
                // 1 单次查询标签
                query_epc_t oper_type = new query_epc_t();

                byte[] epc = new byte[global.PACKET_128];
                byte[] tid = new byte[global.PACKET_128];
                ushort rssi = 0;
                byte ant_id = 0;
                byte epc_len = 0;
                byte tid_len = 0;
                ushort pc = new ushort();

                int ret = sr_api.Sigle_Query_Tags_Epc(epc, ref epc_len,tid ,ref tid_len,ref rssi, ref ant_id);

                if (OPER_OK == ret)
                {
                    oper_type.epc.epc = epc;
                    oper_type.tid = tid;
                    oper_type.rssi_lsb = (byte)(rssi & 0xFF);
                    oper_type.rssi_msb = (byte)((rssi >> 8) & 0xFF);
                    oper_type.ant_id = ant_id;
                    oper_type.epc.epc_len = (char)epc_len;
                    oper_type.tid_len = tid_len;
                    pc = calculate_pc(epc.ToString());
                    oper_type.epc.pc_lsb = (byte)(pc & 0xff);
                    oper_type.epc.pc_msb = (byte)((pc >> 8) & 0xff);
                    UpdataLabel(oper_type);

                    oper_result = "Single query success.";//";单次查寻成功！
                }
                else
                {
                    oper_result = "Single query fail.";//"单次查寻失败！;
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }          
        }

        const string MULTI_START_LABEL = "Query"; //开始寻标签
        const string MULTI_STOP_LABEL = "Stop"; //Stop停止寻标签
        private Thread multi_get_thread;
        
        private void _multi_get()
        {
            while (true)
            {
                multi_query_epc_t multi_epc = new multi_query_epc_t();

                if (!multi_get_thread.IsAlive)
                {
                    break;
                }

                int ret = sr_api.Get_Multi_Query_Tags_Epc_Data(ref multi_epc);

                multi_analyze_data(multi_epc);
                Thread.Sleep(0);

            }
        }


        private void _multi_stop()
        {
            string oper_result;
            try
            {
                // 1 停止循环读标签
                int ret = sr_api.Stop_Multi_Query_Tags_Epc();

                if (OPER_OK == ret)
                {
                    oper_result = "Stop Multi query success.";//";停止循环查寻成功！
                }
                else
                {
                    oper_result = "Stop Multi query fail.";//";停止循环查寻失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }    
        }


        private void multi_query()
        {

            multi_get_thread = new Thread(new ThreadStart(this._multi_get));
            if (MULTI_START_LABEL == this.button_query.Text)    //开始循环寻标签
            {

                button_query.Text = MULTI_STOP_LABEL;

                UInt32 times = UInt32.Parse(textBox_multi_time.Text);
                int ret = sr_api.Multi_Query_Tags_Epc(times);

                if (OPER_OK == ret)
                {
                    UpdateLog("Start Multi query success.");//"启动循环查寻标签成功！
           //         multi_init();
                    multi_get_thread.Start();
                    timer_scan.Enabled = true;
                }
                else
                {
                    UpdateLog("Start Multi query fail.");//启动循环查寻标签失败！
                }
            }
            else               //停止循环寻标签
            {
                button_query.Text = MULTI_START_LABEL;
       //         this.stop_multi_config();

                _multi_stop();
                timer_scan.Enabled = false;
            }
        }
//------------------------------------------------------------------------2015-03-11-00
       static  bool  start_tag=false;
        private void  auto_multi_query()
        {
            multi_get_thread = new Thread(new ThreadStart(this._multi_get));

       //   if (MULTI_START_LABEL == this.button_query.Text)    //开始循环寻标签

           if (start_tag)
            {

            //  button_query.Text = MULTI_STOP_LABEL;

                UInt32 times = UInt32.Parse(textBox_multi_time.Text);
                int ret = sr_api.Multi_Query_Tags_Epc(times);

                if (OPER_OK == ret)
                {
                    UpdateLog("Start Multi query success.");//"启动循环查寻标签成功！
                    //         multi_init();
                    multi_get_thread.Start();
                    timer_scan.Enabled = true;
                }
                else
                {
                    UpdateLog("Start Multi query fail.");//Start Multi query fail.启动循环查寻标签失败！
                }
            }
            else               //停止循环寻标签
            {
                button_query.Text = MULTI_START_LABEL;
                //         this.stop_multi_config();

                _multi_stop();
                timer_scan.Enabled = false;
            }
        }


//------------------------------------------------------------------------

        private void button_query_Click(object sender, EventArgs e)
        {
            if (true == radioButton_single.Checked) // 单次寻标签
            {
                single_query();
            }
            else if (true == radioButton_multi.Checked)                                                // 多次寻标签
            {
                multi_query();
            }
            else
            {
                // 异常
                UpdateLog("Operation Error.");
            }
        }

        //通过EPC计算出其相应的PC值
        private  ushort	calculate_pc(string str_epc)	
        {
            ushort temp_pc = (ushort)str_epc.Length;
            temp_pc /= 4;
            temp_pc <<= 11;
            temp_pc &= 0xF800;

            return temp_pc;
        }

        //通过EPC计算出其相应的PC值
        private ushort calc_pc(string str_epc)
        {
            ushort temp_pc = (ushort)str_epc.Length;
            temp_pc /= 4;
            temp_pc <<= 11;
            temp_pc &= 0xF800;

            return temp_pc;
        }



        // shex 长度为8个字节
        private void HexToDec(string shex, ref uint idec)
        {
            int len = 8;
            int mid = 0; 
	        idec = 0;
	        for( int i=0; i<len; i++ )
	        {
		        if( shex[i]>='0'&&shex[i]<='9' )
			        mid = shex[i]-'0';
		        else if( shex[i]>='a'&&shex[i]<='f' )
			        mid = shex[i] -'a' +10;
		        else if( shex[i]>='A'&&shex[i]<='F' )
			        mid = shex[i] -'A' +10;

		        mid <<= ((len-i-1)<<2);
		        idec |= (uint)mid;
	        }
        }



        private void hex2asc(string hexStr, byte[] ascStr, ref byte ascLen)
        {
	        byte i;
	        byte uc;

            if ((hexStr.Length%2) != 0)
            {
                hexStr += "0";
            }

            for (i = 0; i < hexStr.Length; i++)
	        {
		        uc = (byte)hexStr[i];
		        if (uc >= 0x30  && uc <= 0x39)
			        uc -= 0x30;
		        else if (uc >= 0x41 && uc <= 0x46)
			        uc -= 0x37;
		        else if (uc >= 0x61 && uc <= 0x66)
			        uc -= 0x57;
		        else
			        break;
                if (i % 2 == 1)
                {
                    ascStr[(i - 1) / 2] = (byte)(ascStr[(i - 1) / 2] | uc);
                    ++ascLen;
                }
                else
                    ascStr[i / 2] = (byte)(uc << 4);
	        }
        }

        private void button_data_read_Click(object sender, EventArgs e)
        {
            string oper_result;
            this.textBox_data.Text = null;
            try
            {
                 // 1 获取访问密码
                uint password = 0;
                if (textBox_access_password.Text.Length > 0)
                {
                    if (textBox_access_password.Text.Length != 8)
                    {
                        oper_result = "Access Password shoult be 8 characters!";//Access Password shoult be 8 characters!";访问秘密少于8个字符！
                        UpdateLog(oper_result);
                        return;
                    }
                    else
                    {
                      //  password = uint.Parse(textBox_access_password.Text);
                        HexToDec(textBox_access_password.Text,ref password);
                    }
                }
                

                // 2 判断是否启动过滤，并 获取/设置 PC 和 EPC 的值
                byte[] epc = new byte[global.PACKET_128];
                byte epc_len = 0;
                string filter_data = textBox_tag_epc.Text;
                byte filter_bak = 0;
                if (2 == filterbox.SelectedIndex)
                {
                    // 启动过滤，pc，epc不可为空
                    if (textBox_tag_epc.Text.Length <= 0)
                    {
                        oper_result = "Read Tag data fail, TID cannot be null.";//"; 读取标签失败，指定过滤的 TID 不能为空！
                        UpdateLog(oper_result);
                        return;
                    }
                    else
                    {
                        if ((filter_data.Length % 4) != 0)
                        {
                            int add_zero = filter_data.Length % 4;
                            byte[] str_zero = new byte[add_zero];
                            filter_data += str_zero.ToString();
                        }

                        hex2asc(filter_data, epc, ref epc_len);

                        //pc = calculate_pc(filter_data);

                        filter_bak = 0x01;
                    }
                }
                else if (1 == filterbox.SelectedIndex)
                {
                    // 启动过滤，pc，epc不可为空
                    if (textBox_tag_epc.Text.Length <= 0)
                    {
                        oper_result = "Read Tag data fail, EPC cannot be null.";//";读取标签失败，指定过滤的 EPC 不能为空！
                        UpdateLog(oper_result);
                        return;
                    }
                    else
                    {
                        if ((filter_data.Length % 4) != 0)
                        {
                            int add_zero = filter_data.Length % 4;
                            byte[] str_zero = new byte[add_zero];
                            filter_data += str_zero.ToString();
                        }

                        hex2asc(filter_data, epc, ref epc_len);

                        //pc = calculate_pc(filter_data);


                        filter_bak = 0x00;
                    }
                }
                else
                {
                    // 不过滤，pc，epc都为空                
                    for (int index = 0; index < 12; index++ )
                    {
                        epc[index] = 0x00;
                    }
                    
                    epc_len = 0;   // 不过滤则长度为0
                }

                // 3 获取 memory bank 的类型
                byte mem_bank = (byte)comboBox_mb.SelectedIndex;

                // 4 获取要读取的内存中的起始地址
                ushort temp_addr = ushort.Parse(textBox_head_address.Text);

                // 5 获取要读取的数据的长度
                ushort temp_len = ushort.Parse(textBox_data_len.Text);

                // 6 查询标签数据 命令                
                byte[] recv_buffer = new byte[global.PACKET_MID];
                byte ant_id = 0;
                int ret = 0;

                ret = sr_api.read_data(password, filter_bak, epc_len, epc, mem_bank, temp_addr, temp_len, recv_buffer, ref ant_id);
                //int ret = sr_api.read_data(password, pc, epc, epc_len, mem_bank, temp_addr, temp_len,recv_buffer,ref recv_len, ref ant_id);
                
                if (OPER_OK == ret)
                {
                    oper_result = "Read Tag data success.";//";读取标签成功！
                    switch(ant_id)
                    {
                        case 1:
                            radioButton_ant1.Checked = true;
                            radioButton_ant2.Checked = false;
                            radioButton_ant3.Checked = false;
                            radioButton_ant4.Checked = false;
                            break;
                        case 2:
                            radioButton_ant2.Checked = true;
                            radioButton_ant3.Checked = false;
                            radioButton_ant1.Checked = false;
                            radioButton_ant4.Checked = false;
                            break;
                        case 3:
                            radioButton_ant3.Checked = true;
                            radioButton_ant2.Checked = false;
                            radioButton_ant1.Checked = false;
                            radioButton_ant4.Checked = false;
                            break;
                        case 4:
                            radioButton_ant4.Checked = true;
                            radioButton_ant3.Checked = false;
                            radioButton_ant2.Checked = false;
                            radioButton_ant1.Checked = false;
                            break;
                    }
                    
                    string recv_str = BitConverter.ToString(recv_buffer, 0, temp_len*2).ToUpper().Replace("-", "");
                    textBox_data.Text = recv_str;
                }
                else
                {
                    oper_result = "Read Tag data fail.";//";读取标签失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }    
        }

        private void _lock_mask(ref byte mask,ref byte action)
        {
            ushort tmp = 0x1;

            CheckBox[] array_bank = { checkBox_user, checkBox_tid, checkBox_epc, checkBox_access, checkBox_kill };
            RadioButton[] array_action = { radioButton_rw_unable_forever, radioButton_rw_able_forever, radioButton_rw_unable, radioButton_rw_able };

            for (int index = 0; index < array_bank.Length; index++)
            {
                if (array_bank[index].Checked == true)
                {
                    mask += (byte)(tmp << (index));
                }
            }

            for (int index = 0; index < array_action.Length; index++)
            {
                if (array_action[index].Checked == true)
                {
                    action = (byte)index;
                }
            }
        }

        private void button_data_write_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {
                // 1 获取访问密码
                uint password = 0;
                if (textBox_access_password.Text.Length > 0)
                {
                    if (textBox_access_password.Text.Length != 8)
                    {
                        oper_result = "Access Password shoult be 8 characters!";//";访问密码少于8个字符!
                        UpdateLog(oper_result);
                        return;
                    }
                    else
                    {
                        HexToDec(textBox_access_password.Text, ref password);
                    }
                }


                // 2 判断是否启动过滤，并 获取/设置 PC 和 EPC 的值
                //ushort pc;
                byte[] epc = new byte[global.PACKET_128];
                byte epc_len = 0;
                string filter_data = textBox_tag_epc.Text;
                byte filter_bak = 0x0;
                if (2 == filterbox.SelectedIndex)
                {
                    // 启动过滤，pc，epc不可为空
                    if (textBox_tag_epc.Text.Length <= 0)
                    {
                        oper_result = "Read Tag data fail, TID cannot be null.";//";读取标签失败，指定过滤的TID不能为空！
                        UpdateLog(oper_result);
                        return;
                    }
                    else
                    {
                        if ((filter_data.Length % 4) != 0)
                        {
                            int add_zero = filter_data.Length % 4;
                            byte[] str_zero = new byte[add_zero];
                            filter_data += str_zero.ToString();
                        }

                        hex2asc(filter_data, epc, ref epc_len);

                        //pc = calculate_pc(filter_data);

                        filter_bak = 0x01;
                    }
                }
                else if (1 == filterbox.SelectedIndex)
                {
                    // 启动过滤，pc，epc不可为空
                    if (textBox_tag_epc.Text.Length <= 0)
                    {
                        oper_result = "Read Tag data fail, EPC cannot be null.";//";读取标签失败，指定过滤的EPC不能为空！
                        UpdateLog(oper_result);
                        return;
                    }
                    else
                    {
                        if ((filter_data.Length % 4) != 0)
                        {
                            int add_zero = filter_data.Length % 4;
                            byte[] str_zero = new byte[add_zero];
                            filter_data += str_zero.ToString();
                        }

                        hex2asc(filter_data, epc, ref epc_len);

                        //pc = calculate_pc(filter_data);
                        filter_bak = 0x00;
                    }
                }
                else
                {
                    // 不过滤，pc，epc都为空                
                    for (int index = 0; index < 12; index++)
                    {
                        epc[index] = 0x00;
                    }

                    epc_len = 0;   // 默认标准标签的epc为12个字节，即6个字
                }
                

                // 3 获取 memory bank 的类型
                byte mem_bank = (byte)comboBox_mb.SelectedIndex;

                // 4 获取要读取的内存中的起始地址
                ushort temp_addr = ushort.Parse(textBox_head_address.Text);

                // 5 获取要读取的数据的长度
                ushort temp_len = ushort.Parse(textBox_data_len.Text);

                // 6 查询标签数据 命令                
                byte[] write_buffer = new byte[global.PACKET_MID];
                byte write_len    = 0;
        //        byte[] temp = System.Text.Encoding.Default.GetBytes(textBox_data.Text);
                hex2asc(textBox_data.Text, write_buffer, ref write_len);

                byte ant_id = 0;
                // 7 写标签数据 命令
                int ret = 0;

                ret = sr_api.write_data(password, filter_bak, epc_len, epc, mem_bank, temp_addr, temp_len, write_buffer, ref ant_id);

                if (OPER_OK == ret)
                {
                    oper_result = "Write Tag Data success.";//";向标签写入数据成功！
                }
                else
                {
                    oper_result = "Write Tag Data fail.";//";向标签写入数据失败!
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            } 
        }

        private void button_lock_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {

                // 1 获取访问密码
                uint password = 0;
                if (textBox_access_password.Text.Length > 0)
                {
                    if (textBox_access_password.Text.Length != 8)
                    {
                        oper_result = "Access Password shoult be 8 characters!";//"访问密码少于8个字符！;
                        UpdateLog(oper_result);
                        return;
                    }
                    else
                    {
                        //  password = uint.Parse(textBox_access_password.Text);
                        HexToDec(textBox_access_password.Text, ref password);
                    }
                }


                // 2 获取/设置 PC 和 EPC 的值
                //ushort pc;
                byte[] epc = new byte[global.PACKET_128];
                byte epc_len = 0;
                string filter_data = textBox_tag_epc.Text;
                byte filter_bak = 0;
                byte ant_id = 0x00;
                if (2 == filterbox.SelectedIndex)//过滤TID
                {
                    // 启动过滤，pc，epc不可为空
                    if (textBox_tag_epc.Text.Length <= 0)
                    {
                        oper_result = "Read Tag data fail, TID cannot be null.";//"; 读取标签失败，指定过滤的TID不能为空！
                        UpdateLog(oper_result);
                        return;
                    }
                    else
                    {
                        if ((filter_data.Length % 4) != 0)
                        {
                            int add_zero = filter_data.Length % 4;
                            byte[] str_zero = new byte[add_zero];
                            filter_data += str_zero.ToString();
                        }

                        hex2asc(filter_data, epc, ref epc_len);

                        //pc = calculate_pc(filter_data);

                        filter_bak = 0x01;
                    }
                }
                else if (1 == filterbox.SelectedIndex)//过滤EPC
                {
                    // 启动过滤，pc，epc不可为空
                    if (textBox_tag_epc.Text.Length <= 0)
                    {
                        oper_result = "Read Tag data fail, EPC cannot be null.";//";读取标签失败，指定过滤的EPC不能为空！
                        UpdateLog(oper_result);
                        return;
                    }
                    else
                    {
                        if ((filter_data.Length % 4) != 0)
                        {
                            int add_zero = filter_data.Length % 4;
                            byte[] str_zero = new byte[add_zero];
                            filter_data += str_zero.ToString();
                        }

                        hex2asc(filter_data, epc, ref epc_len);

                        //pc = calculate_pc(filter_data);


                        filter_bak = 0x00;
                    }
                }
                else
                {
                    // 不过滤，pc，epc都为空                
                    for (int index = 0; index < 12; index++ )
                    {
                        epc[index] = 0x00;
                    }
                    
                    epc_len = 0;   // 默认标准标签的epc为12个字节，即6个字
                }

                // 3 获取锁操作
                byte lock_mask = 0x00;
                byte lock_action = 0x00;
               // lock_mask = _lock_mask();
                _lock_mask(ref lock_mask, ref lock_action);

                // 4 发送命令
                int ret = OPER_OK;
                ret = sr_api.lock_tag(password, filter_bak, epc_len, epc, lock_mask, lock_action, ref ant_id);


                if (OPER_OK == ret)
                {
                    oper_result = "Lock Tag success.";//";锁定标签成功！
                }
                else
                {
                    oper_result = "Lock Tag fail.";//";锁定标签失败!
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            } 
        }

        private void button_kill_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {
                // 1 获取访问密码
                uint password = 0;
                if (textBox_kill_password.Text.Length > 0)
                {
                    if (textBox_kill_password.Text.Length != 8)
                    {
                        oper_result = "Kill Password shoult be 8 characters!";//";销毁密码少于8个字符！
                        UpdateLog(oper_result);
                        return;
                    }
                    else
                    {
                        //  password = uint.Parse(textBox_access_password.Text);
                        HexToDec(textBox_kill_password.Text, ref password);
                    }
                }

                // 2 获取/设置 PC 和 EPC 的值
                byte[] epc = new byte[global.PACKET_128];
                byte epc_len = 0;
                string filter_data = textBox_tag_epc.Text;
                byte filter_bak = 0;
                byte ant_id = 0x00;
                if (2 == filterbox.SelectedIndex)//过滤TID
                {
                    // 启动过滤，pc，epc不可为空
                    if (textBox_tag_epc.Text.Length <= 0)
                    {
                        oper_result = "Read Tag data fail, TID cannot be null.";//"; 读取标签失败，指定过滤的TID不能为空！
                        UpdateLog(oper_result);
                        return;
                    }
                    else
                    {
                        if ((filter_data.Length % 4) != 0)
                        {
                            int add_zero = filter_data.Length % 4;
                            byte[] str_zero = new byte[add_zero];
                            filter_data += str_zero.ToString();
                        }
                        hex2asc(filter_data, epc, ref epc_len);

                        filter_bak = 0x01;
                    }
                }
                else if (1 == filterbox.SelectedIndex)//过滤EPC
                {
                    // 启动过滤，pc，epc不可为空
                    if (textBox_tag_epc.Text.Length <= 0)
                    {
                        oper_result = "Read Tag data fail, EPC cannot be null.";//";读取标签失败，指定过滤的EPC不能为空！
                        UpdateLog(oper_result);
                        return;
                    }
                    else
                    {
                        if ((filter_data.Length % 4) != 0)
                        {
                            int add_zero = filter_data.Length % 4;
                            byte[] str_zero = new byte[add_zero];
                            filter_data += str_zero.ToString();
                        }
                        hex2asc(filter_data, epc, ref epc_len);

                        filter_bak = 0x00;
                    }
                }
                else
                {
                    // 不过滤，pc，epc都为空                
                    for (int index = 0; index < 12; index++)
                    {
                        epc[index] = 0x00;
                    }
                    epc_len = 0;   // 默认标准标签的epc为12个字节，即6个字
                }

                // 3 发送命令
                int ret = 0;
                ret = sr_api.kill_tag(password,filter_bak ,epc_len,epc, ref ant_id);

                if (OPER_OK == ret)
                {
                    oper_result = "Kill tag success.";//";销毁标签成功！
                }
                else
                {
                    oper_result = "Kill tag fail.";//";销毁标签失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error:" + ex.Message;
                UpdateLog(oper_result);
            } 
        }




        private void  __SetVal()
        {
            // epc
            string base_epc = textBox_new_epc.Text;
            string epc_high_part = "";
            string temp_epc = "";
            int start_pos = 0;
            int len = 0;
            if (base_epc.Length > 8)
            {
                // 截取字符串末尾8个字符
                start_pos = (base_epc.Length - 8);
                len = 8;
                temp_epc = base_epc.Substring(start_pos, len);

                epc_high_part = base_epc.Substring(0, start_pos);
            }
            else
            {
                start_pos = 0;
                len = base_epc.Length;
                temp_epc = textBox_new_epc.Text;
            }

            

            // 增量
            string temp_increment = "";
            temp_increment =  textBox_epc_interval.Text;
            

            uint hex_epc = uint.Parse(temp_epc, System.Globalization.NumberStyles.HexNumber);
            uint hex_inc = uint.Parse(temp_increment, System.Globalization.NumberStyles.HexNumber);
            string hex_next_epc = "";
            hex_next_epc = (hex_epc + hex_inc).ToString("X");

            if (hex_next_epc.Length < len)
            {
                int temp_len = hex_next_epc.Length;
                string temp_data = hex_next_epc; 
                string temp_zero = "";
                for (int index = 0; index < (len - temp_len); index++)
                {
                    temp_zero += "0";
                }

                hex_next_epc = temp_zero;
                hex_next_epc += temp_data;

            }

            string next_epc = epc_high_part + hex_next_epc;


            textBox_new_epc.Text = next_epc; 
        }



        private void button_increase_write_Click(object sender, EventArgs e)
        {
            string temp_epc = textBox_new_epc.Text;
            string temp_increment = textBox_epc_interval.Text;
            string oper_result;
            try
            {
                // 1 查询标签数据 命令                
                byte[] epc = new byte[global.PACKET_MID];
                byte epc_len = 0;
                //        byte[] temp = System.Text.Encoding.Default.GetBytes(textBox_data.Text);
                hex2asc(textBox_new_epc.Text, epc, ref epc_len);



                // 2 写标签数据 命令
                int ret = OPER_OK;
                //ret = sr_api.write_epc(epc, epc_len);


                if (OPER_OK == ret)
                {
                    oper_result = "Write Tag EPC success.";//";写入标签EPC区数据成功！
                    __SetVal();
                }
                else
                {
                    oper_result = "Write Tag EPC fail.";//";写入标签EPC区数据失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            } 

        }

        private void listView_label_DoubleClick(object sender, EventArgs e)
        {
            textBox_tag_epc.Text = listView_label.SelectedItems[0].SubItems[listView_label_EPC].Text.Replace("-", "");
        }

        private void textBox_access_password_TextChanged(object sender, EventArgs e)
        {
      //      Regex   r   =   new   Regex("^[0-9]{1,}$");   
        //    Regex   r   =   new   Regex("^[1-9a-fA-F]+$");
         //   r.IsMatch(textBox_access_password.Text);
 
         //   r.Matches(textBox_access_password.Text);
            


          //  document.all("textbox").value.match(/^[1-9a-zA-Z]+$/))
           
        }

        private void textBox_access_password_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((e.KeyChar >= '0') && (e.KeyChar <= '9')) || ((e.KeyChar >= 'a') && (e.KeyChar <= 'f')) || ((e.KeyChar >= 'A') && (e.KeyChar <= 'F')))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void radioButton_device_server_CheckedChanged(object sender, EventArgs e)
        {
            textBox_server_ip.Enabled   = false;
            textBox_server_port.Enabled = false;
            textBox_module_port.Enabled = true;
        }

        private void radioButton_device_client_CheckedChanged(object sender, EventArgs e)
        {
            textBox_server_ip.Enabled   = true;
            textBox_server_port.Enabled = true;
            textBox_module_port.Enabled = false;
        }



        private void button_modules_search_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {
                int ret = 0;
                net_device_t temp_net_device = new net_device_t();
                ret = sr_api.basic_init(transfer.CONNECT_NET);   //初始化为串口通信模式
                //sr_api.net_trans_init();
                ret = transfer.transfer_init(ref temp_net_device);
                net_device = temp_net_device;

                if (OPER_OK == ret)
                {
                    UpdataNet(net_device);
                    oper_result = "Search net module success.";//";搜索网络成功！
                }
                else
                {
                    oper_result = "Search net module fail.";//";搜索网络失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error111 :" + ex.Message;
                UpdateLog(oper_result);
            }
        }





        private void    get_device_info(ref device_t param)
        {
            param.device_info.strMAC    = textBox_module_mac.Text.PadRight(20, '\0').ToCharArray();

            param.device_info.strIP     = textBox_module_ip.Text.PadRight(20, '\0').ToCharArray();
            param.net_setting.ipAddr    = textBox_module_ip.Text.PadRight(20, '\0').ToCharArray();

            param.net_setting.ipNetMask = textBox_module_netmask.Text.PadRight(20, '\0').ToCharArray();

            param.net_setting.ipGateway = textBox_module_gateway.Text.PadRight(20, '\0').ToCharArray();

            param.socket_setting.nLocalPort = int.Parse(textBox_module_port.Text); 


            if (radioButton_device_server.Checked == false)
            {   // pc机为服务器，设备为客户端
                param.socket_setting.szPeerName = textBox_server_ip.Text.PadRight(64, '\0').ToCharArray();
                param.socket_setting.nPeerPort  = int.Parse(textBox_server_port.Text);
                // 设置c2000模块 nWorkMode; 
                //0：TCP 客户端；1：TCP 服务器；2：UDP1 或自动；
                //3：UDP2；4：自动，根据具体的产品而定
                param.socket_setting.nWorkMode = 0; 
            }
            else
            {   // pc机为客户端，设备为服务器

                // 设置c2000模块 nWorkMode; 
                //0：TCP 客户端；1：TCP 服务器；2：UDP1 或自动；
                //3：UDP2；4：自动，根据具体的产品而定
                param.socket_setting.nWorkMode = 1; 
            }
        }



        private void button_module_set_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {
                int ret = 0;

                get_device_info(ref net_device.net_device[cur_pitch]);

                device_t temp_net_device = new device_t();
                temp_net_device = net_device.net_device[cur_pitch];
                ret = transfer.transfer_send_set(ref temp_net_device); // 返回ture

                if (1 == ret)
                {
                    oper_result = "Set net module success.";//";设置网络参数成功！
                }
                else
                {
                    oper_result = "Set net module fail.";//";设置网络参数失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }
        }

        private static int query_time = 0;
        private void timer_scan_Tick(object sender, EventArgs e)
        {
            
            ++query_time;

            label_tags_total.Text = tags_list.Count.ToString();
            label_query_time.Text = query_time.ToString();
            int speed = get_count() / 1;
            label_query_speed.Text = speed.ToString();
            reset_count();

            updata_tags_listview();
        }

        private void listView_net_DoubleClick(object sender, EventArgs e)
        {
            int Row = listView_net.SelectedItems[0].Index;
            cur_pitch = Row;


            textBox_module_mac.Text = new string(net_device.net_device[Row].device_info.strMAC);
            textBox_module_ip.Text = new string(net_device.net_device[Row].device_info.strIP);
            textBox_module_netmask.Text = new string(net_device.net_device[Row].net_setting.ipNetMask);
            textBox_module_gateway.Text = new string(net_device.net_device[Row].net_setting.ipGateway);
            textBox_module_port.Text = net_device.net_device[Row].socket_setting.nLocalPort.ToString();

            textBox_server_ip.Text = new string(net_device.net_device[Row].socket_setting.szPeerName);
            textBox_server_port.Text = net_device.net_device[Row].socket_setting.nPeerPort.ToString();

            if (1 == net_device.net_device[Row].socket_setting.nWorkMode)   // 模块为tcp服务器
            {
                radioButton_device_server.Checked = true;
            }
            else if (0 == net_device.net_device[Row].socket_setting.nWorkMode)   // 模块为tcp客户端
            {
                radioButton_device_client.Checked = true;
            }
        }

        private void listView_net_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*
            //获取点击的那一行
            int Row = listView_net;
            m_cur_pitch = Row;

            CString temp_mac;
            temp_mac.Format(_T("%s"), m_net_device.net_device[Row].device_info.strMAC);
            CString temp_ip;
            temp_ip.Format(_T("%s"), m_net_device.net_device[Row].device_info.strIP);
            CString temp_mask;
            temp_mask.Format(_T("%s"), m_net_device.net_device[Row].net_setting.ipNetMask);
            CString temp_gateway;
            temp_gateway.Format(_T("%s"), m_net_device.net_device[Row].net_setting.ipGateway);

            CString temp_module_port;
            temp_module_port.Format(_T("%d"), m_net_device.net_device[Row].socket_setting.nLocalPort);
            CString temp_server_port;
            temp_server_port.Format(_T("%d"), m_net_device.net_device[Row].socket_setting.nPeerPort);
            CString temp_server_addr;
            temp_server_addr.Format(_T("%s"), m_net_device.net_device[Row].socket_setting.szPeerName);


            textBox_module_mac
                textBox_module_ip
                textBox_module_netmask
                    textBox_module_gateway
                    textBox_module_port

                        textBox_server_ip
                        textBox_server_port*/
        }

        private void textBox_new_epc_TextChanged(object sender, EventArgs e)
        {
            if (0 != (textBox_new_epc.Text.Length % 4))
            {
                button_increase_write.Enabled = false;
            }
            else
            {
                button_increase_write.Enabled = true;
            }
        }

        private void rbNet_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton_module_is_server_CheckedChanged(object sender, EventArgs e)
        {
            //textBox_connect_ip.Text = "";
            textBox_connect_ip.Enabled = true;

            label_connect_ip.Text = "Reader IP :";
        }


        protected string GetIP()   //获取本地IP 
        {
            string hostname = Dns.GetHostName();//得到本机名   
            //IPHostEntry localhost = Dns.GetHostByName(hostname);//方法已过期，只得到IPv4的地址  
            IPHostEntry localhost = Dns.GetHostEntry(hostname);
            IPAddress localaddr = localhost.AddressList[0];
            return localaddr.ToString();
        }

        private void radioButton_module_is_client_CheckedChanged(object sender, EventArgs e)
        {
            string host_ip = GetIP();
            textBox_connect_ip.Text = host_ip;
            textBox_connect_ip.Enabled = false;

            label_connect_ip.Text = "Server IP :";
        }

        private void button_clear_Click(object sender, EventArgs e)
        {
            tags_list.Clear();
            listView_label.Items.Clear();
            
            reset_count();
            label_tags_total.Text = "0";

            query_time = 0;
            label_query_time.Text = "0";

            label_query_speed.Text = "0";


        }

        private void button_carrier_get_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {
                int ret = 0;
                byte q_type = 0;
                byte q_start = 0;
                byte q_min = 0;
                byte q_max = 0;
                byte select_q =0;
                byte session_q =0;
                byte target_q = 0;

                ret = sr_api.Get_gen2_param(ref q_type, ref q_start, ref q_min, ref q_max,ref select_q,ref session_q, ref target_q);

                if (OPER_OK == ret)
                {
                    q_data.SelectedIndex = q_type;
                    start_q.SelectedIndex = q_start;
                    min_q.SelectedIndex = q_min;
                    max_q.SelectedIndex = q_max;
                    this.select_q.SelectedIndex = select_q;
                    this.session_q.SelectedIndex = session_q;
                    this.target_q.SelectedIndex = target_q;

                    oper_result = "Get gen_param success.";//";获取 GEN 参数成功！
                }
                else
                {
                    oper_result = "Get gen_param fail.";//";获取 GEN 参数失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }
        }

        private void button_carrier_set_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {
                int ret = 0;
                byte q_type = 0;
                byte q_start = 0;
                byte q_min = 0;
                byte q_max = 0;
                byte select_q=1;
                byte session_q=1;
                byte target_q = 1;

                q_type = (byte)q_data.SelectedIndex;//清点算法
                q_start = (byte)start_q.SelectedIndex;//起始Ｑ值
                q_min = (byte)min_q.SelectedIndex;　//最小Ｑ值
                q_max = (byte)max_q.SelectedIndex;　//最大Ｑ值
                //
                select_q = (byte)this.select_q.SelectedIndex;
                session_q = (byte)this.session_q.SelectedIndex;
                target_q = (byte)this.target_q.SelectedIndex;


                ret = sr_api.Set_gen2_param(q_type, q_start, q_min, q_max,select_q,session_q,target_q);
                if (OPER_OK == ret)
                {
                    oper_result = "Set gen_param success.";//";设置 GEN2 参数成功！
                }
                else
                {
                    oper_result = "Set gen_param fail.";//";设置 GEN2 参数失败!
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }
        }
//--------------------------------------------------------------------------------
private bool IsRegeditKeyExit(string RegistryStr, string KeyStr)
    {
        string[] subkeyNames;

        Microsoft.Win32.RegistryKey rekey = Microsoft.Win32.Registry.LocalMachine;
        Microsoft.Win32.RegistryKey software = rekey.OpenSubKey(RegistryStr);

        if (software == null)
            return false;
        subkeyNames = software.GetValueNames();

        foreach (string keyName in subkeyNames)
        {
            if (keyName == KeyStr)  //判断键值的名称
            {
                rekey.Close();
                return true;
            }
        }

        rekey.Close();

        return false;
    }

private void Create_RegistryKey()//创建注册表
{
    Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine;
    try
    {
        Microsoft.Win32.RegistryKey software = key.CreateSubKey("software\\SanRay");
        software = key.OpenSubKey("software\\SanRay", true);
        software.SetValue("Address", @"C:\Program Files\SanRayDemo\Boot.xml");
    }
    catch (Exception ex)
    {
        MessageBox.Show(ex.ToString());
    }
    finally
    {
        key.Close();
    }

}
private bool Read_RegistryKey() //读取注册表
{
    string startName = "C:\\Program Files\\SanRayDemo\\Boot.xml";
    string info = string.Empty;
    Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine;
    try
    {
        key = key.OpenSubKey("software\\SanRay");

        if (IsRegeditKeyExit("software\\SanRay", "Address"))
        {
            info = key.GetValue("Address").ToString();
            MessageBox.Show("The information in the registry is:" + info);//注册表里的信息为:
        }
        else
        {
            MessageBox.Show("The key value Address doesn't exist;");//键值Address不存在;
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show(ex.ToString());
    }
    finally
    {
        if(key != null)
          key.Close();
    }
    if (startName.Equals(info))
        return true;

    return false;

}
private void Delete_ReistryKey()//
{
    Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine;
    try
    {
        key = key.OpenSubKey("software\\SanRay", true);
        if (IsRegeditKeyExit("software\\SanRay", "Address"))
        {
            key.DeleteValue("Address");
            MessageBox.Show("Delete the success");//删除成功
        }
        else
        {
            MessageBox.Show("The key value Address does not exist");//键值Address不存在;
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show(ex.ToString());
    }
    finally
    {
        key.Close();
    }

}

//--------------------------------------------------------------------------
        static   bool startOK=false ;
        private void button1_Click(object sender, EventArgs e)
        {
            //2015-03-10-22

       }

        private void qtparam_get_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {
                int ret = 0;
                // 1 获取访问密码
                uint password = 0;
                if (textBox_access_password.Text.Length > 0)
                {
                    if (textBox_access_password.Text.Length != 8)
                    {
                        oper_result = "Access Password shoult be 8 characters!";//";访问密码少于8个字符!
                        UpdateLog(oper_result);
                        return;
                    }
                    else
                    {
                        //  password = uint.Parse(textBox_access_password.Text);
                        HexToDec(textBox_access_password.Text, ref password);
                    }
                }

                // 2 判断是否启动过滤，并 获取/设置 PC 和 EPC 的值
                byte[] epc = new byte[global.PACKET_128];
                byte epc_len = 0;
                byte filter_bak = 0x0;
                byte qtparam_data = 0;
                if (1 == filterbox.SelectedIndex)
                {
                    // 启动过滤，pc，epc不可为空
                    if (textBox_tag_epc.Text.Length <= 0)
                    {
                        oper_result = "Read Tag data fail, EPC cannot be null.";//";读取标签失败，指定过滤的 EPC 不能为空!
                        UpdateLog(oper_result);
                        return;
                    }
                    else
                    {
                        string temp_epc = textBox_tag_epc.Text;
                        if ((temp_epc.Length % 4) != 0)
                        {
                            int add_zero = temp_epc.Length % 4;
                            byte[] str_zero = new byte[add_zero];
                            temp_epc += str_zero.ToString();
                        }

                        hex2asc(temp_epc, epc, ref epc_len);

                        //pc = calculate_pc(temp_epc);
                        filter_bak = 0x1;
                    }
                }
                else if (2 == filterbox.SelectedIndex)
                {
                     // 启动过滤，pc，epc不可为空
                    if (textBox_tag_epc.Text.Length <= 0)
                    {
                        oper_result = "Read Tag data fail, EPC cannot be null.";//";读取标签失败，指定过滤的TID不能为空！
                        UpdateLog(oper_result);
                        return;
                    }
                    else
                    {
                        string temp_epc = textBox_tag_epc.Text;
                        if ((temp_epc.Length % 4) != 0)
                        {
                            int add_zero = temp_epc.Length % 4;
                            byte[] str_zero = new byte[add_zero];
                            temp_epc += str_zero.ToString();
                        }

                        hex2asc(temp_epc, epc, ref epc_len);

                        //pc = calculate_pc(temp_epc);
                        filter_bak = 0x1;
                    }
                }
                else
                {
                    // 不过滤，pc，epc都为空                
                    for (int index = 0; index < 12; index++)
                    {
                        epc[index] = 0x00;
                    }
                    //pc = 0;
                    epc_len = 0;   // 不过滤长度设置为0
                }

                ret = sr_api.Get_qtparam(password,filter_bak,epc_len,epc,ref qtparam_data); // 获取qt_param

                if (OPER_OK == ret)
                {
                    if ((qtparam_data & 0x1) == 0x1)
                    {
                        short_range_enable.Checked = true;
                        short_range_disable.Checked = false;
                    }
                    else
                    {
                        short_range_enable.Checked = false;
                        short_range_disable.Checked = true;
                    }

                    if ((qtparam_data & 0x2) == 0x2)
                    {
                        private_type.Checked = false;
                        public_type.Checked = true;
                    }
                    else
                    {
                        private_type.Checked = true;
                        public_type.Checked = false;
                    }

                    oper_result = "Get QT param success.";//";获取 QT 标签参数成功！
                }
                else
                {
                    oper_result = "Get QT param fail.";//";获取 QT 标签参数失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }
        }

        private void qtparam_set_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {
                int ret = 0;
                // 1 获取访问密码
                uint password = 0;
                if (textBox_access_password.Text.Length > 0)
                {
                    if (textBox_access_password.Text.Length != 8)
                    {
                        oper_result = "Access Password shoult be 8 characters!";//";访问密码少于8个字符！
                        UpdateLog(oper_result);
                        return;
                    }
                    else
                    {
                        //  password = uint.Parse(textBox_access_password.Text);
                        HexToDec(textBox_access_password.Text, ref password);
                    }
                }

                // 2 判断是否启动过滤，并 获取/设置 PC 和 EPC 的值
                byte[] epc = new byte[global.PACKET_128];
                byte epc_len = 0;
                byte filter_bak = 0x0;
                byte qtparam_data = 0;
                if (1 == filterbox.SelectedIndex)
                {
                    // 启动过滤，pc，epc不可为空
                    if (textBox_tag_epc.Text.Length <= 0)
                    {
                        oper_result = "Read Tag data fail, EPC cannot be null.";//Read Tag data fail, EPC cannot be null.";读取标签失败，指定过滤的EPC不能为空！
                        UpdateLog(oper_result);
                        return;
                    }
                    else
                    {
                        string temp_epc = textBox_tag_epc.Text;
                        if ((temp_epc.Length % 4) != 0)
                        {
                            int add_zero = temp_epc.Length % 4;
                            byte[] str_zero = new byte[add_zero];
                            temp_epc += str_zero.ToString();
                        }

                        hex2asc(temp_epc, epc, ref epc_len);

                        //pc = calculate_pc(temp_epc);
                        filter_bak = 0x1;
                    }
                }
                else if (2 == filterbox.SelectedIndex)
                {
                    // 启动过滤，pc，epc不可为空
                    if (textBox_tag_epc.Text.Length <= 0)
                    {
                        oper_result = "Read Tag data fail, EPC cannot be null.";//";读取标签失败，指定过滤的TID不能为空！
                        UpdateLog(oper_result);
                        return;
                    }
                    else
                    {
                        string temp_epc = textBox_tag_epc.Text;
                        if ((temp_epc.Length % 4) != 0)
                        {
                            int add_zero = temp_epc.Length % 4;
                            byte[] str_zero = new byte[add_zero];
                            temp_epc += str_zero.ToString();
                        }

                        hex2asc(temp_epc, epc, ref epc_len);

                        //pc = calculate_pc(temp_epc);
                        filter_bak = 0x1;
                    }
                }
                else
                {
                    // 不过滤，pc，epc都为空                
                    for (int index = 0; index < 12; index++)
                    {
                        epc[index] = 0x00;
                    }
                    epc_len = 0;   // 不过滤长度设置为0
                }

                if (short_range_enable.Checked == true)
                {
                    qtparam_data = 0x1;
                }
                else
                {
                    qtparam_data = 0x0;
                }

                if (public_type.Checked == true)
                {
                    qtparam_data |= (0x1<<1);
                }
                else
                {
                    qtparam_data &= 0xFD;
                }

                ret = sr_api.Set_qtparam(password, filter_bak, epc_len, epc, qtparam_data); // 设置QT_PARAM参数

                if (OPER_OK == ret)
                {
                    oper_result = "Set QT param success.";//";设置 QT 参数成功！
                }
                else
                {
                    oper_result = "Set QT param fail.";//";设置 QT 参数失败!
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }
        }

        private void Carrier_set_button_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {
                int ret = OPER_OK;

                byte carrier_switch = 0x0;

                if (true == Carrier_Enable_radioButton.Checked)
                {
                    carrier_switch = 0x01; // 开启
                }
                else
                {
                    carrier_switch = 0x00; // 关闭
                }

                ret = sr_api.Set_antenna_carrier(carrier_switch); // 设置FASTID

                if (OPER_OK == ret)
                {
                    oper_result = "Set carrier success.";//";设置 Carrier 成功！
                }
                else
                {
                    oper_result = "Set carrier fail.";//";设置 Carrier 失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }
        }

        private void Carrier_get_button_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {
                int ret = 0;
                byte carrier_switch = 0x0;

                ret = sr_api.Get_antenna_carrier(ref carrier_switch); // 获取硬件版本号

                if (OPER_OK == ret)
                {
                    if (carrier_switch == 1)
                    {
                        Carrier_Enable_radioButton.Checked = true;
                        carrier_disable_radiobutton.Checked = false;
                    }
                    else
                    {
                        Carrier_Enable_radioButton.Checked = false;
                        carrier_disable_radiobutton.Checked = true;
                    }

                    oper_result = "Get carrier success.";//";获取 Carrier 成功！
                }
                else
                {
                    oper_result = "Get carrier fail.";//";获取 Carrier 失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }
        }

        private void Set_RF_link_profile_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {
                int ret = OPER_OK;
                byte rf_link_profile = 0x0;

                switch (rf_link_profile_index.SelectedIndex)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        rf_link_profile = (byte)(rf_link_profile_index.SelectedIndex);
                        break;
                    default:
                        break;
                }

                ret = sr_api.Set_rf_link_profile(rf_link_profile); // 获取频率区域

                if (OPER_OK == ret)
                {
                    oper_result = "Set RF LINK PROFILE success.";//";设置射频链路配置成功！
                }
                else
                {
                    oper_result = "Set RF LINK PROFILE fail.";//";设置射频链路配置失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }
        }

        private void Get_RF_Link_profile_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {
                int ret = OPER_OK;
                byte rf_link_profile = 0x0;

                ret = sr_api.Get_rf_link_profile(ref rf_link_profile); // 设置区域

                if (OPER_OK == ret)
                {
                    switch (rf_link_profile)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                            rf_link_profile_index.SelectedIndex = rf_link_profile;
                            break;
                        default:
                            break;
                    }

                    oper_result = "Get RF LINK PROFILE success.";//";获取射频链配置成功！
                }
                else
                {
                    oper_result = "Get RF LINK PROFILE fail.";//";获取射频链配置失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
    
 
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
   
        }

        private void qtpek_read_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {
                int ret = 0;
                // 1 获取访问密码
                uint password = 0;
                if (textBox_access_password.Text.Length > 0)
                {
                    if (textBox_access_password.Text.Length != 8)
                    {
                        oper_result = "Access Password shoult be 8 characters!";//";访问密码不少于8个字符!
                        UpdateLog(oper_result);
                        return;
                    }
                    else
                    {
                        //  password = uint.Parse(textBox_access_password.Text);
                        HexToDec(textBox_access_password.Text, ref password);
                    }
                }

                // 2 判断是否启动过滤，并 获取/设置 PC 和 EPC 的值
                byte[] epc = new byte[global.PACKET_128];
                byte epc_len = 0;
                byte filter_bak = 0x0;
                if (1 == filterbox.SelectedIndex)
                {
                    // 启动过滤，pc，epc不可为空
                    if (textBox_tag_epc.Text.Length <= 0)
                    {
                        oper_result = "Read Tag data fail, EPC cannot be null";//.";读取标签失败，指定过滤的EPC不能为空！
                        UpdateLog(oper_result);
                        return;
                    }
                    else
                    {
                        string temp_epc = textBox_tag_epc.Text;
                        if ((temp_epc.Length % 4) != 0)
                        {
                            int add_zero = temp_epc.Length % 4;
                            byte[] str_zero = new byte[add_zero];
                            temp_epc += str_zero.ToString();
                        }
                        hex2asc(temp_epc, epc, ref epc_len);

                        filter_bak = 0x1;
                    }
                }
                else if (2 == filterbox.SelectedIndex)
                {
                    // 启动过滤，pc，epc不可为空
                    if (textBox_tag_epc.Text.Length <= 0)
                    {
                        oper_result = "Read Tag data fail, EPC cannot be null.";//";读取标签失败，指定过滤的TID不能为空!
                        UpdateLog(oper_result);
                        return;
                    }
                    else
                    {
                        string temp_epc = textBox_tag_epc.Text;
                        if ((temp_epc.Length % 4) != 0)
                        {
                            int add_zero = temp_epc.Length % 4;
                            byte[] str_zero = new byte[add_zero];
                            temp_epc += str_zero.ToString();
                        }

                        hex2asc(temp_epc, epc, ref epc_len);

                        //pc = calculate_pc(temp_epc);
                        filter_bak = 0x1;
                    }
                }
                else
                {
                    // 不过滤，pc，epc都为空                
                    for (int index = 0; index < 12; index++)
                    {
                        epc[index] = 0x00;
                    }
                    epc_len = 0;   // 不过滤长度设置为0
                }

                // 3 获取 memory bank 的类型
                byte mem_bank = (byte)comboBox_mb.SelectedIndex;

                // 4 获取要读取的内存中的起始地址
                ushort temp_addr = ushort.Parse(textBox_head_address.Text);

                // 5 获取要读取的数据的长度
                ushort temp_len = ushort.Parse(textBox_data_len.Text);

                // 6 获取数据
                byte[] recv_buffer = new byte[global.PACKET_MID];

                ret = sr_api.qtpek_operat(password, filter_bak, epc_len, epc, READ_FLAG, 0, mem_bank, temp_addr, temp_len, recv_buffer); // 设置QT_PARAM参数

                if (OPER_OK == ret)
                {
                    oper_result = "QT PEK read success.";//";带 QT 标签读取成功！

                    string recv_str = BitConverter.ToString(recv_buffer, 0, temp_len*2).ToUpper().Replace("-", "");
                    textBox_data.Text = recv_str;
                }
                else
                {
                    oper_result = "QT PEK read fail.";//";带 QT 标签读取失败!
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }
        }

        private void qtpek_write_Click(object sender, EventArgs e)
        {
            string oper_result;
            try
            {
                int ret = 0;
                // 1 获取访问密码
                uint password = 0;
                if (textBox_access_password.Text.Length > 0)
                {
                    if (textBox_access_password.Text.Length != 8)
                    {
                        oper_result = "Access Password shoult be 8 characters!";//";访问密码少于8个字节！
                        UpdateLog(oper_result);
                        return;
                    }
                    else
                    {
                        //  password = uint.Parse(textBox_access_password.Text);
                        HexToDec(textBox_access_password.Text, ref password);
                    }
                }

                // 2 判断是否启动过滤，并 获取/设置 PC 和 EPC 的值
                byte[] epc = new byte[global.PACKET_128];
                byte epc_len = 0;
                byte filter_bak = 0x0;
                if (1 == filterbox.SelectedIndex)
                {
                    // 启动过滤，pc，epc不可为空
                    if (textBox_tag_epc.Text.Length <= 0)
                    {
                        oper_result = "Read Tag data fail, EPC cannot be null.";//";读取标签失败，指定过滤的EPC不能为空！
                        UpdateLog(oper_result);
                        return;
                    }
                    else
                    {
                        string temp_epc = textBox_tag_epc.Text;
                        if ((temp_epc.Length % 4) != 0)
                        {
                            int add_zero = temp_epc.Length % 4;
                            byte[] str_zero = new byte[add_zero];
                            temp_epc += str_zero.ToString();
                        }
                        hex2asc(temp_epc, epc, ref epc_len);

                        filter_bak = 0x1;
                    }
                }
                else if (2 == filterbox.SelectedIndex)
                {
                    // 启动过滤，pc，epc不可为空
                    if (textBox_tag_epc.Text.Length <= 0)
                    {
                        oper_result = "Read Tag data fail, EPC cannot be null.";//";读取标签失败，指定过滤的TID不能为空！
                        UpdateLog(oper_result);
                        return;
                    }
                    else
                    {
                        string temp_epc = textBox_tag_epc.Text;
                        if ((temp_epc.Length % 4) != 0)
                        {
                            int add_zero = temp_epc.Length % 4;
                            byte[] str_zero = new byte[add_zero];
                            temp_epc += str_zero.ToString();
                        }

                        hex2asc(temp_epc, epc, ref epc_len);

                        //pc = calculate_pc(temp_epc);
                        filter_bak = 0x1;
                    }
                }
                else
                {
                    // 不过滤，pc，epc都为空                
                    for (int index = 0; index < 12; index++)
                    {
                        epc[index] = 0x00;
                    }
                    epc_len = 0;   // 不过滤长度设置为0
                }

                // 3 写入数据 memory bank 的类型
                byte mem_bank = (byte)comboBox_mb.SelectedIndex;

                // 4 写的数据在内存中的起始地址
                ushort temp_addr = ushort.Parse(textBox_head_address.Text);

                // 5 写的数据的长度
                ushort temp_len = ushort.Parse(textBox_data_len.Text);

                // 6 获取数据
                byte[] write_buffer = new byte[global.PACKET_MID];
                byte write_len = 0;
                hex2asc(textBox_data.Text, write_buffer, ref write_len);

                ret = sr_api.qtpek_operat(password, filter_bak, epc_len, epc, WRITE_FLAG, 0, mem_bank, temp_addr, temp_len, write_buffer); // 设置QT_PARAM参数

                if (OPER_OK == ret)
                {
                    oper_result = "QT PEK write success.";//";向带 QT 标签写入数据成功！
                }
                else
                {
                    oper_result = "PEK write fail.";//";向带 QT 标签写入数据失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
   
        }

        private void Get_ant_mes_Click(object sender, EventArgs e) //获取天线工作时间及天线2015-03-10
        {
            byte  ant = 0;
            int result = -1;
            int res = -1;

            bool ant_flag = false;
            bool ant_time = false;

            result = sr_api.Get_Work_Antanne(ref ant);

            if (result == OPER_OK)
            { 
                int ant1=0;
                int ant2=0;
                int ant3=0;
                int ant4=0;

                ant1 = ant & 0x01;
                ant2 = ant & 0x02;
                ant3 = ant & 0x04;
                ant4 = ant & 0x08;

                ant_flag = true;


                if (ant1 == 1)
                {
                    this.check_ant1.Checked = true;
                    this.check_ant1.Refresh();
                }
                else
                {
                    this.check_ant1.Checked = false;
                    this.check_ant1.Refresh();
                }

                if (ant2 == 2)
                {
                    this.check_ant2.Checked = true;
                    this.check_ant2.Refresh();
                }
                else
                {
                    this.check_ant2.Checked = false;
                    this.check_ant2.Refresh();
                }

                if (ant3 == 4)
                {
                    this.check_ant3.Checked = true;
                    this.check_ant3.Refresh();
                }
                else
                {
                    this.check_ant3.Checked = false;
                    this.check_ant3.Refresh();
                }

                if (ant4 == 8)
                {
                    this.check_ant4.Checked = true;
                    this.check_ant4.Refresh();
                }
                else
                {
                    this.check_ant4.Checked = false;
                    this.check_ant4.Refresh();
                }
            
            }

            //获取天线工作时间
            ushort ant1_wk_time = 0;
            ushort ant2_wk_time = 0;
            ushort ant3_wk_time = 0;
            ushort ant4_wk_time = 0;
            ushort wait_time = 0; 

            res = sr_api.Get_antenna_worktime_and_waittime(ref ant1_wk_time, ref ant2_wk_time, ref ant3_wk_time, ref ant4_wk_time, ref wait_time);

            if (res == OPER_OK)
            {
                ant_time = true;

                //获取设置天线工作时间
                this.ant1_text.Text = ant1_wk_time.ToString();
                this.ant1_text.Refresh();

                this.ant2_text.Text = ant2_wk_time.ToString();
                this.ant2_text.Refresh();

                this.ant3_text.Text = ant3_wk_time.ToString();
                this.ant3_text.Refresh();

                this.ant4_text.Text = ant4_wk_time.ToString();
                this.ant4_text.Refresh();

                this.wait_text.Text = wait_time.ToString();
                this.wait_text.Refresh();
            }

            string str = null;

            res = 0;

            if (!ant_flag && !ant_time)
                res = 1;
            else
                if (!ant_flag)
                    res = 2;
                else
                    if (!ant_time)
                        res = 3;


            switch (res)
            {

                case 0:
                            str = "Obtain antenna number and antenna working time success!";//获取天线号及天线工作时间成功！
                            break;

                case 1:
                            str = " Obtain antenna number and antenna working time failure !";//获取天线号及天线工作时间失败！
                            break;

                case 2:
                            str = " Get the antenna failure!"; //获取天线号失败！
                            break;

                case 3:
                            str = " Get the antenna failure!";
                            break;

                default :
                    break;            
            
            }

            UpdateLog(str);

          //  MessageBox.Show(" ant = " + ant.ToString(), "ant-ant");

        }

        private void timer_auto_Tick(object sender, EventArgs e)
        {
            if (startOK)  //2015-03-12-00
            {
                startOK = false;
                this.timer_auto.Enabled = false;

                this.timer_scan.Enabled = true;
               
                multi_query();
            }

        }
 //--------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------



        private void button5_Click(object sender, EventArgs e) //2015-03-11-11
        {
            /*
           // Microsoft.Win32.RegistryKey regostrKey;
            string name = "LogOutputERecord";
            string str = null;
            str=GetRegistData(name);
            MessageBox.Show(" str = " + str, "RegeDilt");
            */


        }

        private void SrDemo_Load(object sender, EventArgs e)
        {
            /*
            try
            {
                //string startName = "C:\\Program Files\\SanRayDemo\\Boot.xml";//null;
               // this.Read_RegistryKey();  //2015-03-12-00

                if (Read_RegistryKey())//2015-03-12-00
                {   
                    startOK=true ;
                    this.button_query.Text = MULTI_START_LABEL;

                    this.timer_auto.Enabled = true;

                }

            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "信息提示");
            
            }
             * */


        }

        private void default_but_Click(object sender, EventArgs e) //默认参数值恢复　20150407
        {

            string oper_result = "string.Empty;";//恢复部分默认出厂参数如下： 

            UpdateLog(oper_result);

            string readPower = "30";
            string writePower = "30";

            int ret = 0;
            byte loop = 0x00;  //默认为开闭环

            byte read = byte.Parse(readPower);
            byte write = byte.Parse(writePower);

            ret = sr_api.Set_Power(loop, read, write); // 设置读写功率

            if (OPER_OK == ret)
            {
                this.comboBox_write_power.Text = writePower;
                this.comboBox_read_power.Text = readPower;

                this.comboBox_write_power.Refresh();
                this.comboBox_read_power.Refresh();

                oper_result = "Set Power success.";//";恢复功率出厂设置成功！
            }
            else
            {
                oper_result = "Restore power factory Settings failure!";//恢复功率出厂设置失败！
            }
            UpdateLog(oper_result);

            //-------------------------------------------频率区域
            ret = 0;
            byte save_setting = 0;
            byte fequency_region = 2;
            oper_result = string.Empty;

            ret = sr_api.Set_frequency(save_setting, fequency_region); // 获取频率区域  0,2

            if (OPER_OK == ret)
            {
                this.comboBox_region.Text = "China2";//PR _ASK / Miller4 / 250KHZ";

                this.comboBox_region.Refresh();

                oper_result = "Restore frequency area factory setting up success!";//恢复频率区域出厂设置成功！
            }
            else
            {
                oper_result = "Restore frequency area factory failure!";//恢复频率区域出厂设置失败！
            }
            UpdateLog(oper_result);

            //-----------------------------------------------天线

            ret = 0;
            byte ants = 0x01;
            oper_result = string.Empty;

            ret = sr_api.Set_Work_Antanne(ants);//默认工作天线一

            if (OPER_OK == ret)
            {
                oper_result = "Restore antenna factory setting to success!";//恢复天线出厂设置成功！

                // 2 设置天线工作时间
               // set_ant_worktime(); 　天线工作时间不用设置
            }
            else
            {
                oper_result = "Restore antenna factory setting to failure!";//恢复天线出厂设置失败！
            }
            UpdateLog(oper_result);

            //---------------------------------------------------------频率区域

            ret = 0;

            oper_result = string.Empty;

            byte rf_link_profile = 0x01; //1

            ret = sr_api.Set_rf_link_profile(rf_link_profile); // 获取频率区域 250KHZ

            if (OPER_OK == ret)
            {
                this.rf_link_profile_index.Text = "PR _ASK / Miller4 / 250KHZ";
                this.rf_link_profile_index.Refresh();

                oper_result = "Restore the rf link configuration to the factory Settings success!";//恢复射频链路配置出厂设置成功！
            }
            else
            {
                oper_result = "Restore the rf link configuration failure!";//恢复射频链路配置出厂设置失败！
            }
            UpdateLog(oper_result);
            //--------------------------------------------------------------------------// 设置波特率
            ret = 0;
            oper_result = string.Empty;

            byte rate_type = (byte)(4);

            ret = sr_api.Set_module_baud_rate(rate_type); // 设置波特率

            if (OPER_OK == ret)
            {
                oper_result = "Reinstate the baud rate to set the success!";//恢复波特率出厂设置成功！
            }
            else
            {
                oper_result = "Failed to restore the potter rate!";//恢复波特率出厂设置失败！
            }
            UpdateLog(oper_result);

            //-------------------------------------------------------------------------------
            oper_result = "Restore some default factory parameter completion!";//恢复部分默认出厂参数完成！
            UpdateLog(oper_result);
            //-------------------------------------------------------------------------------

        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            string oper_result;
            try
            {
                int ret = 0;

                byte fastid_switch = 0x0;

                if (true == radioButton_fastid_open.Checked)
                {
                    fastid_switch = 0x01; // 开启
                }
                else
                {
                    fastid_switch = 0x01; //关闭
                }

                ret = sr_api.Set_TagFocus(fastid_switch); // 设置FASTID

                if (OPER_OK == ret)
                {
                    oper_result = "Set  FastID success.";//";设置 TagFocus 成功！
                }
                else
                {
                    oper_result = "Set  FastID fail.";//";设置 TagFocus 失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }

        }

        private void get_tagFocus_but_Click(object sender, EventArgs e) //获取TagFocus
        {
            string oper_result;
            try
            {
                int ret = 0;
                byte tagFocus_switch = 0x0;

                ret = sr_api.Get_TagFocus(ref tagFocus_switch);

                if (OPER_OK == ret)
                {
                    switch (tagFocus_switch)
                    {
                        case global.TAGFOCUS_ON :

                            this.start_tagFocus_radioBut.Checked = true;
                            this.stop_tagFocus_radioBut.Checked = false;
                            break;

                        case global.TAGFOCUS_OFF:

                            this.start_tagFocus_radioBut.Checked = false;
                            this.stop_tagFocus_radioBut.Checked = true;

                            break;

                        default:
                            oper_result = "Data Error.";
                            break;
                    }

                    oper_result = "Get  tagFocus success.";//Get  tagFocus success.";获取 tagFocus  状态成功！
                }
                else
                {
                    oper_result = "Get tagFocus fail.";//Get tagFocus fail.";获取 tagFocus  状态失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }

        }

        private void set_tagFocus_but_Click(object sender, EventArgs e) //设置TagFocus
        {
            string oper_result;
            try
            {
                int ret = 0;

                byte tagFocus_switch = 0x0;

                if (true == this.start_tagFocus_radioBut.Checked)
                {
                    tagFocus_switch = 0x01; // 开启
                }
                else
                {
                    tagFocus_switch = 0x00; //关闭
                }

                ret = sr_api.Set_TagFocus(tagFocus_switch); // 设置FASTID

                if (OPER_OK == ret)
                {
                    oper_result = "Set TagFocus success.";//Set TagFocus success.";设置 TagFocus 成功！
                }
                else
                {
                    oper_result = "Set  TagFocus fail.";//Set  TagFocus fail.";设置 TagFocus 失败！
                }
                UpdateLog(oper_result);
            }
            catch (Exception ex)
            {
                oper_result = "Operation Error :" + ex.Message;
                UpdateLog(oper_result);
            }

        }

        private void textBox_module_ip_TextChanged(object sender, EventArgs e)
        {

        }

        private void listView_label_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label29_Click(object sender, EventArgs e)
        {

        }

//----------------------------------------------------------------------------------------------------------------------------------

        
    }
}
