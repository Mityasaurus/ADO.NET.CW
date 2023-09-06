using System.Configuration;
using System.Data;
using System.Data.SqlClient;
namespace Warehouse
{
    internal class Program
    {
        private static string ConnString => ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
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
                        string insertTypeQuery = "INSERT INTO TypesOfProducts (Name) VALUES ('Тип 1'), ('Тип 2'), ('Тип 3')";
                        using (SqlCommand cmd = new SqlCommand(insertTypeQuery, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        string insertManufacturerQuery = "INSERT INTO Manufacturers (Name) VALUES ('Постачальник 1'), ('Постачальник 2'), ('Постачальник 3')";
                        using (SqlCommand cmd = new SqlCommand(insertManufacturerQuery, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        string insertProductQuery = "INSERT INTO Products (Name, TypeID, ManufacturerID, Number, CostPrice, Date) VALUES " +
                                                    "('Товар 1', 1, 1, 100, 10, '2023-08-01'), " +
                                                    "('Товар 2', 2, 2, 150, 15, '2023-08-02'), " +
                                                    "('Товар 3', 1, 1, 200, 8, '2023-08-03')";
                        using (SqlCommand cmd = new SqlCommand(insertProductQuery, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }

                    string query = "select * from Products as p, Manufacturers as m, TypesOfProducts as t where p.TypeID=t.ID and p.ManufacturerID=m.ID";

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    DataSet ds = new DataSet();

                    int choice = -1;
                    while (choice != 0)
                    {
                        Console.WriteLine("\nВведiть свiй вибiр");
                        Console.WriteLine("1 - Вiдображення всiєї iнформацiї про товар");
                        Console.WriteLine("2 - Вiдображення всiх типiв товарiв");
                        Console.WriteLine("3 - Вiдображення всiх постачальникiв");
                        Console.WriteLine("4 - Показати товар з максимальною кiлькiстю");
                        Console.WriteLine("5 - Показати товар з мiнiмальною кiлькiстю");
                        Console.WriteLine("6 - Показати товар з мiнiмальною собiвартiстю");
                        Console.WriteLine("7 - Показати товар з максимальною собiвартiстю");
                        Console.WriteLine("0 - Вихiд");

                        choice = int.Parse(Console.ReadLine());
                        query = "";

                        switch (choice)
                        {
                            case 1: Console.Clear();
                                query = "select * from Products as p, Manufacturers as m, TypesOfProducts as t where p.TypeID=t.ID and p.ManufacturerID=m.ID";
                                GetData(query, ds, conn);
                                DisplayProducts(ds.Tables[0]);
                                break;
                            case 2:
                                Console.Clear();
                                query = "SELECT * from TypesOfProducts";
                                GetData(query, ds, conn);
                                DisplayTypes(ds.Tables[0]);
                                break;
                            case 3:
                                Console.Clear();
                                query = "select * from Manufacturers";
                                GetData(query, ds, conn);
                                DisplaySuppliers(ds.Tables[0]);
                                break;
                            case 4:
                                Console.Clear();
                                query = "SELECT TOP 1 * FROM Products ORDER BY Number DESC";
                                GetData(query, ds, conn);
                                DisplayProducts(ds.Tables[0]);
                                break;
                            case 5:
                                Console.Clear();
                                query = "SELECT TOP 1 * FROM Products ORDER BY Number ASC";
                                GetData(query, ds, conn);
                                DisplayProducts(ds.Tables[0]);
                                break;
                            case 6:
                                Console.Clear();
                                query = "SELECT TOP 1 * FROM Products ORDER BY CostPrice ASC";
                                GetData(query, ds, conn);
                                DisplayProducts(ds.Tables[0]);
                                break;
                            case 7:
                                Console.Clear();
                                query = "SELECT TOP 1 * FROM Products ORDER BY CostPrice DESC";
                                GetData(query, ds, conn);
                                DisplayProducts(ds.Tables[0]);
                                break;
                            case 0:
                                break;
                            default:
                                Console.WriteLine("Помилковий вибір!");
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

        static void DisplayProducts(DataTable dataTable)
        {
            Console.WriteLine($"{"ID".PadRight(5)}{"Назва товару".PadRight(20)}{"Тип товару".PadRight(15)}{"Постачальник".PadRight(20)}{"Кількість".PadRight(10)}{"Собівартість".PadRight(15)}{"Дата постачання"}");
            foreach (DataRow row in dataTable.Rows)
            {
                Console.WriteLine($"{row["ID"].ToString().PadRight(5)}{row["Name"].ToString().PadRight(20)}{row["TypeID"].ToString().PadRight(15)}{row["ManufacturerID"].ToString().PadRight(20)}{row["Number"].ToString().PadRight(10)}{row["CostPrice"].ToString().PadRight(15)}{row["Date"]}");
            }
        }

        static void DisplayTypes(DataTable dataTable)
        {
            Console.WriteLine("Типи товарів:");
            foreach (DataRow row in dataTable.Rows)
            {
                Console.WriteLine(row["Name"]);
            }
        }

        static void DisplaySuppliers(DataTable dataTable)
        {
            Console.WriteLine("Постачальники:");
            foreach (DataRow row in dataTable.Rows)
            {
                Console.WriteLine(row["Name"]);
            }
        }
    }
}