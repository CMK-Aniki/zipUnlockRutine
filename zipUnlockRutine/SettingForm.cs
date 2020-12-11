using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Security.Cryptography;

namespace zipUnlockRutine {
    public partial class SettingForm : Form {
        public SettingForm() {
            InitializeComponent();
        }
        public string cipherStr;
        public string defaltPath;
        public string saveXMLPath = Path.GetDirectoryName(
                    Path.GetFullPath(Environment.GetCommandLineArgs()[0])) + "\\PassRutinSetting.xml";

        /// <summary>
        /// Set ButtonClickEvent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetButton_Click(object sender, EventArgs e) {
            //tab,return etc...Nothing
            MessageBox.Show("Blank is Deleted.\n空白は自動削除されます");
            string getTxtBox = passwordBox.Text.Trim();
            if(getTxtBox.Length == 0) {
                MessageBox.Show("Please Set Zip Password.\nパスワードを入力してください",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            } else {
                string AES_IV = Properties.Resources.AES_IV_Resoce;
                string AES_Key = Properties.Resources.AES_Key_Resoce;
                string cipher = Encrypt(getTxtBox, AES_IV, AES_Key);
                cipherStr = cipher;
            }
        }

        /// <summary>
        /// encryption
        /// 暗号化
        /// </summary>
        /// <param name="text"></param>
        /// <param name="iv"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private string Encrypt(string text, string iv, string key) {
            using(RijndaelManaged rijndael = new RijndaelManaged()) {
                rijndael.BlockSize = 128;
                rijndael.KeySize = 128;
                rijndael.Mode = CipherMode.CBC;
                rijndael.Padding = PaddingMode.PKCS7;
                rijndael.IV = Encoding.UTF8.GetBytes(iv);
                rijndael.Key = Encoding.UTF8.GetBytes(key);
                ICryptoTransform encryptor = rijndael.CreateEncryptor(rijndael.Key, rijndael.IV);
                byte[] encrypted;
                using(MemoryStream mStream = new MemoryStream()) {
                    using(CryptoStream ctStream = new CryptoStream(mStream, encryptor, CryptoStreamMode.Write)) {
                        using(StreamWriter sw = new StreamWriter(ctStream)) {
                            sw.Write(text);
                        }
                        encrypted = mStream.ToArray();
                    }
                }
                return (Convert.ToBase64String(encrypted));
            }
        }

        /// <summary>
        /// pathBox is Clicked => Focus SetPathBtn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clicbox(object sender, MouseEventArgs e) {
            pathBtn.Focus();
        }

        private void pathBtn_Click(object sender, EventArgs e) {
            //Make FolderBrowserDialog Class  And Auto Dispose
            using(FolderBrowserDialog fbd = new FolderBrowserDialog()) {
                //FolderDialogName
                fbd.Description = "Set your Folder.\n出力先Pathを設定してください。";
                //Desktop is Defalt
                fbd.RootFolder = Environment.SpecialFolder.Desktop;
                //RootFolder under folder
                fbd.SelectedPath = @"C:\Windows";
                //Make new Folder
                fbd.ShowNewFolderButton = true;
                //Show Dialog
                if(fbd.ShowDialog(this) == DialogResult.OK) {
                    //Set selectedPath
                    pathBox.Text = fbd.SelectedPath;
                    defaltPath = fbd.SelectedPath;
                }
            }
        }

        /// <summary>
        /// ApplicationLoadEvent
        /// アプリ起動時動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingForm_Load(object sender, EventArgs e) {
            /* Read XML
             * Use Linq
             * After Linq is Gotten Path
             */
            try {
                XElement dataElem = XElement.Load(saveXMLPath);
                var readPath = from item
                              in dataElem.Elements("Path").Elements("SavePath")
                               select item.Value;
                foreach(string getPath in readPath) {
                    pathBox.Text = getPath;
                    defaltPath = getPath;
                }
            }
            catch(FileNotFoundException) {
                pathBox.Text = @"C:\";
            }
        }

        private void FClosing(object sender, FormClosingEventArgs e) {
            if(passwordBox.Text ==string.Empty || pathBox.Text == string.Empty) {
                MessageBox.Show("Password do not write or OutputPath is not selected.\nパスワードが書かれていないか出力先Pathの選択がありません。");
                e.Cancel = true;
            } else {
                XElement iniXML =
                    new XElement("iniXML",
                        new XElement("Path",
                            new XElement("SavePath", defaltPath)
                        ),
                        new XElement("Password",
                            new XElement("SavePassword", cipherStr)
                        )
                    );
                using(FileStream fs = new FileStream(saveXMLPath, FileMode.Create)) {
                    iniXML.Save(fs);
                }
            }
        }
    }
}