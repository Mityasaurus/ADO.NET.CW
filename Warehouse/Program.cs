using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Channels;

namespace Warehouse
{
    internal class Program
    {
        private static string ConnString => ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
        private static Dictionary<string, List<string>> Products = new Dictionary<string, List<string>>();
        private static Dictionary<string, List<string>> Types = new Dictionary<string, List<string>>();
        private static Dictionary<string, List<string>> Manufacturers = new Dictionary<string, List<string>>();
        static void Main(string[] args)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    Console.WriteLine("Connection success");

                    Console.WriteLine("Do you want to create tables [Y/N]");
                    string isTableCreated = Console.ReadLine();
                    if (isTableCreated.ToLower() == "y")
                    {
                        string queryCmd = "create table TypesOfProducts(ID int identity(1, 1) primary key,Name nvarchar(100) not null check (Name <>''))";

                        using (SqlCommand cmd = new SqlCommand(queryCmd, conn))
                        {
                            Console.WriteLine(cmd.ExecuteNonQuery());
                        }

                        queryCmd = "create table Manufacturers(ID int identity(1, 1) primary key,Name nvarchar(100) not null check (Name <>''))";
                        using (SqlCommand cmd = new SqlCommand(queryCmd, conn))
                        {
                            Console.WriteLine(cmd.ExecuteNonQuery());
                        }

                        queryCmd = "create table Products(ID int identity(1, 1) primary key,Name nvarchar(100) not null check (Name <>''), TypeID int not null REFERENCES TypesOfProducts(ID) ON UPDATE CASCADE ON DELETE CASCADE, ManufacturerID int not null REFERENCES Manufacturers(ID) ON UPDATE CASCADE ON DELETE CASCADE, Number int not null, CostPrice int not null, Date date not null)";
                        using (SqlCommand cmd = new SqlCommand(queryCmd, conn))
                        {
                            Console.WriteLine(cmd.ExecuteNonQuery());
                        }
                    }

                    Console.WriteLine("Do you want to add test values? [Y/N]");
                    string isDataNotCtreated = Console.ReadLine();

                    if(isDataNotCtreated.ToLower() == "y")
                    {
                        string insertTypeQuery = "INSERT INTO TypesOfProducts (Name) VALUES ('Type 1'), ('Type 2'), ('Type 3')";
                        using (SqlCommand cmd = new SqlCommand(insertTypeQuery, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        string insertManufacturerQuery = "INSERT INTO Manufacturers (Name) VALUES ('Manufacturer 1'), ('Manufacturer 2'), ('Manufacturer 3')";
                        using (SqlCommand cmd = new SqlCommand(insertManufacturerQuery, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        string insertProductQuery = "INSERT INTO Products (Name, TypeID, ManufacturerID, Number, CostPrice, Date) VALUES " +
                                                    "('Product 1', (select top 1 id from TypesOfProducts where Name='Type 1'), (select top 1 id from Manufacturers where Name='Manufacturer 2'), 100, 10, '2023-08-01'), " +
                                                    "('Product 2', (select top 1 id from TypesOfProducts where Name='Type 3'), (select top 1 id from Manufacturers where Name='Manufacturer 1'), 150, 15, '2023-08-02'), " +
                                                    "('Product 3', (select top 1 id from TypesOfProducts where Name='Type 2'), (select top 1 id from Manufacturers where Name='Manufacturer 3'), 200, 8, '2023-08-03')";
                        using (SqlCommand cmd = new SqlCommand(insertProductQuery, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }

                    string query = "select * from Products as p, Manufacturers as m, TypesOfProducts as t where p.TypeID=t.ID and p.ManufacturerID=m.ID";

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    DataSet ds = new DataSet();

                    UpdateData(conn, new string[] { "Products", "TypesOfProducts", "Manufacturers" }, Products, Types, Manufacturers);

                    int choice = -1;
                    while (choice != 0)
                    {
                        Console.WriteLine("\nВведiть свiй вибiр");
                        Console.WriteLine("1 - Вiдображення всiєї iнформацiї про товар");
                        Console.WriteLine("2 - Вiдображення всiх типiв товарiв");
                        Console.WriteLine("3 - Вiдображення всiх постачальникiв");
                        Console.WriteLine("4 - Показати товар з максимальною кiлькiстю");
                        Console.WriteLine("5 - Показати товар з мiнiмальною кiлькiстю");
                        Console.WriteLine("6 - Показати товар з максимальною собiвартiстю");
                        Console.WriteLine("7 - Показати товар з мiнiмальною собiвартiстю");
                        Console.WriteLine();
                        Console.WriteLine("8 - Додати новий товар");
                        Console.WriteLine("9 - Додати новий тип товару");
                        Console.WriteLine("10 - Додати нового постачальника");
                        Console.WriteLine();
                        Console.WriteLine("11 - Оновити iснуючий товар");
                        Console.WriteLine("12 - Оновити iснуючий тип товару");
                        Console.WriteLine("13 - Оновити iснуючого постачальника");
                        Console.WriteLine();
                        Console.WriteLine("14 - Видалити iснуючий товар");
                        Console.WriteLine("15 - Видалити iснуючий тип товару");
                        Console.WriteLine("16 - Видалити iснуючого постачальника");
                        Console.WriteLine();
                        Console.WriteLine("17 - Показати iнформацiю про постачальника, в якого кiлькiсть товарiв на складi найбiльша");
                        Console.WriteLine("18 - Показати iнформацiю про постачальника, в якого кiлькiсть товарiв на складi найменша");
                        Console.WriteLine("19 - Показати iнформацiю про тип товару з найбiльшою кiлькiстю товарiв на складi");
                        Console.WriteLine("20 - Показати iнформацiю про тип товару з найменшою кiлькiстю товарiв на складi");
                        Console.WriteLine("21 - Показати товари, з постачання яких минула задана кiлькiсть днiв");
                        Console.WriteLine();
                        Console.WriteLine("22 - Показати товари заданої категорiї");
                        Console.WriteLine("23  - Показати товари заданого постачальника");
                        Console.WriteLine("24  - Показати товар, який знаходиться на складi найдовше з усiх");
                        Console.WriteLine("25 - Показати середню кiлькiсть товарiв за кожним типом товару");
                        Console.WriteLine();
                        Console.WriteLine("0 - Вихiд");

                        choice = int.Parse(Console.ReadLine());
                        query = "";

                        switch (choice)
                        {
                            case 1: Console.Clear();
                                Display(Products);
                                break;
                            case 2:
                                Console.Clear();
                                Display(Types);
                                break;
                            case 3:
                                Console.Clear();
                                Display(Manufacturers);
                                break;
                            case 4:
                                Console.Clear();
                                Display(GetProductMaxNumber());
                                break;
                            case 5:
                                Console.Clear();
                                Display(GetProductMinNumber());
                                break;
                            case 6:
                                Console.Clear();
                                Display(GetProductMaxCostPrice());
                                break;
                            case 7:
                                Console.Clear();
                                Display(GetProductMinCostPrice());
                                break;
                            case 8:
                                Console.Clear();
                                AddNewProduct(conn);
                                UpdateData(conn, new string[] { "Products" }, Products);
                                break;
                            case 9:
                                Console.Clear();
                                AddNewProductType(conn);
                                UpdateData(conn, new string[] { "TypesOfProducts" }, Types);
                                break;
                            case 10:
                                Console.Clear();
                                AddNewProductManufacturer(conn);
                                UpdateData(conn, new string[] { "Manufacturers" }, Manufacturers);
                                break;
                            case 11:
                                Console.Clear();
                                UpdateProduct(conn);
                                UpdateData(conn, new string[] { "Products" }, Products);
                                break;
                            case 12:
                                Console.Clear();
                                UpdateProductType(conn);
                                UpdateData(conn, new string[] { "TypesOfProducts" }, Types);
                                break;
                            case 13:
                                Console.Clear();
                                UpdateManufacturer(conn);
                                UpdateData(conn, new string[] { "Manufacturers" }, Manufacturers);
                                break;
                            case 14:
                                Console.Clear();
                                DeleteProduct(conn);
                                UpdateData(conn, new string[] { "Products" }, Products);
                                break;
                            case 15:
                                Console.Clear();
                                DeleteProductType(conn);
                                UpdateData(conn, new string[] { "Products", "TypesOfProducts" }, Products, Types);
                                break;
                            case 16:
                                Console.Clear();
                                DeleteManufacturer(conn);
                                UpdateData(conn, new string[] { "Products", "Manufacturers" }, Products, Manufacturers);
                                break;
                            case 17:
                                Console.Clear();
                                query = "SELECT top 1 M.Name AS Manufacturer, SUM(P.Number) AS TotalQuantity " +
                                    "FROM Manufacturers M " +
                                    "JOIN Products P ON M.ID = P.ManufacturerID " +
                                    "GROUP BY M.Name ORDER BY TotalQuantity DESC";
                                GetData(query, ds, conn);
                                DisplayManufacturerQuantity(ds.Tables[0]);
                                break;
                            case 18:
                                Console.Clear();
                                query = "SELECT top 1 M.Name AS Manufacturer, SUM(P.Number) AS TotalQuantity " +
                                    "FROM Manufacturers M " +
                                    "JOIN Products P ON M.ID = P.ManufacturerID " +
                                    "GROUP BY M.Name ORDER BY TotalQuantity ASC";
                                GetData(query, ds, conn);
                                DisplayManufacturerQuantity(ds.Tables[0]);
                                break;
                            case 19:
                                Console.Clear();
                                query = "SELECT top 1 T.Name AS ProductType, SUM(P.Number) AS TotalQuantity " +
                                    "FROM TypesOfProducts T " +
                                    "JOIN Products P ON T.ID = P.TypeID " +
                                    "GROUP BY T.Name ORDER BY TotalQuantity DESC";
                                GetData(query, ds, conn);
                                DisplayTypeQuantity(ds.Tables[0]);
                                break;
                            case 20:
                                Console.Clear();
                                query = "SELECT top 1 T.Name AS ProductType, SUM(P.Number) AS TotalQuantity " +
                                    "FROM TypesOfProducts T " +
                                    "JOIN Products P ON T.ID = P.TypeID " +
                                    "GROUP BY T.Name ORDER BY TotalQuantity ASC";
                                GetData(query, ds, conn);
                                DisplayTypeQuantity(ds.Tables[0]);
                                break;
                            case 21:
                                Console.Clear();
                                Console.WriteLine("Введiть бажану кiлькiсть днiв");

                                int days = int.Parse(Console.ReadLine());

                                query = $"SELECT P.* FROM Products P WHERE DATEDIFF(DAY, P.Date, GETDATE()) > {days}";
                                GetData(query, ds, conn);
                                DisplayProducts(ds.Tables[0]);
                                break;
                            case 22:
                                Console.Clear();
                                Console.WriteLine("Введiть бажану категорiю");

                                string type = Console.ReadLine();
                                Display(GetProductCertainType(type));
                                break;
                            case 23:
                                Console.Clear();
                                Console.WriteLine("Введiть бажаного постачальника");

                                string manufacturer = Console.ReadLine();
                                Display(GetProductCertainManufacturer(manufacturer));
                                break;
                            case 24:
                                Console.Clear();
                                Display(GetProductOldestDate());
                                break;
                            case 25:
                                Console.Clear();
                                Display(GetAverageProductNumberByType());
                                break;
                            case 0:
                                break;
                            default:
                                Console.WriteLine("Помилковий вибiр!");
                                break;
                        }
                    }

                    conn.Close();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void UpdateData(SqlConnection conn, string[] tbNames, params Dictionary<string, List<string>>[] tables)
        {
            try
            {
                for(int i = 0; i < tables.Length; i++)
                {
                    tables[i].Clear();
                    GetAllValues(conn, tbNames[i], tables[i]);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private static string GetSelectQuery(string tbName) =>
            $"select * from {tbName}";
        private static List<string> GetTableNames(SqlConnection sqlConnection, params string[] name)
        {
            var tables = new List<string>();

            if (name.Length > 0)
            {
                foreach (DataRow dr in sqlConnection.GetSchema("Tables").Rows)
                {
                    if (name.Contains(dr[2].ToString()))
                    {
                        tables.Add($"[{dr[1]}].[{dr[2]}]");
                    }
                }
            }
            else
            {
                foreach (DataRow dr in sqlConnection.GetSchema("Tables").Rows)
                {
                    tables.Add($"[{dr[1]}].[{dr[2]}]");
                }
            }

            return tables;
        }
        private static Dictionary<string, List<string>> GetAllValues(SqlConnection sqlConnection, string tbName, Dictionary<string, List<string>> dict)
        {
            var tables = GetTableNames(sqlConnection, tbName);
            

            using (SqlCommand cmd = new SqlCommand(GetSelectQuery(tables[0]), sqlConnection))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        dict.Add(reader.GetName(i), new List<string>());
                    }

                        while (reader.Read())
                        {
                            foreach(var key in dict.Keys)
                            {
                                dict[$"{key}"].Add(reader[$"{key}"].ToString());
                            }
                        }
                    
                }
            }
            return dict;
        }
        private static Dictionary<string, List<string>> GetProductMaxNumber()
        {
            int maxNumber = Products["Number"].Select(num => int.Parse(num)).Max();

            return GetProductWith("Number", maxNumber.ToString());
        }
        private static Dictionary<string, List<string>> GetProductMinNumber()
        {
            int minNumber = Products["Number"].Select(num => int.Parse(num)).Min();

            return GetProductWith("Number", minNumber.ToString());
        }
        private static Dictionary<string, List<string>> GetProductMaxCostPrice()
        {
            int maxNumber = Products["CostPrice"].Select(num => int.Parse(num)).Max();

            return GetProductWith("CostPrice", maxNumber.ToString());
        }
        private static Dictionary<string, List<string>> GetProductMinCostPrice()
        {
            int minNumber = Products["CostPrice"].Select(num => int.Parse(num)).Min();

            return GetProductWith("CostPrice", minNumber.ToString());
        }
        private static Dictionary<string, List<string>> GetProductCertainType(string type)
        {
            string id = "";

            for(int i = 0; i < Types["Name"].Count; i++)
            {
                if (Types["Name"][i] == type)
                {
                    id = Types["ID"][i];
                    break;
                }
            }

            return GetProductWith("TypeID", id);
        }
        private static Dictionary<string, List<string>> GetProductCertainManufacturer(string type)
        {
            string id = "";

            for (int i = 0; i < Manufacturers["Name"].Count; i++)
            {
                if (Manufacturers["Name"][i] == type)
                {
                    id = Manufacturers["ID"][i];
                    break;
                }
            }

            return GetProductWith("ManufacturerID", id);
        }
        private static Dictionary<string, List<string>> GetProductOldestDate()
        {
            DateTime date = Products["Date"].Select(d => DateTime.Parse(d)).Min();

            return GetProductWith("Date", date.ToString());
        }
        private static Dictionary<string, List<string>> GetAverageProductNumberByType()
        {
            var result = new Dictionary<string, List<string>>
            {
                { "Type", new List<string>() },
                { "Average number", new List<string>() }
            };

            for(int i = 0; i < Types["Name"].Count; i++)
            {
                var type = Types["Name"][i];
                var id = Types["ID"][i];

                int counter = 0;
                int summ = 0;

                for(int j = 0; j < Products["TypeID"].Count; j++)
                {
                    var prTypeId = Products["TypeID"][j];

                    if (prTypeId == id)
                    {
                        summ += int.Parse(Products["Number"][j]);
                        counter++;
                    }
                }

                summ = counter != 0 ? summ /= counter : 0;

                result["Type"].Add(type);
                result["Average number"].Add(summ.ToString());
            }

            return result;
        }
        private static Dictionary<string, List<string>> GetProductWith(string columnName, string value)
        {
            var result = new Dictionary<string, List<string>>();
            try
            {
                foreach (var key in Products.Keys)
                {
                    result.Add(key, new List<string>());
                }

                for (int i = 0; i < Products["ID"].Count; i++)
                {
                    if (Products[columnName][i] == value.ToString())
                    {
                        foreach (var key in Products.Keys)
                        {
                            result[key].Add(Products[key][i]);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result;

        }

        static bool AddNewProduct(SqlConnection conn)
        {
            Console.WriteLine("Введiть назву нового товару");
            string name = Console.ReadLine();

            Console.WriteLine("Введiть тип нового товару");
            string type = Console.ReadLine();

            Console.WriteLine("Введiть постачальника нового товару");
            string manufacturer = Console.ReadLine();

            Console.WriteLine("Введiть кiлькiсть нового товару");
            string number = Console.ReadLine();

            Console.WriteLine("Введiть собiвартiсть нового товару");
            string costPrice = Console.ReadLine();

            Console.WriteLine("Введiть дату постачання нового товару");
            string date = Console.ReadLine();

            string query = $"insert into Products values ('{name}', (select top 1 id from TypesOfProducts where Name='{type}'), (select top 1 id from Manufacturers where Name='{manufacturer}'), {number}, {costPrice}, '{date}')";

            return ExecuteCommand(query, conn);
        }
        static bool AddNewProductType(SqlConnection conn)
        {
            Console.WriteLine("Введiть новий тип товару");
            string name = Console.ReadLine();

            string query = $"insert into TypesOfProducts values ('{name}')";

            return ExecuteCommand(query, conn);
            
        }
        static bool AddNewProductManufacturer(SqlConnection conn)
        {
            Console.WriteLine("Введiть нового постачальника");
            string name = Console.ReadLine();

            string query = $"insert into Manufacturers values ('{name}')";

            return ExecuteCommand(query, conn);

        }

        static bool UpdateProduct(SqlConnection conn)
        {
            Console.WriteLine("Введiть ID товару який хочете змiнити");
            int id = int.Parse(Console.ReadLine());

            string query = "";

            Console.WriteLine("Щоб НЕ змiнювати поточне значення натискайте Enter\n");
            Console.Write("Назва товару:");

            string answer = "";
            answer = Console.ReadLine();
            if(answer != "")
            {
                query = $"update Products Set Name = '{answer}' where ID = {id}";
                if(ExecuteCommand(query, conn) == false)
                {
                    return false;
                }
            }

            Console.Write("Тип товару:");

            answer = "";
            answer = Console.ReadLine();
            if (answer != "")
            {
                query = $"update Products Set TypeID = (select top 1 ID from TypesOfProducts where Name='{answer}') where ID = {id}";
                if (ExecuteCommand(query, conn) == false)
                {
                    return false;
                }
            }

            Console.Write("Постачальник товару:");

            answer = "";
            answer = Console.ReadLine();
            if (answer != "")
            {
                query = $"update Products Set ManufacturerID = (select top 1 ID from Manufacturers where Name='{answer}') where ID = {id}";
                if (ExecuteCommand(query, conn) == false)
                {
                    return false;
                }
            }

            Console.Write("Кiлькiсть товару:");

            answer = "";
            answer = Console.ReadLine();
            if (answer != "")
            {
                query = $"update Products Set Number = {int.Parse(answer)} where ID = {id}";
                if (ExecuteCommand(query, conn) == false)
                {
                    return false;
                }
            }

            Console.Write("Собiвартiсть товару:");

            answer = "";
            answer = Console.ReadLine();
            if (answer != "")
            {
                query = $"update Products Set CostPrice = {int.Parse(answer)} where ID = {id}";
                if (ExecuteCommand(query, conn) == false)
                {
                    return false;
                }
            }

            Console.Write("Дата постачання товару:");

            answer = "";
            answer = Console.ReadLine();
            if (answer != "")
            {
                query = $"update Products Set Date = '{answer}' where ID = {id}";
                if (ExecuteCommand(query, conn) == false)
                {
                    return false;
                }
            }

            return true;
        }
        static bool UpdateProductType(SqlConnection conn)
        {
            Console.WriteLine("Введiть ID типу товару який хочете змiнити");
            int id = int.Parse(Console.ReadLine());

            Console.WriteLine("Введiть оновлену назву");
            string newName = Console.ReadLine();

            string query = $"update TypesOfProducts set Name = '{newName}' where ID = {id}";

            return ExecuteCommand(query , conn);
        }
        static bool UpdateManufacturer(SqlConnection conn)
        {
            Console.WriteLine("Введiть ID постачальника якого хочете змiнити");
            int id = int.Parse(Console.ReadLine());

            Console.WriteLine("Введiть оновлену назву");
            string newName = Console.ReadLine();

            string query = $"update Manufacturers set Name = '{newName}' where ID = {id}";

            return ExecuteCommand(query, conn);
        }

        static bool DeleteProduct(SqlConnection conn)
        {
            Console.WriteLine("Введiть ID товару який хочете видалити");
            int id = int.Parse(Console.ReadLine());

            string query = $"delete from Products where ID = {id}";

            return ExecuteCommand(query, conn);
        }
        static bool DeleteProductType(SqlConnection conn)
        {
            Console.WriteLine("Введiть ID типу товару який хочете видалити");
            int id = int.Parse(Console.ReadLine());

            string query = $"delete from TypesOfProducts where ID = {id}";

            return ExecuteCommand(query, conn);
        }
        static bool DeleteManufacturer(SqlConnection conn)
        {
            Console.WriteLine("Введiть ID постачальника якого хочете видалити");
            int id = int.Parse(Console.ReadLine());

            string query = $"delete from Manufacturers where ID = {id}";

            return ExecuteCommand(query, conn);
        }

        static bool ExecuteCommand(string query, SqlConnection conn)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    Console.WriteLine(cmd.ExecuteNonQuery());
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        static DataSet GetData(string query, DataSet ds, SqlConnection conn)
        {
            SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
            ds.Clear();

            adapter.Fill(ds);

            if (ds != null)
            {
                return ds;
            }
            return new DataSet();
        }

        static void Display(Dictionary<string, List<string>> dict)
        {
            foreach(var key in dict.Keys)
            {
                Console.Write($"{key.PadRight(20)}");
            }
            Console.WriteLine();
            for(int i = 0; i < dict[dict.Keys.First()].Count; i++)
            {
                foreach (var key in dict.Keys)
                {
                    Console.Write(dict[$"{key}"][i].PadRight(20));
                }
                Console.WriteLine();
            }
        }
        static void DisplayManufacturerQuantity(DataTable dataTable)
        {
            Console.WriteLine($"{"Manufacturer".PadRight(20)}Total quantity");
            foreach (DataRow row in dataTable.Rows)
            {
                Console.WriteLine($"{row["Manufacturer"].ToString().PadRight(20)}{row["TotalQuantity"]}");
            }
        }
        static void DisplayTypeQuantity(DataTable dataTable)
        {
            Console.WriteLine($"{"Type".PadRight(20)}Total quantity");
            foreach (DataRow row in dataTable.Rows)
            {
                Console.WriteLine($"{row["ProductType"].ToString().PadRight(20)}{row["TotalQuantity"]}");
            }
        }
        static void DisplayProducts(DataTable dataTable)
        {
            Console.WriteLine($"{"ID".PadRight(20)}{"Name".PadRight(20)}{"TypeID".PadRight(20)}{"ManufacturerID".PadRight(20)}{"Number".PadRight(20)}{"CostPrice".PadRight(20)}Date");
            foreach (DataRow row in dataTable.Rows)
            {
                Console.WriteLine($"{row["ID"].ToString().PadRight(20)}{row["Name"].ToString().PadRight(20)}{row["TypeID"].ToString().PadRight(20)}{row["ManufacturerID"].ToString().PadRight(20)}{row["Number"].ToString().PadRight(20)}{row["CostPrice"].ToString().PadRight(20)}{row["Date"].ToString().PadRight(20)}");
            }
        }
    }
}