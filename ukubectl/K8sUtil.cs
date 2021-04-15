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

        /*
        public static void Apply(IKubernetes client, List<object> resList)
        {
            string applyNamespace = "default";

            foreach (object objRes in resList)
            {
                V1Patch patch = new V1Patch(objRes, V1Patch.PatchType.ApplyPatch);

                switch (objRes)
                {
                    case V1Namespace ns:
                        {
                            applyNamespace = ns.Name();

                            if (client.ListNamespace().Items.FirstOrDefault((item) => item.Name() == applyNamespace) == null)
                            {
                                client.CreateNamespace(ns);
                            }
                            else
                            {
                                client.ReplaceNamespace(ns, applyNamespace);
                            }

                            Console.WriteLine($"{ns.Kind.ToLower()}/{applyNamespace} created");
                        }
                        break;

                    case V1ServiceAccount svcAccount:
                        client.CreateNamespacedServiceAccount(svcAccount, applyNamespace);
                        Console.WriteLine($"{svcAccount.Kind.ToLower()}/{svcAccount.Name()} created");
                        break;

                    case V1Service svc:
                        client.CreateNamespacedService(svc, applyNamespace);
                        break;

                    case V1Secret secret:
                        client.CreateNamespacedSecret(secret, applyNamespace);
                        break;

                    case V1ConfigMap config:
                        client.CreateNamespacedConfigMap(config, applyNamespace);
                        break;

                    case V1Role role:
                        client.CreateNamespacedRole(role, applyNamespace);
                        break;

                    case V1ClusterRole clusterRole:
                        client.CreateClusterRole(clusterRole);
                        break;

                    case V1RoleBinding roleBinding:
                        client.CreateNamespacedRoleBinding(roleBinding, applyNamespace);
                        break;

                    case V1ClusterRoleBinding clusterRoleBinding:
                        client.CreateClusterRoleBinding(clusterRoleBinding);
                        break;

                    case V1Deployment deployment:
                        client.CreateNamespacedDeployment(deployment, applyNamespace);
                        break;
                }
            }
        }
        */

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
