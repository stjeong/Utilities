using k8s.KubeConfigModels;
using k8s;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using k8s.Models;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace ukubectl
{
    class K8sUtil
    {
        public static string GetCurrentContextUserName(string kubeconfigPath, string currentConetxtName)
        {
            K8SConfiguration k8sConfig = KubernetesClientConfiguration.LoadKubeConfig(kubeconfigPath);

            foreach (var item in k8sConfig.Contexts)
            {
                if (item.Name == currentConetxtName)
                {
                    return item.ContextDetails.User;
                }
            }

            return null;
        }

        public static string GetDefaultSecret(IKubernetes client, out string caCertificate)
        {
            caCertificate = string.Empty;

            var list = client.ListNamespacedSecret("default");
            foreach (var item in list.Items)
            {
                if (item.Data.ContainsKey("ca.crt"))
                {
                    caCertificate = Encoding.UTF8.GetString(item.Data["ca.crt"]);
                }

                if (item.Data.ContainsKey("token"))
                {
                    return Encoding.UTF8.GetString(item.Data["token"]);
                }
            }

            return string.Empty;
        }

        public static int RunKubectl(string arg)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "kubectl";
            psi.Arguments = arg;

            using (Process proc = Process.Start(psi))
            {
                proc.WaitForExit();
                return proc.ExitCode;
            }
        }
    }
}
