using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.ServiceModel;
using System.IO;

namespace GhostscriptService
{
	public partial class GhostscriptService : ServiceBase
    {
        public ServiceHost _serviceHost = null;

		public GhostscriptService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (_serviceHost != null)
            {
                _serviceHost.Close();
            }

			//Delete previous dynamic loading DLLs if there are any.
			InstancesManager.DeleteDynamicLoadingDLL();

            // Create a ServiceHost for the ConvertToPdfService type and provide the base address.
            _serviceHost = new ServiceHost(typeof(ConverterService));
			
            // Open the ServiceHostBase to create listeners and start listening for messages.
            _serviceHost.Open();
        }

        protected override void OnStop()
        {
            if (_serviceHost != null)
            {
				_serviceHost.Close();
				InstancesManager.DeleteDynamicLoadingDLL();
                _serviceHost = null;
            }
        }
    }
}
