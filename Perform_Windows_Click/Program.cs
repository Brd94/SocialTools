using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Perform_Windows_Click
{
    internal class Program
    {
        static void Main(string[] args)
        {

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--getX":
                        Environment.Exit(MouseManager.GetCursorPosition().X);
                        break;

                    case "--getY":
                        Environment.Exit(MouseManager.GetCursorPosition().Y);
                        break;

                    case "--setXY":
                        MouseManager.SetCursorPosition(new MouseManager.MousePoint(int.Parse(args[i + 1]), int.Parse(args[i + 2])));
                        Environment.Exit(0);
                        break;

                    case "--performClick":
                        MouseManager.DoMouseClick();
                        Environment.Exit(0);
                        break;

                    case "--write":

                        SendKeys.SendWait(args[i + 1]);

                      
                        break;

                    case "--sendKey":
                        
                        Keys key = (Keys)Enum.Parse(typeof(Keys), args[i + 1].ToString());
                        KeyBoardManager.SendKey(key);

                      
                        
                        break;
                }
            }


        }
    }
}
