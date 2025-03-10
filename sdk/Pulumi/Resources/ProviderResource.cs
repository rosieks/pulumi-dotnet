// Copyright 2016-2021, Pulumi Corporation

using System.Threading.Tasks;
using Pulumi.Serialization;

namespace Pulumi
{
    /// <summary>
    /// <see cref="ProviderResource"/> is a <see cref="Resource"/> that implements CRUD operations
    /// for other custom resources. These resources are managed similarly to other resources,
    /// including the usual diffing and update semantics.
    /// </summary>
    public class ProviderResource : CustomResource
    {
        internal Task<string> Ref
        {
            get
            {
                return Task.WhenAll(
                    this.Urn.GetValueAsync(whenUnknown: default!),
                    this.Id.GetValueAsync(whenUnknown: default!)
                ).ContinueWith(t =>
                {
                    var providerUrn = t.Result[0];
                    var providerId = t.Result[1];
                    if (string.IsNullOrEmpty(providerId))
                    {
                        providerId = Constants.UnknownValue;
                    }

                    return $"{providerUrn}::{providerId}";
                });
            }
        }

        internal string Package { get; }

        private string? _registrationId;

        /// <summary>
        /// Creates and registers a new provider resource for a particular package.
        /// </summary>
        /// <param name="package">The package associated with this provider.</param>
        /// <param name="name">The unique name of the provider.</param>
        /// <param name="args">The configuration to use for this provider.</param>
        /// <param name="options">A bag of options that control this provider's behavior.</param>
        public ProviderResource(
            string package,
            string name,
            ResourceArgs args,
            CustomResourceOptions? options = null)
            : this(package, name, args, options, dependency: false, null)
        {
        }

        /// <summary>
        /// Creates and registers a new provider resource for a particular package.
        /// </summary>
        /// <param name="package">The package associated with this provider.</param>
        /// <param name="name">The unique name of the provider.</param>
        /// <param name="args">The configuration to use for this provider.</param>
        /// <param name="options">A bag of options that control this provider's behavior.</param>
        /// <param name="registerPackageRequest">Options for package parameterization.</param>
        public ProviderResource(
            string package,
            string name,
            ResourceArgs args,
            CustomResourceOptions? options = null,
            RegisterPackageRequest? registerPackageRequest = null)
            : this(package, name, args, options, dependency: false, registerPackageRequest)
        {
        }

        /// <summary>
        /// Creates and registers a new provider resource for a particular package.
        /// </summary>
        /// <param name="package">The package associated with this provider.</param>
        /// <param name="name">The unique name of the provider.</param>
        /// <param name="args">The configuration to use for this provider.</param>
        /// <param name="options">A bag of options that control this provider's behavior.</param>
        /// <param name="dependency">True if this is a synthetic resource used internally for dependency tracking.</param>
        /// <param name="registerPackageRequest">Options for package parameterization.</param>
        private protected ProviderResource(
            string package, string name,
            ResourceArgs args, CustomResourceOptions? options = null, bool dependency = false, RegisterPackageRequest? registerPackageRequest = null)
            : base($"pulumi:providers:{package}", name, args, options, dependency, registerPackageRequest)
        {
            this.Package = package;
        }

        internal static async Task<string?> RegisterAsync(ProviderResource? provider)
        {
            if (provider == null)
            {
                return null;
            }

            if (provider._registrationId == null)
            {
                var providerRef = await provider.Ref.ConfigureAwait(false);
                provider._registrationId = providerRef;
            }

            return provider._registrationId;
        }
    }
}
