using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DllLabWork
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        //
        // DLL Stuff
        //

        [DllImport("kernel32.dll")]
        public static extern bool GetComputerName(
                [Out] char[] lpBuffer,
                ref uint lpnSize
            );

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool GetUserName(
        StringBuilder lpBuffer,
        ref uint lpnSize);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern uint GetSystemDirectory(
        StringBuilder lpBuffer,
        uint uSize);

        [StructLayout(LayoutKind.Sequential)]
        public struct OSVERSIONINFOEX
        {
            public int dwOSVersionInfoSize;
            public int dwMajorVersion;
            public int dwMinorVersion;
            public int dwBuildNumber;
            public int dwPlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szCSDVersion;
            public short wServicePackMajor;
            public short wServicePackMinor;
            public short wSuiteMask;
            public byte wProductType;
            public byte wReserved;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetVersionEx(ref OSVERSIONINFOEX lpVersionInfo);

        public const int SM_CLEANBOOT = 67;

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);

        public const uint SPI_GETCONTRAST = 0x0014;
        const uint SPI_SETDESKWALLPAPER = 0x0014;
        const uint SPIF_UPDATEINIFILE = 0x01;
        const uint SPIF_SENDCHANGE = 0x02;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SystemParametersInfo(
            uint uiAction,
            uint uiParam,
            ref int pvParam,
            uint fWinIni
        );

        public static bool GetSystemContrast(out int contrast)
        {
            contrast = 0;
            return SystemParametersInfo(SPI_GETCONTRAST, 0, ref contrast, 0);
        }
        public static bool SetDesktopColor(int color)
        {
            // Используем SystemParametersInfo для установки цвета фона рабочего стола
            return SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, ref color, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        }

        public static int RGB(byte red, byte green, byte blue)
        {
            return red | (green << 8) | (blue << 16);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEMTIME
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern void GetSystemTime(ref SYSTEMTIME lpSystemTime);

        private void button1_Click(object sender, EventArgs e)
        {
            byte red, green, blue;

            try
            {
                red = byte.Parse(red_textBox.Text);
                green = byte.Parse(green_textBox.Text);
                blue = byte.Parse(blue_textBox.Text);
            }
            catch
            {
                MessageBox.Show("Неверный формат цвета!");
                return;
            }

            int color = RGB(red, green, blue);
            SetDesktopColor(color);
        }
        private string GetPathOfWallpaper()
        {
            string pathWallpaper = "";
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", false);
            if (regKey != null)
            {
                pathWallpaper = regKey.GetValue("WallPaper").ToString();
                regKey.Close();
            }
            return pathWallpaper;
        }
        const uint SPI_GETDESKWALLPAPER = 0x0073;

        const int COLOR_BACKGROUND = 1;

        [DllImport("user32.dll")]
        public static extern int GetSysColor(int nIndex);
        private void button2_Click(object sender, EventArgs e)
        {
            uint size = 256; // Максимальная длина имени компьютера
            char[] buffer = new char[size];

            if (GetComputerName(buffer, ref size))
            {
                string computerName = new string(buffer);
                label1.Text += computerName;
            }
            else
            {
                Console.WriteLine("Не удалось получить название компьютера.");
            }

            size = 256; // Максимальная длина имени пользователя
            StringBuilder buffer1 = new StringBuilder((int)size);

            if (GetUserName(buffer1, ref size))
            {
                string userName = buffer1.ToString();
                label2.Text += userName;
            }
            else
            {
                Console.WriteLine("Не удалось получить имя пользователя.");
            }

            size = 256; // Максимальная длина системной директории
            StringBuilder buffer2 = new StringBuilder((int)size);

            uint result = GetSystemDirectory(buffer2, size);

            if (result != 0)
            {
                string systemDirectory = buffer2.ToString();
                label3.Text += systemDirectory;
            }
            else
            {
                Console.WriteLine("Не удалось получить системную директорию.");
            }

            OSVERSIONINFOEX osVersionInfo = new OSVERSIONINFOEX();
            osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX));

            if (GetVersionEx(ref osVersionInfo))
            {
                label4.Text += osVersionInfo.dwMajorVersion + "." + osVersionInfo.dwMinorVersion;
            }

            int cleanBootStatus = GetSystemMetrics(SM_CLEANBOOT);

            if (cleanBootStatus == 1)
            {
                label5.Text += "Normal boot";
            }

            else
            {
                label5.Text += "Fail safe boot";
            }

            label6.Text += $"{Screen.PrimaryScreen.Bounds.Width} x {Screen.PrimaryScreen.Bounds.Height}";

            label7.Text += GetPathOfWallpaper();

            int contrast;
            if (GetSystemContrast(out contrast))
            {
                label8.Text += $"{contrast}";
            }
            else
            {
                Console.WriteLine("Не удалось получить информацию о контрастности.");
            }

            int backgroundColor = GetSysColor(COLOR_BACKGROUND);

            // Преобразуем значение цвета в RGB
            int red = backgroundColor & 0xFF;
            int green = (backgroundColor >> 8) & 0xFF;
            int blue = (backgroundColor >> 16) & 0xFF;

            label9.Text += $"[R = {red}, G = {green}, B = {blue}]";

            DateTime currentTime = DateTime.Now;

            CultureInfo russianCulture = new CultureInfo("ru-RU");

            string formattedTime = currentTime.ToString("dd/MM/yyyy", russianCulture);

            label10.Text += formattedTime;
        }
    }
}
