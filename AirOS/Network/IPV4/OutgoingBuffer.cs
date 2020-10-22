﻿/*
* PROJECT:          Aura Operating System Development
* CONTENT:          To send packets
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Alexy Da Cruz <dacruzalexy@gmail.com>
*                   Port of Cosmos Code.
*/

using System.Collections.Generic;
using AirOS.Network.Drivers;
using AirOS.Network.ARP;
using System;
using Cosmos.Debug.Kernel;

namespace AirOS.Network.IPV4
{
    internal static class OutgoingBuffer
    {

        private class BufferEntry
        {
            public enum EntryStatus { ADDED, ARP_SENT, ROUTE_ARP_SENT, JUST_SEND, DONE, DHCP_REQUEST };

            public NetworkDevice NIC;
            public IPPacket Packet;
            public EntryStatus Status;
            public Address nextHop;

            public BufferEntry(NetworkDevice nic, IPPacket packet)
            {
                this.NIC = nic;
                this.Packet = packet;

                if(Packet.DestinationIP.IsBroadcastAddress())
                {
                    this.Status = EntryStatus.DHCP_REQUEST;
                }
                else
                {
                    this.Status = EntryStatus.ADDED;
                }                
            }
        }

        public static Debugger debugger = new Debugger("", "");

        private static List<BufferEntry> queue;

        private static void ensureQueueExists()
        {
            if (queue == null)
            {
                queue = new List<BufferEntry>();
            }
        }

        internal static void AddPacket(IPPacket packet)
        {
            ensureQueueExists();
            NetworkDevice nic = Config.FindInterface(packet.SourceIP);
            packet.SourceMAC = nic.MACAddress;
            queue.Add(new BufferEntry(nic, packet));
        }

        internal static void AddPacket(IPPacket packet, NetworkDevice device)
        {
            ensureQueueExists();
            packet.SourceMAC = device.MACAddress;
            queue.Add(new BufferEntry(device, packet));
        }

        internal static void Send()
        {
            bool wasenabled = false;
            /*
            if (Kernel.debugger.enabled)
            {
                Kernel.debugger.enabled = false;
                wasenabled = true;
            }
            */
            ensureQueueExists();
            int _deltaT = 0;
            int second = 0;

            while (!(queue.Count < 1))
            {
                if (_deltaT != Cosmos.HAL.RTC.Second)
                {
                    second++;
                    _deltaT = Cosmos.HAL.RTC.Second;
                }

                if (second >= 4)
                {
                    debugger.Send("No response in 4 secondes...");
                    break;
                }

                //foreach (BufferEntry entry in queue)
                for (int e = 0; e < queue.Count; e++)
                {
                    BufferEntry entry = queue[e];
                    if (entry.Status == BufferEntry.EntryStatus.ADDED)
                    {
                        
                        //Need to figure how this is working
                        if (Config.IsLocalAddress(entry.Packet.DestinationIP) == false)
                        {
                            entry.nextHop = Config.FindRoute(entry.Packet.DestinationIP);
                            if (entry.nextHop == null)
                            {
                                entry.Status = BufferEntry.EntryStatus.DONE;
                                continue;
                            }

                            if (ARPCache.Contains(entry.nextHop) == true)
                            {
                                entry.Packet.DestinationMAC = ARPCache.Resolve(entry.nextHop);

                                entry.NIC.QueueBytes(entry.Packet.RawData);

                                entry.Status = BufferEntry.EntryStatus.DONE;
                            }
                            else
                            {
                                ARPRequest_Ethernet arp_request = new ARPRequest_Ethernet(entry.NIC.MACAddress, entry.Packet.SourceIP,
                                    MACAddress.Broadcast, entry.nextHop, MACAddress.None);

                                entry.NIC.QueueBytes(arp_request.RawData);

                                entry.Status = BufferEntry.EntryStatus.ROUTE_ARP_SENT;
                            }
                            continue;
                        }
                        if (ARPCache.Contains(entry.Packet.DestinationIP) == true)
                        {
                            entry.Packet.DestinationMAC = ARPCache.Resolve(entry.Packet.DestinationIP);

                            entry.NIC.QueueBytes(entry.Packet.RawData);

                            entry.Status = BufferEntry.EntryStatus.DONE;
                        }
                        else
                        {
                            ARPRequest_Ethernet arp_request = new ARPRequest_Ethernet(entry.NIC.MACAddress, entry.Packet.SourceIP,
                                MACAddress.Broadcast, entry.Packet.DestinationIP, MACAddress.None);

                            entry.NIC.QueueBytes(arp_request.RawData);

                            entry.Status = BufferEntry.EntryStatus.ARP_SENT;
                        }
                    }
                    else if (entry.Status == BufferEntry.EntryStatus.DHCP_REQUEST)
                    {
                        entry.NIC.QueueBytes(entry.Packet.RawData);

                        entry.Status = BufferEntry.EntryStatus.DONE;

                    }
                    else if (entry.Status == BufferEntry.EntryStatus.JUST_SEND)
                    {
                        entry.NIC.QueueBytes(entry.Packet.RawData);

                        entry.Status = BufferEntry.EntryStatus.DONE;
                    }
                }
                int i = 0;
                while (i < queue.Count)
                {
                    if (queue[i].Status == BufferEntry.EntryStatus.DONE)
                    {
                        queue.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
            }
            if (wasenabled)
            {
                //Kernel.debugger.enabled = true;
            }
        }

        internal static void ARPCache_Update(ARPReply_Ethernet arp_reply)
        {
            ensureQueueExists();
            //foreach (BufferEntry entry in queue)
            for (int e = 0; e < queue.Count; e++)
            {
                BufferEntry entry = queue[e];
                if (entry.Status == BufferEntry.EntryStatus.ARP_SENT)
                {
                    var xDestIP = entry.Packet.DestinationIP.Hash;
                    var xSenderIP = arp_reply.SenderIP.Hash;
                    if (entry.Packet.DestinationIP.CompareTo(arp_reply.SenderIP) == 0)
                    {
                        entry.Packet.DestinationMAC = arp_reply.SenderMAC;

                        entry.Status = BufferEntry.EntryStatus.JUST_SEND;
                    }
                }
                else if (entry.Status == BufferEntry.EntryStatus.ROUTE_ARP_SENT)
                {
                    if (entry.nextHop.CompareTo(arp_reply.SenderIP) == 0)
                    {
                        entry.Packet.DestinationMAC = arp_reply.SenderMAC;

                        entry.Status = BufferEntry.EntryStatus.JUST_SEND;
                    }
                }
            }
        }
    }
}
