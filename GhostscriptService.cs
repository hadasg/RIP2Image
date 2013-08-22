/*******************************************************************************
	RIP2Image is a program that efficiently converts formats such as PDF or Postscript to image formats such as Jpeg or PNG.
    Copyright (C) 2013 XMPie Ltd.

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*******************************************************************************/

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

namespace RIP2Jmage
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
