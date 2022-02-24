using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Perform_Windows_Click
{


    internal class KeyBoardManager
    {
        private const int KEYEVENTF_KEYDOWN = 0x0000;
        const int KEYEVENTF_KEYUP = 0x0002;

        [DllImport("coredll.dll")]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        public static void SendKey(Keys key)
        {
            keybd_event((byte)key, 0, KEYEVENTF_KEYDOWN, 0);
            keybd_event((byte)key, 0, KEYEVENTF_KEYUP, 0);
        }

       
            
    }
}
