using System;
using AnanasCore;
using AnanasSQLite;
using Microsoft.Data.Sqlite;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string connection = "Data Source=hello.db";

            AnanasApplication aapp = AnanasApplication.getApplication();

            ApplicationSubManager localmanager = new ApplicationSubManager(new DependencyConfigurationSQLite(), connection);

            localmanager.registerType<Tier>();
            aapp.registerApplicationSubManager("local", localmanager, true);
            aapp.start();

            ObjectSpace os = localmanager.createObjectSpace();

            Tier t = new Tier(os);
            t.Age = 22;
            t.Name = "hans";

            os.commitChanges();

        }
    }
}
