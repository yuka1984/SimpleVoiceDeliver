using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleVoiceDeliverService;
using Topshelf;

namespace SimpleVoiceReliverService
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<IService>(s =>
                {
                    s.ConstructUsing(name => new DeliverService());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                //Windowsサービスの設定
                x.RunAsLocalSystem();
                x.SetDescription("This is TopShelfSample");
                x.SetDisplayName("TopShelfSample");
                x.SetServiceName("TopShelfSample_Service");
            });
        }
    }
}
