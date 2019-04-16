using System.Diagnostics.Tracing;

namespace TraceApi.Events
{
    class MyEventSource : EventSource
    {
        public static MyEventSource Log = new MyEventSource();

        public void SomethingHappened() => WriteEvent(1);
        public void SomethingElseHappened(string foo) => WriteEvent(2, foo);
    }
}