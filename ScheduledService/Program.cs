using Newtonsoft.Json;
using ScheduledService;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ScheduledService
{
    class Program
    {
        static void Main(string[] args)
        {
            Timer timer = new Timer(86400000.0);
            timer.Elapsed += new ElapsedEventHandler(Program.timer1);
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Start();
            Starttimer();
            backup();
            Console.ReadKey();
        }
        private static void timer1(object source, ElapsedEventArgs e)
        {
            Starttimer();
            backup();
        }
        private static void SendMess(RoomModel room)
        {
            HttpResquet resquet = new HttpResquet();
            var type = JsonConvert.DeserializeAnonymousType(resquet.HttpGet("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=wx4ce973a2bee533d8&secret=00e36484ccb55a9ac858df55f0ca7328"), new
            {
                errcode = 0,
                access_token = "",
                errmsg = ""
            });
            if (type.errcode == 0)
            {
                string postUrl = "https://api.weixin.qq.com/cgi-bin/message/subscribe/send?access_token=" + type.access_token;
                Sendmodel sendmodel = new Sendmodel();
                sendmodel.template_id = "Axdwzrx9NdoJVc5nXOAOIOiBQ7IgGz7593rtJvdak8k";
                sendmodel.touser = room.tenant.OpenId;
                sendmodel.page = "";
                Senddata senddata1 = new Senddata();
                dataItem item1 = new dataItem();
                item1.value = Convert.ToDateTime(room.room.PayRentDate).ToString("yyyy-MM-dd");
                senddata1.date3 = item1;
                dataItem item2 = new dataItem();
                item2.value = "***";
                senddata1.name4 = item2;
                dataItem item3 = new dataItem();
                item3.value = "￥" + room.room.Rent.ToString();
                senddata1.amount2 = item3;
                dataItem item4 = new dataItem();
                item4.value = room.Community + room.Building + room.room.Name;
                senddata1.thing1 = item4;
                sendmodel.data = senddata1;
                var type2 = JsonConvert.DeserializeAnonymousType(resquet.HttpPost(postUrl, JsonConvert.SerializeObject(sendmodel), Encoding.UTF8), new
                {
                    errcode = 0,
                    errmsg = ""
                });
                if (type2.errcode == 0)
                {
                    Console.WriteLine("[roomId:" + room.room.Id + "]发送完成.");
                }
                else
                {
                    Console.WriteLine(string.Concat(new object[] { "[roomId:", room.room.Id, "]发送失败:", type2.errmsg }));
                }
            }
            else
            {
                Console.WriteLine(string.Concat(new object[] { "获取token失败:", type.errmsg, "(时间:", DateTime.Now, ")" }));
            }
        }
        private static void Starttimer()
        {
            List<RoomModel> source = new List<RoomModel>();
            Console.WriteLine(string.Concat(new object[] { "开始批量发送", Enumerable.Count<RoomModel>(source), "条交租信息...(时间:", DateTime.Now, ")" }));
            foreach (RoomModel model in source)
            {
                if ((model.tenant != null) && !string.IsNullOrEmpty(model.tenant.OpenId))
                {
                    SendMess(model);
                }
            }
            Console.WriteLine("批量发送完成。");
        }

        private static void backup()
        {
            SqlConnection connection = new SqlConnection("Server=.; Database=HouseManage; User ID=sa; Password=abc@123;");
            SqlCommand command = new SqlCommand();
            command.CommandType = CommandType.Text;
            command.Connection = connection;
            command.CommandText = @"backup database [HouseManage] to disk='C:\DataBackup\\" + DateTime.Now.ToString("MMdd") + ".bak'with init";
            try
            {
                connection.Open();
                command.ExecuteNonQuery();
                Console.WriteLine("备份成功");
            }
            catch (Exception exception)
            {
                Console.WriteLine("备份失败:" + exception.Message);
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }

    }
}
