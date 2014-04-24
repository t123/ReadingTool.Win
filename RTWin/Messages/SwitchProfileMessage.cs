using RTWin.Entities;

namespace RTWin.Messages
{
    public class SwitchProfileMessage
    {
        public User User { get; private set; }

        public SwitchProfileMessage(User user)
        {
            User = user;
        }
    }
}