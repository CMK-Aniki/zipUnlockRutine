using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.IO;
using System.Xml.Linq;
using System.Security.Cryptography;

namespace zipUnlockRutine {
    class Program {
        [STAThread]
        static void Main(string[] args) {
            string[] files = Environment.GetCommandLineArgs();
            if(files.Length ==1) {
                SettingForm sf = new SettingForm();
                Application.EnableVisualStyles();
                Application.Run(sf);
            } else if(files.Length == 2) {
                string zipFileName = files[1];
                if(!zipFileName.EndsWith(".zip")) {
                    MessageBox.Show("ImportErorr.Please ZIP import.\nZIPでないファイルが読み込まれました");
                    return;
                }
                List<string> a = new List<string>();
                string xmlPath = Path.GetDirectoryName(Path.GetFullPath(Environment.GetCommandLineArgs()[0])) + "\\PassRutinSetting.xml";
                string AES_IV = Properties.Resources.AES_IV_Resoce;
                string AES_Key = Properties.Resources.AES_Key_Resoce;
                /* Read XML
            * Use Linq
            * After Linq is Gotten Path
            */
                XElement dataElem = XElement.Load(xmlPath);
                var readpassword = from item
                              in dataElem.Elements("Password").Elements("SavePassword")
                               select item.Value;
                foreach(string getPassword in readpassword) {
                    string Decrystr = Decrypt(getPassword, AES_IV, AES_Key);
                    a.Add(Decrystr);
                }
                var readPath = from item
                          in dataElem.Elements("Path").Elements("SavePath")
                               select item.Value;
                foreach(string getPath in readPath) {
                    string targetDirectory = getPath;
                    a.Add(targetDirectory);
                }
                string[] ltoa = a.ToArray();
                string fileFilter = "";
                ICSharpCode.SharpZipLib.Zip.FastZip fastZip = new ICSharpCode.SharpZipLib.Zip.FastZip();
                fastZip.RestoreAttributesOnExtract = true;
                fastZip.RestoreDateTimeOnExtract = true;
                fastZip.CreateEmptyDirectories = true;
                fastZip.Password = ltoa[0];
                fastZip.ExtractZip(zipFileName, ltoa[1], fileFilter);
            }
        }
        /// <summary>
        /// Decryption
        /// 復号化
        /// </summary>
        /// <param name="chip"></param>
        /// <param name="iv"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string Decrypt(string chip, string iv, string key) {
            using(RijndaelManaged rijndael = new RijndaelManaged()) {
                rijndael.BlockSize = 128;
                rijndael.KeySize = 128;
                rijndael.Mode = CipherMode.CBC;
                rijndael.Padding = PaddingMode.PKCS7;

                rijndael.IV = Encoding.UTF8.GetBytes(iv);
                rijndael.Key = Encoding.UTF8.GetBytes(key);

                ICryptoTransform decryptor = rijndael.CreateDecryptor(rijndael.Key, rijndael.IV);

                string plain = string.Empty;
                using(MemoryStream mStream = new MemoryStream(Convert.FromBase64String(chip))) {
                    using(CryptoStream ctStream = new CryptoStream(mStream, decryptor, CryptoStreamMode.Read)) {
                        using(StreamReader sr = new StreamReader(ctStream)) {
                            plain = sr.ReadLine();
                        }
                    }
                }
                return plain;
            }
        }
    }
}
