namespace mklink程序
{
    using System.Diagnostics;
    using System.ComponentModel;
    using System.IO;
    public class MkLink
    {
        const int Eorro_One = 2;
        const int Eorro_Two = 5;
        private string MoveDir;
        private string target;
        private string _Dir;
        public void ChooseModel()
        {
            Console.WriteLine("请选择模式\n1:为先将文件移动到目标位置再创建连接\n2:在目标位置创建链接，不移动文件夹");
            string Select = Console.ReadLine();
            if (Select.ToLower() == "1")
            {
                GetMassage();
                Move();
                RunCmd();
            }
            else if (Select.ToLower() == "2")
            {
                GetMassage();
                string temp = MoveDir;
                MoveDir = target + _Dir;
                target = temp;
                _Dir = "";
                RunCmd();
            }
            else
            {
                Console.WriteLine("请输入正确的数字");
            }
        }

        private void GetMassage()
        {
            Console.WriteLine("请输入要移动的文件夹");
            MoveDir = Console.ReadLine();
            Console.WriteLine("请输入要移动到的文件夹");
            target = Console.ReadLine();
            if (MoveDir == "" || MoveDir == null || target == "" || target == null)
            {
                Console.WriteLine("请输入正确的位置\n按ESC退出程序\n按Q返回第一步\n其他任意键重新开始此步骤");
                ConsoleKeyInfo key = Console.ReadKey();
                if (key.Key == ConsoleKey.Escape)
                {
                    Environment.Exit(0);
                }
                else if (key.Key == ConsoleKey.Q)
                {
                    ChooseModel();
                }
                else
                {
                    GetMassage();
                }
                return;
            }
            string[] temp = MoveDir.Split("\\");

            _Dir = "\\" + temp[temp.Length - 1];
        }

        /// <summary>
        /// 执行CMD命令
        /// </summary>
        private void RunCmd()
        {
            Process process = new Process();
            Console.WriteLine(MoveDir + "\t" + target + _Dir);
            try
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                process.StandardInput.WriteLine("mklink /j " + "\"" + MoveDir + "\"" + " " + "\"" + target + _Dir + "\"");
                process.StandardInput.AutoFlush = true;
                process.StandardInput.WriteLine("exit");

                #region 可以把CMD显示的东西显示在程序上
                // StreamReader reader = process.StandardOutput;

                // string Curline = reader.ReadLine();

                // while (!reader.EndOfStream)
                // {
                //     if (!String.IsNullOrEmpty(Curline))
                //     {
                //         Console.WriteLine(Curline);
                //     }
                //     Curline = reader.ReadLine();
                // }

                // reader.Close();
                #endregion

                process.WaitForExit();
                process.Close();
            }
            catch (Win32Exception e)
            {
                if (e.NativeErrorCode == Eorro_One)
                {
                    Console.WriteLine(e.Message + "路径错误");
                }
                if (e.NativeErrorCode == Eorro_Two)
                {
                    Console.WriteLine(e.Message + "你没有权限");
                }
            }

            Console.WriteLine("按任意键结束");
            Console.ReadKey();
        }

        /// <summary>
        /// 移动文件操作
        /// </summary>
        /// <param name="MoveDir"></param>
        /// <param name="target"></param>
        /// <param name="_Dir"></param>
        private void Move()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(MoveDir);
            string[] strfilename = Directory.GetFiles(MoveDir);
            if (Directory.Exists(target + _Dir))
            {
                Console.WriteLine("目标位置已存在同名文件夹");
                return;
            }
            else
            {
                try
                {
                    Console.WriteLine("正在移动请稍后");

                    // Directory.CreateDirectory(target + _Dir);
                    // for (var i = 0; i < strfilename.Length; i++)
                    // {
                    //     // Console.WriteLine(strfilename[i]);

                    //     string[] templist = strfilename[i].Split("\\");

                    //     File.Copy(strfilename[i], target + _Dir + "\\" + templist[templist.Length - 1]);
                    // }

                    CopyToTarget(MoveDir, target, _Dir);

                    Console.WriteLine("复制完成");
                    Console.WriteLine("开始删除源文件");
                    DeleteOrigin(MoveDir, target, _Dir);
                    Console.WriteLine("删除完毕");
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.Message + "传输问题");
                    Console.ReadKey();
                }

            }
        }

        /// <summary>
        /// 遍历文件夹里所有文件，并且递归删除文件夹
        /// </summary>
        /// <param name="MoveDir">移动文件</param>
        /// <param name="target">目标父文件</param>
        /// <param name="_Dir">目标文件名</param>

        private void CopyToTarget(string MoveDir, string target, string _Dir)
        {
            //先判断文件夹里是否还有文件夹，搬运。
            if (Directory.GetDirectories(MoveDir).Length != 0)
            {
                string[] dirlist = Directory.GetDirectories(MoveDir);

                foreach (var item in dirlist)
                {

                    string dirname = "\\" + item.Split("\\")[item.Split("\\").Length - 1];

                    Directory.CreateDirectory(target + _Dir + dirname);
                    Console.WriteLine(target + _Dir + dirname);

                    CopyToTarget(MoveDir + dirname, target + _Dir, dirname);
                }
            }

            string[] filelist = Directory.GetFiles(MoveDir);

            if (!Directory.Exists(target + _Dir))
            {
                Directory.CreateDirectory(target + _Dir);
            }
            for (var i = 0; i < filelist.Length; i++)
            {
                string strfilename = "\\" + filelist[i].Split("\\")[filelist[i].Split("\\").Length - 1];//获取文件名
                if (File.Exists(target + _Dir + strfilename))
                {
                    Console.WriteLine("出现错误请把目标文件删除重试，不是源文件\n按任意键退出");
                    System.Environment.Exit(System.Environment.ExitCode);

                }
                Console.WriteLine("正在把 " + MoveDir + strfilename + " 复制到 " + target + _Dir + strfilename);
                File.Copy(filelist[i], target + _Dir + strfilename);//完成文件拷贝
                // File.Delete(filelist[i]);
            }

            //执行完后删除文件，退出递归。
            // Directory.Delete(MoveDir);
            //可以遍历删除文件，设置为true的话
            // Directory.Delete(MoveDir, true);
            return;
        }

        /// <summary>
        /// 删除源文件
        /// </summary>
        /// <param name="MoveDir"></param>
        /// <param name="target"></param>
        /// <param name="_Dir"></param>
        private void DeleteOrigin(string MoveDir, string target, string _Dir)
        {
            //先判断文件夹里是否还有文件夹，搬运。
            if (Directory.GetDirectories(MoveDir).Length != 0)
            {
                string[] dirlist = Directory.GetDirectories(MoveDir);

                foreach (var item in dirlist)
                {
                    string dirname = "\\" + item.Split("\\")[item.Split("\\").Length - 1];

                    // Directory.CreateDirectory(target + _Dir + dirname);
                    // Console.WriteLine(target + _Dir + dirname);

                    DeleteOrigin(MoveDir + dirname, target + _Dir, dirname);
                }
            }

            string[] filelist = Directory.GetFiles(MoveDir);

            if (!Directory.Exists(target + _Dir))
            {
                Directory.CreateDirectory(target + _Dir);
            }
            for (var i = 0; i < filelist.Length; i++)
            {
                string strfilename = "\\" + filelist[i].Split("\\")[filelist[i].Split("\\").Length - 1];//获取文件名
                Console.WriteLine("正在删除" + MoveDir + strfilename);
                //File.GetAttributes(filelist[i]).ToString().IndexOf("ReadOnly") != -1
                if ((File.GetAttributes(filelist[i]) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    // Console.WriteLine(filelist[i] + "是只读文件");
                    // Console.WriteLine(File.GetAttributes(filelist[i]) & FileAttributes.ReadOnly);
                    // Console.WriteLine(File.GetAttributes(filelist[i]) + "\t" + FileAttributes.ReadOnly);
                    // Console.WriteLine(File.GetAttributes(filelist[i]) == FileAttributes.ReadOnly);
                    // Console.WriteLine(File.GetAttributes(filelist[i]));
                    // Console.WriteLine(FileAttributes.ReadOnly);
                    // Console.WriteLine(File.GetAttributes(filelist[i]).ToString());
                    // Console.WriteLine(FileAttributes.ReadOnly.ToString());
                    File.SetAttributes(filelist[i], FileAttributes.Normal);
                    // Console.WriteLine(File.GetAttributes(filelist[i]));
                    // Console.ReadKey();
                }
                File.Delete(filelist[i]);
            }

            //执行完后删除文件，退出递归。
            if ((File.GetAttributes(MoveDir) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                File.SetAttributes(MoveDir, FileAttributes.Normal);
            }
            Directory.Delete(MoveDir);
            return;
        }
    }
}