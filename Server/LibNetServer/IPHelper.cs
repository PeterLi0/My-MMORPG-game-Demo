
using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

public class IPHelper
{
    public static IPAddress GetEthernetIP(NetworkInterfaceType type)
    {
        //获取所有网卡信息
        NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
        foreach (NetworkInterface adapter in nics)
        {
            //判断是否为以太网卡
            if (adapter.NetworkInterfaceType == type && adapter.OperationalStatus == OperationalStatus.Up)
            {
                //获取以太网卡网络接口信息
                IPInterfaceProperties ip = adapter.GetIPProperties();

                //获取单播地址集
                UnicastIPAddressInformationCollection ipCollection = ip.UnicastAddresses;
                foreach (UnicastIPAddressInformation ipadd in ipCollection)
                {
                    // InterNetwork    IPV4地址      
                    // InterNetworkV6        IPV6地址
                    // Max            MAX 位址
                    if (ipadd.Address.AddressFamily == AddressFamily.InterNetwork)
                        //判断是否为ipv4
                        return ipadd.Address;//获取ip
                }
            }
        }

        return null;
    }
}