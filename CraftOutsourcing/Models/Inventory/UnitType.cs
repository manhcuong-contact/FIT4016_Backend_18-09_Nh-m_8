namespace CraftOutsourcing.Models.Inventory
{
    public static class UnitType
    {
        // Các đơn vị tính chuẩn
        public static readonly List<string> AllUnits = new List<string>
        {
            "kg",        // Kilogram
            "g",         // Gram
            "l",         // Lít
            "ml",        // Mililit
            "m",         // Mét
            "cm",        // Centimet
            "mm",        // Milimet
            "cái",       // Cái (pieces/items)
            "cuộn",      // Cuộn (roll)
            "bộ",        // Bộ (set)
            "hộp",       // Hộp (box)
            "túi",       // Túi (bag)
            "chiếc",     // Chiếc (item)
            "sợi",       // Sợi (thread)
            "đoạn",      // Đoạn (length)
            "lớp"        // Lớp (layer)
        };

        public static string[] GetAllUnits()
        {
            return AllUnits.ToArray();
        }
    }
}
