using System.Threading;
using Interceptor;

namespace Gamegato.SpelunkIO.Services.Outputs
{
    public class SpelunkyOutput
    {
        Input input;

        public SpelunkyOutput()
        {
            input = new Input();
            input.KeyboardFilterMode = KeyboardFilterMode.All;
            input.Load();

            input.OnKeyPressed += Input_OnKeyPressed;
        }

        private void Input_OnKeyPressed(object sender, KeyPressedEventArgs e)
        {
            //Console.WriteLine("");
            //Console.WriteLine(e.Key + " - " + e.State);
        }

        public void PressKey(ButtonActions action)
        {
            input.SendKey(KeyMap.Convert(action), KeyState.Down);
        }

        public void LiftKey(ButtonActions action)
        {
            input.SendKey(KeyMap.Convert(action), KeyState.Up);
        }

        public void PressDown()
        {
            input.SendKey(Keys.Down, KeyState.E0);
            Thread.Sleep(50);
            input.SendKey(Keys.Down, KeyState.Up | KeyState.E0);
        }

        public void PressEscape()
        {
            input.SendKey(Keys.Escape, KeyState.Down);
            Thread.Sleep(50);
            input.SendKey(Keys.Escape, KeyState.Up);
        }
    }
}
