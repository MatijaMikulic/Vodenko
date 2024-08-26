using PlcCommunication;
using S7.Net;
using S7.Net.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PLCTestService.Service
{
    public class Test
    {
        private readonly PlcCommunicationService _plcCommunicationService;
        private readonly System.Timers.Timer _timer;

        public Test(PlcCommunicationService plcCommunicationService)
        {
            _plcCommunicationService = plcCommunicationService;
            _timer = new System.Timers.Timer(2000) { AutoReset = true };
            _timer.Elapsed += TimerElapsed;
        }
        public async Task Start()
        {
            _plcCommunicationService.Start();
            _plcCommunicationService.PropertyChanged += LogPlcConnectionChange;
            _timer.Start();
        }
        private void TimerElapsed(object? sender, ElapsedEventArgs e)
        {
            if (!_plcCommunicationService.IsConnected)
            {
                return;
            }
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                _plcCommunicationService.DataAccess.TestReading();
                stopwatch.Stop();
                Console.WriteLine($"TimerElapsed function execution time: {stopwatch.ElapsedMilliseconds} ms");
                //float pv = 90;
                //_plcCommunicationService.DataAccess._plc.Write("DB24.DBD0", pv);
                //Console.WriteLine("Successfully wrote");
            }
            catch (PlcException ex)
            {
                //Console.WriteLine(ex.Message);
                return;
            }
        }
        public void Stop()
        {
            _plcCommunicationService.Dispose();
        }
        private void LogPlcConnectionChange(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_plcCommunicationService.IsConnected))
            {
                bool isConnected = _plcCommunicationService.IsConnected;
                if (isConnected)
                {
                    Console.WriteLine($"Established connection to plc!");
                }
                else
                {
                    Console.WriteLine($"Lost connection to plc!");
                }
            }
        }
    }
}
