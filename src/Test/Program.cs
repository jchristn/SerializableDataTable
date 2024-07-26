namespace Test
{
    using System;
    using System.Data;
    using SerializableDataTables;
    using SerializationHelper;

    public static class Program
    {
        public static void Main(string[] args)
        {
            #region Create-DataTable

            DataTable dt1 = new DataTable();

            dt1.Columns.Add(new DataColumn
            {
                ColumnName = "id",
                DataType = typeof(int)
            });
            dt1.Columns.Add(new DataColumn
            {
                ColumnName = "first",
                DataType = typeof(string)
            });
            dt1.Columns.Add(new DataColumn
            {
                ColumnName = "last",
                DataType = typeof(string)
            });
            dt1.Columns.Add(new DataColumn
            {
                ColumnName = "bday",
                DataType = typeof(DateTime)
            });

            DataRow row1 = dt1.NewRow();
            row1["id"] = 1;
            row1["first"] = "barack";
            row1["last"] = "obama";
            row1["bday"] = DateTime.Parse("06/01/1965");
            dt1.Rows.Add(row1);

            DataRow row2 = dt1.NewRow();
            row2["id"] = 2;
            row2["first"] = "george";
            row2["last"] = "bush";
            row2["bday"] = DateTime.Parse("07/01/1955");
            dt1.Rows.Add(row2);

            DataRow row3 = dt1.NewRow();
            row3["id"] = 3;
            row3["first"] = "bill";
            row3["last"] = "clinton";
            row3["bday"] = DateTime.Parse("08/01/1965");
            dt1.Rows.Add(row3);

            #endregion

            #region Serialize-and-Display

            string json = Serializer.SerializeJson(SerializableDataTable.FromDataTable(dt1));
            Console.WriteLine("");
            Console.WriteLine("JSON:");
            Console.WriteLine(json);
            Console.WriteLine("");

            #endregion

            #region Deserialize-and-Iterate

            DataTable dt2 = Serializer.DeserializeJson<SerializableDataTable>(json).ToDataTable();
            Console.WriteLine("");
            Console.WriteLine("DataTable:");
            DisplayDataTable(dt2);
            Console.WriteLine("");

            #endregion
        }

        private static void DisplayDataTable(DataTable dt)
        {
            if (dt == null || dt.Columns.Count == 0) return;

            // Display column headers and types
            foreach (DataColumn column in dt.Columns)
                Console.Write($"{column.ColumnName} ({column.DataType.Name})\t");
            Console.WriteLine();

            Console.WriteLine(new string('-', dt.Columns.Count * 20));

            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    Console.Write($"{row[i]}\t");
                }
                Console.WriteLine();
            }

            Console.WriteLine($"\nTotal Rows: {dt.Rows.Count}");
        }
    }
}
