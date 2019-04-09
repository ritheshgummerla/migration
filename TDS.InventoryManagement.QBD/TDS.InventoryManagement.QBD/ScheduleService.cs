using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDS.InventoryManagement.QBD.Jobs;
using TDS.InventoryManagement.QBD.Log;

namespace TDS.InventoryManagement.QBD
{
    public class ScheduleService
    {
        private readonly IScheduler _scheduler;
        public ScheduleService()
        {
            if (_scheduler == null)
                _scheduler = StdSchedulerFactory.GetDefaultScheduler();
        }

        public void Start()
        {
            try
            {
                _scheduler.Start();
                SchedulePrsReportExportJob();
               LogMessageService.log.Fatal("Daily Reporting service started " + DateTime.Now.ToLongDateString());
            }
            catch (Exception e)
            { }
        }
        public void Stop()
        {
            _scheduler.Shutdown(true);
            LogMessageService.log.Fatal("Daily Reporting service stopped " + DateTime.Now.ToLongDateString());
        }
        //Scheduling part
        private void SchedulePrsReportExportJob()
        {

            var jobDetail = JobBuilder.Create<TDSJobService>()
                .WithIdentity("job1", "group1")
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("trigger1", "group1")
                //.WithSchedule(CronScheduleBuilder.CronSchedule("0 1 0 ? * *"))  //minute past midnight everyday
                //.StartAt(_startTime)
                //.WithSchedule(CronScheduleBuilder.CronSchedule("0 0/5 * ? * *"))  //minute past midnight everyday
                //.WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(Hour, Min))
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(600).RepeatForever())
                .StartNow()
                .Build();

            _scheduler.ScheduleJob(jobDetail, trigger);
        }

    }
    //public class MyJob : IJob
    //{

    //    public void Execute(IJobExecutionContext context)
    //    {
    //        try
    //        {
    //            DataManipulation.GetMetrialData();
    //            //Email("Execution Execution");
    //        }
    //        catch (Exception ex)
    //        {
    //            DataManipulation.Email(ex.ToString());
    //        }

    //    }

    //}
}
