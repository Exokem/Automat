using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Automat
{
    /// <summary>
    /// Capabilities
    /// 
    /// 1. Scan directory for all aseprite files
    /// 2. Run aseprite split command to dump all split resources into a default folder
    /// 
    /// 3. Displays a list of all configured directories, with an edit/run button
    /// 
    /// Configuration
    /// 
    /// 1. Ignored files
    /// 2. Whitelist mode (only split specified files)
    /// 3. Configuration saved per-directory
    /// 4. Destination folder
    /// 5. Destination folder per aseprite file
    /// 6. Move or copy mode for exported resources
    /// 
    /// </summary>
    public partial class App : Application
    {

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            
        }
    }
}
