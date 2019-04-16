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
        public async Task<ActionResult<string>> GetGcStress()
        {
            await RunLambdaUntil((token) =>
            {
                string str = "";
                while (!token.IsCancellationRequested) str += "Str";
            }, new TimeSpan(0,0,5));

            return "looping string concatenation creates GC stress";
        }

        // GET api/trace/eventsource
        [HttpGet("eventsource")]
        public async Task<ActionResult<string>> GetEventSource()
        {
            await RunLambdaUntil((token) =>
            {
                while (!token.IsCancellationRequested) MyEventSource.Log.SomethingHappened();
            }, new TimeSpan(0,0,5));

            return "looping over MyEventSource" ;
        }

        // GET api/trace/eventsource/{msg}
        [HttpGet("eventsource/{msg}")]
        public async Task<ActionResult<string>> GetEventSource(string msg)
        {
            await RunLambdaUntil((token) =>
            {
                while (!token.IsCancellationRequested) MyEventSource.Log.SomethingElseHappened(msg);
            }, new TimeSpan(0,0,5));

            return "looping over MyEventSource with custom msg" ;
        }

        // GET api/trace/cpustress
        [HttpGet("cpustress")]
        public async Task<ActionResult<string>> GetCpuStress()
        {
            await RunLambdaUntil((token) =>
            {
                var foo = 0;
                while (!token.IsCancellationRequested) foo++;
            }, new TimeSpan(0,0,5));

            return "looping int summation to induce a little CPU strain";
        }

        private async Task RunLambdaUntil(Action<CancellationToken> lambda, TimeSpan delay)
        {
            var cts = new CancellationTokenSource(delay);
            await Task.Run(() => lambda(cts.Token), cts.Token);
        }
    }
}
