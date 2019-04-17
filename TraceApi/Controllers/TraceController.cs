using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TraceApi.Events;

namespace TraceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TraceController : ControllerBase
    {

        // GET api/trace
        [HttpGet]
        public ActionResult<string> Get(int id)
        {
            return "I'm a traceable dotnet core API!  Attach via dotnet-trace and Event Pipe!";
        }

        // GET api/trace/gcstress
        [HttpGet("gcstress")]
        public ActionResult<string> GetGcStress()
        {
            RunLambdaUntil((token) =>
            {
                while (!token.IsCancellationRequested) GC.Collect();
            }, new TimeSpan(0,0,5));

            return "looping string concatenation creates GC stress";
        }

        // GET api/trace/eventsource
        [HttpGet("eventsource")]
        public ActionResult<string> GetEventSource()
        {
            // await RunLambdaUntil((token) =>
            // {
            //     while (!token.IsCancellationRequested) MyEventSource.Log.SomethingHappened();
            // }, new TimeSpan(0,0,5));

            var n = RecurUntil(() => MyEventSource.Log.SomethingHappened(), new TimeSpan(0,0,5));

            return $"Recurred over MyEventSource {n} times" ;
        }

        // GET api/trace/eventsource/{msg}
        [HttpGet("eventsource/{msg}")]
        public ActionResult<string> GetEventSource(string msg)
        {
            // RunLambdaUntil((token) =>
            // {
            //     while (!token.IsCancellationRequested) MyEventSource.Log.SomethingElseHappened(msg);
            // }, new TimeSpan(0,0,5));

            var n = RecurUntil(() => MyEventSource.Log.SomethingElseHappened(msg), new TimeSpan(0,0,5));


            return $"Recurred over MyEventSource with custom msg {n} times";
        }

        // GET api/trace/cpustress
        [HttpGet("cpustress")]
        public ActionResult<string> GetCpuStress()
        {
            RunLambdaUntil((token) =>
            {
                var foo = 0;
                while (!token.IsCancellationRequested) foo++;
            }, new TimeSpan(0,0,5));

            return "looping int summation to induce a little CPU strain";
        }

        private void RunLambdaUntil(Action<CancellationToken> lambda, TimeSpan delay)
        {
            var cts = new CancellationTokenSource(delay);
            lambda(cts.Token);
        }

        private int Recur(CancellationToken ct, Action action, int acc = 0)
        {
            action();
            System.Threading.Thread.Sleep(500);
            if (ct.IsCancellationRequested)
                return acc;
            return Recur(ct, action, acc) + 1;
        }

        private int RecurUntil(Action action, TimeSpan delay)
        {
            var cts = new CancellationTokenSource(delay);
            return Recur(cts.Token, action);
        }
    }
}
