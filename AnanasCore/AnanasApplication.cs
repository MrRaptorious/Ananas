using AnanasCore.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnanasCore
{
    /// <summary>
    /// The main object representing the entire program
    /// </summary>
    public class AnanasApplication
    {
        public ApplicationSubManager DefaultSubManager { get; private set; }

        private readonly Dictionary<string, ApplicationSubManager> subManagers;
        private static AnanasApplication application;

        private AnanasApplication()
        {
            subManagers = new Dictionary<string, ApplicationSubManager>();
        }

        public static AnanasApplication GetApplication()
        {
            if (application == null)
                application = new AnanasApplication();

            return application;
        }

        public void RegisterApplicationSubManager(string name, ApplicationSubManager subManager, bool isDefault = false)
        {
            subManagers.Put(name, subManager);

            if (isDefault)
                DefaultSubManager = subManager;
        }

        public ApplicationSubManager GetApplicationSubManager(string name)
        {
            return subManagers[name];
        }

        public void Start()
        {
            foreach (var managerSet in subManagers)
            {
                managerSet.Value.Start();
            }
        }
    }
}
