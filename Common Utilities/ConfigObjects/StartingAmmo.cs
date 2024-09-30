namespace Common_Utilities.ConfigObjects
{
    public class StartingAmmo
    {
        public ItemType Type { get; set; }

        public ushort Amount { get; set; }

        public string Group { get; set; } = "none";

        public void Deconstruct(out ItemType ammoType, out ushort amount, out string group)
        {
            ammoType = Type;
            amount = Amount;
            group = Group;
        }
    }
}
