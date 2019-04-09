using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace TDS.InventoryManagement.QBD
{
  public  class ConfigureService
    {
        internal static void Configure()
        {

            HostFactory.Run(configure =>
            {
                configure.Service<ScheduleService>(service =>
                {
                    service.ConstructUsing(s => new ScheduleService());
                    service.WhenStarted(s => s.Start());
                    service.WhenStopped(s => s.Stop());

                    //service.ScheduleQuartzJob(z =>
                    //z.WithJob(() => JobBuilder.Create<MyJob>().Build())
                    //.AddTrigger(() => TriggerBuilder.Create()
                    //.WithSimpleSchedule(b => b.WithIntervalInSeconds(10)
                    //.RepeatForever())
                    //.Build()));

                });
                //configure.s
                //Setup Account that window service use to run.  
                configure.RunAsLocalSystem().DependsOnEventLog()
                                            .StartAutomatically();
                                           // .EnableServiceRecovery(rc => rc.RestartService(60000));

                configure.SetServiceName("TDS data checking in QB Schedulling...");
                configure.SetDisplayName("DS data checking in QB ...");
                configure.SetDescription("DS data checking in QB.");
            });
        }
    }
}
