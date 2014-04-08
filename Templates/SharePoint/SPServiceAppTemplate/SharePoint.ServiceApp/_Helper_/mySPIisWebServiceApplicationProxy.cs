using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;

namespace SBrickey.SPServiceAppTemplate.SharePoint.ServiceApp._Helper_
{
    /// <summary>
    /// Provides the SPIisWebServiceApplicationProxy plumbing between the contract (T) and the service application.
    /// </summary>
    /// <typeparam name="T">The WCF contract being exposed as a service application</typeparam>
    /// <remarks>
    /// This is a slight adaptation based on previous work found at:
    /// - http://www.mattjcowan.com/funcoding/2012/10/03/sharepoint-2010-service-application-development-101-base-solution/
    /// - http://www.danlarson.com/step-by-step-sharepoint-service-applications-part-2-plumbing/
    /// </remarks>
    [IisWebServiceApplicationProxyBackupBehavior]
    [System.Runtime.InteropServices.Guid("4F06EEAB-51DC-4857-9DDF-5B7695EFAA15")]
    abstract public class mySPIisWebServiceApplicationProxy<T> : SPIisWebServiceApplicationProxy
    {
        [Persisted]
        private SPServiceLoadBalancer _loadBalancer;

        // Constructors
        public mySPIisWebServiceApplicationProxy() { }
        public mySPIisWebServiceApplicationProxy(string name, SPIisWebServiceProxy proxy, Uri serviceAddress) : base(name, proxy, serviceAddress)
        {
            // create instance of a new load balancer
            _loadBalancer = new SPRoundRobinServiceLoadBalancer(serviceAddress);
            this.Status = SPObjectStatus.Disabled;
        }

        #region Provisioning and Unprovisioning (relevant for the load balancer)

        public override void Provision()
        {
            this.Status = SPObjectStatus.Provisioning;
            this.Update();

            _loadBalancer.Provision();  // provision to load balancer / topology
            base.Provision();

            this.Status = SPObjectStatus.Online;
            this.Update();
        }

        public override void Unprovision(bool deleteData)
        {
            this.Status = SPObjectStatus.Unprovisioning;
            this.Update();
            _loadBalancer.Unprovision();    // remove from load balancer / topology
            base.Unprovision(deleteData);
            this.Status = SPObjectStatus.Disabled;
            this.Update();
        }

        #endregion

        #region Internal channel magic

        private object _channelFactoryLock = new object();
        private string _endpointConfigName;

        private ChannelFactory<T> _channelFactory;
        private ChannelFactory<T> CreateChannelFactory(string endpointConfigName)
        {
            // open the client.config
            var clientConfigPath = SPUtility.GetGenericSetupPath(@"WebClients\SBrickey\SPServiceAppTemplate");
            var clientConfig = OpenClientConfiguration(clientConfigPath);

            // configure the channel factory
            ConfigurationChannelFactory<T> factory = new ConfigurationChannelFactory<T>(endpointConfigName, clientConfig, null);
            factory.ConfigureCredentials(SPServiceAuthenticationMode.Claims);

            return factory;
        }

        private string GetEndpointConfigName(Uri endPointAddress)
        {
            if (endPointAddress == null)
                throw new ArgumentNullException("endPointAddress");

            // get the the config name for the provided address
            if (endPointAddress.Scheme == Uri.UriSchemeHttp)
                return "http";

            if (endPointAddress.Scheme == Uri.UriSchemeHttps)
                return "https";

            throw new NotSupportedException("Unsupported endpoint address.");

        }

        private T GetChannel(Uri address)
        {
            string endpointConfig = GetEndpointConfigName(address);

            // if there's a cached channel, use that
            if ((_channelFactory == null) || (endpointConfig != _endpointConfigName))
            {
                lock (_channelFactoryLock)
                {
                    // create a channel factory using the endpoint name
                    _channelFactory = CreateChannelFactory(endpointConfig);
                    // cache the created channel
                    _endpointConfigName = endpointConfig;
                }
            }

            T channel;

            // create a channel that acts as the logged on user when authenticating with the service
            channel = _channelFactory.CreateChannelActingAsLoggedOnUser<T>(new EndpointAddress(address));

            return channel;
        }

        #endregion

        /// <summary>
        /// Path to the WCF Client configuration
        /// ex: @"WebClients\Bdc"
        /// </summary>
        abstract protected string WebClientsConfigPath { get; }

        #region Exposed methods

        protected _Contract_.iServiceAppContract_Ping_Response ChannelIsReady()
        {
            // confirm that the load balancer exists
            if (_loadBalancer == null)
                return new _Contract_.iServiceAppContract_Ping_Response() { Success = false, FailureMessage = "Service App Proxy's Load Balancer is NULL" };

            // declare any objects that will need cleaup in the finalization
            SPServiceLoadBalancerContext loadBalancerContext = null;
            IChannel channel = null;
            try
            {
                // confirm that we can begin an operation (and obtain the context)
                loadBalancerContext = _loadBalancer.BeginOperation();
                if (loadBalancerContext == null)
                    return new _Contract_.iServiceAppContract_Ping_Response() { Success = false, FailureMessage = "Service App Proxy Load Balancer was unable to begin an operation" };

                // confirm that we can get a channel
                channel = (IChannel)GetChannel(loadBalancerContext.EndpointAddress);
                if (channel == null)
                    return new _Contract_.iServiceAppContract_Ping_Response() { Success = false, FailureMessage = "Service App Proxy was unable to obtain a communications channel to the Service App" };

                // all validation passed
                return new _Contract_.iServiceAppContract_Ping_Response() { Success = true };
            }
            catch (Exception ex)
            {
                return new _Contract_.iServiceAppContract_Ping_Response() { Success = false, FailureMessage = ex.Message };
            } // any exception results in failure
            finally
            {
                if (channel != null &&
                    channel.State != CommunicationState.Closed)
                    channel.Close();

                if (loadBalancerContext != null)
                    loadBalancerContext.EndOperation();
            }
        }

        protected void ExecuteOnChannel(Action<T> codeBlock)
        {
            SPServiceLoadBalancerContext loadBalancerContext = _loadBalancer.BeginOperation();

            try
            {
                // get a channel to the service app endpoint
                var channel = (IChannel)GetChannel(loadBalancerContext.EndpointAddress);
                try
                {
                    // execute the code block
                    codeBlock((T)channel);
                    channel.Close();
                }
                catch (TimeoutException)
                {
                    loadBalancerContext.Status = SPServiceLoadBalancerStatus.Failed;
                    throw;
                }
                catch (EndpointNotFoundException)
                {
                    loadBalancerContext.Status = SPServiceLoadBalancerStatus.Failed;
                    throw;
                }
                finally
                {
                    if (channel.State != CommunicationState.Closed)
                        channel.Abort();
                }
            }
            finally
            {
                loadBalancerContext.EndOperation();
            }
        }

        protected I ExecuteOnChannel<I>(Func<T, I> codeBlock)
        {
            SPServiceLoadBalancerContext loadBalancerContext = _loadBalancer.BeginOperation();

            try
            {
                // get a channel to the service app endpoint
                var channel = (IChannel)GetChannel(loadBalancerContext.EndpointAddress);
                try
                {
                    // execute the code block
                    var outval = codeBlock((T)channel);

                    channel.Close();

                    return outval;
                }
                catch (TimeoutException)
                {
                    loadBalancerContext.Status = SPServiceLoadBalancerStatus.Failed;
                    throw;
                }
                catch (EndpointNotFoundException)
                {
                    loadBalancerContext.Status = SPServiceLoadBalancerStatus.Failed;
                    throw;
                }
                finally
                {
                    if (channel.State != CommunicationState.Closed)
                        channel.Abort();
                }
            }
            finally
            {
                loadBalancerContext.EndOperation();
            }
        }

        #endregion




    } // class
} // namespace
