using AnanasCore.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnanasCore
{
    public class AnanasApplication
    {
        // has List<ApplicationSupManager>
        private readonly Dictionary<string, ApplicationSubManager> subManagers;
        private ApplicationSubManager defaultSubManager;

        private static AnanasApplication application;

        private AnanasApplication()
        {
            subManagers = new Dictionary<string, ApplicationSubManager>();
        }

        public static AnanasApplication getApplication()
        {
            if (application == null)
                application = new AnanasApplication();

            return application;
        }

        public void registerApplicationSubManager(string name, ApplicationSubManager subManager, bool isDefault = false)
        {
            subManagers.Put(name, subManager);

            if (isDefault)
                defaultSubManager = subManager;
        }

        public ApplicationSubManager getDefaultSubManager()
        {
            return defaultSubManager;
        }

        public ApplicationSubManager getApplicationSubManagerByName(String name)
        {
            return subManagers[name];
        }

        public void start()
        {
            foreach (var managerSet in subManagers)
            {
                managerSet.Value.start();
            }
        }
    }
}
