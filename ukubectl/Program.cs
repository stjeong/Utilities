using k8s;
using k8s.KubeConfigModels;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ukubectl
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Help();
                return;
            }

            string path = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.kube\config");
            KubernetesClientConfiguration config = KubernetesClientConfiguration.BuildConfigFromConfigFile(path);

            IKubernetes client = new Kubernetes(config);
            string cmd = args[0];

            switch (cmd)
            {
                case "--set-default-token":
                    {
                        string userName = K8sUtil.GetCurrentContextUserName(path, config.CurrentContext);
                        string defaultSecret = K8sUtil.GetDefaultSecret(client, out _);
                        if (config.AccessToken == defaultSecret)
                        {
                            return;
                        }

                        K8sUtil.RunKubectl($"config set-credentials {userName} --token=\"{defaultSecret}\"");
                    }
                    break;

                case "--register-cert":
                    {
                        InstallCaCertData(path);

                        string password = null;

                        do
                        {
                            if (args.Length == 2)
                            {
                                password = args[1];
                            }
                            else
                            {
                                password = ConsoleHelper.ReadPassword("PFX Password: ");
                            }
                        } while (string.IsNullOrEmpty(password) == true);

                        string userName = K8sUtil.GetCurrentContextUserName(path, config.CurrentContext);
                        string userKey = Encoding.UTF8.GetString(Convert.FromBase64String(config.ClientCertificateKeyData));
                        string userCrt = config.ClientCertificateData;

                        string friendlyName = BouncyCastleHelper.InstallCertificate(userName, password, userKey, userCrt);
                        Console.WriteLine($"CN={friendlyName} certificate is installed at CurrentUser/Personal");
                    }
                    break;

                default:
                    Help();
                    break;
            }
        }

        private static void InstallCaCertData(string kubeconfigPath)
        {
            K8SConfiguration k8sConfig = KubernetesClientConfiguration.LoadKubeConfig(kubeconfigPath);
            foreach (Cluster item in k8sConfig.Clusters)
            {
                string cnName = BouncyCastleHelper.InstallCertificate(item.ClusterEndpoint.CertificateAuthorityData);
                Console.WriteLine($"{cnName} certificate is installed at CurrentUser/Root");
            }
        }

        private static void Help()
        {
            Console.WriteLine(@"Usage: ukubectl <commands>
ukubectl controls the Kubernetes cluster manager.

Basic Commands:
  --set-default-token           Set access token to kubeconfig from secrets.
  --register-cert [password]    Register certificates for CA and User in kubeconfig.
");
        }
    }
}
