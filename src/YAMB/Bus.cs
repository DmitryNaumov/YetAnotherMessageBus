using YAMB.Configuration;

namespace YAMB
{
    public static class Bus
    {
        public static BusConfiguration Configure()
        {
            return new BusConfiguration();
        }
    }
}