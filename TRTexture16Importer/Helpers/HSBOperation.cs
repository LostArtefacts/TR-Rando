namespace TRTexture16Importer.Helpers
{
    public class HSBOperation
    {
        private static readonly int _unset = int.MinValue;

        public int Hue { get; set; }
        public int Saturation { get; set; }
        public int Brightness { get; set; }

        public HSBOperation()
        {
            Hue = Saturation = Brightness = _unset;
        }

        public int ModifyHue(int hue)
        {
            if (Hue == _unset)
            {
                return hue;
            }

            hue += Hue;
            if (hue < 0)
            {
                hue += 360;
            }
            else if (hue > 360)
            {
                hue -= 360;
            }
            return hue;
        }

        public int ModifySaturation(int saturation)
        {
            if (Saturation == _unset)
            {
                return saturation;
            }

            double d = Saturation / 100.0;
            d = saturation * d;
            return (int)d;
        }

        public int ModifyBrightness(int brightness)
        {
            if (Brightness == _unset)
            {
                return brightness;
            }

            double d = Brightness / 100.0;
            d = brightness * d;
            return (int)d;
        }
    }
}