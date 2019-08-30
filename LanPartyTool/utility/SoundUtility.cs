using System;
using System.Media;
using System.Threading;
using LanPartyTool.agent;
using log4net;

namespace LanPartyTool.utility
{
    internal class SoundUtility
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Agent));

        public enum Sound
        {
            Ping,
            Success
        };

        public static void Play(Sound sound)
        {
            switch (sound)
            {
                case Sound.Ping:
                    PlaySound(@"sounds/ping.wav");
                    break;

                case Sound.Success:
                    PlaySound(@"sounds/success.wav");
                    break;

                default:
                    return;
            }
        }

        private static void PlaySound(string soundLocation)
        {
            new Thread(() =>
            {
                Logger.Debug("Playing ping sound");

                var player = new SoundPlayer {SoundLocation = soundLocation};
                player.Load();
                player.PlaySync();
                player.Stop();
                player.Dispose();
            }).Start();
        }
    }
}