using System;
using System.Drawing;

namespace DoomCloneV2
{
    /// <summary>
    /// Secondary represents a player's right-click secondary weapon (pistol or grenade).
    /// It mirrors <see cref="Powerup"/> in spirit (an animation-frame holder plus a sound),
    /// but reads its frames from a simple left-to-right filmstrip (<see cref="FRAME_COUNT"/>
    /// frames of <see cref="FRAME_SIZE"/> square each) rather than Powerup's hardcoded
    /// 2-row/4-column sheet, since these sprites were laid out fresh for this feature.
    /// </summary>
    public class Secondary
    {
        public const int FRAME_COUNT = 8;
        public const int FRAME_WIDTH = 700;
        public const int FRAME_HEIGHT = 500;

        //Marker colours painted into the base art that get swapped for a random camo colour per
        //player - the same "Modular" recolor trick Gun.cs and Powerup.cs already use via
        //Globals.ReColorImage.
        public static readonly Color PrimaryMarker = Color.FromArgb(255, 0, 255, 0);
        public static readonly Color SecondaryMarker = Color.FromArgb(255, 0, 180, 0);

        Bitmap baseImage;
        System.Media.SoundPlayer sound;
        public Bitmap[] animationImages;
        public Player.SecondaryType secondaryType;

        public Secondary(Bitmap baseImage, Player.SecondaryType type)
        {
            this.baseImage = baseImage;
            this.secondaryType = type;
            this.animationImages = BuildFrames();
            String soundFile = type == Player.SecondaryType.GRENADE
                ? "Resources/Sound/Bang_002_000.wav"
                : "Resources/Sound/Bang_001_000.wav";
            this.sound = new System.Media.SoundPlayer(soundFile);
        }

        private Bitmap[] BuildFrames()
        {
            Bitmap[] frames = new Bitmap[FRAME_COUNT];
            for (int i = 0; i < FRAME_COUNT; i++)
            {
                Rectangle section = new Rectangle(i * FRAME_WIDTH, 0, FRAME_WIDTH, FRAME_HEIGHT);
                frames[i] = Globals.CropImage(baseImage, section);
            }
            return frames;
        }

        public System.Media.SoundPlayer GetSecondarySound()
        {
            return sound;
        }
    }
}
