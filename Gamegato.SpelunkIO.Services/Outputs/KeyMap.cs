using System;
using Interceptor;

namespace Gamegato.SpelunkIO.Services.Outputs
{
    public class KeyMap
    {
        public static Keys Convert(ButtonActions action)
        {
            switch (action)
            {
                case ButtonActions.Left:
                    return Keys.E;
                case ButtonActions.Right:
                    return Keys.R;
                case ButtonActions.Up:
                    return Keys.Q;
                case ButtonActions.Duck:
                    return Keys.W;
                case ButtonActions.Journal:
                    return Keys.Tab;
                case ButtonActions.WhipAction:
                    return Keys.X;
                case ButtonActions.Jump:
                    return Keys.Z;
                case ButtonActions.Bomb:
                    return Keys.S;
                case ButtonActions.Rope:
                    return Keys.A;
                case ButtonActions.Run:
                    return Keys.LeftShift;
                case ButtonActions.PurchaseDoor:
                    return Keys.Space;
                default:
                    throw new Exception(action + " isn't mapped to a key");
            }
        }
    }
}
