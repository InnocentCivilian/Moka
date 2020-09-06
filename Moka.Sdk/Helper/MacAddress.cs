using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace Moka.Sdk.Helper
{
    public class MacAddress
    {
        public static string GetMacAddress()
        {
            string macAddresses = string.Empty;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    macAddresses += nic.GetPhysicalAddress().ToString();
                    break;
                }
            }

            return macAddresses;
        }

        public static string GetDefaultMacAddress()
        {
            Dictionary<string, long> macAddresses = new Dictionary<string, long>();
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                    macAddresses[nic.GetPhysicalAddress().ToString()] =
                        nic.GetIPStatistics().BytesSent + nic.GetIPStatistics().BytesReceived;
            }

            long maxValue = 0;
            string mac = "";
            foreach (KeyValuePair<string, long> pair in macAddresses)
            {
                if (pair.Value > maxValue)
                {
                    mac = pair.Key;
                    maxValue = pair.Value;
                }
            }

            return mac;
        }

        public static PhysicalAddress GetCurrentMAC(string allowedURL = "www.google.com", int port = 80)
        {
            //create tcp client
            var client = new TcpClient();

            //start connection
            client.Client.Connect(new IPEndPoint(Dns.GetHostAddresses(allowedURL)[0], port));

            //wai while connection is established
            while (!client.Connected)
            {
                Thread.Sleep(500);
            }

            //get the ip address from the connected endpoint
            var ipAddress = ((IPEndPoint) client.Client.LocalEndPoint).Address;

            //if the ip is ipv4 mapped to ipv6 then convert to ipv4
            if (ipAddress.IsIPv4MappedToIPv6)
                ipAddress = ipAddress.MapToIPv4();

            Debug.WriteLine(ipAddress);

            //disconnect the client and free the socket
            client.Client.Disconnect(false);

            //this will dispose the client and close the connection if needed
            client.Close();

            var allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            //return early if no network interfaces found
            if (!(allNetworkInterfaces?.Length > 0))
                return null;

            foreach (var networkInterface in allNetworkInterfaces)
            {
                //get the unicast address of the network interface
                var unicastAddresses = networkInterface.GetIPProperties().UnicastAddresses;

                //skip if no unicast address found
                if (!(unicastAddresses?.Count > 0))
                    continue;

                //compare the unicast addresses to see 
                //if any match the ip address used to connect over the network
                for (var i = 0; i < unicastAddresses.Count; i++)
                {
                    var unicastAddress = unicastAddresses[i];

                    //this is unlikely but if it is null just skip
                    if (unicastAddress.Address == null)
                        continue;

                    var ipAddressToCompare = unicastAddress.Address;

                    Debug.WriteLine(ipAddressToCompare);

                    //if the ip is ipv4 mapped to ipv6 then convert to ipv4
                    if (ipAddressToCompare.IsIPv4MappedToIPv6)
                        ipAddressToCompare = ipAddressToCompare.MapToIPv4();

                    Debug.WriteLine(ipAddressToCompare);

                    //skip if the ip does not match
                    if (!ipAddressToCompare.Equals(ipAddress))
                        continue;

                    //return the mac address if the ip matches
                    return networkInterface.GetPhysicalAddress();
                }
            }

            //not found so return null
            return null;
        }
    }
}