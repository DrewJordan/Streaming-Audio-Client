namespace AudioService
{
    public class Host
    {
        public Host()
        {

        }

        public Host(string name, string ip, int port)
        {
            Name = name;
            IP = ip;
            Port = port;
        }
        public string Name { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }

        public string Display { get { return string.Join(" | ", Name, IP, Port); } }
    }
}
