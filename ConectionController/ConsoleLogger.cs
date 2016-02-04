using ConectionController.Communication.ReqResp;
using LRM.Communication;
using NetworkNode.LRM.Communication;
using RoutingController.Elements;
using System;
using System.Collections.Generic;

namespace ConectionController
{
    public class ConsoleLogger
    {
        public static string ConnectionRequest = "Connection request";
        public static string RouteQry = "Route table query";

        private static void Log(string data)
        {
            Console.WriteLine(data);
        }

        public static void PrintConnectionRequest(HigherLevelConnectionRequest request)
        {
            Log(ConnectionRequest);
            Log(TextUtils.Dash);
            Log("src: " + request.Src.Name + " at port " + request.Src.Port);
            Log("dst: " + request.Dst.Name + " at port " + request.Dst.Port);
        }

        public static void PrintRouteTableQuery(SimpleConnection request)
        {
            Log(RouteQry);
            Log("@ " + request.Domain);
            Log(TextUtils.Dash);
            Log("src: " + request.Ends[0].Name + " at port " + request.Ends[0].Port);
            Log("dst: " + request.Ends[1].Name + " at port " + request.Ends[1].Port);
        }

        public static void PrintConnection(ConnectionRequest connection, bool details)
        {
            foreach (ConnectionStep step in connection.Steps)
            {
                foreach (LrmPort port in step.Ports)
                {
                    Log("Element : " + step.Node + " at port " + port.Number);
                    if (details)
                    {
                        Console.Write(" at vc index " + port.Index);
                    }
                }
                Log("");
            }
        }

        public static void PrintSNPP(List<SNP> snpp)
        {
            foreach (SNP snp in snpp)
            {
                foreach (string port in snp.Ports)
                {
                    if (port != null)
                    {
                        Log("Snpp : " + snp.Node + " at port " + port + " at domain " + snp.Domain);
                    }
                }
                Log("");
            }
        }
    }
}