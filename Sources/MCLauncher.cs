﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.VersionLoader;

namespace VRPLancher
{
    class MCLauncher
    {
        private void setLauncher(bool enabled)
        {
            Central.launcherGUI.launch.Enabled = enabled;
        }
        private void setStatus  (string status) {
            Central.launcherGUI.statusLabel.Text = status;
        }
        public static readonly string pathToMCFolder = Environment.CurrentDirectory + @"\minecraft";
        public static readonly string pathToJava = Environment.CurrentDirectory + @"\bin\jre1.8.0_301\bin";
        public static readonly string profileName = "1.16.4";
        public static readonly MinecraftPath minecraftPath = new MinecraftPath(pathToMCFolder);

        /// <summary>
        /// <h2>Config reference</h2> <br/>
        /// 
        /// <list type="bullet">
        /// <item> (Mojang auth true/false) </item>
        /// <item> (nick or email) </item>
        /// <item> (password) </item>
        /// <item> (close launcher?) </item>
        /// <item> (max RAM) </item>
        /// </list>
        /// 
        /// Example:<br/>
        /// <code>
        /// launchNow(new string[] {"true", "creeperywime@gmail.com", "myPassword", "false", "2048"}); // Mojang login
        /// launchNow(new string[] {"false", "BlackLightHack", "", "false", "2048"}); // no login
        /// </code>
        /// </summary>
        /// <param name="config"></param>
        public void launchNow(string[] config) {
            #region Generate session
            MSession session;
            setStatus("Процесс запуска запущен...");
            bool closeLauncher = false;
            if (config[0] == "true") {
                string email = config[1];
                string pswd = config[2];
                closeLauncher = config[3] == "true";

                string blurredPswd = "";
                foreach (var item in pswd) {
                    blurredPswd += "*";
                }
                setStatus($"Попытка аутентификации: {email}, {blurredPswd}");
                blurredPswd = null;

                var login = new MLogin();
                var response = login.Authenticate(email, pswd);

                if (!response.IsSuccess) {
                    setStatus("Аутентификация не удалась!");
                    MessageBox.Show("Неверный логин или пароль!");
                    setLauncher(true);
                    return;
                }
                setStatus("Вход успешный!");
                session = response.Session;

            } else {
                string nick = config[1];
                closeLauncher = config[3] == "true";

                setStatus($"Используется ник {nick}");
                session = MSession.GetOfflineSession(nick);
            }
            System.Net.ServicePointManager.DefaultConnectionLimit = 256;
            #endregion
            #region Config parser
            int ram;
            try
            {
                int.Parse(config[4]);
            }
            catch (Exception) {
                setStatus("Введено неверное значение RAM!");
                MessageBox.Show("RAM может быть только цифрой!");
                setLauncher(true);
                return;
            }
            ram = int.Parse(config[4]);
            int maxram = Central.maxram;

            if (ram > maxram)
            {
                setStatus("Выделено больше RAM чем есть в компьютере!");
                MessageBox.Show("Вы выделили Майнкрафту больше памяти чем есть в вашем компьютере!\nВы выделили: " + ram + " MB\nИ у вас есть: " + maxram);
                setLauncher(true);
                return;
            }
            setStatus("Очистка памяти...");
            System.GC.Collect();
            #endregion
            #region Create launcher & launch
            var launcher = new CMLauncher(minecraftPath);

            var launchOption = new MLaunchOption
            {
                MaximumRamMb = ram,
                Session = session,
                ScreenWidth = 640,
                ScreenHeight = 640,
                Path = minecraftPath,
                
                //ServerIp = "mc.vanillarp.fun",
            };
            setStatus("Запуск Майнкрафта...");
            var process = launcher.CreateProcess("fabric-loader-0.11.6-1.16.4", launchOption);

            setLauncher(true);
            process.Start();
            setStatus("Майнкрафт запущен! Ура!");
            #endregion

            DialogResult dialogResult = MessageBox.Show(null, "Вы можете закрывать лаунчер.", "Процесс с Майнкрафтом запущен", MessageBoxButtons.YesNo);
            
            if (dialogResult.Equals(DialogResult.Yes)) { // If user preferred to close launcher
                setStatus("Лаунчер закрывается...");
                Application.Exit();
            }

            launcher.ProgressChanged += (s, e) => { Central.launcherGUI.progress = e.ProgressPercentage; };

            if (closeLauncher)
            {
                setStatus("Лаунчер закрывается...");
                Application.Exit();
            }
        }
    }
}
