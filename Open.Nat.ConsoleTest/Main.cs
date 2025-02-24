//
// Authors:
//   Ben Motmans <ben.motmans@gmail.com>
//
// Copyright (C) 2007 Ben Motmans
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Open.Nat.ConsoleTest
{
  class NatTest
  {
    public static void Main(string[] args)
    {
      //Nat();
      NatNat();
    }

    private static void NatNat()
    {
      const int PublicPort = 1700;
      const int PrivatePort = 1600;
      const int NatNatPort = 1701;
      NatDiscoverer.TraceSource.Switch.Level = SourceLevels.Verbose;
      NatDiscoverer.TraceSource.Listeners.Add(new ConsoleTraceListener());

      var t = Task.Run(async () =>
      {
        var nat = new NatDiscoverer();
        var cts = new CancellationTokenSource(5000);
        var device = await nat.DiscoverDeviceAsync(PortMapper.Upnp, cts);
            
        var sb = new StringBuilder();
        var ip = await device.GetExternalIPAsync();

        sb.AppendFormat("\nYour IP: {0}", ip);
        //await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, PrivatePort, NatNatPort, "Internal NAT"));
        //await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, ip, NatNatPort, PublicPort, 86400, "Public NAT"));
        await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, PrivatePort, PublicPort, "NAT"));
        sb.AppendFormat("\nAdded mapping: {0}:1700 -> 127.0.0.1:1600\n", ip);
        sb.AppendFormat("\n+------+-------------------------------+--------------------------------+------------------------------------+-------------------------+");
        sb.AppendFormat("\n| PROT | PUBLIC (Reacheable)		   | PRIVATE (Your computer)		| Descriptopn						|						 |");
        sb.AppendFormat("\n+------+----------------------+--------+-----------------------+--------+------------------------------------+-------------------------+");
        sb.AppendFormat("\n|	  | IP Address		   | Port   | IP Address			| Port   |  | Expires				 |");
        sb.AppendFormat("\n+------+----------------------+--------+-----------------------+--------+------------------------------------+-------------------------+");

        foreach (var mapping in await device.GetAllMappingsAsync())
        {
          sb.AppendFormat("\n|  {5} | {0,-20} | {1,6} | {2,-21} | {3,6} | {4,-35}|{6,25}|", ip, mapping.PublicPort, mapping.PrivateIP, mapping.PrivatePort, mapping.Description, mapping.Protocol == Protocol.Tcp ? "TCP" : "UDP", mapping.Expiration.ToLocalTime());
        }

        sb.AppendFormat("\n+------+----------------------+--------+-----------------------+--------+------------------------------------+-------------------------+");

        Console.WriteLine(sb.ToString());
        sb.Clear();

        Console.ReadKey();

        sb.AppendFormat("\n[Removing TCP mapping] {0}:1700 -> 127.0.0.1:1600", ip);
        await device.DeletePortMapAsync(new Mapping(Protocol.Tcp, PrivatePort, PublicPort));
        //await device.DeletePortMapAsync(new Mapping(Protocol.Tcp, PrivatePort, NatNatPort));
        sb.AppendFormat("\n[Done]");

        Console.WriteLine(sb.ToString());
        /*
                                        var mappings = await device.GetAllMappingsAsync();
                                        var deleted = mappings.All(x => x.Description != "Open.Nat Testing");
                                        Console.WriteLine(deleted
                                                ? "[SUCCESS]: Test mapping effectively removed ;)"
                                                : "[FAILURE]: Test mapping wan not removed!");
        */
      });

      try
      {
        t.Wait();
      }
      catch (AggregateException e)
      {
        if (e.InnerException is NatDeviceNotFoundException)
        {
          Console.WriteLine("Not found");
          Console.WriteLine("Press any key to exit...");
        }
      }

      Console.ReadKey();
    }


    public static void Nat()
    {
      var t = Task.Run(async () =>
      {
        var nat = new NatDiscoverer();
        var cts = new CancellationTokenSource(5000);
        var device = await nat.DiscoverDeviceAsync(PortMapper.Upnp, cts);

        var sb = new StringBuilder();
        var ip = await device.GetExternalIPAsync();

        sb.AppendFormat("\nYour IP: {0}", ip);
        await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, 1600, 1700, "Open.Nat (temporary)"));
        await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, 1601, 1701, "Open.Nat (Session lifetime)"));
        await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, 1602, 1702, 0, "Open.Nat (Permanent lifetime)"));
        await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, 1603, 1703, 20, "Open.Nat (Manual lifetime)"));
        sb.AppendFormat("\nAdded mapping: {0}:1700 -> 127.0.0.1:1600\n", ip);
        sb.AppendFormat("\n+------+-------------------------------+--------------------------------+------------------------------------+-------------------------+");
        sb.AppendFormat("\n| PROT | PUBLIC (Reacheable)		   | PRIVATE (Your computer)		| Descriptopn						|						 |");
        sb.AppendFormat("\n+------+----------------------+--------+-----------------------+--------+------------------------------------+-------------------------+");
        sb.AppendFormat("\n|	  | IP Address		   | Port   | IP Address			| Port   |  | Expires				 |");
        sb.AppendFormat("\n+------+----------------------+--------+-----------------------+--------+------------------------------------+-------------------------+");

        foreach (var mapping in await device.GetAllMappingsAsync())
        {
          sb.AppendFormat("\n|  {5} | {0,-20} | {1,6} | {2,-21} | {3,6} | {4,-35}|{6,25}|", ip, mapping.PublicPort, mapping.PrivateIP, mapping.PrivatePort, mapping.Description, mapping.Protocol == Protocol.Tcp ? "TCP" : "UDP", mapping.Expiration.ToLocalTime());
        }

        sb.AppendFormat("\n+------+----------------------+--------+-----------------------+--------+------------------------------------+-------------------------+");

        Console.WriteLine(sb.ToString());
        sb.Clear();

        Console.ReadKey();

        sb.AppendFormat("\n[Removing TCP mapping] {0}:1700 -> 127.0.0.1:1600", ip);
        await device.DeletePortMapAsync(new Mapping(Protocol.Tcp, 1600, 1700));
        sb.AppendFormat("\n[Removing TCP mapping] {0}:1701 -> 127.0.0.1:1601", ip);
        await device.DeletePortMapAsync(new Mapping(Protocol.Tcp, 1601, 1701));
        sb.AppendFormat("\n[Removing TCP mapping] {0}:1702 -> 127.0.0.1:1602", ip);
        await device.DeletePortMapAsync(new Mapping(Protocol.Tcp, 1602, 1702));
        sb.AppendFormat("\n[Removing TCP mapping] {0}:1703 -> 127.0.0.1:1603", ip);
        await device.DeletePortMapAsync(new Mapping(Protocol.Tcp, 1603, 1703));
        sb.AppendFormat("\n[Done]");

        Console.WriteLine(sb.ToString());
        /*
                                        var mappings = await device.GetAllMappingsAsync();
                                        var deleted = mappings.All(x => x.Description != "Open.Nat Testing");
                                        Console.WriteLine(deleted
                                                ? "[SUCCESS]: Test mapping effectively removed ;)"
                                                : "[FAILURE]: Test mapping wan not removed!");
        */
      });

      try
      {
        t.Wait();
      }
      catch (AggregateException e)
      {
        if (e.InnerException is NatDeviceNotFoundException)
        {
          Console.WriteLine("Not found");
          Console.WriteLine("Press any key to exit...");
        }
      }

      Console.ReadKey();
    }
  }
}