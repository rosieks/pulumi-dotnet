// Copyright 2016-2020, Pulumi Corporation

using System;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Pulumirpc;
using Grpc.Core;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Pulumi
{
    internal class GrpcMonitor : IMonitor
    {
        private readonly ResourceMonitor.ResourceMonitorClient _client;
        private static ConcurrentDictionary<string, GrpcChannel> _monitorChannels = new ConcurrentDictionary<string, GrpcChannel>();
        public GrpcMonitor(string monitor)
        {
            // maxRpcMessageSize raises the gRPC Max Message size from `4194304` (4mb) to `419430400` (400mb)
            var maxRpcMessageSize = 400 * 1024 * 1024;
            if (!_monitorChannels.ContainsKey(monitor))
            {
                // Allow for insecure HTTP/2 transport (only needed for netcoreapp3.x)
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                // Inititialize the monitor channel once for this monitor address
                _monitorChannels[monitor] = GrpcChannel.ForAddress(new Uri($"http://{monitor}"), new GrpcChannelOptions
                {
                    MaxReceiveMessageSize = maxRpcMessageSize,
                    MaxSendMessageSize = maxRpcMessageSize,
                    Credentials = ChannelCredentials.Insecure
                });
            }

            this._client = new ResourceMonitor.ResourceMonitorClient(_monitorChannels[monitor]);
        }
        
        public async Task<SupportsFeatureResponse> SupportsFeatureAsync(SupportsFeatureRequest request)
            => await this._client.SupportsFeatureAsync(request);

        public async Task<InvokeResponse> InvokeAsync(InvokeRequest request)
            => await this._client.InvokeAsync(request);

        public async Task<CallResponse> CallAsync(CallRequest request)
            => await this._client.CallAsync(request);
        
        public async Task<ReadResourceResponse> ReadResourceAsync(Resource resource, ReadResourceRequest request)
            => await this._client.ReadResourceAsync(request);

        public async Task<RegisterResourceResponse> RegisterResourceAsync(Resource resource, RegisterResourceRequest request)
            => await this._client.RegisterResourceAsync(request);
        
        public async Task RegisterResourceOutputsAsync(RegisterResourceOutputsRequest request)
            => await this._client.RegisterResourceOutputsAsync(request);
    }
}
