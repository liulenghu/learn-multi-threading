using System;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Recipe9_3
{
    class Program
    {
        /// <summary>
        /// 异步操作数据库
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            const string dataBaseName = "CustomDatabase";
            var t = ProcessAsynchronousIO(dataBaseName);
            t.GetAwaiter().GetResult();
            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();
        }

        static async Task ProcessAsynchronousIO(string dbName)
        {
            try
            {
                const string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True";
                string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                string dbFileName = Path.Combine(outputFolder, $"{dbName}.mdf");
                string dbLogFileName = Path.Combine(outputFolder, $"{dbName}_log.ldf");
                string dbConnectionString = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDBFileName={dbFileName};Integrated Security=True;";

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    if (File.Exists(dbFileName))
                    {
                        Console.WriteLine("Detaching the database...");
                        // 分离数据库（sp_detach_db：该命令常用来删除数据库日志文件或者移动数据库文件）
                        var detachCommand = new SqlCommand("sp_detach_db", connection);
                        detachCommand.CommandType = System.Data.CommandType.StoredProcedure;
                        detachCommand.Parameters.AddWithValue("@dbname", dbName);

                        await detachCommand.ExecuteNonQueryAsync();

                        Console.WriteLine("The database was detached successfully.");
                        Console.WriteLine("Deleting the database");

                        // 删除数据库日志文件
                        if (File.Exists(dbLogFileName))
                        {
                            File.Delete(dbLogFileName);
                        }
                        // 删除数据库文件
                        File.Delete(dbFileName);

                        Console.WriteLine("The database was deleted successfully");
                    }

                    Console.WriteLine("Creating the database");
                    // 创建数据库
                    string createCommand = $"CREATE DATABASE {dbName} ON (NAME = N'{dbName}', FILENAME = '{dbFileName}')";
                    var cmd = new SqlCommand(createCommand, connection);

                    await cmd.ExecuteNonQueryAsync();

                    Console.WriteLine("The database was created successfully");
                }

                using (var connection = new SqlConnection(dbConnectionString))
                {
                    // 异步连接数据库
                    await connection.OpenAsync();
                    var cmd = new SqlCommand("SELECT newid()", connection);
                    var result = await cmd.ExecuteNonQueryAsync();

                    // 创建表
                    cmd = new SqlCommand(@"CREATE TABLE [dbo].[CustomTable]([ID] [int] IDENTITY(1,1) NOT NULL,
[NAME] [nvarchar](50) NOT NULL,
CONSTRAINT [PK_ID] PRIMARY KEY CLUSTERED ([ID] ASC) ON [PRIMARY]) ON [PRIMARY]", connection);
                    await cmd.ExecuteNonQueryAsync();
                    Console.WriteLine("Table was created successfully");

                    // 插入数据
                    cmd = new SqlCommand(@"INSERT INTO [dbo].[CustomTable] (Name) VALUES ('John');
INSERT INTO [dbo].[CustomTable] (Name) VALUES ('Peter');
INSERT INTO [dbo].[CustomTable] (Name) VALUES ('james');
INSERT INTO [dbo].[CustomTable] (Name) VALUES ('Eugene');", connection);
                    await cmd.ExecuteNonQueryAsync();
                    Console.WriteLine("Inserted data successfully");
                    Console.WriteLine("Reading data from table...");

                    // 查询上面插入的数据
                    cmd = new SqlCommand(@"SELECT * FROM [dbo].[CustomTable]", connection);
                    // 异步查询数据
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        // 异步读取查询结果
                        while (await reader.ReadAsync())
                        {
                            var id = reader.GetFieldValue<int>(0);
                            var name = reader.GetFieldValue<string>(1);

                            Console.WriteLine("Table row: Id {0}, Name {1}", id, name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }
        }
    }
}
