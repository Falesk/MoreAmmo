namespace MoreAmmo
{
    public static class Register
    {
        public static void RegisterValues()
        {
            Flies = new JokeRifle.AbstractRifle.AmmoType("Flies", true);
            Mold = new JokeRifle.AbstractRifle.AmmoType("Mold", true);
            Overseer = new JokeRifle.AbstractRifle.AmmoType("Overseer", true);
            Shock = new JokeRifle.AbstractRifle.AmmoType("Shock", true);
            Haze = new JokeRifle.AbstractRifle.AmmoType("Haze", true);
        }

        public static void UnregisterValues()
        {
            JokeRifle.AbstractRifle.AmmoType flies = Flies;
            flies?.Unregister();
            Flies = null;

            JokeRifle.AbstractRifle.AmmoType mold = Mold;
            mold?.Unregister();
            Mold = null;

            JokeRifle.AbstractRifle.AmmoType overseer = Overseer;
            overseer?.Unregister();
            Overseer = null;

            JokeRifle.AbstractRifle.AmmoType shock = Shock;
            shock?.Unregister();
            Shock = null;

            JokeRifle.AbstractRifle.AmmoType haze = Haze;
            haze?.Unregister();
            Haze = null;
        }

        public static JokeRifle.AbstractRifle.AmmoType Flies;
        public static JokeRifle.AbstractRifle.AmmoType Mold;
        public static JokeRifle.AbstractRifle.AmmoType Overseer;
        public static JokeRifle.AbstractRifle.AmmoType Shock;
        public static JokeRifle.AbstractRifle.AmmoType Haze;
    }
}
